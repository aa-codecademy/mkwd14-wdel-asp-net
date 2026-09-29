# 🛒 Practical ASP.NET: an online shop with ASP.NET Core MVC, .NET 10 and SQL Server

Class materials and code. Over ten sessions we build **Lamazon**, an online shop with its own admin area, starting from an empty folder:
- a **shop** where customers browse products, fill a cart and place orders,
- a **CMS** (content management system) where admins manage products and handle orders and invoices,

all in one **ASP.NET Core MVC** app (.NET 10) that stores its data in **SQL Server**.

## 📖 About the subject

- 🗓️ **10 sessions** of 2h45 each (17:30–20:45, with two 15-minute breaks).
- 🛍️ **Sessions 1–6** build the shop: the database, the pages, the accounts, the cart and the orders.
- 🧑‍💼 **Sessions 7–10** build the CMS: the dashboard, the tables and the admin workflows.
- 🔨 **A practical subject.** We mostly write code, together. It brings together what you learned in the C#, database and ASP.NET MVC subjects, and adds a few new tools.
- 🎒 **You should already know** C#, OOP, SQL (SQL Server), ASP.NET Core MVC and the basics of Entity Framework Core, plus HTML, CSS and JavaScript.
- 🏁 **By the end** you'll have built a complete layered web app from end to end: database, business logic, security and user interface. It's one more project for your portfolio.

## 🎯 What we build

**Customers** can:
- browse the products, with featured products and discounts,
- register and log in,
- add products to a cart and remove them, without reloading the page,
- place an order.

**Admins** can (in the CMS, at `/Administration`):
- see a dashboard: customers, orders, invoices and sales,
- manage product categories and products,
- accept or reject orders (accepting an order creates its invoice),
- mark invoices as paid or cancelled,
- change a user's role.

```
                    ┌───────────────────────────────────────────────┐
                    │  Browser                                      │
                    │  the shop: Bootstrap 5 + jQuery               │
                    │  the CMS:  Material Dashboard + DataTables    │
                    └───────────────────────────────────────────────┘
               pages, forms, AJAX  │   ▲  HTML, and JSON for
              + the login cookie   ▼   │  the cart and the tables
┌─────────────────────────────────────────────────────────────────────────────┐
│  ASP.NET Core MVC app (.NET 10), one project per layer                      │
│                                                                             │
│  Lamazon.Web          controllers, Razor views, login, the CMS area         │
│  Lamazon.Services     business rules, mapping, validation                   │
│  Lamazon.ViewModels   what the views show and the forms send                │
│  Lamazon.DataAccess   EF Core: DbContext, repositories, migrations          │
│  Lamazon.Domain       entities, enums, exceptions                           │
└─────────────────────────────────────────────────────────────────────────────┘
                                   │   ▲  EF Core
                                   ▼   │
                    ┌───────────────────────────────────────────────┐
                    │  SQL Server Express: LamazonDb                │
                    └───────────────────────────────────────────────┘
```

## 🧰 Tech stack

| | Technology | What it does for us |
|---|---|---|
| 🗄️ **Database** | SQL Server Express + SQL Server Management Studio (SSMS) | the relational database, and a UI to look inside it |
| ⚙️ **Backend** | .NET 10 (LTS) and ASP.NET Core MVC | pages built on the server with controllers and Razor views, and a separate area for the CMS |
| | EF Core 10 | reads and writes SQL Server from C#, and creates the tables through migrations |
| | Cookie authentication | login, roles, and anti-forgery tokens against forged requests |
| | Mapster | copies data between entities and view models |
| | FluentValidation | the validation rules for every form |
| | Serilog | logs to the console, a file and a database table |
| | Polly (`Microsoft.Extensions.Http.Resilience`) | timeouts and retries when we call another web API |
| 🎨 **Frontend** | Razor, Bootstrap 5, jQuery | the shop's pages, and the cart without page reloads |
| | Material Dashboard, DataTables | the CMS layout, and tables that the server pages, searches and sorts |
| | LibMan | installs client-side libraries into `wwwroot/lib` |
| 🛠️ **Tools** | Visual Studio 2026 | the editor, with the .NET 10 SDK |

