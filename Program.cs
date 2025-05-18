// Program.cs — copy & paste this entire file

using SendGrid;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1) Register a default CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://jubilo-wedding.netlify.app",
                "http://localhost:8080"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 2) Add controllers & Swagger support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Jubilo API",
        Version = "v1"
    });
});

// 3) SendGrid client registration
builder.Services.AddSingleton<ISendGridClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["SendGrid:ApiKey"]
              ?? Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
              ?? throw new InvalidOperationException("Missing SendGrid API key.");
    return new SendGridClient(apiKey);
});

// 4) LootLocker HttpClient registration
builder.Services.AddHttpClient("LootLocker", client =>
{
    client.BaseAddress = new Uri("https://api.lootlocker.io/");
    client.DefaultRequestHeaders.Add("x-api-key", "dev_be8cdc0c9a9943ec80f9e99bfb1be7a5");
    client.DefaultRequestHeaders.Add("x-lootlocker-game-domain", "dev");
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

var app = builder.Build();

// 5) Dev‐only middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jubilo API v1");
        c.RoutePrefix = string.Empty;
    });
}

// 6) Always redirect HTTP→HTTPS
app.UseHttpsRedirection();

// 7) Routing & CORS (order matters)
app.UseRouting();
app.UseCors();        // applies default policy to all endpoints

// 8) Map your controllers
app.MapControllers();

app.Run();
