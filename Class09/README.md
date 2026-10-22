# 🗂️ Class 09: Categories and orders in the CMS

> 🧑‍💼 The CMS · class 9 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 08](../Class08/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

Class 8 got DataTables ready: the contract, the shared defaults, and the request as view models. Today the server answers, and the CMS gets its first table: the **product categories**, paged, searched and sorted by the server. Then the CMS starts **changing data**: an admin creates, edits and deletes categories, with validation, **Post/Redirect/Get**, and a **soft delete** that a business rule can refuse. Last, the **orders**: their table, and the admin's decision. Accepting an order creates its invoice, and the two are saved together in one **transaction**, or not at all.

📂 This page explains the ideas behind today's code, with small pieces of it. Every file is whole in the `Lamazon` folder next to this README: open it next to the explanation.

## 🗺️ What we do today

1. 🔁 Recap of class 8
2. 📋 DataTables, the rest: the answer, the repository side, sorting and paging
3. 🗂️ The first table: product categories
4. ✏️ Create and edit a category
5. 🗑️ Delete a category: a soft delete and a business rule
6. 📦 The orders table
7. ✅ Accept or reject an order: one transaction

---

## 💡 Used concepts

### 🔁 From class 8
The CMS is an **area**, `/Administration`, for admins only: `AdminBaseController` carries `[Area]` and `[Authorize(Roles = Roles.Admin)]`, and every admin controller inherits them. `ActiveRouteTagHelper` colors the current page in the sidebar. The dashboard counts with one `GROUP BY` per table. DataTables 3 and `admin-site.js` are on every CMS page, and the four request view models (`DataTableRequestViewModel` and its parts) are in `Lamazon.ViewModels/DataTable`.

🧪 **Check it first:** log in as `admin@lamazon.com`, open the CMS, and compare the dashboard with SSMS. **Product Categories** in the sidebar is still an empty page: today it gets its table.

---
