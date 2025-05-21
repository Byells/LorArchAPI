using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class HistoricoManutencaoEndpoints
{
    public static WebApplication MapHistoricoManutencaoEndpoints(this WebApplication app)
    {
        app.MapGet("/historicos", async (ApplicationDbContext db) =>
                Results.Ok(await db.HistoricoManutencoes.ToListAsync())
            )
            .WithName("GetHistoricos")
            .WithTags("Historicos")
            .Produces<List<HistoricoManutencao>>(StatusCodes.Status200OK)
            .WithSummary("Listar histórico de movimentações")
            .WithDescription("Retorna todas as movimentações de motos entre setores.");

        app.MapGet("/historicos/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.HistoricoManutencoes.FindAsync(id)
                    is HistoricoManutencao h 
                        ? Results.Ok(h) 
                        : Results.NotFound()
            )
            .WithName("GetHistoricoById")
            .WithTags("Historicos")
            .Produces<HistoricoManutencao>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter movimentação por ID")
            .WithDescription("Retorna a movimentação correspondente ao `IdMovimentacao` informado.");

        app.MapPost("/historicos", async (HistoricoManutencao input, ApplicationDbContext db) =>
                {
                    if (await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    if (await db.Setores.FindAsync(input.IdSetorOrigem) is null)
                        return Results.BadRequest($"SetorOrigem {input.IdSetorOrigem} não encontrado.");
                    if (await db.Setores.FindAsync(input.IdSetorDestino) is null)
                        return Results.BadRequest($"SetorDestino {input.IdSetorDestino} não encontrado.");
                    db.HistoricoManutencoes.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/historicos/{input.IdMovimentacao}", input);
                }
            )
            .WithName("CreateHistorico")
            .WithTags("Historicos")
            .Accepts<HistoricoManutencao>("application/json")
            .Produces<HistoricoManutencao>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar movimentação")
            .WithDescription("Registra a movimentação de uma moto entre setores.");

        app.MapPut("/historicos/{id:int}", async (int id, HistoricoManutencao input, ApplicationDbContext db) =>
                {
                    var h = await db.HistoricoManutencoes.FindAsync(id);
                    if (h is null) return Results.NotFound();
                    if (h.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    if (h.IdSetorOrigem != input.IdSetorOrigem && await db.Setores.FindAsync(input.IdSetorOrigem) is null)
                        return Results.BadRequest($"SetorOrigem {input.IdSetorOrigem} não encontrado.");
                    if (h.IdSetorDestino != input.IdSetorDestino && await db.Setores.FindAsync(input.IdSetorDestino) is null)
                        return Results.BadRequest($"SetorDestino {input.IdSetorDestino} não encontrado.");
                    h.IdMoto = input.IdMoto;
                    h.IdSetorOrigem = input.IdSetorOrigem;
                    h.IdSetorDestino = input.IdSetorDestino;
                    h.DataMovimento = input.DataMovimento;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateHistorico")
            .WithTags("Historicos")
            .Accepts<HistoricoManutencao>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar movimentação")
            .WithDescription("Atualiza dados de uma movimentação existente.");

        app.MapDelete("/historicos/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var h = await db.HistoricoManutencoes.FindAsync(id);
                    if (h is null) return Results.NotFound();
                    db.HistoricoManutencoes.Remove(h);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteHistorico")
            .WithTags("Historicos")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir movimentação")
            .WithDescription("Remove a movimentação especificada.");

        return app;
    }
}
