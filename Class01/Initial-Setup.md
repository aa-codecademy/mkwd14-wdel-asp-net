# 🛠️ Initial setup

Install everything below **before the first class**, so we can start coding right away.
Plan about an hour: Visual Studio and SQL Server are big downloads.

> 💡 **Already have SQL Server and SSMS** from the databases subject? Keep them, and skip to [✅ Check everything](#check). You only need to know your **server name** (see [step 2](#server-name)).

---

## 🧰 What to install

### 1️⃣ Visual Studio 2026
1. Download **Visual Studio 2026 Community** (free) from https://visualstudio.microsoft.com ([installation guide](https://learn.microsoft.com/visualstudio/install/install-visual-studio)).
2. In the installer, select the **ASP.NET and web development** workload. It includes the **.NET 10 SDK**.
3. Check it: open a new terminal and run `dotnet --version`. You should see `10.0.x`.

> 🍎 **On macOS or Linux?** Visual Studio is Windows-only. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download), then use **VS Code** with the *C# Dev Kit* extension, or **JetBrains Rider** (free for non-commercial use). SQL Server doesn't install on macOS: it runs in **Docker** instead (the official `mcr.microsoft.com/mssql/server` image), with a SQL login instead of your Windows login. Tell me before class 2, and we'll set up your connection string together.

### 2️⃣ SQL Server Express
The database server that stores Lamazon's data. Express is the free edition.

1. Download **SQL Server 2022 Express** or newer from https://www.microsoft.com/sql-server/sql-server-downloads (the **Express** box, *Download now*).
2. Run it and choose the **Basic** installation type. It installs a server called **`SQLEXPRESS`**, and makes your Windows user its administrator.
3. At the end, the installer shows the instance name `SQLEXPRESS` and a connection string. You don't need to copy them.

<a id="server-name"></a>
**Your server name** is what you type to connect to SQL Server. It depends on how SQL Server was installed:

| Installed as | Server name |
|---|---|
| **SQL Server Express** (the steps above) | `.\SQLEXPRESS` |
| a **default instance** (for example SQL Server Developer edition) | `.` |

The `.` means "this computer". `localhost\SQLEXPRESS` works too.
Not sure what you have? In PowerShell, run `Get-Service MSSQL*`:
- `MSSQL$SQLEXPRESS` means `.\SQLEXPRESS`;
- `MSSQLSERVER` means `.`.

In class 2 this server name goes into the app's connection string. Lamazon uses `.\SQLEXPRESS` by default.

### 3️⃣ SQL Server Management Studio (SSMS)
The app for looking inside SQL Server: the databases, the tables and the rows. We use it in class 2 to see the tables that EF Core creates for us.

1. Download the latest SSMS from https://learn.microsoft.com/ssms/install/install. The installer uses the Visual Studio Installer; keep the default options.
2. Check it: open SSMS and connect with:
   - **Server name:** your server name (`.\SQLEXPRESS`)
   - **Authentication:** Windows Authentication
   - ✅ **Trust server certificate**
3. In **Object Explorer** you should see your server, with a **Databases** folder.

> 💡 **Why "Trust server certificate"?** SSMS encrypts the connection and checks the server's certificate. A SQL Server on your own computer has a self-signed certificate, which SSMS doesn't trust by default. Lamazon's connection string has the same setting: `TrustServerCertificate=True`.

### 4️⃣ A browser with developer tools
Edge, Chrome or Firefox. We use the developer tools (**F12**) to watch the AJAX requests, look at cookies, and, in the security demos, try to cheat the shop.

---

That's all. The EF Core command-line tool (`dotnet-ef`) comes with the project: in class 2 one command (`dotnet tool restore`) installs it, with nothing to install globally.

<a id="check"></a>
## ✅ Check everything

| Check | You should see |
|---|---|
| `dotnet --version` | `10.0.x` |
| `Get-Service MSSQL*` (in PowerShell) | your SQL Server with the status **Running** |
| SSMS → Connect → your server name, Windows Authentication, ✅ Trust server certificate | your server in Object Explorer, with a **Databases** folder |

## 🆘 Something doesn't work?

| Problem | Fix |
|---|---|
| `dotnet` is *not recognized* | Close the terminal and open a new one. If it still fails, restart the computer. |
| `dotnet --version` shows 8.x or 9.x | The .NET 10 SDK is missing. Re-run the Visual Studio Installer and check the *ASP.NET and web development* workload. |
| SSMS: *The certificate chain was issued by an authority that is not trusted* | Check ✅ **Trust server certificate** in the connect window. |
| SSMS: *A network-related or instance-specific error occurred* / *server was not found* | Check the server name (`.\SQLEXPRESS`: a dot, a backslash, then the name). If the name is right, the server isn't running: open **Services** (Windows key → type *services*), find **SQL Server (SQLEXPRESS)**, and click **Start**. |
| SSMS: *Login failed for user* | Choose **Windows Authentication**, not SQL Server Authentication. |
| `Get-Service MSSQL*` shows nothing | SQL Server isn't installed. Go back to step 2. |

Still stuck? Send me an email: ilija.mitev3@gmail.com.
