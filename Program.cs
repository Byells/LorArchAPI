using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LorArchApi.Data;
using LorArchApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "LorArch API",
        Version     = "v1",
        Description = "API para gerenciar as motos nos pátios da Mottu."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Locarch API v1");
});

app.UseHttpsRedirection();

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

app.Run();