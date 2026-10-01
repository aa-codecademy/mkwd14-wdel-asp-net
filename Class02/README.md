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

### 📦 The EF Core packages
| Package | Project | Why |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | `Lamazon.DataAccess` | EF Core and the SQL Server provider |
| `Microsoft.EntityFrameworkCore.Design` | `Lamazon.Web` | the migration commands need it, at design time only |
| `Microsoft.EntityFrameworkCore.Tools` | `Lamazon.Web` | the Package Manager Console commands (`Add-Migration`, `Update-Database`) |

The versions live only in `Directory.Packages.props` (Central Package Management, class 1). A `.csproj` only says **which** package it uses.

---

### 🗄️ DbContext and DbSet
`LamazonDbContext` is our **session with the database**: it opens the connection, keeps track of the entities we load, and saves our changes. Each `DbSet<T>` is one table.
```csharp
public class LamazonDbContext : DbContext
{
    public LamazonDbContext(DbContextOptions<LamazonDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Order> Orders { get; set; }
    // … one DbSet per table, 12 in total
}
```
The `options` (which database, which connection string) come from dependency injection, so the `DbContext` never hard-codes a connection string.

---

### ⚙️ Configuration classes + ApplyConfigurationsFromAssembly
Instead of configuring every table inside `OnModelCreating`, each entity gets its own class that implements `IEntityTypeConfiguration<T>`:
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Price).IsRequired().HasPrecision(10, 2);
        builder.Property(x => x.ProductStatusId).HasDefaultValue((int)ProductStatusEnum.Active);

        builder.HasOne(x => x.ProductCategory)        // a product has one category
            .WithMany(x => x.Products)                // a category has many products
            .HasForeignKey(x => x.ProductCategoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Product_ProductCategory");
    }
}
```
One line in the `DbContext` finds and applies **every** configuration class in the project:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(LamazonDbContext).Assembly);
}
```
A new entity only needs a new configuration class. The `DbContext` doesn't change.

What a configuration sets, and what it becomes in SQL Server:

| | C# | SQL Server |
|---|---|---|
| 🔑 Primary key | `HasKey(x => x.Id)` | `PRIMARY KEY` (an `int` key also gets `IDENTITY`) |
| ❗ Required text | `IsRequired().HasMaxLength(255)` | `nvarchar(255) NOT NULL` |
| 💰 Money | `HasPrecision(10, 2)` | `decimal(10,2)`: up to 99,999,999.99 |
| 🔗 Relationship | `HasOne(…).WithMany(…).HasForeignKey(…)` | a foreign key |
| ☝️ Unique | `HasIndex(x => x.Email).IsUnique()` | a unique index |
| 🏷️ Default value | `HasDefaultValue((int)ProductStatusEnum.Active)` | `DEFAULT 1` |

☝️ **Our unique indexes:** `Users.Email` (one account per email address), `Orders.OrderNumber` and `Invoices.InvoiceNumber`. The database checks them too, not only our code.

🤔 **Why `DeleteBehavior.NoAction` everywhere?** The default for a required relationship is `Cascade`: deleting a user would silently delete their orders and invoices too. We never delete business data: a deleted product only gets the `Deleted` status (soft delete, class 8). SQL Server also refuses `Cascade` when a table can be reached by two delete paths (*"may cause cycles or multiple cascade paths"*), as `Invoices` can: from `Users` directly, and through `Orders`.

### 🔗 One-to-one: an order and its invoice
```csharp
// InvoiceConfiguration
builder.HasOne(x => x.Order)
    .WithOne(x => x.Invoice)
    .HasForeignKey<Invoice>(x => x.OrderId);
```
The foreign key lives in `Invoices`, because an invoice can't exist without its order. Since the relationship is one-to-one, EF Core gives `Invoices.OrderId` a **unique index**: the database itself refuses a second invoice for the same order.

### 🏷️ Lookup tables and the string key
The status tables (`OrderStatuses`, `ProductStatuses`, …) use the **same ids as their enums** (class 1). So **we** choose the ids, not SQL Server:
```csharp
builder.Property(x => x.Id).ValueGeneratedNever();   // no IDENTITY
```
Now `OrderStatusId = (int)OrderStatusEnum.Accepted` in C# always points to the right row.

`Role` isn't a `BaseEntity`: its key is a readable string, `builder.HasKey(x => x.Key)`. A user row says `RoleKey = 'admin'`, not `RoleId = 1`.

---

