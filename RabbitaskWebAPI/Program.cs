using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Services;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Configuration.AddEnvironmentVariables();

// =======================
// Database setup (MySQL)
// =======================
builder.Services.AddDbContext<RabbitaskContext>(options =>
{
    var connectionString = $"Server={builder.Configuration["RABBITASK_DB_HOST"]};" +
                           $"Port={builder.Configuration["RABBITASK_DB_PORT"]};" +
                           $"Database={builder.Configuration["RABBITASK_DB_NAME"]};" +
                           $"User={builder.Configuration["RABBITASK_DB_USER"]};" +
                           $"Password={builder.Configuration["RABBITASK_DB_PASSWORD"]};";
 options.UseMySQL(connectionString);
});

// =======================
// Services & Authorization
// =======================
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
            var httpContext = context.Resource as HttpContext;
            if (httpContext == null) return false;

            var authService = httpContext.RequestServices.GetRequiredService<IUserAuthorizationService>();
            var userId = authService.GetCurrentUserId();
            return await authService.IsAgenteAsync(userId);
        }));
});

// =======================
// JWT Authentication
// =======================
var jwtConfig = builder.Configuration;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = jwtConfig["JWT_KEY"];
    if (string.IsNullOrWhiteSpace(key))
        throw new InvalidOperationException("JWT_KEY is not set in environment.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["JWT_ISSUER"],
        ValidAudience = jwtConfig["JWT_AUDIENCE"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// =======================
// Swagger
// =======================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RabbitaskWebAPI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// =======================
// CORS
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8100")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RabbitaskContext>();

    // Retry simples para aguardar o MySQL ficar pronto
    var retries = 10;
    var delay = TimeSpan.FromSeconds(5);

    while (retries > 0)
    {
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations aplicadas com sucesso!");
            break;
        }
        catch (MySql.Data.MySqlClient.MySqlException ex)
        {
            retries--;
            Console.WriteLine($"Erro ao conectar com MySQL, tentando novamente em {delay.Seconds}s... ({retries} tentativas restantes)");
            System.Threading.Thread.Sleep(delay);
        }
    }

    if (retries == 0)
        throw new Exception("Não foi possível aplicar migrations. Verifique o banco de dados.");
}


app.Run();
