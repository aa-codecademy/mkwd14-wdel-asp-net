using Lamazon.Domain.Constants;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Extensions;

/// <summary>
/// Seed data: rows that EF Core inserts with the migrations (HasData).
/// Each method returns the ModelBuilder, so the calls can be chained in LamazonDbContext.OnModelCreating.
/// Values must be fixed (no DateTime.Now, no Guid.NewGuid()), or every new migration would update them again.
/// </summary>
internal static class DataSeedExtensions
{
    public static ModelBuilder SeedProductCategoryStatuses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductCategoryStatus>().HasData(
            new ProductCategoryStatus { Id = (int)ProductCategoryStatusEnum.Active, Name = "Active" },
            new ProductCategoryStatus { Id = (int)ProductCategoryStatusEnum.Deleted, Name = "Deleted" }
        );

        return modelBuilder;
    }

    public static ModelBuilder SeedProductStatuses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductStatus>().HasData(
            new ProductStatus { Id = (int)ProductStatusEnum.Active, Name = "Active" },
            new ProductStatus { Id = (int)ProductStatusEnum.Deleted, Name = "Deleted" }
        );
        return modelBuilder;
    }

    public static ModelBuilder SeedProductCategories(this ModelBuilder modelBuilder)
    {
        const int active = (int)ProductCategoryStatusEnum.Active;

        modelBuilder.Entity<ProductCategory>().HasData(
            new ProductCategory { Id = 1, Name = "Food", ProductCategoryStatusId = active },
            new ProductCategory { Id = 2, Name = "Drinks", ProductCategoryStatusId = active },
            new ProductCategory { Id = 3, Name = "Books", ProductCategoryStatusId = active },
            new ProductCategory { Id = 4, Name = "Software", ProductCategoryStatusId = active },
            new ProductCategory { Id = 5, Name = "Computers", ProductCategoryStatusId = active }
        );
        return modelBuilder;
    }

    public static ModelBuilder SeedProducts(this ModelBuilder modelBuilder)
    {
        const int active = (int)ProductStatusEnum.Active;

        modelBuilder.Entity<Product>().HasData(
            // Books
            new Product
            {
                Id = 1,
                Name = "Clean Code",
                Description = "Even bad code can function. But if code isn’t clean, it can bring a development organization to its knees. Every year, countless hours and significant resources are lost because of poorly written code. But it doesn’t have to be that way.\r\n\r\nNoted software expert Robert C. Martin, presents a revolutionary paradigm with Clean Code: A Handbook of Agile Software Craftsmanship. Martin, who has helped bring agile principles from a practitioner’s point of view to tens of thousands of programmers, has teamed up with his colleagues from Object Mentor to distill their best agile practice of cleaning code “on the fly” into a book that will instill within you the values of software craftsman, and make you a better programmer―but only if you work at it.\r\n\r\nWhat kind of work will you be doing? You’ll be reading code―lots of code. And you will be challenged to think about what’s right about that code, and what’s wrong with it. More importantly you will be challenged to reassess your professional values and your commitment to your craft.\r\n\r\nClean Code is divided into three parts. The first describes the principles, patterns, and practices of writing clean code. The second part consists of several case studies of increasing complexity. Each case study is an exercise in cleaning up code―of transforming a code base that has some problems into one that is sound and efficient. The third part is the payoff: a single chapter containing a list of heuristics and “smells” gathered while creating the case studies. The result is a knowledge base that describes the way we think when we write, read, and clean code.\r\n",
                ImageUrl = "https://m.media-amazon.com/images/I/51E2055ZGUL._SY522_.jpg",
                ProductCategoryId = 3, // Books
                ProductStatusId = active,
                Price = 55m,
                IsFeatured = true,
                DiscountPercentage = 10
            },
            new Product
            {
                Id = 2,
                Name = "The Pragmatic Programmer",
                Description = "Ward Cunningham Straight from the programming trenches, The Pragmatic Programmer cuts through the increasing specialization and technicalities of modern software development to examine the core process--taking a requirement and producing working, maintainable code that delights its users. It covers topics ranging from personal responsibility and career development to architectural techniques for keeping your code flexible and easy to adapt and reuse. Read this book, and you’ll learn how to Fight software rot; Avoid the trap of duplicating knowledge; Write flexible, dynamic, and adaptable code; Avoid programming by coincidence; Bullet-proof your code with contracts, assertions, and exceptions; Capture real requirements; Test ruthlessly and effectively; Delight your users; Build teams of pragmatic programmers; and Make your developments more precise with automation.",
                ImageUrl = "https://m.media-amazon.com/images/I/61ztlXgCmpL._SY522_.jpg",
                ProductCategoryId = 3,
                ProductStatusId = active,
                Price = 50.99m,
                IsFeatured = false,
                DiscountPercentage = 15
            },
            new Product
            {
                Id = 3,
                Name = "Introduction to Algorithms",
                Description = "A comprehensive update of the leading algorithms text, with new material on matchings in bipartite graphs, online algorithms, machine learning, and other topics.",
                ImageUrl = "https://m.media-amazon.com/images/I/61Mw06x2XcL._AC_UY327_FMwebp_QL65_.jpg",
                ProductCategoryId = 3,
                ProductStatusId = active,
                Price = 60.99m,
                IsFeatured = false,
                DiscountPercentage = 0
            },

            // Software
            new Product
            {
                Id = 4,
                Name = "Windows 11 Home",
                Description = "OEM IS TO BE INSTALLED ON A NEW PC with no prior version of Windows installed and cannot be transferred to another machine.",
                ImageUrl = "https://m.media-amazon.com/images/I/61JfosHunyL._AC_SX679_.jpg",
                ProductCategoryId = 4,
                ProductStatusId = active,
                Price = 119.99m,
                IsFeatured = false,
                DiscountPercentage = 0
            },

            // Computers
            new Product
            {
                Id = 5,
                Name = "Lenovo ThinkPad T14s Gen 6-2024",
                Description = "AHEAD OF THE PACK- Breathtakingly light and insanely powerful, the ThinkPad T14s Gen 6 sets the new industry-leading standard for AI PCs.\r\nBETTER THAN THE BEST - The Snapdragon X Elite processor is engineered to fulfill the needs of today’s IT teams and hybrid workforce. Meet one of the most powerful, intuitive, and efficient Windows platforms.\r\nGLITTERING GRAPHICS - Enjoy vibrant visuals and sharp details in the sunniest of spots with the Eyesafe Certified, anti-glare WUXGA IPS panel. The energy-efficient display delivers vivid hues and stark contrasts.\r\nSEAMLESS CONNECTION - For relentless on-the-go productivity, Qualcomm Wi-Fi 7 ensures faster bandwidth usage.",
                ImageUrl = "https://m.media-amazon.com/images/I/71EYq-mmleL._AC_SX679_.jpg",
                ProductCategoryId = 5,
                ProductStatusId = active,
                Price = 1711.99m,
                IsFeatured = true,
                DiscountPercentage = 0
            },
            new Product
            {
                Id = 6,
                Name = "ASUS Chromebook Flip CX1 Convertible Laptop, 14\" FHD",
                Description = "SMOOTH MULTITASKING — Powered by the Intel Pentium N6000 4-Core Processor, enabling decent multitasking experience\r\nTOUCHSCREEN VERSATILITY — 14-inch FHD 1920x1080 NanoEdge 360-degree flippable touchscreen display\r\nWORK AND PLAY FROM ANY ANGLE — Convertible 2-in-1 design with four different work modes: traditional clamshell, tent, stand, tablet mode\r\nLIGHTWEIGHT YET DURABLE — Durable and built to US Military Grade standard MIL- STD 810H weighing just 3.59 lbs",
                ImageUrl = "https://m.media-amazon.com/images/I/61UucaPmVtL._AC_SX679_.jpg",
                ProductCategoryId = 5,
                ProductStatusId = active,
                Price = 350,
                IsFeatured = false,
                DiscountPercentage = 7
            }
        );

        return modelBuilder;
    }


    public static ModelBuilder SeedRoles(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Key = Roles.Admin, Name = "Administrator" },
            new Role { Key = Roles.User, Name = "User" }
        );
        return modelBuilder;
    }

    public static ModelBuilder SeedUsers(this ModelBuilder modelBuilder)
    {
        // The hashes were made once with PasswordHasher<User>.HashPassword (the salt is random, so they are fixed here).
        // For development only: Admin123! and User123!
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "Bob Bobsky",
                RoleKey = Roles.Admin,
                Email = "admin@lamazon.com",
                PasswordHash = "AQAAAAIAAYagAAAAEHNR9fcvPWoUqe2TnGkce1c0d1MmKkSa7JciS0qH9qkiMEtd5voYWgpz8ihIeojMRw==", // Admin123!
            },
            new User
            {
                Id = 2,
                FullName = "Jane Doe",
                RoleKey = Roles.User,
                Email = "user@lamazon.com",
                PasswordHash = "AQAAAAIAAYagAAAAEBTHEfZvz2CIJf/rhq+0yELC0RwAEdl0UtEIZfRGZeK72TNFaAWe9LStqHv+68XmKA==", // User123!
            }
        );
        return modelBuilder;
    }

    public static ModelBuilder SeedOrderStatuses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderStatus>().HasData(
            new OrderStatus { Id = (int)OrderStatusEnum.Pending, Name = "Pending" },
            new OrderStatus { Id = (int)OrderStatusEnum.Accepted, Name = "Accepted" },
            new OrderStatus { Id = (int)OrderStatusEnum.Rejected, Name = "Rejected" }
        );
        return modelBuilder;
    }

    public static ModelBuilder SeedInvoiceStatuses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvoiceStatus>().HasData(
            new InvoiceStatus { Id = (int)InvoiceStatusEnum.PendingPayment, Name = "Pending payment" },
            new InvoiceStatus { Id = (int)InvoiceStatusEnum.Paid, Name = "Paid" },
            new InvoiceStatus { Id = (int)InvoiceStatusEnum.Canceled, Name = "Canceled" }
        );
        return modelBuilder;
    }

}
