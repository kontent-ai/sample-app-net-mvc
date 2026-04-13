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
services.Configure<PreviewOptions>(configuration.GetSection("PreviewOptions"));
services.Configure<WebhookOptions>(configuration.GetSection("WebhookOptions"));

// Space context — resolved once per request by SpaceContextMiddleware
services.AddScoped<SpaceContext>();
services.AddScoped<ISpaceContext>(sp => sp.GetRequiredService<SpaceContext>());

// Content service
services.AddScoped<IContentService, ContentService>();
services.AddSingleton<IRouteResolver, RouteResolver>();

// Register Kontent.ai Delivery Client
// TODO (step 3): Replace with IDeliveryClientFactory for preview/production switching.
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
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Resolve active space from subdomain / query string / cookie / default
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
