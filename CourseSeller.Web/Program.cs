using System.Configuration;
using CourseSeller.Core.Convertors;
using CourseSeller.Core.Senders;
using CourseSeller.Core.Services;
using CourseSeller.Core.Services.Interfaces;
using CourseSeller.DataLayer.Contexts;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
ConfigurationManager conf = builder.Configuration;
IWebHostEnvironment env = builder.Environment;

#region HangFire

services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(conf.GetConnectionString("MSSQLSConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add the processing server as IHostedService
services.AddHangfireServer();


#endregion


// Add services to the container.
services.AddControllersWithViews();


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
services.AddTransient<IUserPanelService, UserPanelService>();
services.AddTransient<IViewRenderService, RenderViewToString>();
services.AddTransient<ISendEmail, SendEmail>();

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

app.UseHangfireDashboard();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});

app.Run();
