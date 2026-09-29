# 🗂️ The Lamazon domain: what the tables mean

What each table stands for in the shop's business, how the tables are connected, and the rules they follow.
This page doesn't list every column: the database diagram shows those. It explains **why** the data looks the way it does.

---

## 🗺️ The big picture

Lamazon's data falls into four groups:

| Group | Tables | In one sentence |
|---|---|---|
| 🛍️ **Catalog** | `ProductCategories`, `Products` | what we sell |
| 👥 **People** | `Roles`, `Users` | who uses the shop, and what they're allowed to do |
| 🧾 **Sales** | `Orders`, `OrderLineItems`, `Invoices`, `InvoiceLineItems` | what was bought, and what was billed |
| 🏷️ **Statuses** | `ProductCategoryStatuses`, `ProductStatuses`, `OrderStatuses`, `InvoiceStatuses` | the states the other rows can be in |

Plus one technical table, `Logs`, which has nothing to do with the business (see [Logs](#logs)).

```
 CATALOG                                PEOPLE
 ┌─────────────────┐                    ┌──────┐
 │ ProductCategory │                    │ Role │
 └────────┬────────┘                    └──┬───┘
          │ has many                       │ has many
 ┌────────▼────────┐                    ┌──▼───┐
 │     Product     │                    │ User │
 └────────┬────────┘                    └──┬───┘
          │ copied into                    │ places
 ─ ─ ─ ─ ─│─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ │ ─ ─ ─ SALES
 ┌────────▼────────┐    has many       ┌───▼───┐
 │  OrderLineItem  │◄──────────────────┤ Order │
 └────────┬────────┘                   └───┬───┘
          │ copied into                    │ accepting it creates
 ┌────────▼────────┐    has many      ┌────▼────┐
 │ InvoiceLineItem │◄─────────────────┤ Invoice │
 └─────────────────┘                  └─────────┘
```
Not drawn, to keep the picture readable:
- every invoice also points to its customer (`User`);
- every invoice line also points to its `Product`;
- each catalog and sales table points to its status table.

---

## 🛍️ Catalog

### 📂 ProductCategory: a shelf in the shop
A group of products, such as *Food*, *Drinks*, *Books*, *Software* or *Computers*. Every product sits on exactly one shelf.

- **Who manages it:** admins create, rename and delete categories in the CMS.
- **Deleting is a "soft delete":** the row stays, and its status becomes *Deleted*. A deleted category disappears from the CMS lists and from the category drop-down.
- **🚫 A category that still has products can't be deleted.** Every product must keep a category the admin can see and select. Move or delete the products first.

### 📦 Product: something we sell
A product has a name, a description, a picture (a link to an image), a category and a price.

- **The discount:** `DiscountPercentage` (0–100) lowers the price. The price the customer actually pays, `DiscountedPrice`, is **calculated** from the price and the discount, and never stored: if it were stored, the two could disagree.
  > *Clean Code* costs $55.00 with a 10% discount, so the customer pays $49.50. The shop shows the old price crossed out.
- **Featured products** (`IsFeatured`) are the ones on the home page.
- **Deleting is a soft delete too.** Old orders and invoices still point to the product, so the row must stay. A deleted product disappears from the shop and from the CMS, and it can't be added to a cart.
- **Not modelled (yet):** stock levels. Lamazon never runs out of anything.

---

## 👥 People

### 🎭 Role: what a user is allowed to do
There are exactly two roles:

| Key | Name | Can |
|---|---|---|
| `user` | User | shop: fill a cart, place orders |
| `admin` | Administrator | everything a user can, plus the whole CMS (`/Administration`) |

The key is a readable word (`"admin"`), not a number. That's why `Role` is the only table without a numeric `Id`, and the code can say `[Authorize(Roles = Roles.Admin)]`.

### 👤 User: an account
Anyone who registers gets an account: a full name, an email and a password.

- **The email is the login name, and it's unique:** one account per email address.
- **The password is never stored,** only a salted hash of it (`PasswordHash`). Not even an admin, nor anyone with access to the database, can read a password back.
- **Everyone who registers is a customer** (the `user` role). Only an admin can make someone an admin, in the CMS.
- **🚫 There's always at least one admin.** The last admin can't lose the admin role, or nobody could open the CMS any more.
- **A role change works from the user's next login,** because the role is stored in the login cookie when the user logs in.
- **The seeded accounts:** `admin@lamazon.com` / `Admin123!` (Bob Bobsky, admin) and `user@lamazon.com` / `User123!` (Jane Doe, customer).

The dashboard's "Customers" number counts the users with the `user` role.

---

## 🧾 Sales

> 🛒 **The shopping cart isn't a table.** Until the customer places the order, the cart lives in a cookie in the browser (just the product ids). The database only hears about it when the order is placed.

### 📋 Order: "I want to buy these products"
Created when a logged-in customer clicks **Create order** in the cart.

- **Order number:** `7/2026` style: a running number, then the year. It's unique (the database checks it), and it's what people use when they talk about an order. `Id` is only for the database.
- **Total:** the sum of the order's lines, **calculated by the server** from the prices in the database. The browser only says *which* products are in the cart, never what they cost.
- **Where it came from:** the customer's IP address, the country it belongs to, and that country's flag. It's shown to the admin and is optional: when the location service doesn't answer, the order is still placed, just without a country.
- **Status:**
  ```
                  admin: Accept
   Pending ──────────────────────► Accepted   (an invoice is created)
      │
      │ admin: Reject
      ▼
   Rejected
  ```
  Every new order is *Pending*. **Only a Pending order can be accepted or rejected**, and once decided, the decision is final.
- **Not modelled (yet):** a delivery address, and an order history page for the customer.

### 🧺 OrderLineItem: one product in an order
One line per product: which product, its name, the price paid, the discount it had, the quantity and the line total.

**Why copy the name and price when the product table already has them?** Because the line is a **snapshot of the moment of purchase**. If the admin changes the price or deletes the product next week, this order must still show what the customer actually agreed to pay. The line keeps its link to the product too, so we always know which product it was.

For now the quantity is always 1: the cart holds each product only once.

### 🧾 Invoice: the bill
Created **automatically when an admin accepts an order**. Nobody creates an invoice by hand.

- **It copies the order:** the customer, the total and all the lines. It keeps a link to its order.
- **Invoice number:** `3/2026` style, unique, like the order number. The two numbers are counted separately: order `7/2026` can get invoice `3/2026`.
- **All or nothing:** accepting an order makes two changes, the order's status and the new invoice. They're saved in **one transaction**, so either both happen or neither does. There's never an accepted order without an invoice, or an invoice for an order that is still pending.
- **Status:**
  ```
                            admin: Set as paid
   Pending payment ──────────────────────────► Paid
          │
          │ admin: Cancel invoice
          ▼
       Canceled
  ```
  **Only an invoice that is *Pending payment* can change.** *Paid* and *Canceled* are final.
- **"Paid" is only a status** that the admin sets. Lamazon doesn't take payments; that would be a job for a payment provider.
- The table allows several invoices per order (for example a corrected one, one day), but today every accepted order has exactly one.

### 📄 InvoiceLineItem: one line on the bill
A copy of an order line (product, name, price, discount, quantity, total), linked back to the order line it came from and to the product. It's copied, not shared, for the same reason as the order lines: an invoice is a document, and it must never change after it's issued.

---

## 🏷️ Status tables

Categories, products, orders and invoices each have a **status**, and each kind of status has its own small table:

| Table | Statuses |
|---|---|
| `ProductCategoryStatuses` | Active, Deleted |
| `ProductStatuses` | Active, Deleted |
| `OrderStatuses` | Pending, Accepted, Rejected |
| `InvoiceStatuses` | Pending payment, Paid, Canceled |

Each table has a twin **enum** in C# with the same ids (`OrderStatusEnum.Accepted = 2`):
- **the table** keeps the database honest: a status id must exist, and anyone reading the data in SSMS sees a name instead of a number;
- **the enum** keeps the code readable: `OrderStatusEnum.Accepted` instead of the magic number `2`.

*Deleted* has the id 255, far from the others. That leaves room to add new statuses later (for example *Out of stock*) without renumbering anything.

---

<a id="logs"></a>
## 📜 Logs

Not business data. When something goes wrong (a warning or an error), the app writes a row here: when it happened, how serious it was, the message, and the error details. Admins and developers read it to find problems. The app writes the rows through Serilog; EF Core only creates the table, with the migrations.

---

## 🔄 One purchase, through the tables

1. **Jane browses** the shop. Only the catalog is read: `Products` with their `ProductCategories`.
2. **She adds two books to her cart.** Nothing is written to the database; the cart is a cookie.
3. **She logs in and clicks Create order:**
   - one row in `Orders`: `8/2026`, *Pending*, with her country;
   - two rows in `OrderLineItems`, with the names and prices of that moment.
4. **The admin opens the order in the CMS and accepts it.** In one transaction:
   - the order becomes *Accepted*;
   - one row in `Invoices`: `5/2026`, *Pending payment*;
   - two rows in `InvoiceLineItems`.
5. **The payment arrives, and the admin clicks Set as paid.** The invoice becomes *Paid*, and the dashboard's "paid" amount grows.
6. **Next month the admin raises the books' prices.** Jane's order and invoice don't change: their lines are snapshots.

---
