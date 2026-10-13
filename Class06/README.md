# 🛒 Class 06: The cart and orders

> 🛍️ The shop · class 6 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 05](../Class05/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

The **rest of class 5**: pop-up notifications, a **cart** that works on every page without reloading it, and **orders** saved with the prices from the database. Middleware, HttpClient, Polly and logging move to class 7.

## 🗺️ What we do today

1. 🔁 Recap and the class 5 homework. The last fixes from class 5 are already done in the `Class06` folder: copy what your code is missing
2. 📦 LibMan, and pop-up notifications with Notyf. XSS: why a message needs encoding
3. 🍪 The cart in a cookie: `BaseController`
4. ➕ `AddToCart` and `RemoveFromCart`: actions that answer with JSON
5. ⚡ `site.js`: Add to cart and Remove from cart without reloading the page
6. 🛒 The cart page (mostly a handout)
7. 📦 Placing an order: the prices come from the database, never from the browser
8. ⏩ If there's time: middleware

---

## 💡 Used concepts
