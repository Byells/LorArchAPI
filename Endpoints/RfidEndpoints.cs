using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class RfidEndpoints
{
    public static WebApplication MapRfidEndpoints(this WebApplication app)
    {
        app.MapGet("/rfid", async (ApplicationDbContext db) =>
                Results.Ok(await db.Rfids.ToListAsync())
            )
            .WithName("GetRfids")
            .WithTags("Rfid")
            .Produces<List<Rfid>>(StatusCodes.Status200OK)
            .WithSummary("Listar RFID")
            .WithDescription("Retorna todos os registros RFID.");

        app.MapGet("/rfid/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Rfids.FindAsync(id)
                    is Rfid r 
                        ? Results.Ok(r) 
                        : Results.NotFound()
            )
            .WithName("GetRfidById")
            .WithTags("Rfid")
            .Produces<Rfid>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter RFID por ID")
            .WithDescription("Retorna o registro RFID correspondente ao `IdRfid` informado.");

        app.MapPost("/rfid", async (Rfid input, ApplicationDbContext db) =>
                {
                    if (await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    db.Rfids.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/rfid/{input.IdRfid}", input);
                }
            )
            .WithName("CreateRfid")
            .WithTags("Rfid")
            .Accepts<Rfid>("application/json")
            .Produces<Rfid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar RFID")
            .WithDescription("Registra um novo tag RFID vinculado a uma moto.");

        app.MapPut("/rfid/{id:int}", async (int id, Rfid input, ApplicationDbContext db) =>
                {
                    var r = await db.Rfids.FindAsync(id);
                    if (r is null) return Results.NotFound();
                    if (r.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    r.NumeroRfid = input.NumeroRfid;
                    r.IdMoto = input.IdMoto;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateRfid")
            .WithTags("Rfid")
            .Accepts<Rfid>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar RFID")
            .WithDescription("Atualiza dados de um tag RFID existente.");

        app.MapDelete("/rfid/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var r = await db.Rfids.FindAsync(id);
                    if (r is null) return Results.NotFound();
                    db.Rfids.Remove(r);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteRfid")
            .WithTags("Rfid")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir RFID")
            .WithDescription("Remove o tag RFID especificado.");

        return app;
    }
}
