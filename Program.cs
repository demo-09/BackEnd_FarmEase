using backEnd.Configurations;
using backEnd.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ─────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Database
builder.Services.AddDatabase(builder.Configuration);

// ✅ Custom Services
builder.Services.AddApplicationServices();

// ✅ JWT Auth
builder.Services.AddJwtAuthentication(builder.Configuration);

// ✅ Swagger
builder.Services.AddSwaggerWithJwt();

// ✅ CORS (FIXED: added configuration parameter)
builder.Services.AddAngularCors(builder.Configuration);

var app = builder.Build();

// ─── Middleware ───────────────────────────

// Global Exception Middleware
app.UseGlobalExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files (for images, etc.)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

// ✅ CORS (must be before auth)
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint();
});
app.Run();