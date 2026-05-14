using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AutoMapper;

using backEnd.Data;
using backEnd.Helpers;
using backEnd.Interfaces;
using backEnd.Repositories;
using backEnd.Services;

namespace backEnd.Configurations;

public static class ServiceExtensions
{
    // ─────────────────────────────────────────────
    // PostgreSQL Database Configuration
    // ─────────────────────────────────────────────
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        var connStr = config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connStr))
        {
            throw new InvalidOperationException(
                "Database connection string not found.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connStr, npgsqlOptions =>
            {
                // Retry on transient failures
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);

                npgsqlOptions.CommandTimeout(60);
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        return services;
    }

    // ─────────────────────────────────────────────
    // JWT Authentication
    // ─────────────────────────────────────────────
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        var jwtKey =
            Environment.GetEnvironmentVariable("JWT_KEY")
            ?? config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key missing");

        var issuer =
            Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? config["Jwt:Issuer"];

        var audience =
            Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? config["Jwt:Audience"];

        var key = Encoding.UTF8.GetBytes(jwtKey);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(key),

                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.Zero
                    };

                // ✅ SignalR JWT Support
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken =
                            context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    // ─────────────────────────────────────────────
    // Swagger + JWT
    // ─────────────────────────────────────────────
    public static IServiceCollection AddSwaggerWithJwt(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FarmEase API",
                Version = "v1",
                Description = "FarmEase Backend API"
            });

            // JWT Security Definition
            options.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description =
                        "Enter JWT Token like: Bearer {your token}"
                });

            // JWT Security Requirement
            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference =
                                new OpenApiReference
                                {
                                    Type =
                                        ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        return services;
    }

    // ─────────────────────────────────────────────
    // Dependency Injection
    // ─────────────────────────────────────────────
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Helpers
        services.AddScoped<JwtHelper>();

        // Repositories
        services.AddScoped<IMachineryRepository, MachineryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IMachineryService, MachineryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMessagesService, MessagesService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        return services;
    }

    // ─────────────────────────────────────────────
    // CORS Configuration
    // ─────────────────────────────────────────────
    public static IServiceCollection AddAngularCors(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:4200",
                        "https://front-end-farm-ease.vercel.app",
                        "https://backend-farmease-1.onrender.com"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}