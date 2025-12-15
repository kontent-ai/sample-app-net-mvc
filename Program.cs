using Ficto.Services.Content;
using Ficto.Services.Content.Interfaces;
using Ficto.Generated.Models;
using Kontent.Ai.Delivery.Extensions;
using Kontent.Ai.Delivery.Abstractions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.
services.AddControllersWithViews();
services.AddScoped<IContentService, ContentService>();
services.Configure<SiteOptions>(configuration.GetSection("SiteOptions"));

// TODO: Configure webhook options

// TODO: Add delivery client and other services here.

services.AddDeliveryClient(configuration);

// TODO: Add a singleton CustomTypeProvider from Generated/Models. Overrides the default type provider from the SDK.
// Make sure to specify the type provider interface as the first type param (similarly to how we inject the ContentService).


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

// Possibly use for preview mode gating, likely not in scope for this project.
app.UseAuthorization();

// This is default routing for the application. If a segment is not found, it will default to Home controller and Index action.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
