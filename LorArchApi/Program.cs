using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LorArchApi.Data;
using LorArchApi.Endpoints;
using LorArchApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(opts =>
        opts.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));
}

builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,           
            ValidateAudience = true,         
            ValidateLifetime = true,         
            ValidateIssuerSigningKey = true, 
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "LorArch API",
            Version = "v1",
            Description = "API para gerenciar as motos nos pátios da Mottu."
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o Bearer token que retornou do método de login"
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
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LorArch API v1");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapMotoEndpoints();
app.MapUnidadeEndpoints();
app.MapSetorEndpoints();
app.MapCidadeEndpoints();
app.MapEstadoEndpoints();
app.MapDefeitoEndpoints();
app.MapDefeitoMotoEndpoints();
app.MapManutencaoEndpoints();
app.MapHistoricoManutencaoEndpoints();
app.MapLocalizacaoEndpoints();
app.MapLoraEndpoints();
app.MapRfidEndpoints();
app.MapAuthEndpoints();

await app.RunAsync();
public partial class Program { };