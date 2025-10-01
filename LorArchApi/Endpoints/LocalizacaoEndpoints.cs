using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class LocalizacaoEndpoints
{
    public static WebApplication MapLocalizacaoEndpoints(this WebApplication app)
    {
        const string LocalizacoesTag = "Localizacoes";
        
        app.MapGet("/localizacoes", GetAllLocalizacoes)
            .WithName("GetLocalizacoes")
            .WithTags(LocalizacoesTag)
            .Produces<PaginatedResponse<LocalizacaoDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar localizações")
            .WithDescription("Retorna todas as localizações paginadas, com filtros opcionais.")
            .RequireAuthorization();

        app.MapGet("/localizacoes/{id:int}", GetLocalizacaoById)
            .WithName("GetLocalizacaoById")
            .WithTags(LocalizacoesTag)
            .Produces<LocalizacaoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter localização por ID")
            .WithDescription("Retorna a localização correspondente ao IdLocalizacao informado.")
            .RequireAuthorization();

        app.MapPost("/localizacoes", CreateLocalizacao)
            .WithName("CreateLocalizacao")
            .WithTags(LocalizacoesTag)
            .Accepts<Localizacao>("application/json")
            .Produces<LocalizacaoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar localização")
            .WithDescription("Registra a localização de uma moto em um setor.")
            .RequireAuthorization();

        app.MapPut("/localizacoes/{id:int}", UpdateLocalizacao)
            .WithName("UpdateLocalizacao")
            .WithTags(LocalizacoesTag)
            .Accepts<Localizacao>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar localização")
            .WithDescription("Atualiza dados de uma localização existente.")
            .RequireAuthorization();

        app.MapDelete("/localizacoes/{id:int}", DeleteLocalizacao)
            .WithName("DeleteLocalizacao")
            .WithTags(LocalizacoesTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir localização")
            .WithDescription("Remove a localização especificada.")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetAllLocalizacoes(
        ApplicationDbContext db,
        int? motoId,
        int? setorId,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Localizacoes.AsQueryable();

        if (motoId.HasValue)
            query = query.Where(l => l.IdMoto == motoId.Value);
        if (setorId.HasValue)
            query = query.Where(l => l.IdSetor == setorId.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var localizacoes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        var dtos = localizacoes.Select(l =>
        {
            var dto = ToDto(l);
            dto.Links.RemoveAll(link => link.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<LocalizacaoDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, motoId, setorId)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetLocalizacaoById(int id, ApplicationDbContext db)
    {
        var localizacao = await db.Localizacoes.FindAsync(id);
        
        return localizacao is not null 
            ? Results.Ok(ToDto(localizacao)) 
            : Results.NotFound();
    }

    private static async Task<IResult> CreateLocalizacao(Localizacao input, ApplicationDbContext db)
    {
        if (await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");

        if (await db.Setores.FindAsync(input.IdSetor) is null)
            return Results.BadRequest($"Setor com Id {input.IdSetor} não encontrado.");

        db.Localizacoes.Add(input);
        await db.SaveChangesAsync();
        
        var dto = ToDto(input);
        
        return Results.Created($"/localizacoes/{input.IdLocalizacao}", dto);
    }

    private static async Task<IResult> UpdateLocalizacao(int id, Localizacao input, ApplicationDbContext db)
    {
        var localizacao = await db.Localizacoes.FindAsync(id);
        if (localizacao is null)
        {
            return Results.NotFound();
        }

        if (localizacao.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");

        if (localizacao.IdSetor != input.IdSetor && await db.Setores.FindAsync(input.IdSetor) is null)
            return Results.BadRequest($"Setor com Id {input.IdSetor} não encontrado.");

        localizacao.Latitude = input.Latitude;
        localizacao.Longitude = input.Longitude;
        localizacao.IdMoto = input.IdMoto;
        localizacao.IdSetor = input.IdSetor;
        
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteLocalizacao(int id, ApplicationDbContext db)
    {
        var localizacao = await db.Localizacoes.FindAsync(id);
        if (localizacao is null)
        {
            return Results.NotFound();
        }

        db.Localizacoes.Remove(localizacao);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }
    
    private static LocalizacaoDto ToDto(Localizacao localizacao) => new()
    {
        IdLocalizacao = localizacao.IdLocalizacao,
        Latitude = localizacao.Latitude,
        Longitude = localizacao.Longitude,
        IdMoto = localizacao.IdMoto,
        IdSetor = localizacao.IdSetor,
        Links = new List<Link>
        {
            new("self", $"/localizacoes/{localizacao.IdLocalizacao}", "GET"),
            new("update", $"/localizacoes/{localizacao.IdLocalizacao}", "PUT"),
            new("delete", $"/localizacoes/{localizacao.IdLocalizacao}", "DELETE"),
            new("all", "/localizacoes", "GET")
        }
    };
    
    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, int? motoId, int? setorId)
    {
        var links = new List<Link>();
        var baseUrl = "/localizacoes";
        
        var queryBuilder = new StringBuilder();
        if (motoId.HasValue)
            queryBuilder.Append($"&motoId={motoId.Value}");
        if (setorId.HasValue)
            queryBuilder.Append($"&setorId={setorId.Value}");
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


public class LocalizacaoDto
{
    public int IdLocalizacao { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int IdMoto { get; set; }
    public int IdSetor { get; set; }
    public List<Link> Links { get; set; } = new();
}
