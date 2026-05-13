using backEnd.Configurations;
using backEnd.Middlewares;
using backEnd.Hubs;
using dotenv.net;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// ─── Services ─────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Database
builder.Services.AddDatabase(builder.Configuration);

// ✅ JWT Auth
builder.Services.AddJwtAuthentication(builder.Configuration);

// ✅ Swagger
builder.Services.AddSwaggerWithJwt();

// ✅ CORS
builder.Services.AddAngularCors(builder.Configuration);

// ✅ SignalR (Added before application services)
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// ✅ Custom Services
builder.Services.AddApplicationServices();

var app = builder.Build();

// ─── Middleware ───────────────────────────

// Global Exception Middleware
app.UseGlobalExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ CORS (Must be before Routing, SignalR, and Auth)
app.UseCors("AllowAngularApp");

// Serve static files (for images, etc.)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chatHub");
app.MapHub<StockHub>("/stockHub");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FarmEase API v1");
});
app.Run();