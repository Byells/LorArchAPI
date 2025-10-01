using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class DefeitoMotoEndpoints
{
    public static WebApplication MapDefeitoMotoEndpoints(this WebApplication app)
    {
        const string DefeitosMotoTag = "DefeitosMoto";
        
        app.MapGet("/defeitos-moto", GetDefeitosMoto)
            .WithName("GetDefeitosMoto")
            .WithTags(DefeitosMotoTag)
            .Produces<PaginatedResponse<DefeitoMotoDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar defeitos de moto")
            .WithDescription("Retorna registros paginados de defeitos de motos, opcionalmente filtrados por `motoId` ou `defeitoId`.");

        app.MapGet("/defeitos-moto/{id:int}", GetDefeitoMotoById)
            .WithName("GetDefeitoMotoById")
            .WithTags(DefeitosMotoTag)
            .Produces<DefeitoMotoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter defeito de moto por ID")
            .WithDescription("Retorna o registro de defeito de moto correspondente ao `IdDefeitoMoto` informado.");

        app.MapPost("/defeitos-moto", CreateDefeitoMoto)
            .WithName("CreateDefeitoMoto")
            .WithTags(DefeitosMotoTag)
            .Accepts<DefeitoMoto>("application/json")
            .Produces<DefeitoMotoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar defeito de moto")
            .WithDescription("Cria um novo relacionamento de defeito para uma moto.");

        app.MapPut("/defeitos-moto/{id:int}", UpdateDefeitoMoto)
            .WithName("UpdateDefeitoMoto")
            .WithTags(DefeitosMotoTag)
            .Accepts<DefeitoMoto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar defeito de moto")
            .WithDescription("Atualiza um registro de defeito de moto existente.");

        app.MapDelete("/defeitos-moto/{id:int}", DeleteDefeitoMoto)
            .WithName("DeleteDefeitoMoto")
            .WithTags(DefeitosMotoTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir defeito de moto")
            .WithDescription("Remove o registro de defeito de moto especificado.");

        return app;
    }

    private static async Task<IResult> GetDefeitosMoto(
        ApplicationDbContext db,
        int? motoId,
        int? defeitoId,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.DefeitoMotos.AsQueryable();

        if (motoId.HasValue)
            query = query.Where(dm => dm.IdMoto == motoId.Value);

        if (defeitoId.HasValue)
            query = query.Where(dm => dm.IdDefeito == defeitoId.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var defeitosMoto = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = defeitosMoto.Select(dm =>
        {
            var dto = ToDto(dm);
            dto.Links.RemoveAll(l => l.Rel == "all");
            return dto;
        }).ToList();
        
        var response = new PaginatedResponse<DefeitoMotoDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, motoId, defeitoId)
        };
        
        return Results.Ok(response);
    }

    private static async Task<IResult> GetDefeitoMotoById(int id, ApplicationDbContext db)
    {
        var defeitoMoto = await db.DefeitoMotos.FindAsync(id);
        
        return defeitoMoto is null
            ? Results.NotFound()
            : Results.Ok(ToDto(defeitoMoto));
    }

    private static async Task<IResult> CreateDefeitoMoto(DefeitoMoto input, ApplicationDbContext db)
    {
        if (await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");
        
        if (await db.Defeitos.FindAsync(input.IdDefeito) is null)
            return Results.BadRequest($"Defeito com Id {input.IdDefeito} não encontrado.");
        
        db.DefeitoMotos.Add(input);
        await db.SaveChangesAsync();
        
        var dto = ToDto(input);
        
        return Results.Created($"/defeitos-moto/{input.IdDefeitoMoto}", dto);
    }

    private static async Task<IResult> UpdateDefeitoMoto(int id, DefeitoMoto input, ApplicationDbContext db)
    {
        var defeitoMoto = await db.DefeitoMotos.FindAsync(id);
        if (defeitoMoto is null)
        {
            return Results.NotFound();
        }
        
        if (defeitoMoto.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");
        
        if (defeitoMoto.IdDefeito != input.IdDefeito && await db.Defeitos.FindAsync(input.IdDefeito) is null)
            return Results.BadRequest($"Defeito com Id {input.IdDefeito} não encontrado.");
        
        defeitoMoto.IdMoto = input.IdMoto;
        defeitoMoto.IdDefeito = input.IdDefeito;
        defeitoMoto.DataRegistro = input.DataRegistro;
        defeitoMoto.DataAtualizacao = input.DataAtualizacao;
        
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteDefeitoMoto(int id, ApplicationDbContext db)
    {
        var defeitoMoto = await db.DefeitoMotos.FindAsync(id);
        if (defeitoMoto is null)
        {
            return Results.NotFound();
        }
        
        db.DefeitoMotos.Remove(defeitoMoto);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static DefeitoMotoDto ToDto(DefeitoMoto defeitoMoto) => new()
    {
        IdDefeitoMoto = defeitoMoto.IdDefeitoMoto,
        IdMoto = defeitoMoto.IdMoto,
        IdDefeito = defeitoMoto.IdDefeito,
        DataRegistro = defeitoMoto.DataRegistro,
        DataAtualizacao = defeitoMoto.DataAtualizacao,
        Links = new List<Link>
        {
            new("self", $"/defeitos-moto/{defeitoMoto.IdDefeitoMoto}", "GET"),
            new("update", $"/defeitos-moto/{defeitoMoto.IdDefeitoMoto}", "PUT"),
            new("delete", $"/defeitos-moto/{defeitoMoto.IdDefeitoMoto}", "DELETE"),
            new("all", "/defeitos-moto", "GET")
        }
    };
    
    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, int? motoId, int? defeitoId)
    {
        var links = new List<Link>();
        var baseUrl = "/defeitos-moto";
        
        var queryBuilder = new StringBuilder();
        if (motoId.HasValue)
            queryBuilder.Append($"&motoId={motoId.Value}");
        if (defeitoId.HasValue)
            queryBuilder.Append($"&defeitoId={defeitoId.Value}");
        var filter = queryBuilder.ToString();

        links.Add(new Link("self", $"{baseUrl}?page={currentPage}&pageSize={pageSize}{filter}", "GET"));

        if (currentPage > 1)
        {
            links.Add(new Link("first", $"{baseUrl}?page=1&pageSize={pageSize}{filter}", "GET"));
            links.Add(new Link("previous", $"{baseUrl}?page={currentPage - 1}&pageSize={pageSize}{filter}", "GET"));
        }

        if (currentPage < totalPages)
        {
            links.Add(new Link("next", $"{baseUrl}?page={currentPage + 1}&pageSize={pageSize}{filter}", "GET"));
            links.Add(new Link("last", $"{baseUrl}?page={totalPages}&pageSize={pageSize}{filter}", "GET"));
        }

        return links;
    }
}


public class DefeitoMotoDto
{
    public int IdDefeitoMoto { get; set; }
    public int IdMoto { get; set; }
    public int IdDefeito { get; set; }
    public DateTime DataRegistro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public List<Link> Links { get; set; } = new();
}
