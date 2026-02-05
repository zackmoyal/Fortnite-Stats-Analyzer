using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using FortniteStatsAnalyzer.Configuration;
using FortniteStatsAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// MVC + Razor views
builder.Services.AddControllersWithViews();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Bind config sections
builder.Services.Configure<FortniteApiSettings>(builder.Configuration.GetSection("FortniteApiSettings"));
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAISettings"));

// ---- Typed HttpClient for Fortnite API ----
// (Removes need for a plain AddHttpClient(); do not add that separately.)
builder.Services.AddHttpClient<IFortniteApiService, FortniteApiService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<FortniteApiSettings>>().Value;

    var baseUrl = (settings.BaseUrl ?? "https://fortnite-api.com/").TrimEnd('/') + "/";

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var apiKey = settings.ApiKey?.Trim();
    if (string.IsNullOrWhiteSpace(apiKey))
        throw new InvalidOperationException("Fortnite API key is missing. Set FortniteApiSettings:ApiKey in appsettings.");

    // fortniteapi.io expects the raw key in the Authorization header (no Bearer prefix)
    client.DefaultRequestHeaders.Add("Authorization", apiKey);
});

// Other services
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IFortniteStatsService, FortniteStatsService>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
