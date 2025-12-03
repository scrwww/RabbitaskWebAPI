using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Middleware;
using RabbitaskWebAPI.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var connectionString =
    builder.Configuration.GetConnectionString("RabbitaskDb")
    ?? throw new InvalidOperationException("Connection string 'RabbitaskDb' not found in configuration");

builder.Services.AddDbContext<RabbitaskContext>(options =>
{
    // Use explicit MySQL version instead of AutoDetect to avoid connection at startup
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
    options.UseMySql(connectionString, serverVersion,
        mysql =>
        {
            mysql.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
builder.Services.AddScoped<ICodigoConexaoService, CodigoConexaoService>();
builder.Services.AddScoped<IAuthorizationHandler, ManageUserHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManageUser", policy =>
        policy.Requirements.Add(new ManageUserRequirement()));

    options.AddPolicy("AgenteOnly", policy =>
        policy.RequireAssertion(async context =>
        {
            if (context.Resource is HttpContext httpContext)
            {
                var authService = httpContext.RequestServices
                                             .GetRequiredService<IUserAuthorizationService>();

                var cdUsuario = authService.ObterCdUsuarioAtual();
                return await authService.EhAgenteAsync(cdUsuario);
            }

            return false;
        }));
});

var jwt = builder.Configuration;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = jwt["JWT_KEY"];
    if (string.IsNullOrWhiteSpace(key))
        throw new InvalidOperationException("JWT_KEY missing");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["JWT_ISSUER"],
        ValidAudience = jwt["JWT_AUDIENCE"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RabbitaskWebAPI", Version = "v1" });

    // XML doc
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Ex: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", cors =>
    {
        cors.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto-run migrations on startup
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RabbitaskContext>();
    
    Console.WriteLine("Applying database migrations...");
    db.Database.Migrate();
    Console.WriteLine("Database migrations completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error applying migrations: {ex.Message}");
    // Don't fail startup if migrations fail - allow health checks to report issues
}

if (args.Contains("--migrate"))
{
    return;
}

// Add exception handler middleware
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Only use developer exception page in development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RabbitaskWebAPI v1");
    c.InjectStylesheet("/swagger-ui/custom.css");
});

app.UseStaticFiles();

app.MapControllers();

app.Run();
