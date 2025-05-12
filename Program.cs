var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGitHubPages", policy =>
    {
        policy.WithOrigins("https://maorbit.github.io")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Use CORS with named policy
app.UseCors("AllowGitHubPages");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
