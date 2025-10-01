using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class UnidadeEndpoints
{
    public static WebApplication MapUnidadeEndpoints(this WebApplication app)
    {
        const string UnidadesTag = "Unidades";

        app.MapGet("/unidades", GetAllUnidades)
            .WithName("GetUnidades")
            .WithTags(UnidadesTag)
            .Produces<PaginatedResponse<UnidadeDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar unidades")
            .WithDescription("Retorna todas as unidades paginadas, opcionalmente filtradas por `cidadeId` ou por `nome`.");

        app.MapGet("/unidades/{id:int}", GetUnidadeById)
            .WithName("GetUnidadeById")
            .WithTags(UnidadesTag)
            .Produces<UnidadeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter unidade por ID")
            .WithDescription("Retorna a unidade correspondente ao `IdUnidade` informado.");

        app.MapPost("/unidades", CreateUnidade)
            .WithName("CreateUnidade")
            .WithTags(UnidadesTag)
            .Accepts<Unidade>("application/json")
            .Produces<UnidadeDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar unidade")
            .WithDescription("Cria uma nova unidade vinculada a uma cidade existente.");

        app.MapPut("/unidades/{id:int}", UpdateUnidade)
            .WithName("UpdateUnidade")
            .WithTags(UnidadesTag)
            .Accepts<Unidade>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar unidade")
            .WithDescription("Atualiza o nome ou cidade de uma unidade existente.");

        app.MapDelete("/unidades/{id:int}", DeleteUnidade)
            .WithName("DeleteUnidade")
            .WithTags(UnidadesTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir unidade")
            .WithDescription("Remove a unidade especificada.");

        return app;
    }

    private static async Task<IResult> GetAllUnidades(
        ApplicationDbContext db,
        int? cidadeId,
        string? nome,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Unidades.AsQueryable();

        if (cidadeId.HasValue)
            query = query.Where(u => u.IdCidade == cidadeId.Value);
        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(u => u.Nome.Contains(nome));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var unidades = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = unidades.Select(u =>
        {
            var dto = ToDto(u);
            dto.Links.RemoveAll(link => link.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<UnidadeDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, cidadeId, nome)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetUnidadeById(int id, ApplicationDbContext db)
    {
        var unidade = await db.Unidades.FindAsync(id);
        return unidade is not null ? Results.Ok(ToDto(unidade)) : Results.NotFound();
    }

    private static async Task<IResult> CreateUnidade(Unidade input, ApplicationDbContext db)
    {
        if (await db.Cidades.FindAsync(input.IdCidade) is null)
            return Results.BadRequest($"Cidade com Id {input.IdCidade} não encontrada.");

        db.Unidades.Add(input);
        await db.SaveChangesAsync();
        return Results.Created($"/unidades/{input.IdUnidade}", ToDto(input));
    }

    private static async Task<IResult> UpdateUnidade(int id, Unidade input, ApplicationDbContext db)
    {
        var unidade = await db.Unidades.FindAsync(id);
        if (unidade is null)
        {
            return Results.NotFound();
        }

        if (unidade.IdCidade != input.IdCidade && await db.Cidades.FindAsync(input.IdCidade) is null)
            return Results.BadRequest($"Cidade com Id {input.IdCidade} não encontrada.");

        unidade.Nome = input.Nome;
        unidade.IdCidade = input.IdCidade;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteUnidade(int id, ApplicationDbContext db)
    {
        var unidade = await db.Unidades.FindAsync(id);
        if (unidade is null)
        {
            return Results.NotFound();
        }

        db.Unidades.Remove(unidade);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static UnidadeDto ToDto(Unidade unidade) => new()
    {
        IdUnidade = unidade.IdUnidade,
        Nome = unidade.Nome,
        IdCidade = unidade.IdCidade,
        Links = new List<Link>
        {
            new("self", $"/unidades/{unidade.IdUnidade}", "GET"),
            new("update", $"/unidades/{unidade.IdUnidade}", "PUT"),
            new("delete", $"/unidades/{unidade.IdUnidade}", "DELETE"),
            new("all", "/unidades", "GET")
        }
    };

    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, int? cidadeId, string? nome)
    {
        var links = new List<Link>();
        var baseUrl = "/unidades";

        var queryBuilder = new StringBuilder();
        if (cidadeId.HasValue)
            queryBuilder.Append($"&cidadeId={cidadeId.Value}");
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


public class UnidadeDto
{
    public int IdUnidade { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int IdCidade { get; set; }
    public List<Link> Links { get; set; } = new();
}
