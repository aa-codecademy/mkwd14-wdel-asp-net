# 🧑‍💼 Class 08: The CMS: area, dashboard and DataTables

> 🧑‍💼 The CMS · class 8 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 07](../Class07/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

The shop is finished, and today the **CMS** starts: a part of the app that only admins can open, at `/Administration`, with its own controllers, views and layout. A **custom tag helper** marks the current page in the sidebar, the **dashboard** shows the numbers of the shop, and the product categories get the first admin table with **DataTables**. The server does the paging, the search and the sorting, so the table stays fast however many rows there are.

📂 This page explains the ideas behind today's code, with small pieces of it. Every file is whole in the `Lamazon` folder next to this README: open it next to the explanation.

## 🗺️ What we do today

1. 🔁 Recap of class 7
2. 🧑‍💼 The CMS: an area for admins only
3. 🎨 The CMS layout with Material Dashboard (a handout)
4. 🏷️ A custom tag helper: the current page in the sidebar
5. 📊 The dashboard
6. 📋 DataTables: paging, search and sorting on the server
7. 🗂️ The first table: product categories

---

## 💡 Used concepts
