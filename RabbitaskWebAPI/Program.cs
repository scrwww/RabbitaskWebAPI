using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? $"Server={builder.Configuration["RABBITASK_DB_HOST"]};" +
       $"Port={builder.Configuration["RABBITASK_DB_PORT"]};" +
       $"Database={builder.Configuration["RABBITASK_DB_NAME"]};" +
       $"User={builder.Configuration["RABBITASK_DB_USER"]};" +
       $"Password={builder.Configuration["RABBITASK_DB_PASSWORD"]};";

builder.Services.AddDbContext<RabbitaskContext>(options =>
{
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString),
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

                var userId = authService.GetCurrentUserId();
                return await authService.IsAgenteAsync(userId);
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

var frontendOrigin = builder.Configuration["FRONTEND_ORIGIN"]
                     ?? "http://localhost:8100";

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", cors =>
    {
        cors.WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RabbitaskContext>();
    db.Database.Migrate();
    Console.WriteLine("Migrations aplicadas com sucesso.");
    return;
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
