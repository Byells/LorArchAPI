using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class DefeitoEndpoints
{
    public static WebApplication MapDefeitoEndpoints(this WebApplication app)
    {
        const string DefeitosTag = "Defeitos";
        
        app.MapGet("/defeitos", GetDefeitos)
            .WithName("GetDefeitos")
            .WithTags(DefeitosTag)
            .Produces<PaginatedResponse<DefeitoDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar defeitos")
            .WithDescription("Retorna defeitos paginados, opcionalmente filtrados por `nome`.")
            .RequireAuthorization();

        app.MapGet("/defeitos/{id:int}", GetDefeitoById)
            .WithName("GetDefeitoById")
            .WithTags(DefeitosTag)
            .Produces<DefeitoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter defeito por ID")
            .WithDescription("Retorna o defeito correspondente ao `IdDefeito` informado.")
            .RequireAuthorization();

        app.MapPost("/defeitos", CreateDefeito)
            .WithName("CreateDefeito")
            .WithTags(DefeitosTag)
            .Accepts<Defeito>("application/json")
            .Produces<DefeitoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar defeito")
            .WithDescription("Cria um novo defeito.")
            .RequireAuthorization();

        app.MapPut("/defeitos/{id:int}", UpdateDefeito)
            .WithName("UpdateDefeito")
            .WithTags(DefeitosTag)
            .Accepts<Defeito>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar defeito")
            .WithDescription("Atualiza nome e descrição de um defeito existente.")
            .RequireAuthorization();

        app.MapDelete("/defeitos/{id:int}", DeleteDefeito)
            .WithName("DeleteDefeito")
            .WithTags(DefeitosTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir defeito")
            .WithDescription("Remove o defeito especificado.")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetDefeitos(
        ApplicationDbContext db,
        string? nome,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Defeitos.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(d => d.Nome.Contains(nome));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var defeitos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var defeitosDto = defeitos.Select(d =>
        {
            var dto = ToDto(d);
            dto.Links.RemoveAll(l => l.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<DefeitoDto>
        {
            Data = defeitosDto,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, nome)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetDefeitoById(int id, ApplicationDbContext db)
    {
        var defeito = await db.Defeitos.FindAsync(id);
        
        return defeito is null
            ? Results.NotFound()
            : Results.Ok(ToDto(defeito));
    }

    private static async Task<IResult> CreateDefeito(Defeito input, ApplicationDbContext db)
    {
        if (string.IsNullOrWhiteSpace(input.Nome))
        {
            return Results.BadRequest("O nome do defeito é obrigatório.");
        }

        db.Defeitos.Add(input);
        await db.SaveChangesAsync();

        var defeitoDto = ToDto(input);

        return Results.Created($"/defeitos/{input.IdDefeito}", defeitoDto);
    }

    private static async Task<IResult> UpdateDefeito(int id, Defeito input, ApplicationDbContext db)
    {
        var defeito = await db.Defeitos.FindAsync(id);
        if (defeito is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(input.Nome))
        {
            return Results.BadRequest("O nome do defeito é obrigatório.");
        }
        
        defeito.Nome = input.Nome;
        defeito.Descricao = input.Descricao;
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteDefeito(int id, ApplicationDbContext db)
    {
        var defeito = await db.Defeitos.FindAsync(id);
        if (defeito is null)
        {
            return Results.NotFound();
        }
        
        db.Defeitos.Remove(defeito);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static DefeitoDto ToDto(Defeito defeito) => new()
    {
        IdDefeito = defeito.IdDefeito,
        Nome = defeito.Nome,
        Descricao = defeito.Descricao,
        Links = new List<Link>
        {
            new("self", $"/defeitos/{defeito.IdDefeito}", "GET"),
            new("update", $"/defeitos/{defeito.IdDefeito}", "PUT"),
            new("delete", $"/defeitos/{defeito.IdDefeito}", "DELETE"),
            new("all", "/defeitos", "GET")
        }
    };

    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, string? nome)
    {
        var links = new List<Link>();
        var baseUrl = "/defeitos";
        
        var queryBuilder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(nome))
            queryBuilder.Append($"&nome={Uri.EscapeDataString(nome)}");
        
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


public class DefeitoDto
{
    public int IdDefeito { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public List<Link> Links { get; set; } = new();
}

