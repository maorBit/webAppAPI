using SendGrid;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1) CORS — allow your WebGL origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigins", policy =>
    {
        policy.WithOrigins("https://maorbit.github.io")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2) Add controllers
builder.Services.AddControllers();

// 3) SendGrid client — read from appsettings or ENV
builder.Services.AddSingleton<ISendGridClient>(sp =>
{
    // prefer appsettings (user-secrets/local dev), else ENV
    var cfgKey = builder.Configuration["SendGrid:ApiKey"];
    var envKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
    var apiKey = !string.IsNullOrWhiteSpace(cfgKey) ? cfgKey : envKey;

    if (string.IsNullOrWhiteSpace(apiKey))
        throw new InvalidOperationException(
            "SendGrid API key not configured. " +
            "Set SendGrid:ApiKey in user-secrets or the SENDGRID_API_KEY env-var."
        );

    return new SendGridClient(apiKey);
});

var app = builder.Build();

// 4) Pipeline
app.UseCors("AllowFrontendOrigins");

app.UseHttpsRedirection();

// (Optional) If you later add authentication/authorization:
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();
