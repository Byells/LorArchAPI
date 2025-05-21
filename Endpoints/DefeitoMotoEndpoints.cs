using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;

namespace LorArchApi.Endpoints;

public static class DefeitoMotoEndpoints
{
    public static WebApplication MapDefeitoMotoEndpoints(this WebApplication app)
    {
        app.MapGet("/defeitos-moto", async (ApplicationDbContext db) =>
                Results.Ok(await db.DefeitoMotos.ToListAsync())
            )
            .WithName("GetDefeitosMoto")
            .WithTags("DefeitosMoto")
            .Produces<List<DefeitoMoto>>(StatusCodes.Status200OK)
            .WithSummary("Listar defeitos de moto")
            .WithDescription("Retorna todos os registros de defeito atribuídos a motos.");

        app.MapGet("/defeitos-moto/{id:int}", async (int id, ApplicationDbContext db) =>
                await db.DefeitoMotos.FindAsync(id)
                    is DefeitoMoto dm 
                        ? Results.Ok(dm) 
                        : Results.NotFound()
            )
            .WithName("GetDefeitoMotoById")
            .WithTags("DefeitosMoto")
            .Produces<DefeitoMoto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter defeito de moto por ID")
            .WithDescription("Retorna o registro de defeito de moto correspondente ao `IdDefeitoMoto` informado.");

        app.MapPost("/defeitos-moto", async (DefeitoMoto input, ApplicationDbContext db) =>
                {
                    if (await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    if (await db.Defeitos.FindAsync(input.IdDefeito) is null)
                        return Results.BadRequest($"Defeito {input.IdDefeito} não encontrado.");
                    db.DefeitoMotos.Add(input);
                    await db.SaveChangesAsync();
                    return Results.Created($"/defeitos-moto/{input.IdDefeitoMoto}", input);
                }
            )
            .WithName("CreateDefeitoMoto")
            .WithTags("DefeitosMoto")
            .Accepts<DefeitoMoto>("application/json")
            .Produces<DefeitoMoto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar defeito de moto")
            .WithDescription("Cria um novo relacionamento de defeito para uma moto.");

        app.MapPut("/defeitos-moto/{id:int}", async (int id, DefeitoMoto input, ApplicationDbContext db) =>
                {
                    var dm = await db.DefeitoMotos.FindAsync(id);
                    if (dm is null) return Results.NotFound();
                    if (dm.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
                        return Results.BadRequest($"Moto {input.IdMoto} não encontrada.");
                    if (dm.IdDefeito != input.IdDefeito && await db.Defeitos.FindAsync(input.IdDefeito) is null)
                        return Results.BadRequest($"Defeito {input.IdDefeito} não encontrado.");
                    dm.IdMoto = input.IdMoto;
                    dm.IdDefeito = input.IdDefeito;
                    dm.DataRegistro = input.DataRegistro;
                    dm.DataAtualizacao = input.DataAtualizacao;
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("UpdateDefeitoMoto")
            .WithTags("DefeitosMoto")
            .Accepts<DefeitoMoto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar defeito de moto")
            .WithDescription("Atualiza um registro de defeito de moto existente.");

        app.MapDelete("/defeitos-moto/{id:int}", async (int id, ApplicationDbContext db) =>
                {
                    var dm = await db.DefeitoMotos.FindAsync(id);
                    if (dm is null) return Results.NotFound();
                    db.DefeitoMotos.Remove(dm);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
            )
            .WithName("DeleteDefeitoMoto")
            .WithTags("DefeitosMoto")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir defeito de moto")
            .WithDescription("Remove o registro de defeito de moto especificado.");

        return app;
    }
}
