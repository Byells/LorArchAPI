using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class UnidadeEndpoints
{
    public static WebApplication MapUnidadeEndpoints(this WebApplication app)
    {
        app.MapGet("/unidades", async (int? cidade, string? nome, ApplicationDbContext db) =>
                {
                    var q = db.Unidades.AsQueryable();
                    if (cidade.HasValue) q = q.Where(u => u.IdCidade == cidade.Value);
                    if (!string.IsNullOrWhiteSpace(nome)) q = q.Where(u => u.Nome.Contains(nome));
                    return Results.Ok(await q.ToListAsync());
                }
            )
            .WithName("GetUnidades")
            .WithTags("Unidades")
            .Produces<List<Unidade>>(StatusCodes.Status200OK)
            .WithSummary("Listar unidades")
            .WithDescription("Retorna todas as unidades, opcionalmente filtradas por `cidade` ou por `nome`.");

        app.MapGet("/unidades/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Unidades.FindAsync(id)
                    is Unidade u 
                        ? Results.Ok(u) 
                        : Results.NotFound()
            )
            .WithName("GetUnidadeById")
            .WithTags("Unidades")
            .Produces<Unidade>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter unidade por ID")
            .WithDescription("Retorna a unidade correspondente ao `IdUnidade` informado.");

        app.MapPost("/unidades", async (Unidade input, ApplicationDbContext db) =>
                {
                    if (await db.Cidades.FindAsync(input.IdCidade) is null)
                        return Results.BadRequest($"Cidade {input.IdCidade} não encontrada.");
                    db.Unidades.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/unidades/{input.IdUnidade}", input);
                }
            )
            .WithName("CreateUnidade")
            .WithTags("Unidades")
            .Accepts<Unidade>("application/json")
            .Produces<Unidade>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar unidade")
            .WithDescription("Cria uma nova unidade vinculada a uma cidade existente.");

        app.MapPut("/unidades/{id:int}", async (int id, Unidade input, ApplicationDbContext db) =>
                {
                    var u = await db.Unidades.FindAsync(id);
                    if (u is null) return Results.NotFound();
                    if (u.IdCidade != input.IdCidade && await db.Cidades.FindAsync(input.IdCidade) is null)
                        return Results.BadRequest($"Cidade {input.IdCidade} não encontrada.");
                    u.Nome = input.Nome;
                    u.IdCidade = input.IdCidade;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateUnidade")
            .WithTags("Unidades")
            .Accepts<Unidade>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar unidade")
            .WithDescription("Atualiza o nome ou cidade de uma unidade existente.");

        app.MapDelete("/unidades/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var u = await db.Unidades.FindAsync(id);
                    if (u is null) return Results.NotFound();
                    db.Unidades.Remove(u);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteUnidade")
            .WithTags("Unidades")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir unidade")
            .WithDescription("Remove a unidade especificada.");

        return app;
    }
}
