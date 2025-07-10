using AI_Raports_Generators.Data;
using AI_Raports_Generators.Models.Domains;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using AI_Raports_Generators.Services;

using System;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();



builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IAIContentGeneratorService, AIContentGeneratorService>();

builder.Services.AddScoped<IAIResponseGeneratorService, AIResponseGeneratorService>();
builder.Services.AddScoped<IAIEmailGeneratorService, AIGeneratedEmailService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "custom",
        pattern: "moja-strona/{nazwa?}",
        defaults: new { controller = "Home", action = "Wyswietl" });


app.MapControllerRoute(
    name: "custom2",
    pattern: "view/{nazwa?}",
    defaults: new { controller = "Home", action = "Form" });

app.MapControllerRoute(
    name: "custom3",
    pattern: "generate/{nazwa?}",
    defaults: new { controller = "Raport", action = "SpecialRaport" });


app.MapControllerRoute(
    name: "custom4",
    pattern: "mail/{nazwa?}",
    defaults: new { controller = "Email", action = "SpecialEmail" });







app.MapRazorPages();

app.Run();