## 🗓️ Course plan

### 🛍️ The shop

| # | Session | What we cover | You leave with |
|---|---|---|---|
| 1 | **Foundations: the app and the domain** | the finished app and its database diagram, the N-tier architecture, the solution and its five projects, `Directory.Build.props`, `.editorconfig`, Central Package Management, the entities | a solution with five projects and the domain model, building with zero warnings |
| 2 | **Data access with EF Core** | `DbContext`, entity configurations, relationships and indexes, seed data, migrations on SQL Server, the repository pattern | a `LamazonDb` database with tables and seed data, created from C# classes |
| 3 | **The shop: services and views** | view models, Mapster, services, `async` and `CancellationToken`, layouts, sections, partial views, a 404 page | the home page, the products page and product details, with data from the database |
| 4 | **Accounts and cookie authentication** | password hashing, FluentValidation, cookie login with claims, `[Authorize]`, anti-forgery tokens, open redirects | register, log in and log out |
| 5 | **The cart and orders** | a cart in a cookie, jQuery AJAX, LibMan and pop-up notifications, cross-site scripting (XSS), prices the client can't change | a working cart, and orders in the database |
| 6 | **Middleware, HttpClient, Polly and logging** | custom middleware, `IHttpClientFactory`, timeouts and retries with Polly, `ILogger<T>` and Serilog, a second migration | orders that record the customer's country, and logs in a file and a database table |

### 🧑‍💼 The CMS

| # | Session | What we cover | You leave with |
|---|---|---|---|
| 7 | **The CMS: area, dashboard and DataTables** | Areas, role-based authorization, a custom tag helper, a dashboard, server-side DataTables (paging, search, sorting) | an admin-only CMS with a dashboard and its first table |
| 8 | **Categories and products** | create, edit and delete with shared form partials, validation, Post/Redirect/Get, business rules, soft delete | full management of product categories and products |
| 9 | **Orders, invoices and transactions** | the order status workflow, `TransactionScope`, invoices, status rules | the whole flow: order → accepted → invoice → paid |
| 10 | **Security review and wrap-up** | CSRF, XSS, open redirects and other attacks and their fixes, a code review (SOLID, DRY), getting ready for production | the finished app, and ideas for what to build next |

## 🧭 How the sessions work

- 🔁 **Recap first.** Every session starts with a short recap of the previous one.
- ⌨️ **Live coding.** We write the code together, one layer at a time.
- 📄 **Handouts.** Some files (settings, seed data, styles, long page layouts) are pre-written. We open them, explain them and copy them in, instead of typing them.
- 🏠 **Homework.** Short exercises that repeat a pattern from class, such as one more repository or one more page. It's practice, not new material.
- 🆘 **Stuck?** Ask, in class or by email (see [Contact](#contact)). Every class folder has the code as it was at the end of that class, so you can compare your file with the same file there.

## 📚 Class materials

Every class has its own folder: what we do in that class, the concepts behind it, and the code at the end of the class.

## 🔗 Useful links
- ASP.NET Core MVC: https://learn.microsoft.com/aspnet/core/mvc/overview
- EF Core: https://learn.microsoft.com/ef/core
- SQL Server: https://learn.microsoft.com/sql/sql-server
- Bootstrap 5: https://getbootstrap.com/docs/5.3
- DataTables: https://datatables.net/manual
- Mapster: https://github.com/MapsterMapper/Mapster/wiki
- FluentValidation: https://docs.fluentvalidation.net
- Serilog: https://serilog.net
- HTTP resilience (Polly): https://learn.microsoft.com/dotnet/core/resilience/http-resilience
- draw.io (diagrams): https://draw.io

<a id="contact"></a>
## 📬 Contact

**Ilija Mitev**, trainer

- 📧 Email: ilija.mitev3@gmail.com
