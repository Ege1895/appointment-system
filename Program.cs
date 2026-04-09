using AppointmentSystem.Api.Modules.EmployeeModule.Services;
using AppointmentSystem.Api.Modules.HealthModule.Services;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Modules.UserModule.Services;
using AppointmentSystem.Api.Modules.UserModule.DTOs;
using AppointmentSystem.Api.Modules.OfferingModule.Services;
using AppointmentSystem.Api.Modules.WorkingHourModule.Services;
using AppointmentSystem.Api.Modules.AppointmentModule.Services;
using AppointmentSystem.Api.Modules.FeedbackModule.Services;
using AppointmentSystem.Api.Modules.CatalogModule.Services;
using AppointmentSystem.Api.Modules.HomeModule.Services;
using AppointmentSystem.Api.Modules.BranchModule.Services;
using AppointmentSystem.Api.Modules.Auth.Services;
using AppointmentSystem.Api.Shared.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Responses;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Notifications;
using AppointmentSystem.Api.Modules.ReportingModule.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Modules.AppointmentModule.Workers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Log.Logger = CreateBootstrapLogger(builder.Configuration);

builder.Host.UseSerilog();

#region Services

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(RateLimitingPolicies.LOGIN_POLICY, opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter(RateLimitingPolicies.REFRESH_TOKEN_POLICY, opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter(RateLimitingPolicies.EXPORT_POLICY, opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter(RateLimitingPolicies.IMPORT_POLICY, opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(BusinessRules.CORS_POLICY_NAME, policy =>
    {
        string[] origins = builder.Configuration.GetSection(AppSettingsKeys.CORS_ALLOWED_ORIGINS).Get<string[]>() ?? Array.Empty<string>();
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});


builder.Services.AddStackExchangeRedisCache(options =>
{
    string? redisConnection = builder.Configuration.GetConnectionString(AppSettingsKeys.REDIS_CONNECTION);
    if (string.IsNullOrWhiteSpace(redisConnection))
    {
        throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");
    }

    options.Configuration = redisConnection;
});

builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddScoped<IHealthService, HealthService>();

builder.Services.AddHostedService<AppointmentStateWorker>();
builder.Services.AddHostedService<AppointmentReminderWorker>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        Dictionary<string, string[]> errors = context.ModelState
            .Where(x => x.Value != null && x.Value.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        ApiResponse<object> response = ApiResponse<object>.ValidationFailure(errors);

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddScoped<IBusinessAuthService, BusinessAuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<IUnifiedAuthService, UnifiedAuthService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IOfferingService, OfferingService>();
builder.Services.AddScoped<IWorkingHourService, WorkingHourService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IAppointmentNoteService, AppointmentNoteService>();
builder.Services.AddScoped<IAppointmentMessageService, AppointmentMessageService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IBusinessCustomerService, BusinessCustomerService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<AuditableEntitySaveChangesInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    string? dbConnection = builder.Configuration.GetConnectionString(AppSettingsKeys.DEFAULT_CONNECTION);
    if (string.IsNullOrWhiteSpace(dbConnection))
    {
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
    }

    AuditableEntitySaveChangesInterceptor interceptor =
        serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();

    options.UseNpgsql(dbConnection);
    options.AddInterceptors(interceptor);
});

builder.Services.AddScoped<JwtTokenGenerator>();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(AppSettingsKeys.JWT_SECTION)
);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        JwtSettings jwtSettings = builder.Configuration.GetSection(AppSettingsKeys.JWT_SECTION).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings not configured");
        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("Jwt SecretKey is missing");
        }

        byte[] key = System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true
        };

        options.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

#endregion

WebApplication app = builder.Build();

#region Middlewares

using (IServiceScope scope = app.Services.CreateScope())
{
    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; frame-ancestors 'none'");
    await next();
});


app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseCors(BusinessRules.CORS_POLICY_NAME);
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();

static Serilog.ILogger CreateBootstrapLogger(IConfiguration configuration)
{
    LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(configuration)
        .WriteTo.Console();

    string? elasticUrl = configuration[AppSettingsKeys.ELASTIC_URI];
    if (!string.IsNullOrWhiteSpace(elasticUrl) && Uri.TryCreate(elasticUrl, UriKind.Absolute, out Uri? uri))
    {
        try
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(uri)
            {
                AutoRegisterTemplate = true,
                IndexFormat = AppSettingsKeys.ELASTIC_INDEX_FORMAT
            });
        }
        catch
        {
            // Console logging fallback — Elasticsearch is optional.
        }
    }

    return loggerConfiguration.CreateLogger();
}
