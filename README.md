# TaskManager — ASP.NET Core Web API (.NET 10, Clean Architecture)

## راه‌اندازی (به ترتیب)

1. نصب: Visual Studio 2022 (آخرین آپدیت) یا Visual Studio 2026 با workload ‏ASP.NET + **.NET 10 SDK** و SQL Server LocalDB.
2. فایل `TaskManager.sln` را باز کنید و صبر کنید NuGet Restore تمام شود.
3. (اختیاری) Connection String و کلید JWT را در `src/TaskManager.Api/appsettings.json` تغییر دهید.
   مقدار `Jwt:SecretKey` باید حداقل ۳۲ کاراکتر باشد.
4. در Visual Studio: Tools ▸ NuGet Package Manager ▸ Package Manager Console.
   پروژه Default را روی `TaskManager.Infrastructure` بگذارید و اجرا کنید:

   ```
   Add-Migration InitialCreate -Project TaskManager.Infrastructure -StartupProject TaskManager.Api -OutputDir Persistence/Migrations
   ```

   یا با CLI از پوشه Solution:

   ```
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate --project src/TaskManager.Infrastructure --startup-project src/TaskManager.Api --output-dir Persistence/Migrations
   ```
5. پروژه `TaskManager.Api` را Startup Project کنید و F5 بزنید.
   در حالت Development دیتابیس خودکار ساخته (Migrate) و داده اولیه (Seed) وارد می‌شود.
6. با F5 (پروفایل https) مرورگر روی **Swagger UI** باز می‌شود: `https://localhost:7100/swagger`.
   ابتدا `POST /api/auth/login` را اجرا کنید، `token` را کپی کنید، روی دکمه **Authorize** بزنید و فقط توکن را بچسبانید.
   همچنین می‌توانید تست را با فایل `src/TaskManager.Api/TaskManager.Api.http` (در Visual Studio باز کنید) یا Postman.
7. لاگ درخواست/پاسخ در `src/TaskManager.Api/Logs/http-yyyy-MM-dd.log` ثبت می‌شود.

## کاربران اولیه (Seed)

| نقش | ایمیل | رمز |
|---|---|---|
| Admin | admin@taskmanager.com | Admin@123 |
| TeamManager | manager@taskmanager.com | Manager@123 |
| Employee | employee1@taskmanager.com | Employee@123 |
| Employee | employee2@taskmanager.com | Employee@123 |

## معماری

```
Domain          Entityها و Enumها (بدون وابستگی)
Application     Use caseها/سرویس‌ها، DTOها، Interfaceها (Repository / Query / Security)
Infrastructure  EF Core (نوشتن)، Dapper (خواندن)، JWT، PasswordHasher، Seed
Api             Controllerها، Middlewareها، Program.cs (Composition Root)
```

## سطوح دسترسی

| عملیات | Admin | TeamManager | Employee |
|---|---|---|---|
| ساخت کاربر / تیم / تعیین مدیر تیم | ✔ | ✘ | ✘ |
| لیست کاربران و تیم‌ها | همه | فقط تیم خودش | ✘ |
| دیدن تسک‌ها | همه | فقط تیم خودش | فقط تسک‌های خودش |
| ساخت / ویرایش / حذف تسک | همه | فقط تیم خودش | ✘ |
| تغییر وضعیت تسک | همه | فقط تیم خودش | فقط تسک‌های خودش |
