using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class MotoEndpoints
{
    public static WebApplication MapMotoEndpoints(this WebApplication app)
    {
        app.MapGet("/motos", async (ApplicationDbContext db) =>
                await db.Motos.ToListAsync()
            )
            .WithName("GetAllMotos")
            .WithTags("Motos")
            .Produces<List<Moto>>(StatusCodes.Status200OK)
            .WithSummary("Listar todas as motos")
            .WithDescription("Retorna uma lista de todas as motos cadastradas.");

        app.MapGet("/motos/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Motos.FindAsync(id) is Moto m 
                    ? Results.Ok(m) 
                    : Results.NotFound()
            )
            .WithName("GetMotoById")
            .WithTags("Motos")
            .Produces<Moto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter moto por ID")
            .WithDescription("Retorna a moto correspondente ao `IdMoto` informado.");

        app.MapPost("/motos", async (Moto input, ApplicationDbContext db) =>
            {
                if (await db.Setores.FindAsync(input.IdSetor) is null)
                    return Results.BadRequest($"Setor {input.IdSetor} not found.");
                db.Motos.Add(input);
                await db.SaveChangesAsync();
                return Results.Created($"/motos/{input.IdMoto}", input);
            }
        )
        .WithName("CreateMoto")
        .WithTags("Motos")
        .Accepts<Moto>("application/json")
        .Produces<Moto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithSummary("Criar nova moto")
        .WithDescription("Cria uma nova moto com os campos `Modelo`, `Placa`, `Status` e `IdSetor`.");

        app.MapPut("/motos/{id:int}", async (int id, Moto input, ApplicationDbContext db) =>
            {
                var m = await db.Motos.FindAsync(id);
                if (m is null) return Results.NotFound();
                if (m.IdSetor != input.IdSetor && await db.Setores.FindAsync(input.IdSetor) is null)
                    return Results.BadRequest($"Setor {input.IdSetor} not found.");
                m.Modelo = input.Modelo;
                m.Placa = input.Placa;
                m.Status = input.Status;
                m.IdSetor = input.IdSetor;
                m.DataAtualizacao = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return Results.NoContent();
            }
        )
        .WithName("UpdateMoto")
        .WithTags("Motos")
        .Accepts<Moto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .WithSummary("Atualizar moto")
        .WithDescription("Atualiza os campos `Modelo`, `Placa`, `Status` e `IdSetor` de uma moto existente.");

        app.MapDelete("/motos/{id:int}", async (int id, ApplicationDbContext db) =>
            {
                var m = await db.Motos.FindAsync(id);
                if (m is null) return Results.NotFound();
                db.Motos.Remove(m);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }
        )
        .WithName("DeleteMoto")
        .WithTags("Motos")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithSummary("Excluir moto")
        .WithDescription("Remove a moto correspondente ao `IdMoto` informado.");

        return app;
    }
}
