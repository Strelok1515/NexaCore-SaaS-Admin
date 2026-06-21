using System;
using AleksandarIvanov_NexaCore.Data;
using AleksandarIvanov_NexaCore.Filters;
using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1) Configure EF & Identity
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(conn));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(opts => opts.SignIn.RequireConfirmedAccount = true)
       .AddEntityFrameworkStores<ApplicationDbContext>();

// 2) Register your audit filter
builder.Services.AddScoped<AuditLogActionFilter>();

// 3) “AdminsOnly” policy
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminsOnly", p => p.RequireClaim("IsAdmin", "True"));
});

// 4) MVC + global audit‐log filter
builder.Services.AddControllersWithViews(opts =>
{
    opts.Filters.AddService<AuditLogActionFilter>();
});

// **do not** call AddControllersWithViews() a second time!

var app = builder.Build();

// 5) Middleware order is critical:
if (app.Environment.IsDevelopment())
    app.UseMigrationsEndPoint();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

//  Must authenticate before you authorize
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await SeedData.InitializeAsync(app.Services);
app.Run();
