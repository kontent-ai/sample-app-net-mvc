using Ficto.Configuration;
using Ficto.Middleware;
using Ficto.Models.Helpers;
using Ficto.Models.Mappers;
using Ficto.Services.Content;
using Ficto.Services.Content.Interfaces;
using Kontent.Ai.Delivery;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.
services.AddControllersWithViews();
services.AddHttpContextAccessor();

// Options
services.Configure<SiteOptions>(configuration.GetSection("SiteOptions"));
services.Configure<WebhookOptions>(configuration.GetSection("WebhookOptions"));

// Space and preview context — resolved once per request by SpaceContextMiddleware
services.AddScoped<SpaceContext>();
services.AddScoped<ISpaceContext>(sp => sp.GetRequiredService<SpaceContext>());
services.AddScoped<PreviewContext>();
services.AddScoped<IPreviewContext>(sp => sp.GetRequiredService<PreviewContext>());

// Content service
services.AddScoped<IContentService, ContentService>();
services.AddSingleton<IRouteResolver, RouteResolver>();

// Kontent.ai Delivery clients — production uses published content, preview uses draft content.
// Both share the same environment ID and endpoint config from DeliveryOptions.
var cacheExpiration = TimeSpan.FromSeconds(
    configuration.GetValue("SiteOptions:CacheExpirationSeconds", 60));

services.AddDeliveryClient("production", options =>
{
    configuration.GetSection("DeliveryOptions").Bind(options);
    options.UsePreviewApi = false;
});
services.AddDeliveryMemoryCache("production", opts =>
{
    opts.DefaultExpiration = cacheExpiration;
    opts.IsFailSafeEnabled = true;
});

// Preview client is only registered when PreviewApiKey is set in appsettings.json.
// Without it the app still starts; preview URLs fall back to production content with a warning.
// No cache is registered for the preview client — editors expect to see changes immediately.
var previewApiKey = configuration["DeliveryOptions:PreviewApiKey"];
if (!string.IsNullOrWhiteSpace(previewApiKey))
{
    services.AddDeliveryClient("preview", options =>
    {
        configuration.GetSection("DeliveryOptions").Bind(options);
        options.UsePreviewApi = true;
    });
}

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
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Resolve active space and preview flag from host, query string, and cookie.
app.UseMiddleware<SpaceContextMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// CMS page fallback — matches any single segment not claimed by a controller above.
app.MapControllerRoute(
    name: "page",
    pattern: "{slug}",
    defaults: new { controller = "Page", action = "Index" });

app.Run();
