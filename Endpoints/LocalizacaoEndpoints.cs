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
        const string Localizacoes = "Localizacoes";
        app.MapGet("/localizacoes", GetAllLocalizacoes)
            .WithName("GetLocalizacoes")
            .WithTags(Localizacoes)
            .Produces<List<Localizacao>>(StatusCodes.Status200OK)
            .WithSummary("Listar localizações")
            .WithDescription("Retorna todas as localizações registradas.");

        app.MapGet("/localizacoes/{id:int}", GetLocalizacaoById)
            .WithName("GetLocalizacaoById")
            .WithTags(Localizacoes)
            .Produces<Localizacao>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter localização por ID")
            .WithDescription("Retorna a localização correspondente ao IdLocalizacao informado.");

        app.MapPost("/localizacoes", CreateLocalizacao)
            .WithName("CreateLocalizacao")
            .WithTags(Localizacoes)
            .Accepts<Localizacao>("application/json")
            .Produces<Localizacao>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar localização")
            .WithDescription("Registra a localização de uma moto em um setor.");

        app.MapPut("/localizacoes/{id:int}", UpdateLocalizacao)
            .WithName("UpdateLocalizacao")
            .WithTags(Localizacoes)
            .Accepts<Localizacao>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar localização")
            .WithDescription("Atualiza dados de uma localização existente.");

        app.MapDelete("/localizacoes/{id:int}", DeleteLocalizacao)
            .WithName("DeleteLocalizacao")
            .WithTags(Localizacoes)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir localização")
            .WithDescription("Remove a localização especificada.");

        return app;
    }

    private static async Task<IResult> GetAllLocalizacoes(ApplicationDbContext db)
    {
        return Results.Ok(await db.Localizacoes.ToListAsync());
    }

    private static async Task<IResult> GetLocalizacaoById(int id, ApplicationDbContext db)
    {
        var localizacao = await db.Localizacoes.FindAsync(id);
        return localizacao is not null ? Results.Ok(localizacao) : Results.NotFound();
    }

    private static async Task<IResult> CreateLocalizacao(Localizacao input, ApplicationDbContext db)
    {
        var validationResult = await ValidateLocalizacaoInput(input, db);
        if (validationResult is not null)
            return validationResult;

        db.Localizacoes.Add(input);
        await db.SaveChangesAsync();
        return Results.Created($"/localizacoes/{input.IdLocalizacao}", input);
    }

    private static async Task<IResult> UpdateLocalizacao(int id, Localizacao input, ApplicationDbContext db)
    {
        var existingLocalizacao = await db.Localizacoes.FindAsync(id);
        if (existingLocalizacao is null)
            return Results.NotFound();

        var validationResult = await ValidateLocalizacaoUpdate(existingLocalizacao, input, db);
        if (validationResult is not null)
            return validationResult;

        UpdateLocalizacaoProperties(existingLocalizacao, input);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteLocalizacao(int id, ApplicationDbContext db)
    {
        var localizacao = await db.Localizacoes.FindAsync(id);
        if (localizacao is null)
            return Results.NotFound();

        db.Localizacoes.Remove(localizacao);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult?> ValidateLocalizacaoInput(Localizacao input, ApplicationDbContext db)
    {
        if (await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");

        if (await db.Setores.FindAsync(input.IdSetor) is null)
            return Results.BadRequest($"Setor {input.IdSetor} não encontrado.");

        return null;
    }

    private static async Task<IResult?> ValidateLocalizacaoUpdate(Localizacao existing, Localizacao input, ApplicationDbContext db)
    {
        if (existing.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");

        if (existing.IdSetor != input.IdSetor && await db.Setores.FindAsync(input.IdSetor) is null)
            return Results.BadRequest($"Setor {input.IdSetor} não encontrado.");

        return null;
    }

    private static void UpdateLocalizacaoProperties(Localizacao existing, Localizacao input)
    {
        existing.Latitude = input.Latitude;
        existing.Longitude = input.Longitude;
        existing.IdMoto = input.IdMoto;
        existing.IdSetor = input.IdSetor;
    }
}