using Microsoft.EntityFrameworkCore;
using WeddingApi;
using WeddingApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var rawConn = ConnectionStringResolver.Resolve(builder.Configuration, builder.Environment);
Console.WriteLine($"Using database host: {ConnectionStringResolver.DescribeHost(rawConn)}");

builder.Services.AddDbContext<WeddingDbContext>(o => o.UseNpgsql(rawConn));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("WeddingPolicy", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                      ?? ["http://localhost:4200"];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeddingDbContext>();
    db.Database.Migrate();
}

app.UseCors("WeddingPolicy");
app.UseAuthorization();
app.MapControllers();
app.Run();
