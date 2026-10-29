using Lamazon.DataAccess;
using Lamazon.Services;
using Lamazon.Web.Extensions;
using Lamazon.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddDataAccess(builder.Configuration)
    .AddServices()
    .AddWeb();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseMiddleware<DebugIpAddressMiddleware>();
}

// The order of middleware is important !
// Client => Http Request => Middleware Pipeline (UseHttpsRedirection, UseRouting, UseAuthentication, UseAuthorization, etc...) => Controller => Action (Endpoint)

// 404 and other error status codes without a body show the friendly error page too
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseHttpsRedirection();

// Prices are written and read as 1,234.56 on every computer, whatever its Windows language settings
app.UseRequestLocalization("en-US");
app.UseRouting();

app.Use((context, next) =>
{
    context.Items["ThisIsTest"] = "Hello from middleware!";
    return next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


await app.RunAsync();
