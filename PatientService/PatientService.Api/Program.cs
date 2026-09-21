using HospitalManager.Contracts.Identity.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PatientService.Application.Authentication;
using PatientService.Infrastructure;
using PatientService.Infrastructure.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste the JWT access token."
        });

    options.AddSecurityRequirement(document =>
    new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddDbContext<PatientDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PatientDb")
        ?? throw new InvalidOperationException(
            "PatientDb connection string is missing.")));

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Missing configuration: Jwt:Issuer");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Missing configuration: Jwt:Audience");

var jwtPublicKey = builder.Configuration["Jwt:PublicKey"]
    ?? throw new InvalidOperationException("Missing configuration: Jwt:PublicKey");

var rsa = RSA.Create();

rsa.ImportSubjectPublicKeyInfo(
    Convert.FromBase64String(jwtPublicKey),
    out _);

var rsaSecurityKey = new RsaSecurityKey(rsa);


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSecurityKey,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role"
        };
    });

builder.Services.AddSingleton<RSA>(_ => rsa);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));
});

var identityGrpcAddress =
    builder.Configuration["IdentityGrpc:Address"]
    ?? throw new InvalidOperationException(
        "Missing configuration: IdentityGrpc:Address");

builder.Services
    .AddGrpcClient<IdentityInternal.IdentityInternalClient>(
        options => options.Address = new Uri(identityGrpcAddress));

builder.Services.AddScoped<
    IIdentityLoginClient,
    GrpcIdentityLoginClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/health/ready",
    async (PatientDbContext db, CancellationToken cancellationToken) =>
        await db.Database.CanConnectAsync(cancellationToken)
            ? Results.Ok(new { status = "Healthy" })
            : Results.StatusCode(503));

app.Run();
