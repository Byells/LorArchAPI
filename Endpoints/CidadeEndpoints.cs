using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class CidadeEndpoints
{
    public static WebApplication MapCidadeEndpoints(this WebApplication app)
    {
        app.MapGet("/cidades", async (int? estadoId, string? nome, ApplicationDbContext db) =>
                {
                    var q = db.Cidades.AsQueryable();
                    if (estadoId.HasValue) q = q.Where(c => c.IdEstado == estadoId.Value);
                    if (!string.IsNullOrWhiteSpace(nome)) q = q.Where(c => c.Nome.Contains(nome));
                    return Results.Ok(await q.ToListAsync());
                }
            )
            .WithName("GetCidades")
            .WithTags("Cidades")
            .Produces<List<Cidade>>(StatusCodes.Status200OK)
            .WithSummary("Listar cidades")
            .WithDescription("Retorna todas as cidades, opcionalmente filtradas por `estadoId` ou `nome`.");

        app.MapGet("/cidades/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Cidades.FindAsync(id)
                    is Cidade c 
                        ? Results.Ok(c) 
                        : Results.NotFound()
            )
            .WithName("GetCidadeById")
            .WithTags("Cidades")
            .Produces<Cidade>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter cidade por ID")
            .WithDescription("Retorna a cidade correspondente ao `IdCidade` informado.");

        app.MapPost("/cidades", async (Cidade input, ApplicationDbContext db) =>
                {
                    if (await db.Estados.FindAsync(input.IdEstado) is null)
                        return Results.BadRequest($"Estado {input.IdEstado} não encontrado.");
                    db.Cidades.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/cidades/{input.IdCidade}", input);
                }
            )
            .WithName("CreateCidade")
            .WithTags("Cidades")
            .Accepts<Cidade>("application/json")
            .Produces<Cidade>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar cidade")
            .WithDescription("Cria uma nova cidade vinculada a um estado existente.");

        app.MapPut("/cidades/{id:int}", async (int id, Cidade input, ApplicationDbContext db) =>
                {
                    var c = await db.Cidades.FindAsync(id);
                    if (c is null) return Results.NotFound();
                    if (c.IdEstado != input.IdEstado && await db.Estados.FindAsync(input.IdEstado) is null)
                        return Results.BadRequest($"Estado {input.IdEstado} não encontrado.");
                    c.Nome = input.Nome;
                    c.IdEstado = input.IdEstado;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateCidade")
            .WithTags("Cidades")
            .Accepts<Cidade>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar cidade")
            .WithDescription("Atualiza nome ou estado de uma cidade existente.");

        app.MapDelete("/cidades/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var c = await db.Cidades.FindAsync(id);
                    if (c is null) return Results.NotFound();
                    db.Cidades.Remove(c);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteCidade")
            .WithTags("Cidades")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir cidade")
            .WithDescription("Remove a cidade especificada.");

        return app;
    }
}
