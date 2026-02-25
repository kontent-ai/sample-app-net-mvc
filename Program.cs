using Ficto.Services.Content;
using Ficto.Services.Content.Interfaces;
using Ficto.Generated.Models;
using Ficto.Models.Mappers;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.
services.AddControllersWithViews();
services.AddScoped<IContentService, ContentService>();
services.Configure<SiteOptions>(configuration.GetSection("SiteOptions"));

// TODO: Configure webhook options

// Register Kontent.ai Delivery Client
services.AddDeliveryClient(options =>
{
    configuration.GetSection("DeliveryOptions").Bind(options);
});

// Mappers
services.AddScoped<ReferenceMapper>();
services.AddScoped<FactMapper>();
services.AddScoped<PersonMapper>();
services.AddScoped<ProductMapper>();
services.AddScoped<NavigationItemMapper>();
services.AddScoped<ContentChunkMapper>();
services.AddScoped<SolutionMapper>();
services.AddScoped<VisualContainerMapper>();
services.AddScoped<IPageBlockMapperFactory, PageBlockMapperFactory>();
services.AddScoped<PageMapper>();
services.AddScoped<ArticleMapper>();
services.AddScoped<WebsiteRootMapper>();


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
