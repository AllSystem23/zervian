using System.Text;
using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.MemoryStorage;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Zorvian.Application.Interfaces;
using Zorvian.Application.Mapping;
using Zorvian.Application.Messages;
using Zorvian.Application.Services;
using Zorvian.Application.Services.DepreciationCalculators;
using Zorvian.Application.Services.PayrollStrategies;
using Zorvian.Core.Interfaces;
using Zorvian.Infrastructure.Data;
using Zorvian.Infrastructure.Identity;
using Zorvian.Infrastructure.Messaging.Consumers;
using Zorvian.Infrastructure.Repositories;
using Zorvian.Infrastructure.Services;
using Zorvian.Infrastructure.Data.Interceptors;
using Zorvian.Web.Hubs;
using Zorvian.Web.Jobs;
using Zorvian.Infrastructure.Jobs;
using Zorvian.Application.Jobs;
using Zorvian.Web.Services;

namespace Zorvian.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZorvianFirebase(this IServiceCollection services, IConfiguration configuration, bool mockExternal)
    {
        if (!mockExternal)
        {
            var projectId = configuration["Firebase:ProjectId"];
            var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
            var logger = loggerFactory.CreateLogger("FirebaseInit");

            // Skip if already initialized (e.g. by another call)
            if (FirebaseApp.DefaultInstance != null)
            {
                logger.LogInformation("FirebaseApp already initialized");
                return services;
            }

            logger.LogInformation("Initializing FirebaseApp (ProjectId={ProjectId})...", projectId);

            var credJson = configuration["Firebase:CredentialsJson"];
            if (!string.IsNullOrEmpty(credJson))
            {
                try
                {
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(credJson));
                    var googleCred = CredentialFactory.FromStream<ServiceAccountCredential>(stream).ToGoogleCredential().CreateScoped();
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = googleCred,
                        ProjectId = projectId,
                    });
                    logger.LogInformation("FirebaseApp initialized from CredentialsJson");
                    return services;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to init FirebaseApp from CredentialsJson, trying file...");
                }
            }

            var credPath = configuration["Firebase:CredentialsFilePath"] ?? string.Empty;
            var fbCredFile = Path.Combine(AppContext.BaseDirectory, credPath);
            logger.LogInformation("Checking {Path}", fbCredFile);
            if (!File.Exists(fbCredFile))
            {
                fbCredFile = Path.Combine("/etc/secrets", credPath);
                logger.LogInformation("File not found, checking {Path}", fbCredFile);
            }
            if (File.Exists(fbCredFile))
            {
                try
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = CredentialFactory.FromFile<ServiceAccountCredential>(fbCredFile).ToGoogleCredential().CreateScoped(),
                        ProjectId = projectId,
                    });
                    logger.LogInformation("FirebaseApp initialized from file: {Path}", fbCredFile);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create FirebaseApp from file: {Path}", fbCredFile);
                }
            }
            else
            {
                logger.LogWarning("Firebase credentials not found at {Path1} or {Path2}", 
                    Path.Combine(AppContext.BaseDirectory, credPath),
                    Path.Combine("/etc/secrets", credPath));
            }
        }
        return services;
    }

    public static IServiceCollection AddZorvianDatabase(this IServiceCollection services, IConfiguration configuration, bool mockExternal)
    {
        services.AddSingleton<EncryptionInterceptor>();
        services.AddScoped<TenantAuditInterceptor>();

        services.AddDbContext<ZorvianDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<Zorvian.Infrastructure.Data.AuditInterceptor>();
            var immutabilityInterceptor = sp.GetRequiredService<Zorvian.Infrastructure.Data.AuditImmutabilityInterceptor>();
            var entityHistoryInterceptor = sp.GetRequiredService<Zorvian.Infrastructure.Data.EntityHistoryInterceptor>();
            var tenantSessionInterceptor = sp.GetRequiredService<Zorvian.Infrastructure.Data.TenantSessionInterceptor>();
            var encryptionInterceptor = sp.GetRequiredService<EncryptionInterceptor>();
            var tenantAuditInterceptor = sp.GetRequiredService<TenantAuditInterceptor>();
            var fileCleanupInterceptor = sp.GetRequiredService<Zorvian.Infrastructure.Data.FileCleanupSaveChangesInterceptor>();

            var connStr = configuration.GetConnectionString("ZorvianDb");
            if (mockExternal || string.IsNullOrEmpty(connStr))
            {
                options.UseInMemoryDatabase("ZorvianInMemoryDb")
                        .AddInterceptors(fileCleanupInterceptor, tenantAuditInterceptor, entityHistoryInterceptor, auditInterceptor, immutabilityInterceptor, encryptionInterceptor)
                        .ConfigureWarnings(w =>
                        {
                            w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning);
                            w.Ignore(RelationalEventId.PendingModelChangesWarning);
                            w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
                            w.Ignore(new EventId(10622, "RequiredRelatedEntityWithGlobalQueryFilterWarning"));
                        });
            }
            else
            {
                options.UseNpgsql(connStr)
                        .AddInterceptors(fileCleanupInterceptor, tenantAuditInterceptor, tenantSessionInterceptor, entityHistoryInterceptor, auditInterceptor, immutabilityInterceptor, encryptionInterceptor)
                        .ConfigureWarnings(w =>
                        {
                            w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning);
                            w.Ignore(RelationalEventId.PendingModelChangesWarning);
                            w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
                            w.Ignore(new EventId(10622, "RequiredRelatedEntityWithGlobalQueryFilterWarning"));
                        });
            }
        });
        return services;
    }

    public static IServiceCollection AddZorvianAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, typeof(MappingProfile).Assembly);
        return services;
    }

    public static IServiceCollection AddZorvianAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret not configured");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "zorvian",
                    ValidAudience = "zorvian-api",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
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

    public static IServiceCollection AddZorvianSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Zorvian ERP API",
                Version = "v1",
                Description = "API del sistema Zorvian ERP. Multi-tenant, con autenticación JWT + Firebase.",
                Contact = new OpenApiContact
                {
                    Name = "Soporte Zorvian",
                    Email = "soporte@zorvian.com",
                },
                License = new OpenApiLicense
                {
                    Name = "Uso interno",
                },
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new() { Id = "Bearer", Type = ReferenceType.SecurityScheme }
                    },
                    Array.Empty<string>()
                }
            });
            var xmlAssemblies = new[] { "Zorvian.Web", "Zorvian.Application", "Zorvian.Core", "Zorvian.Infrastructure" };
            foreach (var name in xmlAssemblies)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{name}.xml");
                if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
            }
        });
        return services;
    }

    public static IServiceCollection AddZorvianCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("ZorvianCors", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                policy.SetIsOriginAllowed(origin =>
                    {
                        // In development, allow localhost
                        if (origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:") ||
                            origin.StartsWith("http://127.0.0.1:") || origin.StartsWith("https://127.0.0.1:"))
                            return true;
                        return allowedOrigins.Contains(origin);
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        return services;
    }

    public static IServiceCollection AddZorvianHangfire(this IServiceCollection services, IConfiguration configuration, bool mockExternal)
    {
        if (!mockExternal)
        {
            var connectionString = configuration.GetConnectionString("ZorvianDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return services.AddHangfire(config => config.UseMemoryStorage());
            }

            services.AddHangfire((sp, config) =>
            {
                config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString!));
            });
            services.AddHangfireServer();
        }
        else
        {
            services.AddHangfire(config => config.UseMemoryStorage());
        }
        return services;
    }

    public static IServiceCollection AddZorvianHealthChecks(this IServiceCollection services, IConfiguration configuration, bool mockExternal)
    {
        var healthChecks = services.AddHealthChecks();

        if (!mockExternal)
        {
            var connStr = configuration.GetConnectionString("ZorvianDb");
            if (!string.IsNullOrEmpty(connStr))
            {
                healthChecks.AddNpgSql(connStr, name: "postgresql", tags: new[] { "db", "ready" });
            }
        }

        healthChecks.AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "ready" });

        return services;
    }

    public static IServiceCollection AddZorvianMessageBus(this IServiceCollection services, IConfiguration configuration, bool mockExternal)
    {
        services.AddMassTransit(busConfig =>
        {
            busConfig.AddConsumer<SaleCreatedConsumer>();
            busConfig.AddConsumer<SaleCancelledConsumer>();
            busConfig.AddConsumer<PaymentReceivedConsumer>();
            busConfig.AddConsumer<EmployeeCreatedConsumer>();
            busConfig.AddConsumer<PalmTrackWebhookConsumer>();
            busConfig.AddConsumer<PalmTrackWebhookDlqConsumer>();

            var rabbitEnabled = configuration.GetValue<bool>("RabbitMQ:Enabled");
            var host = configuration["RabbitMQ:Host"] ?? "localhost";
            var useInMemory = mockExternal || !rabbitEnabled || string.IsNullOrWhiteSpace(host);

            if (useInMemory)
            {
                busConfig.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                var portRaw = configuration["RabbitMQ:Port"] ?? "5672";
                if (!ushort.TryParse(portRaw, out var port))
                    port = 5672;
                var user = configuration["RabbitMQ:Username"] ?? "zorvian";
                var pass = configuration["RabbitMQ:Password"] ?? "zorvian_dev_2026";
                var vhost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host, port, vhost, h =>
                    {
                        h.Username(user);
                        h.Password(pass);
                    });

                    cfg.ConfigureEndpoints(context);

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                    cfg.UseCircuitBreaker(cb =>
                    {
                        cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                        cb.TripThreshold = 15;
                        cb.ActiveThreshold = 10;
                        cb.ResetInterval = TimeSpan.FromMinutes(5);
                    });
                });
            }
        });

        return services;
    }
}