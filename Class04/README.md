# 🔑 Class 04: Accounts and cookie authentication

> 🛍️ The shop · class 4 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 03](../Class03/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

First the **rest of class 3**: the products page, a product's details, the nested layout and a friendly **404 page**. Then **accounts**: register, log in and log out.

## 🗺️ What we do today

1. 🔁 Recap
2. 🛍️ From class 3: `ProductsController`, its views, the `_ProductItem` and `_ProductPrice` partials, and the tag helpers in them
3. 🪆 From class 3: the nested layout `_LayoutNoFooter`
4. 🚫 From class 3: errors. `/Products/Details/999` → `NotFoundException` → the 404 page; `AddWeb()`
5. 🔐 Authentication vs authorization; a cookie vs a token; claims
6. 👤 `UsersRepository`, the user view models and `UserMappingConfig`
7. 🧂 Password hashing with `IPasswordHasher<User>`
8. ⚙️ `UsersService`: register, and check a login
9. ✅ FluentValidation
10. 🍪 Cookie authentication
11. 🖥️ `UsersController`: Login, Register, Logout, `[Authorize]`, anti-forgery tokens

---

## 💡 Used concepts
