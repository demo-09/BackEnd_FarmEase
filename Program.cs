using backEnd.Configurations;
using backEnd.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ─────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Database (FIXED: now supports PostgreSQL / SQL Server)
builder.Services.AddDatabase(builder.Configuration);

// ✅ Custom Services
builder.Services.AddApplicationServices();

// ✅ JWT Auth
builder.Services.AddJwtAuthentication(builder.Configuration);

// ✅ Swagger
builder.Services.AddSwaggerWithJwt();

// ✅ CORS
builder.Services.AddAngularCors();

var app = builder.Build();

// Global Exception Middleware
app.UseGlobalExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();