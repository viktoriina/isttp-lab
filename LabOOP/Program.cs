using Microsoft.EntityFrameworkCore;
using LabOOP.Models;
using LabOOP;
using LabOOP.Data;
using Microsoft.AspNetCore.Identity;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.Cookies;
using DocumentFormat.OpenXml.Wordprocessing;
using LabOOP.RoleInitializer;
using LabOOP.IdentityClass;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var emailConfiguration = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddDbContext<DBSHOPContext>(optional => optional.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<UserAndContext>(optional => optional.UseSqlServer(builder.Configuration.GetConnectionString("DefaultIdentityConnection")));
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<EmailService>();
builder.Services.AddSingleton(emailConfiguration);
builder.Services.AddIdentity<User, IdentityRole>(option =>
{
    option.Password.RequiredLength = 6;
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireLowercase = false;
    option.Password.RequireUppercase = false;
    option.Password.RequireDigit = true;
    option.Password.RequiredUniqueChars = 0;
})
.AddEntityFrameworkStores<UserAndContext>().AddUserManager<UserManager<User>>().AddDefaultTokenProviders();
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await RoleInitializer.InitializeAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, $"An error occurred while seeding the database at {DateTime.Now.ToString()}");
    }
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
