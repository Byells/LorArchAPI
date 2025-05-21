using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class DefeitoEndpoints
{
    public static WebApplication MapDefeitoEndpoints(this WebApplication app)
    {
        app.MapGet("/defeitos", async (string? nome, ApplicationDbContext db) =>
                {
                    var q = db.Defeitos.AsQueryable();
                    if (!string.IsNullOrWhiteSpace(nome))
                        q = q.Where(d => d.Nome.Contains(nome));
                    return Results.Ok(await q.ToListAsync());
                }
            )
            .WithName("GetDefeitos")
            .WithTags("Defeitos")
            .Produces<List<Defeito>>(StatusCodes.Status200OK)
            .WithSummary("Listar defeitos")
            .WithDescription("Retorna todos os defeitos, opcionalmente filtrados por `nome`.");

        app.MapGet("/defeitos/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Defeitos.FindAsync(id)
                    is Defeito d 
                        ? Results.Ok(d) 
                        : Results.NotFound()
            )
            .WithName("GetDefeitoById")
            .WithTags("Defeitos")
            .Produces<Defeito>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter defeito por ID")
            .WithDescription("Retorna o defeito correspondente ao `IdDefeito` informado.");

        app.MapPost("/defeitos", async (Defeito input, ApplicationDbContext db) =>
                {
                    db.Defeitos.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/defeitos/{input.IdDefeito}", input);
                }
            )
            .WithName("CreateDefeito")
            .WithTags("Defeitos")
            .Accepts<Defeito>("application/json")
            .Produces<Defeito>(StatusCodes.Status201Created)
            .WithSummary("Criar defeito")
            .WithDescription("Cria um novo defeito.");

        app.MapPut("/defeitos/{id:int}", async (int id, Defeito input, ApplicationDbContext db) =>
                {
                    var d = await db.Defeitos.FindAsync(id);
                    if (d is null) return Results.NotFound();
                    d.Nome = input.Nome;
                    d.Descricao = input.Descricao;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateDefeito")
            .WithTags("Defeitos")
            .Accepts<Defeito>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar defeito")
            .WithDescription("Atualiza nome e descrição de um defeito existente.");

        app.MapDelete("/defeitos/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var d = await db.Defeitos.FindAsync(id);
                    if (d is null) return Results.NotFound();
                    db.Defeitos.Remove(d);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteDefeito")
            .WithTags("Defeitos")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir defeito")
            .WithDescription("Remove o defeito especificado.");

        return app;
    }
}
