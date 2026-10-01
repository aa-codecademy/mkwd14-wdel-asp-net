# 🗄️ Class 02: Data access with EF Core

> 🛍️ The shop · class 2 of 10 · 🏠 [Course overview](../README.md) · 🛠️ [Initial setup](../Class01/Initial-Setup.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

From **C# classes** to a **`LamazonDb` database in SQL Server** with every table and the seed data, plus the **first repository**.

## 🗺️ What we do today

1. 🔁 Recap
2. 📦 The EF Core packages, and `LamazonDbContext` with one `DbSet` per table
3. ⚙️ One configuration class per entity. We type `Product`, `ProductCategory`, `Order`, `OrderLineItem` and `User` together
4. 📄 Handout: the other configurations (the statuses, the invoices, the role) and the seed data (`DataSeedExtensions`)
5. 💉 `AddDataAccess()`: register the `DbContext`; the connection string in `appsettings.json`
6. 🐣 The first migration, then a look at the tables and the seed rows in SSMS
7. 📚 The repository pattern: `BaseRepository<T>` and `ProductsRepository`

---

## 💡 Used concepts
