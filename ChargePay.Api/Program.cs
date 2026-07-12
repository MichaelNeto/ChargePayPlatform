using Microsoft.AspNetCore.Mvc;
using ChargePay.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============ CONFIGURAÇÃO INICIAL ============

// Adicionar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/chargepay-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ChargePay.Api")
    .CreateLogger();

builder.Host.UseSerilog();

// ============ AUTENTICAÇÃO JWT ============

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "seu_super_secreto_chave_de_desenvolvimento_aqui_123456";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ChargePay";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ChargePayApi";

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ============ SERVIÇOS ============

builder.Services.AddControllers();
            builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Registrar serviços de dados
builder.Services.AddDataServices(builder.Configuration);

// ============ BUILD ============

var app = builder.Build();

// ============ MIDDLEWARE ============

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Executar migrações automaticamente
app.Services.MigrateDatabase();

// ============ ROTAS ============

app.MapControllers();

app.Run();
