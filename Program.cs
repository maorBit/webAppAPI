// Program.cs
using SendGrid;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1) CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigins", policy =>
    {
        policy.WithOrigins(
                "https://jubilo-wedding.netlify.app",
                "http://localhost:8080"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 2) Controllers
builder.Services.AddControllers();

// 3) Swagger (in dev)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Jubilo Email & Score API",
        Version = "v1",
    });
});

// 4) SendGrid client
builder.Services.AddSingleton<ISendGridClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var cfgKey = config["SendGrid:ApiKey"];
    var envKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
    var apiKey = !string.IsNullOrWhiteSpace(cfgKey) ? cfgKey : envKey;
    if (string.IsNullOrWhiteSpace(apiKey))
        throw new InvalidOperationException(
            "SendGrid API key not configured. " +
            "Define SendGrid:ApiKey in appsettings or set the SENDGRID_API_KEY env var."
        );
    return new SendGridClient(apiKey);
});

// 5) LootLocker HttpClient registration
builder.Services.AddHttpClient("LootLocker", client =>
{
    client.BaseAddress = new Uri("https://api.lootlocker.io/");
    client.DefaultRequestHeaders.Add("x-api-key", "dev_be8cdc0c9a9943ec80f9e99bfb1be7a5");
    client.DefaultRequestHeaders.Add("x-lootlocker-game-domain", "dev");
})
// Optional: tune handler lifetime or add retry policies here
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

var app = builder.Build();

// 6) Dev-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jubilo Email & Score API v1");
        c.RoutePrefix = string.Empty;
    });
}

// 7) Redirect HTTP→HTTPS
app.UseHttpsRedirection();

// 8) Apply CORS
app.UseCors("AllowFrontendOrigins");

// 9) Map controllers
app.MapControllers();

app.Run();
