using Microsoft.EntityFrameworkCore;
using WeddingApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// PostgreSQL — reads DATABASE_URL from Railway or connection string from config
var rawConn = Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? builder.Configuration.GetConnectionString("DefaultConnection")!;

if (rawConn.StartsWith("postgres://"))
{
    var uri = new Uri(rawConn);
    var userInfo = uri.UserInfo.Split(':');
    rawConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

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
