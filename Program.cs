using SendGrid;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;    // ← needed for OpenApiInfo
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
        // .AllowCredentials(); // only if you need cookies/auth headers
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
        Title = "Jubilo Email API",
        Version = "v1",
        Description = "SendGrid-powered email endpoints"
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

var app = builder.Build();

// 5) Dev-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jubilo Email API v1");
        c.RoutePrefix = string.Empty; // swagger at root (https://.../)
    });
}

// 6) Redirect HTTP→HTTPS
app.UseHttpsRedirection();

// 7) CORS must come before routing/controllers
app.UseCors("AllowFrontendOrigins");

// 8) (Optional) Authn / Authz
// app.UseAuthentication();
// app.UseAuthorization();

// 9) Map your API controllers
app.MapControllers();

app.Run();
