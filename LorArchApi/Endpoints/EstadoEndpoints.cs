using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class EstadoEndpoints
{
    public static WebApplication MapEstadoEndpoints(this WebApplication app)
    {
        const string EstadosTag = "Estados";
        
        app.MapGet("/estados", GetEstados)
            .WithName("GetEstados")
            .WithTags(EstadosTag)
            .Produces<PaginatedResponse<EstadoDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar estados")
            .WithDescription("Retorna estados paginados, opcionalmente filtrados por `sigla`.")
            .RequireAuthorization();

        app.MapGet("/estados/{id:int}", GetEstadoById)
            .WithName("GetEstadoById")
            .WithTags(EstadosTag)
            .Produces<EstadoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter estado por ID")
            .WithDescription("Retorna o estado correspondente ao `IdEstado` informado.")
            .RequireAuthorization();

        app.MapPost("/estados", CreateEstado)
            .WithName("CreateEstado")
            .WithTags(EstadosTag)
            .Accepts<Estado>("application/json")
            .Produces<EstadoDto>(StatusCodes.Status201Created)
            .WithSummary("Criar estado")
            .WithDescription("Cria um novo estado.")
            .RequireAuthorization();

        app.MapPut("/estados/{id:int}", UpdateEstado)
            .WithName("UpdateEstado")
            .WithTags(EstadosTag)
            .Accepts<Estado>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar estado")
            .WithDescription("Atualiza nome e sigla de um estado existente.")
            .RequireAuthorization();

        app.MapDelete("/estados/{id:int}", DeleteEstado)
            .WithName("DeleteEstado")
            .WithTags(EstadosTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir estado")
            .WithDescription("Remove o estado especificado.")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetEstados(
        ApplicationDbContext db,
        string? sigla,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Estados.AsQueryable();

        if (!string.IsNullOrWhiteSpace(sigla))
            query = query.Where(e => e.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var estados = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = estados.Select(e =>
        {
            var dto = ToDto(e);
            dto.Links.RemoveAll(l => l.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<EstadoDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, sigla)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetEstadoById(int id, ApplicationDbContext db)
    {
        var estado = await db.Estados.FindAsync(id);
        
        return estado is null 
            ? Results.NotFound() 
            : Results.Ok(ToDto(estado));
    }

    private static async Task<IResult> CreateEstado(Estado input, ApplicationDbContext db)
    {
        db.Estados.Add(input);
        await db.SaveChangesAsync();
        
        var dto = ToDto(input);

        return Results.Created($"/estados/{input.IdEstado}", dto);
    }

    private static async Task<IResult> UpdateEstado(int id, Estado input, ApplicationDbContext db)
    {
        var estado = await db.Estados.FindAsync(id);
        if (estado is null)
        {
            return Results.NotFound();
        }

        estado.Nome = input.Nome;
        estado.Sigla = input.Sigla;
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteEstado(int id, ApplicationDbContext db)
    {
        var estado = await db.Estados.FindAsync(id);
        if (estado is null)
        {
            return Results.NotFound();
        }

        db.Estados.Remove(estado);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static EstadoDto ToDto(Estado estado) => new()
    {
        IdEstado = estado.IdEstado,
        Nome = estado.Nome,
        Sigla = estado.Sigla,
        Links = new List<Link>
        {
            new("self", $"/estados/{estado.IdEstado}", "GET"),
            new("update", $"/estados/{estado.IdEstado}", "PUT"),
            new("delete", $"/estados/{estado.IdEstado}", "DELETE"),
            new("all", "/estados", "GET")
        }
    };

    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, string? sigla)
    {
        var links = new List<Link>();
        var baseUrl = "/estados";

        var filter = !string.IsNullOrWhiteSpace(sigla) 
            ? $"&sigla={Uri.EscapeDataString(sigla)}" 
            : string.Empty;

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


public class EstadoDto
{
    public int IdEstado { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public List<Link> Links { get; set; } = new();
}
