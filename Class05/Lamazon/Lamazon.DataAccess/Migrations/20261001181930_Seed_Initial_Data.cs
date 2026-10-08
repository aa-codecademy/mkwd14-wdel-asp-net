using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lamazon.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Seed_Initial_Data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InvoiceStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pending payment" },
                    { 2, "Paid" },
                    { 3, "Canceled" }
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Accepted" },
                    { 3, "Rejected" }
                });

            migrationBuilder.InsertData(
                table: "ProductCategoriesStatus",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 255, "Deleted" }
                });

            migrationBuilder.InsertData(
                table: "ProductStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 255, "Deleted" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Key", "Name" },
                values: new object[,]
                {
                    { "admin", "Administrator" },
                    { "user", "User" }
                });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "Name", "ProductCategoryStatusId" },
                values: new object[,]
                {
                    { 1, "Food", 1 },
                    { 2, "Drinks", 1 },
                    { 3, "Books", 1 },
                    { 4, "Software", 1 },
                    { 5, "Computers", 1 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Age", "Email", "FullName", "PasswordHash", "RoleKey" },
                values: new object[,]
                {
                    { 1, null, "admin@lamazon.com", "Bob Bobsky", "AQAAAAIAAYagAAAAEHNR9fcvPWoUqe2TnGkce1c0d1MmKkSa7JciS0qH9qkiMEtd5voYWgpz8ihIeojMRw==", "admin" },
                    { 2, null, "user@lamazon.com", "Jane Doe", "AQAAAAIAAYagAAAAEBTHEfZvz2CIJf/rhq+0yELC0RwAEdl0UtEIZfRGZeK72TNFaAWe9LStqHv+68XmKA==", "user" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "DiscountPercentage", "ImageUrl", "IsFeatured", "Name", "Price", "ProductCategoryId", "ProductStatusId" },
                values: new object[] { 1, "Even bad code can function. But if code isn’t clean, it can bring a development organization to its knees. Every year, countless hours and significant resources are lost because of poorly written code. But it doesn’t have to be that way.\r\n\r\nNoted software expert Robert C. Martin, presents a revolutionary paradigm with Clean Code: A Handbook of Agile Software Craftsmanship. Martin, who has helped bring agile principles from a practitioner’s point of view to tens of thousands of programmers, has teamed up with his colleagues from Object Mentor to distill their best agile practice of cleaning code “on the fly” into a book that will instill within you the values of software craftsman, and make you a better programmer―but only if you work at it.\r\n\r\nWhat kind of work will you be doing? You’ll be reading code―lots of code. And you will be challenged to think about what’s right about that code, and what’s wrong with it. More importantly you will be challenged to reassess your professional values and your commitment to your craft.\r\n\r\nClean Code is divided into three parts. The first describes the principles, patterns, and practices of writing clean code. The second part consists of several case studies of increasing complexity. Each case study is an exercise in cleaning up code―of transforming a code base that has some problems into one that is sound and efficient. The third part is the payoff: a single chapter containing a list of heuristics and “smells” gathered while creating the case studies. The result is a knowledge base that describes the way we think when we write, read, and clean code.\r\n", 10, "https://m.media-amazon.com/images/I/51E2055ZGUL._SY522_.jpg", true, "Clean Code", 55m, 3, 1 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "DiscountPercentage", "ImageUrl", "Name", "Price", "ProductCategoryId", "ProductStatusId" },
                values: new object[] { 2, "Ward Cunningham Straight from the programming trenches, The Pragmatic Programmer cuts through the increasing specialization and technicalities of modern software development to examine the core process--taking a requirement and producing working, maintainable code that delights its users. It covers topics ranging from personal responsibility and career development to architectural techniques for keeping your code flexible and easy to adapt and reuse. Read this book, and you’ll learn how to Fight software rot; Avoid the trap of duplicating knowledge; Write flexible, dynamic, and adaptable code; Avoid programming by coincidence; Bullet-proof your code with contracts, assertions, and exceptions; Capture real requirements; Test ruthlessly and effectively; Delight your users; Build teams of pragmatic programmers; and Make your developments more precise with automation.", 15, "https://m.media-amazon.com/images/I/61ztlXgCmpL._SY522_.jpg", "The Pragmatic Programmer", 50.99m, 3, 1 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "ImageUrl", "Name", "Price", "ProductCategoryId", "ProductStatusId" },
                values: new object[,]
                {
                    { 3, "A comprehensive update of the leading algorithms text, with new material on matchings in bipartite graphs, online algorithms, machine learning, and other topics.", "https://m.media-amazon.com/images/I/61Mw06x2XcL._AC_UY327_FMwebp_QL65_.jpg", "Introduction to Algorithms", 60.99m, 3, 1 },
                    { 4, "OEM IS TO BE INSTALLED ON A NEW PC with no prior version of Windows installed and cannot be transferred to another machine.", "https://m.media-amazon.com/images/I/61JfosHunyL._AC_SX679_.jpg", "Windows 11 Home", 119.99m, 4, 1 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "ImageUrl", "IsFeatured", "Name", "Price", "ProductCategoryId", "ProductStatusId" },
                values: new object[] { 5, "AHEAD OF THE PACK- Breathtakingly light and insanely powerful, the ThinkPad T14s Gen 6 sets the new industry-leading standard for AI PCs.\r\nBETTER THAN THE BEST - The Snapdragon X Elite processor is engineered to fulfill the needs of today’s IT teams and hybrid workforce. Meet one of the most powerful, intuitive, and efficient Windows platforms.\r\nGLITTERING GRAPHICS - Enjoy vibrant visuals and sharp details in the sunniest of spots with the Eyesafe Certified, anti-glare WUXGA IPS panel. The energy-efficient display delivers vivid hues and stark contrasts.\r\nSEAMLESS CONNECTION - For relentless on-the-go productivity, Qualcomm Wi-Fi 7 ensures faster bandwidth usage.", "https://m.media-amazon.com/images/I/71EYq-mmleL._AC_SX679_.jpg", true, "Lenovo ThinkPad T14s Gen 6-2024", 1711.99m, 5, 1 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "DiscountPercentage", "ImageUrl", "Name", "Price", "ProductCategoryId", "ProductStatusId" },
                values: new object[] { 6, "SMOOTH MULTITASKING — Powered by the Intel Pentium N6000 4-Core Processor, enabling decent multitasking experience\r\nTOUCHSCREEN VERSATILITY — 14-inch FHD 1920x1080 NanoEdge 360-degree flippable touchscreen display\r\nWORK AND PLAY FROM ANY ANGLE — Convertible 2-in-1 design with four different work modes: traditional clamshell, tent, stand, tablet mode\r\nLIGHTWEIGHT YET DURABLE — Durable and built to US Military Grade standard MIL- STD 810H weighing just 3.59 lbs", 7, "https://m.media-amazon.com/images/I/61UucaPmVtL._AC_SX679_.jpg", "ASUS Chromebook Flip CX1 Convertible Laptop, 14\" FHD", 350m, 5, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InvoiceStatuses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InvoiceStatuses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InvoiceStatuses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductCategoriesStatus",
                keyColumn: "Id",
                keyValue: 255);

            migrationBuilder.DeleteData(
                table: "ProductStatuses",
                keyColumn: "Id",
                keyValue: 255);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ProductStatuses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Key",
                keyValue: "admin");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Key",
                keyValue: "user");

            migrationBuilder.DeleteData(
                table: "ProductCategoriesStatus",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
