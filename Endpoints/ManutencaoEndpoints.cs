using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class ManutencaoEndpoints
{
    public static WebApplication MapManutencaoEndpoints(this WebApplication app)
    {
        app.MapGet("/manutencoes", async (ApplicationDbContext db) =>
                Results.Ok(await db.Manutencoes.ToListAsync())
            )
            .WithName("GetManutencoes")
            .WithTags("Manutencoes")
            .Produces<List<Manutencao>>(StatusCodes.Status200OK)
            .WithSummary("Listar manutenções")
            .WithDescription("Retorna todas as manutenções registradas.");

        app.MapGet("/manutencoes/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Manutencoes.FindAsync(id)
                    is Manutencao m 
                        ? Results.Ok(m) 
                        : Results.NotFound()
            )
            .WithName("GetManutencaoById")
            .WithTags("Manutencoes")
            .Produces<Manutencao>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter manutenção por ID")
            .WithDescription("Retorna a manutenção correspondente ao `IdManutencao` informado.");

        app.MapPost("/manutencoes", async (Manutencao input, ApplicationDbContext db) =>
                {
                    if (await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    db.Manutencoes.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/manutencoes/{input.IdManutencao}", input);
                }
            )
            .WithName("CreateManutencao")
            .WithTags("Manutencoes")
            .Accepts<Manutencao>("application/json")
            .Produces<Manutencao>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar manutenção")
            .WithDescription("Cria um novo registro de manutenção para uma moto.");

        app.MapPut("/manutencoes/{id:int}", async (int id, Manutencao input, ApplicationDbContext db) =>
                {
                    var m = await db.Manutencoes.FindAsync(id);
                    if (m is null) return Results.NotFound();
                    if (m.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    m.Descricao = input.Descricao;
                    m.DataManutencao = input.DataManutencao;
                    m.CustoEstimado = input.CustoEstimado;
                    m.Tipo = input.Tipo;
                    m.IdMoto = input.IdMoto;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateManutencao")
            .WithTags("Manutencoes")
            .Accepts<Manutencao>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar manutenção")
            .WithDescription("Atualiza um registro de manutenção existente.");

        app.MapDelete("/manutencoes/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var m = await db.Manutencoes.FindAsync(id);
                    if (m is null) return Results.NotFound();
                    db.Manutencoes.Remove(m);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteManutencao")
            .WithTags("Manutencoes")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir manutenção")
            .WithDescription("Remove o registro de manutenção especificado.");

        return app;
    }
}
