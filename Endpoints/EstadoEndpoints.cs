using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class EstadoEndpoints
{
    public static WebApplication MapEstadoEndpoints(this WebApplication app)
    {
        app.MapGet("/estados", async (string? sigla, ApplicationDbContext db) =>
                {
                    var q = db.Estados.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(sigla))
                        q = q.Where(e => e.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
                    return Results.Ok(await q.ToListAsync());
                }
            )
            .WithName("GetEstados")
            .WithTags("Estados")
            .Produces<List<Estado>>(StatusCodes.Status200OK)
            .WithSummary("Listar estados")
            .WithDescription("Retorna todos os estados, opcionalmente filtrados por `sigla`.");

        app.MapGet("/estados/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Estados.FindAsync(id)
                    is Estado e 
                        ? Results.Ok(e) 
                        : Results.NotFound()
            )
            .WithName("GetEstadoById")
            .WithTags("Estados")
            .Produces<Estado>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter estado por ID")
            .WithDescription("Retorna o estado correspondente ao `IdEstado` informado.");

        app.MapPost("/estados", async (Estado input, ApplicationDbContext db) =>
                {
                    db.Estados.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/estados/{input.IdEstado}", input);
                }
            )
            .WithName("CreateEstado")
            .WithTags("Estados")
            .Accepts<Estado>("application/json")
            .Produces<Estado>(StatusCodes.Status201Created)
            .WithSummary("Criar estado")
            .WithDescription("Cria um novo estado.");

        app.MapPut("/estados/{id:int}", async (int id, Estado input, ApplicationDbContext db) =>
                {
                    var e = await db.Estados.FindAsync(id);
                    if (e is null) return Results.NotFound();
                    e.Nome = input.Nome;
                    e.Sigla = input.Sigla;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateEstado")
            .WithTags("Estados")
            .Accepts<Estado>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar estado")
            .WithDescription("Atualiza nome e sigla de um estado existente.");

        app.MapDelete("/estados/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var e = await db.Estados.FindAsync(id);
                    if (e is null) return Results.NotFound();
                    db.Estados.Remove(e);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteEstado")
            .WithTags("Estados")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir estado")
            .WithDescription("Remove o estado especificado.");

        return app;
    }
}
