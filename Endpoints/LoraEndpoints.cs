using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class LoraEndpoints
{
    public static WebApplication MapLoraEndpoints(this WebApplication app)
    {
        app.MapGet("/lora", async (ApplicationDbContext db) =>
                Results.Ok(await db.Loras.ToListAsync())
            )
            .WithName("GetLoras")
            .WithTags("Lora")
            .Produces<List<Lora>>(StatusCodes.Status200OK)
            .WithSummary("Listar LoRa")
            .WithDescription("Retorna todos os registros de LoRa.");

        app.MapGet("/lora/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.Loras.FindAsync(id)
                    is Lora l 
                        ? Results.Ok(l) 
                        : Results.NotFound()
            )
            .WithName("GetLoraById")
            .WithTags("Lora")
            .Produces<Lora>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter LoRa por ID")
            .WithDescription("Retorna o registro de LoRa correspondente ao `IdLora` informado.");

        app.MapPost("/lora", async (Lora input, ApplicationDbContext db) =>
                {
                    if (await db.Motos.FindAsync(input.Moto) is null)
                        return Results.BadRequest($"Moto {input.Moto} não encontrada.");
                    db.Loras.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/lora/{input.IdLora}", input);
                }
            )
            .WithName("CreateLora")
            .WithTags("Lora")
            .Accepts<Lora>("application/json")
            .Produces<Lora>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar LoRa")
            .WithDescription("Registra um novo dispositivo LoRa vinculado a uma moto.");

        app.MapPut("/lora/{id:int}", async (int id, Lora input, ApplicationDbContext db) =>
                {
                    var l = await db.Loras.FindAsync(id);
                    if (l is null) return Results.NotFound();
                    if (l.Moto != input.Moto && await db.Motos.FindAsync(input.Moto) is null)
                        return Results.BadRequest($"Moto {input.Moto} não encontrada.");
                    l.NumeroLora = input.NumeroLora;
                    l.Moto = input.Moto;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateLora")
            .WithTags("Lora")
            .Accepts<Lora>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar LoRa")
            .WithDescription("Atualiza dados de um dispositivo LoRa existente.");

        app.MapDelete("/lora/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var l = await db.Loras.FindAsync(id);
                    if (l is null) return Results.NotFound();
                    db.Loras.Remove(l);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteLora")
            .WithTags("Lora")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir LoRa")
            .WithDescription("Remove o dispositivo LoRa especificado.");

        return app;
    }
}
