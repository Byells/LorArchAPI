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
        const String Historicos = "Historicos";
        app.MapGet("/historicos", GetAllHistoricos)
            .WithName("GetHistoricos")
            .WithTags(Historicos)
            .Produces<List<HistoricoManutencao>>(StatusCodes.Status200OK)
            .WithSummary("Listar histórico de movimentações")
            .WithDescription("Retorna todas as movimentações de motos entre setores.");

        app.MapGet("/historicos/{id:int}", GetHistoricoById)
            .WithName("GetHistoricoById")
            .WithTags(Historicos)
            .Produces<HistoricoManutencao>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter movimentação por ID")
            .WithDescription("Retorna a movimentação correspondente ao IdMovimentacao informado.");

        app.MapPost("/historicos", CreateHistorico)
            .WithName("CreateHistorico")
            .WithTags(Historicos)
            .Accepts<HistoricoManutencao>("application/json")
            .Produces<HistoricoManutencao>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar movimentação")
            .WithDescription("Registra a movimentação de uma moto entre setores.");

        app.MapPut("/historicos/{id:int}", UpdateHistorico)
            .WithName("UpdateHistorico")
            .WithTags(Historicos)
            .Accepts<HistoricoManutencao>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar movimentação")
            .WithDescription("Atualiza dados de uma movimentação existente.");

        app.MapDelete("/historicos/{id:int}", DeleteHistorico)
            .WithName("DeleteHistorico")
            .WithTags(Historicos)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir movimentação")
            .WithDescription("Remove a movimentação especificada.");

        return app;
    }

    private static async Task<IResult> GetAllHistoricos(ApplicationDbContext db)
    {
        return Results.Ok(await db.HistoricoManutencoes.ToListAsync());
    }

    private static async Task<IResult> GetHistoricoById(int id, ApplicationDbContext db)
    {
        var historico = await db.HistoricoManutencoes.FindAsync(id);
        return historico is not null ? Results.Ok(historico) : Results.NotFound();
    }

    private static async Task<IResult> CreateHistorico(HistoricoManutencao input, ApplicationDbContext db)
    {
        var validationResult = await ValidateHistoricoInput(input, db);
        if (validationResult is not null)
            return validationResult;

        db.HistoricoManutencoes.Add(input);
        await db.SaveChangesAsync();
        return Results.Created($"/historicos/{input.IdMovimentacao}", input);
    }

    private static async Task<IResult> UpdateHistorico(int id, HistoricoManutencao input, ApplicationDbContext db)
    {
        var existingHistorico = await db.HistoricoManutencoes.FindAsync(id);
        if (existingHistorico is null)
            return Results.NotFound();

        var validationResult = await ValidateHistoricoUpdate(existingHistorico, input, db);
        if (validationResult is not null)
            return validationResult;

        UpdateHistoricoProperties(existingHistorico, input);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteHistorico(int id, ApplicationDbContext db)
    {
        var historico = await db.HistoricoManutencoes.FindAsync(id);
        if (historico is null)
            return Results.NotFound();

        db.HistoricoManutencoes.Remove(historico);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult?> ValidateHistoricoInput(HistoricoManutencao input, ApplicationDbContext db)
    {
        if (await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");

        if (await db.Setores.FindAsync(input.IdSetorOrigem) is null)
            return Results.BadRequest($"SetorOrigem {input.IdSetorOrigem} não encontrado.");

        if (await db.Setores.FindAsync(input.IdSetorDestino) is null)
            return Results.BadRequest($"SetorDestino {input.IdSetorDestino} não encontrado.");

        return null;
    }

    private static async Task<IResult?> ValidateHistoricoUpdate(HistoricoManutencao existing, HistoricoManutencao input, ApplicationDbContext db)
    {
        if (existing.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");

        if (existing.IdSetorOrigem != input.IdSetorOrigem && await db.Setores.FindAsync(input.IdSetorOrigem) is null)
            return Results.BadRequest($"SetorOrigem {input.IdSetorOrigem} não encontrado.");

        if (existing.IdSetorDestino != input.IdSetorDestino && await db.Setores.FindAsync(input.IdSetorDestino) is null)
            return Results.BadRequest($"SetorDestino {input.IdSetorDestino} não encontrado.");

        return null;
    }

    private static void UpdateHistoricoProperties(HistoricoManutencao existing, HistoricoManutencao input)
    {
        existing.IdMoto = input.IdMoto;
        existing.IdSetorOrigem = input.IdSetorOrigem;
        existing.IdSetorDestino = input.IdSetorDestino;
        existing.DataMovimento = input.DataMovimento;
    }
}