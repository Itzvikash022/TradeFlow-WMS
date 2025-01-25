using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using WMS_Application.Models;
using WMS_Application.Repositories;
using WMS_Application.Repositories.Auth;
using WMS_Application.Repositories.Interfaces;
using WMS_Application.Repositories.Sidebar;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailSenderRepository, EmailSenderRepository>(); // Email service interface and implementation

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<dbMain>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1440); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Ensures cookie is essential
});
builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<ISidebarRepository, SidebarRepository>();
builder.Services.AddScoped<IAdminsRepository, AdminsRepository>();
builder.Services.AddMemoryCache();
var app = builder.Build();
app.UseSession();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Auth/Error");
    app.UseHsts();
}

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Auth/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