### 🌱 Seed data: HasData
Some rows must exist from the very first run: the statuses, the roles, 5 categories, 6 products and 2 users. `HasData` puts them into the migration as `INSERT`s:
```csharp
public static ModelBuilder SeedRoles(this ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Role>().HasData(
        new Role { Key = Roles.Admin, Name = "Administrator" },
        new Role { Key = Roles.User, Name = "User" }
    );
    return modelBuilder;
}
```
Each `Seed…` method returns the `ModelBuilder`, so the calls chain in `OnModelCreating`:
```csharp
modelBuilder.SeedProductCategoryStatuses()
            .SeedProductStatuses()
            .SeedProductCategories()
            .SeedProducts()
            // …
```
⚠️ **Seed values must be fixed:**
- **Fixed ids** (`Id = 1`, `Id = 2`, …): EF Core compares the seed data with the previous migration, and the key is how it tells the rows apart.
- **No `DateTime.Now`, no `Guid.NewGuid()`**: a value that changes on every run looks like a change, so every new migration would update the row again.
- **Precomputed password hashes**: hashing adds a random salt, so a hash made in code would be different every time. How hashing works: class 4.

🔐 **The seeded logins** (for development only): `admin@lamazon.com` / `Admin123!` and `user@lamazon.com` / `User123!`

---

### 💉 AddDataAccess(): an extension method on IServiceCollection
Each layer registers its own classes. `Program.cs` calls one method and doesn't need to know what's inside:
```csharp
// Program.cs
builder.Services.AddDataAccess(builder.Configuration);
```
```csharp
// Lamazon.DataAccess/DependencyInjection.cs
public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("LamazonDb");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'LamazonDb' is missing. Add it to appsettings.json under \"ConnectionStrings\".");
    }

    services.AddDbContext<LamazonDbContext>(options => options.UseSqlServer(connectionString));
    services.AddScoped<IProductsRepository, ProductsRepository>();

    return services;
}
```
- `this IServiceCollection services` makes it an **extension method**, so it reads like a built-in: `builder.Services.AddDataAccess(…)`.
- It returns `services`, so the calls can chain: `.AddDataAccess(…).AddServices()` (class 3).
- **Fail fast:** without a connection string the app stops at startup, with a clear message, instead of failing later on the first page that reads the database.

### 🔌 The connection string
```json
"ConnectionStrings": {
  "LamazonDb": "Server=.\\SQLEXPRESS;Database=LamazonDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```
| Part | Meaning |
|---|---|
| `Server=.\\SQLEXPRESS` | this computer (`.`), the `SQLEXPRESS` instance. JSON writes `\` as `\\` |
| `Database=LamazonDb` | the database. The first `database update` creates it |
| `Trusted_Connection=True` | log in as your Windows user |
| `TrustServerCertificate=True` | accept the self-signed certificate of a SQL Server on your own computer |

🔐 **No password in it**, so there's no secret to hide, and it can stay in `appsettings.json`.
🖥️ **A different server?** Change only the `Server` part: `.` for a default instance, `(localdb)\\MSSQLLocalDB` for LocalDB.

---

### 🐣 Migrations
A migration is a C# file with the steps that take the database from the previous model to the current one. `dotnet ef` writes it for us by comparing our classes with the last snapshot.

In a terminal, in the solution folder (the one with `Lamazon.slnx`):
```powershell
dotnet tool restore     # installs dotnet-ef, the version pinned in dotnet-tools.json
dotnet ef migrations add Init --project Lamazon.DataAccess --startup-project Lamazon.Web
dotnet ef database update --project Lamazon.DataAccess --startup-project Lamazon.Web
```
- `--project`: where the `DbContext` lives, and where the `Migrations` folder goes.
- `--startup-project`: the app whose `Program.cs` and `appsettings.json` give the `DbContext` its connection string.

Or in Visual Studio: **Tools → NuGet Package Manager → Package Manager Console**, with **Default project: Lamazon.DataAccess** (and `Lamazon.Web` as the startup project):
```powershell
Add-Migration Init
Update-Database
```

`migrations add` creates three files in `Lamazon.DataAccess/Migrations`:

| File | What it is |
|---|---|
| `<timestamp>_Init.cs` | `Up()` creates the tables and inserts the seed data, `Down()` undoes it |
| `<timestamp>_Init.Designer.cs` | the model as it was for this migration |
| `LamazonDbContextModelSnapshot.cs` | the current model: the **next** migration is compared against it |

🔍 **In SSMS:** refresh **Databases → LamazonDb → Tables**. You should see our 12 tables, plus **`__EFMigrationsHistory`**: the list of migrations already applied, so `database update` never runs one twice. Right-click `dbo.Products` → **Select Top 1000 Rows** to see the seed data.

⚠️ **Don't change the tables by hand in SSMS.** Change the C# classes, then add a new migration. Made a mistake in a migration you haven't applied yet? `dotnet ef migrations remove` deletes the last one.

---

### 📚 The repository pattern
The rest of the app never uses the `DbContext` directly: it asks a **repository**. The repository hides **how** the data is loaded (the joins, the filters, the tracking) behind methods with clear names, like `GetFeaturedAsync()`.

```
IRepository<T>            AddAsync, UpdateAsync: what every repository can do
      ▲
BaseRepository<T>         the Context, its Table, saving: the code they share
      ▲
ProductsRepository        its own queries: GetFeaturedAsync, GetByIdAsync, …
```