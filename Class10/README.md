# 🧾 Class 10: Accepting orders, invoices, and a look back

> 🧑‍💼 The CMS · class 10 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 09](../Class09/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

Class 9 ended on an order's page, with **Reject** and **Accept** buttons that didn't do anything yet. Today they work. Accepting an order creates its **invoice**, and the two are saved in one **transaction**: both, or neither. Then the invoices get their table and their page, where the admin marks them paid or cancels them, with the same kind of rules as the orders. That completes the flow the app was built for: a customer orders → the admin accepts → an invoice → paid.

It's the last class, so after the code we look back at the whole app: how it protects itself, and what comes next.

📂 This page explains the ideas behind today's code, with small pieces of it. Every file is whole in the `Lamazon` folder next to this README: open it next to the explanation.

## 🗺️ What we do today

1. 🔁 Recap of class 9, and the products homework
2. 💬 Accept or reject: a confirmation dialog
3. 🔒 Accepting an order: one transaction
4. 🧾 The invoices: the table, the page, paid or cancelled
5. 🔄 The whole flow, on the dashboard
6. 🛡️ A look back: how Lamazon protects itself
7. 🏁 What's next

---

## 💡 Used concepts

### 🔁 From class 9
The CMS has its tables: the categories and the orders, paged, searched and sorted by the server, with a **whitelist** of the columns the browser may sort by and `DataTable.render.text()` on every text column. An admin creates, edits and deletes categories: explicit validation, **Post/Redirect/Get** with a green notification, and a **soft delete** that a business rule can refuse (`BusinessRuleException` → a red notification). An order's page shows its customer and its lines, and **Reject** and **Accept** while the order is Pending.

**The homework: the products in the CMS.** The `Class10` folder has the answer: compare it with your files, or start from the folder. Two lines from it now help every page:
- `app.UseRequestLocalization("en-US");` in `Program.cs`: numbers are read and written as `1,234.56` on every computer. The amounts on the dashboard and on the order's page change from `1.711,99` to `1,711.99` on a Windows set to Macedonian.
- `SetValueMustNotBeNullAccessor` in `AddWeb()`: "This field is required." when a number field is sent empty.

🧪 **Check it first:** log in as `admin@lamazon.com`. **Products** in the sidebar shows the table, and a product created there appears in the shop. **Orders** shows Jane's three **Pending** orders from class 9. No orders? In a private window, log in as `user@lamazon.com` / `User123!` and place three, with different products: today we accept one, reject one, and use the third to break things on purpose.

---

### 💬 Accept or reject: a confirmation dialog
The buttons on an order's page change data, so they don't act straight away. Each one opens a dialog (a **modal**) that asks "are you sure?", and the dialog's form does the POST. It's the categories' Delete page again (class 9): a link or a button never changes data by itself, and the POST carries the anti-forgery token.

The order's page needs two dialogs, and the invoice's page will need two more, so the dialog is a partial, `_ConfirmModal.cshtml`, with a small model of its own, `ConfirmModalViewModel`. Each page fills one in:
```cshtml
<partial name="_ConfirmModal" model="@(new ConfirmModalViewModel
{
    ModalId = "acceptModal",
    Title = "Accept order",
    Message = $"Accept order {Model.OrderNumber}? Its invoice will be created.",
    Action = "Accept",
    EntityId = Model.Id,
    ConfirmText = "Accept",
    ConfirmCssClass = "btn-success",
})" />
```
Inside the partial, the form posts to that action, with the id in the URL:
```cshtml
<form asp-action="@Model.Action" asp-route-id="@Model.EntityId" method="post">
```
| Piece | Why |
|---|---|
| `data-toggle="modal" data-target="#acceptModal"` on the button (already in the view) | opens the dialog whose `id` is `acceptModal`: the `ModalId`. It's Bootstrap's own JavaScript, so we write none. The CMS runs on Bootstrap 4 (Material Dashboard), hence `data-toggle`; the shop's Bootstrap 5 says `data-bs-toggle` |
| `asp-action` + `asp-route-id` | `POST /Administration/Orders/Accept/5`. The form tag helper adds the anti-forgery token, as on every form |
| the `@if (Model.OrderStatus == OrderStatusEnum.Pending)` around the dialogs | no buttons and no dialogs once the order is decided. That's only the view, though: the service checks it again (next section) |
| `ConfirmModalViewModel` in `Lamazon.Web/Models`, next to `ErrorViewModel` | it describes a piece of a page, not data from the database. Only the views use it, and the services never see it |

🧪 **Try it:** an order's page → **Accept** opens the dialog; **Cancel** and **×** close it. **Accept** in the dialog takes you back to the orders table, and the order is still Pending: the action is still empty. That's next.

---

### 🔒 Accepting an order: one transaction
**Accept = two changes.** The order's status becomes Accepted (an `UPDATE` of `Orders`), and a new invoice is created with its lines (an `INSERT` into `Invoices` and `InvoicesLineItems`). `BaseRepository` saves after each change, so that's two `SaveChanges`. What if the second one fails? The order would be Accepted with no invoice, and nobody would ever bill it.

```
AcceptAsync(id)                           one transaction, from the first line to Complete()
 ├─ load the order (tracked)              SELECT … FROM Orders …
 ├─ still Pending? no → BusinessRuleException
 ├─ status = Accepted, UpdateAsync        UPDATE Orders SET OrderStatusId = 2 …
 ├─ Order → Invoice (Mapster)
 ├─ the number: the highest id + 1        SELECT MAX(Id) FROM Invoices
 ├─ AddAsync(invoice)                     INSERT INTO Invoices …, then InvoicesLineItems …
 └─ scope.Complete()                      COMMIT
```

**The invoice is a copy of the order**, made by Mapster in a new `InvoiceMappingConfig`:
```csharp
config.NewConfig<Order, Invoice>()
    .Ignore(dest => dest.Id)
    .Ignore(dest => dest.User)
    .Map(dest => dest.OrderId, src => src.Id)
    .Map(dest => dest.InvoiceDate, src => DateTime.UtcNow)
    .Map(dest => dest.InvoiceStatusId, src => (int)InvoiceStatusEnum.PendingPayment)
    .Map(dest => dest.InvoiceLineItems, src => src.OrderLineItems);
```
| Line | Why |
|---|---|
| `Ignore(Id)` | the database gives the invoice its own id. Copied, the order's id would become the invoice's |
| `Ignore(User)` | Mapster would copy the customer too, and EF Core would try to save that copy as a second user. The invoice needs only `UserId`, which Mapster copies by name, like `TotalAmount` |
| `OrderId` ← `Id` | the invoice points to its order. `Invoices.OrderId` has a unique index (`InvoiceConfiguration`): one order, at most one invoice |
| `InvoiceDate`, `InvoiceStatusId` | today, in UTC like every date in Lamazon, and **Pending payment** |
| `InvoiceLineItems` ← `OrderLineItems` | the lines are copied too, with a second mapping: `OrderLineItem` → `InvoiceLineItem`, which ignores `Id` and `Product`, and sets `OrderLineItemId` from the line's `Id` |

**The invoice number** is built like the order's (class 6): the highest id so far + 1, and the year, `1/2026`. `InvoicesRepository` gets the same `GetMaxIdAsync` as `OrdersRepository`.

**The transaction**, in `OrdersService`:
```csharp
public async Task AcceptAsync(int id, CancellationToken cancellationToken = default)
{
    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

    var order = await FindOrderAsync(id, cancellationToken);
    EnsurePending(order, "accepted");

    order.OrderStatusId = (int)OrderStatusEnum.Accepted;
    await _ordersRepository.UpdateAsync(order, cancellationToken);

    var invoice = _mapper.Map<Invoice>(order);
    var maxId = await _invoicesRepository.GetMaxIdAsync(cancellationToken);
    invoice.InvoiceNumber = $"{maxId + 1}/{invoice.InvoiceDate.ToLocalTime().Year}";
    await _invoicesRepository.AddAsync(invoice, cancellationToken);

    scope.Complete();
}
```
| Piece | Why |
|---|---|
| `using var scope = new TransactionScope(...)` | from here to the end of the method, every database call is part of one transaction. `TransactionScope` is in `System.Transactions`, part of .NET: no package |
| `TransactionScopeAsyncFlowOption.Enabled` | the transaction follows the code across every `await`. Without it, the scope belongs to the thread that created it, and the code after an `await` can run on another thread |
| `scope.Complete()` | the last line, "everything worked". The `using` disposes the scope when the method ends, and a completed scope **commits** |
| no `Complete()` | an exception left the method before the last line. The scope is disposed without it and **rolls back**: the order's `UPDATE` is undone |
| no `try`/`catch` | last year's version caught the error, logged it, and showed "Order was accepted" anyway: the order had no invoice and nobody knew. Here the error goes up to the error page, which is the honest answer |

**The rule: only Pending orders.** The view hides the buttons, but a second tab, the browser's Back button or a hand-made POST can still send an Accept. The service checks:
```csharp
private static void EnsurePending(Order order, string action)
{
    if (order.OrderStatusId != (int)OrderStatusEnum.Pending)
    {
        var status = (OrderStatusEnum)order.OrderStatusId;
        throw new BusinessRuleException($"Only pending orders can be {action}. Order {order.OrderNumber} is {status}.");
    }
}
```
Last year, accepting twice made two invoices. Now the unique index on `Invoices.OrderId` would refuse the second one anyway, but with an error page. The rule turns it into a message the admin understands: a red notification, back on the order's page (`AppExceptionFilter`, class 7).

**Reject** is the same rule and one `UPDATE`: the status becomes Rejected, and there's no invoice. One `SaveChanges` is a transaction of its own, so it needs no `TransactionScope`.

**The controller:** `Accept` and `Reject` call the service, add a green notification (`AddSuccessNotification`), and go back to the orders table. They have no `try`/`catch`: a `BusinessRuleException` goes to `AppExceptionFilter`.

🧪 **Try it:**
| Do | You should see |
|---|---|
| Jane's first order → **Accept** → **Accept** in the dialog | the orders table, a green "The order was accepted and its invoice was created.", and the order **Accepted** |
| its 👁️ icon again | the order, with no buttons |
| the dashboard | Orders: one Accepted, and its total under "accepted". Invoices: 1, Pending: 1 |
| in SSMS: `SELECT Id, InvoiceNumber, InvoiceDate, OrderId, InvoiceStatusId, TotalAmount FROM Invoices;` | `1/2026`, status `1` (Pending payment), the order's id and total. The date is in UTC: one hour earlier than on the page (winter time) |
| `SELECT InvoiceId, OrderLineItemId, ProductName, ProductPrice, TotalPrice FROM InvoicesLineItems;` | one line for each line of the order. The table is called `InvoicesLineItems`, after the `DbSet` in `LamazonDbContext` |
| the second order, open in **two tabs**. In the first: **Reject** | "The order was rejected." |
| in the second tab, which still has the buttons: **Accept** | back on the order's page, with a red "Only pending orders can be accepted. Order …/2026 is Rejected.", and no buttons now |

🧨 **The rollback, live.** Make the invoice fail on purpose, and watch the order's status:
1. In `AcceptAsync`, add a test line right after the one that sets the number. `1/2026` is taken, and `InvoiceNumber` has a unique index:
   ```csharp
   invoice.InvoiceNumber = "1/2026";
   ```
2. Accept the **third** order. The error page says *Cannot insert duplicate key row in object 'dbo.Invoices' with unique index 'IX_Invoices_InvoiceNumber'*. In SSMS, the order is still **Pending**: its `UPDATE` was rolled back.
3. Now comment out the `using var scope = …` line and `scope.Complete();`, and accept it again. The same error, but this time the order stays **Accepted**, with no invoice:
   ```sql
   -- Accepted, but no invoice: what the transaction prevents
   SELECT Id, OrderNumber, OrderStatusId FROM Orders
   WHERE OrderStatusId = 2 AND Id NOT IN (SELECT OrderId FROM Invoices);
   ```
4. Put everything back: delete the test line, and uncomment the two lines. Then give the order its Pending status back:
   ```sql
   UPDATE Orders SET OrderStatusId = 1
   WHERE OrderStatusId = 2 AND Id NOT IN (SELECT OrderId FROM Invoices);
   ```

---

### 🧾 The invoices
The invoices pages are the orders pages again: a table, a page per invoice, and two buttons. Most of the files are copies with `Invoice` instead of `Order` (they're all in the `Class10` folder). What's new:

**The view models.** `InvoiceViewModel` is like `OrderViewModel`: the status as the view models' own enum, and its name as text for the table. `Lamazon.ViewModels/Enums` gets `InvoiceStatusEnum`, a copy of Domain's with the same ids (`PendingPayment = 1`, `Paid = 2`, `Canceled = 3`), and `InvoiceLineItemViewModel` holds one line.

**Two enums with the same name, in one file.** `InvoiceMappingConfig` now needs both: Domain's `InvoiceStatusEnum` for Order → Invoice (`(int)InvoiceStatusEnum.PendingPayment`), and the view models' for Invoice → `InvoiceViewModel`. With both namespaces in `using`s, the compiler can't tell which one you mean: *CS0104: 'InvoiceStatusEnum' is an ambiguous reference*. An **alias** gives the second one its own name:
```csharp
using InvoiceStatusViewModelEnum = Lamazon.ViewModels.Enums.InvoiceStatusEnum;
```
```csharp
config.NewConfig<Invoice, InvoiceViewModel>()
    .Map(dest => dest.InvoiceStatus, src => (InvoiceStatusViewModelEnum)src.InvoiceStatusId)
    .Map(dest => dest.InvoiceDate, src => src.InvoiceDate.ToLocalTime());
```

**The repository:** `GetByIdAsync` (tracked, with the lines and the customer, because the status is changed and saved afterwards) and `GetFilteredAsync` with a whitelist: the orders' code, with `Invoice`. The number sorts by `Id`, and the status by `InvoiceStatusId`, for the same reasons as the orders' (class 9).

**The rule, in one place.** Paying and cancelling are two actions with the same rule: only an invoice that's still **Pending payment** can change, because a paid or cancelled invoice is final. So `InvoicesService` has one private method for both:
```csharp
private async Task ChangeStatusAsync(int id, InvoiceStatusEnum newStatus, CancellationToken cancellationToken)
{
    var invoice = await FindInvoiceAsync(id, cancellationToken);

    // A paid or canceled invoice is final
    if (invoice.InvoiceStatusId != (int)InvoiceStatusEnum.PendingPayment)
    {
        var status = (InvoiceStatusEnum)invoice.InvoiceStatusId;
        throw new BusinessRuleException($"Invoice {invoice.InvoiceNumber} is already {status}.");
    }

    invoice.InvoiceStatusId = (int)newStatus;
    await _invoicesRepository.UpdateAsync(invoice, cancellationToken);
}
```
`SetAsPaidAsync` and `CancelAsync` are one line each: `await ChangeStatusAsync(id, InvoiceStatusEnum.Paid, cancellationToken);` and the same with `Canceled`.

The two workflows, one after the other:
```
Orders:    Pending ──Accept──▶ Accepted  (and its invoice is created: Pending payment)
              │
              └──Reject──▶ Rejected

Invoices:  Pending payment ──Set as paid──▶ Paid
              │
              └──Cancel──▶ Canceled
```

**The controller**, `InvoicesController`, is the orders' controller with other names: `Index`, `GetInvoices`, `Details`, and two POSTs, `SetAsPaid` and `Cancel`.

**The invoice's page** uses `_ConfirmModal` twice, for **Cancel invoice** and **Set as paid**. That's the partial paying off: two more dialogs, and no new markup.

🧪 **Try it:**
| Do | You should see |
|---|---|
| **Invoices** in the sidebar | `1/2026`, Jane Doe, the total, **PendingPayment**, and **Invoices** active in the sidebar |
| its 👁️ icon | the invoice: its number, the date in local time, Jane Doe and her email, the total, the status, the lines, and **Cancel invoice** and **Set as paid** |
| **Set as paid** → confirm | the table, a green "The invoice was marked as paid.", and **Paid** |
| accept the third order (it's Pending again after the rollback demo) | invoice `2/2026` |
| `2/2026` open in **two tabs**. In the first: **Cancel invoice** → confirm | "The invoice was canceled.", and **Canceled** |
| in the second tab: **Set as paid** → confirm | back on the invoice's page, with a red "Invoice 2/2026 is already Canceled." |
| search `jane` | Jane Doe's invoices |
| `/Administration/Invoices/Details/9999` | the "not found" page |

---

### 🔄 The whole flow
Everything at once, with the dashboard open in the admin's window. Jane orders in the private window:

| Step | Where | On the dashboard |
|---|---|---|
| Jane places an order | the shop | Orders +1, Pending +1 |
| the admin accepts it | CMS → Orders → 👁️ → **Accept** | Pending −1, Accepted +1, and the "accepted" total grows. Invoices +1, Pending +1 |
| the admin marks the invoice paid | CMS → Invoices → 👁️ → **Set as paid** | Invoices: Pending −1, Paid +1, and the "paid" total grows |

The dashboard counts with one `GROUP BY` per table (class 8), so it was ready for statuses that didn't exist yet.

---

### 🛡️ A look back: how Lamazon protects itself
Most of these attacks were shown live during the course, and each one has a fix in our code. The table is also a checklist for reviewing code: yours, a colleague's, or an AI's.

| Attack | What could happen | What stops it in Lamazon | Class |
|---|---|---|---|
| 🎭 **CSRF**, a forged request | another site makes your browser send a POST to Lamazon, with your login cookie | anti-forgery tokens: the `<form>` tag helper adds them, `[ValidateAntiForgeryToken]` checks them, `site.js` sends them in a header; Logout is a POST | 5, 6 |
| 💉 **XSS**, cross-site scripting | text that someone typed runs as JavaScript in another person's browser, even an admin's | Razor's `@` encodes; `_Notification` uses `Html.Encode` + `Json.Serialize`; every CMS table uses `DataTable.render.text()` | 6, 9 |
| ↪️ **Open redirect** | `Login?returnUrl=https://evil.example` sends a user from our login page to a fake one | `Url.IsLocalUrl` → `LocalRedirect`; `AppExceptionFilter` follows the `Referer` back only to our own site | 5, 7 |
| 💰 **Price tampering** | a price in a form or a cookie, edited in the dev tools | the browser sends only product ids; the prices and the total come from the database | 6 |
| 🔢 **Sorting by anything** | a hand-made request sorts a table by a column we never meant to expose, like `PasswordHash` | each repository's `_sortableColumns` whitelist | 9 |
| 🔑 **Stolen passwords**, and guessing who has an account | a leaked table of passwords; "user not found" tells an attacker which emails exist | `IPasswordHasher<User>`: PBKDF2 with a salt; the same "Invalid email or password." for both mistakes | 4 |
| 🍪 **Cookie theft and editing** | JavaScript reads the login cookie; a hand-edited cart cookie crashes the page | `HttpOnly` and `SameSite=Lax`; a broken cart cookie becomes an empty cart | 5, 6 |
| 🚪 **A customer in the CMS** | `/Administration` opened by anyone with an account | `[Authorize(Roles = Roles.Admin)]` on `AdminBaseController`: every CMS page and every JSON answer | 8 |
| 🧾 **Skipping a rule** | a second tab or a hand-made POST accepts an order twice | the rules live in the services (`EnsurePending`, `ChangeStatusAsync`, `HasActiveProductsAsync`), not in hidden buttons | 9, 10 |
| 🔗 **A GET that changes data** | a prefetched link, a crawler or an `<img src>` deletes something | Delete pages and dialogs whose forms POST | 9, 10 |
| 📡 **Listening on the network** | passwords and cookies read on the Wi-Fi | HTTPS: `UseHttpsRedirection`, and `UseHsts` outside Development | 1 |

**The code, layer by layer:**
- 🧱 **Each layer registers its own classes:** `AddDataAccess`, `AddServices` and `AddWeb`, each in its own project. `Program.cs` only calls them, and it reads like a table of contents.
- ♻️ **One place for each job:** `BaseRepository` saves, `QueryableExtensions` sorts and pages, `AppExceptionFilter` turns errors into pages and messages, `admin-site.js` holds the tables' defaults, and the partials (`_ProductForm`, `_ConfirmModal`, …) hold the markup that repeats. A fix in one of them fixes every page.
- ⏱️ **async all the way,** with a `CancellationToken` from the action down to SQL Server (class 3).

**Towards production:** what changes when Lamazon runs on a real server.
- 🌍 **The environment.** `ASPNETCORE_ENVIRONMENT` is `Development` on your computer (`launchSettings.json`). On a server it's `Production`, and `Program.cs` switches to the friendly error page (`UseExceptionHandler`) and HSTS, without `DebugIpAddressMiddleware`. Today's rollback demo would show the friendly error page there, not the details of the exception.
- 🔐 **Secrets.** Our connection string has no password: `Trusted_Connection=True` logs in with your Windows account. A real server usually has a SQL login with a password, which must never be in `appsettings.json` in Git. On your computer, use user secrets: in the `Lamazon.Web` folder, `dotnet user-secrets init` once, then `dotnet user-secrets set "ConnectionStrings:LamazonDb" "…"`. On the server, use an environment variable: `ConnectionStrings__LamazonDb`.
- 🕒 **The time zone.** The database stores UTC, and the pages show the **server's** local time. A server in another country shows its own time.
- 📦 **Publishing.** `dotnet publish -c Release` makes a folder to copy to the server. The database gets the migrations with `dotnet ef database update`, or a SQL script for the server's administrator: `dotnet ef migrations script --idempotent`.

---

## 🏁 What's next
Lamazon works from end to end, and it's a good base to keep practicing on. Ideas, from the simplest:
- 👤 **The Users tab** in the CMS. The sidebar already links to it. A table of the users with a role filter and a search, and **Edit**, where only the role can change. One rule: the last admin can't lose the admin role. It's all patterns you know.
- 🛒 **A quantity in the cart.** Today every line has a quantity of 1.
- 🛟 **Polly and Serilog:** retries and timeouts for the geo-location API, and logs in a file and a database table. They were on the course plan, but we didn't get to them. The links are in the [course overview](../README.md).
- 🧪 **Automated tests,** **ASP.NET Core Identity** (instead of our own users table), and a **Web API** for a mobile app: each one a project of its own.

## ✅ By the end of the class
- 💬 Accept and Reject ask for a confirmation in a dialog, and the dialog's form POSTs with its token
- 🔒 Accepting an order creates its invoice in the same transaction: when the invoice fails, the order stays Pending
- 🚦 An order that isn't Pending can't be accepted or rejected, and an invoice that isn't Pending payment can't change: a red notification
- 🧾 The invoices table pages, searches and sorts, and an invoice's page marks it paid or cancels it
- 📊 The dashboard follows the whole flow: order → accepted → invoice → paid

---

🎓 **That's the course.** You built a layered web app from an empty folder: the database, the business rules, the security, and two user interfaces.
