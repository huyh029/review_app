using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using reviewApi.Models;
using reviewApi.Service;
using reviewApi.Service.Auth;
using reviewApi.Service.General;
using reviewApi.Service.Page.Configuration;
using reviewApi.Service.Page.EvaluationBoard.Detail;
using reviewApi.Service.Page.EvaluationBoard.ManagerEvaluation;
using reviewApi.Service.Page.EvaluationBoard.ResultEvaluation;
using reviewApi.Service.Page.EvaluationBoard.SelfEvaluation;
using reviewApi.Service.Page.Reports;
using reviewApi.Service.Repositories;
using reviewApi.Service.Repositories.Auth;
using reviewApi.Service.Repositories.General;
using reviewApi.Service.Repositories.Auth;
using reviewApi.Service.Repositories.Page.Configuration;
using reviewApi.Service.Repositories.Page.EvaluationBoard.Detail;
using reviewApi.Service.Repositories.Page.EvaluationBoard.ManagerEvaluation;
using reviewApi.Service.Repositories.Page.EvaluationBoard.ResultEvaluation;
using reviewApi.Service.Repositories.Page.EvaluationBoard.SelfEvaluation;
using reviewApi.Service.Repositories.Page.Reports;
using System.Text;
using System.Text.Json;
using reviewApi.Service.General;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.License.SetNonCommercialPersonal("YourName");
// ----------------- Add services -----------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.MaxDepth = 64;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger với JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Nhập JWT: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS cho Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200", "https://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// DB Oracle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));


// Register UnitOfWork and GenericRepository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISetupService, SetupService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();
builder.Services.AddScoped<IKeycloakSyncService, KeycloakSyncService>();
builder.Services.AddHostedService<KeycloakSyncBackgroundService>();
builder.Services.AddScoped<IEvaluationObjectService, EvaluationObjectService>();
builder.Services.AddScoped<IEvaluationObjectRoleService, EvaluationObjectRoleService>();
builder.Services.AddScoped<IEvaluationFlowService, EvaluationFlowService>();
builder.Services.AddScoped<IEvaluationFlowDetailService, EvaluationFlowDetailService>();
builder.Services.AddScoped<ITreeBuilderService, TreeBuilderService>();
builder.Services.AddScoped<IEvaluationCriteriaService, EvaluationCriteriaService>();
builder.Services.AddScoped<IEvaluationCriteriaDetailService, EvaluationCriteriaDetailService>();
builder.Services.AddScoped<IReportTypeService, ReportTypeService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISelfEvaluationService, SelfEvaluationService>();
builder.Services.AddScoped<IManagerEvaluationService, ManagerEvaluationService>();
builder.Services.AddScoped<IResultEvaluationService, ResultEvaluationService>();
builder.Services.AddScoped<IEvaluationCommentService, EvaluationCommentService>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Authorization

// ----------------- Authentication: JWT nội bộ + Keycloak -----------------
var key = builder.Configuration["JwtSettings:Key"];
if (string.IsNullOrWhiteSpace(key))
    throw new Exception("JWT secret key is missing or empty!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiScheme";
    options.DefaultChallengeScheme = "MultiScheme";
})
.AddJwtBearer("LocalJwt", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var jti = context.Principal.FindFirst("jti")?.Value;
            if (jti != null && cache.TryGetValue($"blacklist_{jti}", out _))
                context.Fail("Token is blacklisted!");
        },
        OnAuthenticationFailed = context =>
        {
            // Không log lỗi ở đây vì MultiScheme sẽ thử scheme tiếp theo
            return Task.CompletedTask;
        }
    };
})
.AddJwtBearer("Keycloak", options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"];
    options.MetadataAddress = builder.Configuration["Keycloak:MetadataAddress"];
    options.RequireHttpsMetadata = bool.Parse(builder.Configuration["Keycloak:RequireHttpsMetadata"] ?? "false");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false, // Keycloak token audience là realm-management/account
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["Keycloak:Authority"],
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        }
    };
})
.AddPolicyScheme("MultiScheme", "LocalJwt or Keycloak", options =>
{
    // Tự động chọn scheme dựa vào issuer trong token
    options.ForwardDefaultSelector = context =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var parts = token.Split('.');
            if (parts.Length == 3)
            {
                try
                {
                    var pad = (string s) => s + new string('=', (-s.Length % 4 + 4) % 4);
                    var payload = System.Text.Json.JsonDocument.Parse(
                        System.Text.Encoding.UTF8.GetString(
                            Convert.FromBase64String(pad(parts[1].Replace('-', '+').Replace('_', '/')))));
                    if (payload.RootElement.TryGetProperty("iss", out var iss))
                    {
                        var issuer = iss.GetString();
                        if (issuer != null && issuer.Contains("keycloak") || issuer != null && issuer.Contains("realms"))
                            return "Keycloak";
                    }
                }
                catch { }
            }
        }
        return "LocalJwt";
    };
});


// ----------------- App Pipeline -----------------
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Attempting to ensure database is created...");
        context.Database.EnsureCreated();
        
        // Auto-migrate database if needed
        try
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying pending migrations...");
                context.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Migration failed, database might already exist: {Message}", ex.Message);
        }

        // Kiểm tra và đồng bộ dữ liệu từ Keycloak khi startup
        try
        {
            var syncService = services.GetRequiredService<IKeycloakSyncService>();
            await syncService.SyncAllAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Keycloak sync thất bại khi khởi động, app vẫn tiếp tục: {Message}", ex.Message);
        }    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB: {Message}", ex.Message);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");
app.UseExceptionHandler(config =>
{
    config.Run(async context =>
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var result = JsonSerializer.Serialize(new
            {
                message = error.Error.Message
            });
            await context.Response.WriteAsync(result);
        }
    });
});

app.UseHttpsRedirection();

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".mp4"] = "video/mp4";
provider.Mappings[".webm"] = "video/webm";
provider.Mappings[".mov"] = "video/quicktime";
provider.Mappings[".avi"] = "video/x-msvideo";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "",
    ContentTypeProvider = provider
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
