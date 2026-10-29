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
