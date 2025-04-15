using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using WMS_Application.Models;
using WMS_Application.Repositories;
using WMS_Application.Repositories.Auth;
using WMS_Application.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailSenderRepository, EmailSenderRepository>(); // Email service interface and implementation

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<dbMain>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1440); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Ensures cookie is essential
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = "826379557782-rvcmaurug5qiesbi218e3jrqm5ubgikb.apps.googleusercontent.com"; 
    options.ClientSecret = "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";
    options.CallbackPath = "/signin-google"; // or whatever you’ve set
});


builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<ISidebarRepository, SidebarRepository>();
builder.Services.AddScoped<IAdminsRepository, AdminsRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IRolesRepository, RolesRepository>();
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
builder.Services.AddScoped<IPermisionHelperRepository, PermisionHelperRepository>();
builder.Services.AddScoped<IExportServiceRepository, ExportServiceRepository>();
builder.Services.AddScoped<IImportServiceRepository, ImportServiceRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();


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
