// Program.cs
using SendGrid;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1) Define a default CORS policy:
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://jubilo-wedding.netlify.app")  // your Netlify URL
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

// 2) Usual services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Jubilo API", Version = "v1" });
});

// SendGrid
builder.Services.AddSingleton<ISendGridClient>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var key = cfg["SendGrid:ApiKey"]
          ?? Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
          ?? throw new InvalidOperationException("Missing SendGrid key");
    return new SendGridClient(key);
});

// LootLocker
builder.Services.AddHttpClient("LootLocker", cli =>
{
    cli.BaseAddress = new Uri("https://api.lootlocker.io/");
    cli.DefaultRequestHeaders.Add("x-api-key", "dev_be8cdc0c9a9943ec80f9e99bfb1be7a5");
    cli.DefaultRequestHeaders.Add("x-lootlocker-game-domain", "dev");
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5));

var app = builder.Build();

// 3) Dev‐only bits
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

// 4) Global middleware
app.UseHttpsRedirection();

// **CORS must come before MapControllers()**
app.UseCors();           // applies that default policy to every endpoint

// 5) Map your controllers in one line
app.MapControllers();

app.Run();
