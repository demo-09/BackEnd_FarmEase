using backEnd.Configurations;
using backEnd.Middlewares;
using backEnd.Hubs;
using dotenv.net;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────
// Controllers
// ─────────────────────────────────────────────
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// ─────────────────────────────────────────────
// Database Configuration
// ─────────────────────────────────────────────
builder.Services.AddDatabase(builder.Configuration);

// ─────────────────────────────────────────────
// JWT Authentication
// ─────────────────────────────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);

// ─────────────────────────────────────────────
// Swagger Configuration
// ─────────────────────────────────────────────
builder.Services.AddSwaggerWithJwt();

// ─────────────────────────────────────────────
// CORS Configuration
// ─────────────────────────────────────────────
builder.Services.AddAngularCors(builder.Configuration);

// ─────────────────────────────────────────────
// SignalR Configuration
// ─────────────────────────────────────────────
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// ─────────────────────────────────────────────
// Custom Application Services
// ─────────────────────────────────────────────
builder.Services.AddApplicationServices();

var app = builder.Build();

// ─────────────────────────────────────────────
// Global Exception Middleware
// ─────────────────────────────────────────────
app.UseGlobalExceptionMiddleware();

// ─────────────────────────────────────────────
// Swagger Middleware
// ─────────────────────────────────────────────
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FarmEase API v1");
    c.RoutePrefix = string.Empty;
});

// ─────────────────────────────────────────────
// HTTPS Redirection
// ─────────────────────────────────────────────
app.UseHttpsRedirection();

// ─────────────────────────────────────────────
// Static Files
// ─────────────────────────────────────────────
app.UseDefaultFiles();
app.UseStaticFiles();

// ─────────────────────────────────────────────
// CORS
// ─────────────────────────────────────────────
app.UseCors("AllowAngularApp");

// ─────────────────────────────────────────────
// Authentication & Authorization
// ─────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ─────────────────────────────────────────────
// SignalR Hubs
// ─────────────────────────────────────────────
app.MapHub<ChatHub>("/chatHub");
app.MapHub<StockHub>("/stockHub");

// ─────────────────────────────────────────────
// Controllers
// ─────────────────────────────────────────────
app.MapControllers();

// ─────────────────────────────────────────────
// Run Application
// ─────────────────────────────────────────────
app.Run();