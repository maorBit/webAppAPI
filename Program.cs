using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Enable controller support
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection(); // optional on Render
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
