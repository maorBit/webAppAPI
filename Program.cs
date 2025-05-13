using SendGrid;             // ← install SendGrid via NuGet
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigins", policy =>
    {
        policy.WithOrigins(
            "https://maorbit.github.io"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Controllers
builder.Services.AddControllers();

// SendGrid client registration
builder.Services.AddSingleton<ISendGridClient>(sp =>
    new SendGridClient(builder.Configuration["SendGrid:ApiKey"])
);

var app = builder.Build();

// Apply CORS policy before anything else
app.UseCors("AllowFrontendOrigins");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
