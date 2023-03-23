using CourseSeller.Core.Convertors;
using CourseSeller.Core.Services;
using CourseSeller.Core.Services.Interfaces;
using CourseSeller.DataLayer.Contexts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services.AddControllersWithViews();
ConfigurationManager conf = builder.Configuration;
IWebHostEnvironment env = builder.Environment;


#region Authentication

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = "/Account/Login/";
    options.LogoutPath = "/Account/Logout/";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
});

#endregion


#region DB Context

services.AddDbContext<MSSQLSContext>(options =>
{
    options.UseSqlServer(conf.GetConnectionString("MSSQLSConnection"));
});

#endregion


#region IoC

services.AddTransient<IAccountService, AccountService>();
services.AddTransient<IViewRenderService, RenderViewToString>();

#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
