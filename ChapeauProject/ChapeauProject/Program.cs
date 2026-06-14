using ChapeauProject.Repositories;
using ChapeauProject.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});


// Made global connection string to get rid of IConfiguration. (Not 100% sure if this is actually better)
var connectionString = builder.Configuration.GetConnectionString("ChapeauProject")
    ?? throw new InvalidOperationException("Connection string 'ChapeauProject' not found.");

// Scoped so each request gets its own instance, consistent with the other services (could change back to singleton)
builder.Services.AddScoped<IStaffRepository>(_ => new StaffRepository(connectionString));
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<ITableRepository>(_ => new TableRepository(connectionString));
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IMenuRepository>(_ => new MenuRepository(connectionString));
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IOrderRepository>(_ => new OrderRepository(connectionString));
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IBillRepository>(_ => new BillRepository(connectionString));
builder.Services.AddScoped<IBillService, BillService>();

// enabling cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
builder.Services.AddAuthorization();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tables}/{action=Index}/{id?}");

app.Run();