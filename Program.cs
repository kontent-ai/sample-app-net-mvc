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
services.AddSingleton<ITypeProvider, CustomTypeProvider>();
// TODO: Configure webhook options
services.AddDeliveryClient(configuration);

// TODO: Add delivery client and other services here.


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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
