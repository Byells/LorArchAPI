using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class SetorEndpoints
{
    public static WebApplication MapSetorEndpoints(this WebApplication app)
    {
        app.MapGet("/setores", async (int? unidadeId, ApplicationDbContext db) =>
                {
                    var q = db.Setores.AsQueryable();
                    if (unidadeId.HasValue) q = q.Where(s => s.IdUnidade == unidadeId.Value);
                    return Results.Ok(await q.ToListAsync());
                }
            )
            .WithName("GetSetores")
            .WithTags("Setores")
            .Produces<List<Setor>>(StatusCodes.Status200OK)
            .WithSummary("Listar setores")
            .WithDescription("Retorna todos os setores, opcionalmente filtrados por `unidadeId`.");

        app.MapGet("/setores/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Setores.FindAsync(id)
                    is Setor s 
                        ? Results.Ok(s) 
                        : Results.NotFound()
            )
            .WithName("GetSetorById")
            .WithTags("Setores")
            .Produces<Setor>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter setor por ID")
            .WithDescription("Retorna o setor correspondente ao `IdSetor` informado.");

        app.MapPost("/setores", async (Setor input, ApplicationDbContext db) =>
                {
                    if (await db.Unidades.FindAsync(input.IdUnidade) is null)
                        return Results.BadRequest($"Unidade {input.IdUnidade} não encontrada.");
                    db.Setores.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/setores/{input.IdSetor}", input);
                }
            )
            .WithName("CreateSetor")
            .WithTags("Setores")
            .Accepts<Setor>("application/json")
            .Produces<Setor>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar setor")
            .WithDescription("Cria um novo setor vinculado a uma unidade existente.");

        app.MapPut("/setores/{id:int}", async (int id, Setor input, ApplicationDbContext db) =>
                {
                    var s = await db.Setores.FindAsync(id);
                    if (s is null) return Results.NotFound();
                    if (s.IdUnidade != input.IdUnidade && await db.Unidades.FindAsync(input.IdUnidade) is null)
                        return Results.BadRequest($"Unidade {input.IdUnidade} não encontrada.");
                    s.Nome = input.Nome;
                    s.IdUnidade = input.IdUnidade;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateSetor")
            .WithTags("Setores")
            .Accepts<Setor>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar setor")
            .WithDescription("Atualiza o nome ou unidade de um setor existente.");

        app.MapDelete("/setores/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var s = await db.Setores.FindAsync(id);
                    if (s is null) return Results.NotFound();
                    db.Setores.Remove(s);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteSetor")
            .WithTags("Setores")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir setor")
            .WithDescription("Remove o setor especificado.");

        return app;
    }
}
