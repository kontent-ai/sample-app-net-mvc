using Ficto.Configuration;
using Ficto.Middleware;
using Ficto.Models.Mappers;
using Ficto.Services.Content;
using Ficto.Services.Routing;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.
services.AddControllersWithViews();
services.AddHttpContextAccessor();
services.AddDataProtection();

// Options — ValidateOnStart ensures the app fails fast with a clear message
// if required configuration is missing, rather than surfacing opaque errors at request time.
services.AddOptions<SiteOptions>()
    .Bind(configuration.GetSection("SiteOptions"))
    .Validate(o => o.Spaces is { Length: > 0 }, "SiteOptions:Spaces must contain at least one space codename.")
    .Validate(o => o.RouteTemplates is { Count: > 0 }, "SiteOptions:RouteTemplates must define at least one route template.")
    .ValidateOnStart();

services.AddOptions<WebhookOptions>()
    .Bind(configuration.GetSection("WebhookOptions"))
    .ValidateOnStart();

services.AddOptions<PreviewOptions>()
    .Bind(configuration.GetSection("PreviewOptions"))
    .ValidateOnStart();

// Space and preview context — resolved once per request by SpaceContextMiddleware
services.AddScoped<SpaceContext>();
services.AddScoped<ISpaceContext>(sp => sp.GetRequiredService<SpaceContext>());
services.AddScoped<PreviewContext>();
services.AddScoped<IPreviewContext>(sp => sp.GetRequiredService<PreviewContext>());

// Preview mode wiring:
//   - IPreviewTokenProtector: signs/validates the ficto_preview cookie.
//   - IPreviewAccessGate: pluggable authorization seam. SecretPreviewAccessGate is the sample default:
//     it checks a shared secret from PreviewOptions (user-secrets). Set PreviewOptions:Secret empty
//     for zero-config local dev (the gate then logs a warning and allows anyone).
//     Real deployments should swap in an auth-aware implementation (OIDC / custom policy / etc.).
services.AddSingleton<IPreviewTokenProtector, PreviewTokenProtector>();
services.AddSingleton<IPreviewAccessGate, SecretPreviewAccessGate>();

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

// Rich-text resolution: a single IHtmlResolver instance, injected into mappers that call
// .ToHtmlAsync(). Content-item links go through IRouteResolver; Fact/Action/Callout
// components get inline HTML templates (distinct from their block-level rendering).
services.AddSingleton<IHtmlResolver>(sp =>
    RichTextResolver.Build(sp.GetRequiredService<IRouteResolver>()));


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
