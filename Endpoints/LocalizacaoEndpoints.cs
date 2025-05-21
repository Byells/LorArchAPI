using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class LocalizacaoEndpoints
{
    public static WebApplication MapLocalizacaoEndpoints(this WebApplication app)
    {
        app.MapGet("/localizacoes", async (ApplicationDbContext db) =>
                Results.Ok(await db.Localizacoes.ToListAsync())
            )
            .WithName("GetLocalizacoes")
            .WithTags("Localizacoes")
            .Produces<List<Localizacao>>(StatusCodes.Status200OK)
            .WithSummary("Listar localizações")
            .WithDescription("Retorna todas as localizações registradas.");

        app.MapGet("/localizacoes/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Localizacoes.FindAsync(id)
                    is Localizacao loc 
                        ? Results.Ok(loc) 
                        : Results.NotFound()
            )
            .WithName("GetLocalizacaoById")
            .WithTags("Localizacoes")
            .Produces<Localizacao>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter localização por ID")
            .WithDescription("Retorna a localização correspondente ao `IdLocalizacao` informado.");

        app.MapPost("/localizacoes", async (Localizacao input, ApplicationDbContext db) =>
                {
                    if (await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    if (await db.Setores.FindAsync(input.IdSetor) is null)
                        return Results.BadRequest($"Setor {input.IdSetor} não encontrado.");
                    db.Localizacoes.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/localizacoes/{input.IdLocalizacao}", input);
                }
            )
            .WithName("CreateLocalizacao")
            .WithTags("Localizacoes")
            .Accepts<Localizacao>("application/json")
            .Produces<Localizacao>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar localização")
            .WithDescription("Registra a localização de uma moto em um setor.");

        app.MapPut("/localizacoes/{id:int}", async (int id, Localizacao input, ApplicationDbContext db) =>
                {
                    var loc = await db.Localizacoes.FindAsync(id);
                    if (loc is null) return Results.NotFound();
                    if (loc.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    if (loc.IdSetor != input.IdSetor && await db.Setores.FindAsync(input.IdSetor) is null)
                        return Results.BadRequest($"Setor {input.IdSetor} não encontrado.");
                    loc.Latitude = input.Latitude;
                    loc.Longitude = input.Longitude;
                    loc.IdMoto = input.IdMoto;
                    loc.IdSetor = input.IdSetor;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateLocalizacao")
            .WithTags("Localizacoes")
            .Accepts<Localizacao>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar localização")
            .WithDescription("Atualiza dados de uma localização existente.");

        app.MapDelete("/localizacoes/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var loc = await db.Localizacoes.FindAsync(id);
                    if (loc is null) return Results.NotFound();
                    db.Localizacoes.Remove(loc);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteLocalizacao")
            .WithTags("Localizacoes")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir localização")
            .WithDescription("Remove a localização especificada.");

        return app;
    }
}
