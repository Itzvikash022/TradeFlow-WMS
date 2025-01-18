using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using WMS_Application.Models;
using WMS_Application.Repositories.Classes;
using WMS_Application.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<EmailSenderInterface, EmailSenderClass>(); // Email service interface and implementation

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<dbMain>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Ensures session cookie is accessible only via HTTP
    options.Cookie.IsEssential = true; // Ensures cookie is essential
});
builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<UsersInterface, UsersClass>();
builder.Services.AddScoped<LoginInterface, LoginClass>();
builder.Services.AddScoped<ForgotPasswordInterface, ForgotPasswordClass>();
builder.Services.AddMemoryCache();
var app = builder.Build();
app.UseSession();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
