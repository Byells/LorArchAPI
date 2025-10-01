using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class CidadeEndpoints
{
    public static WebApplication MapCidadeEndpoints(this WebApplication app)
    {
        const string CidadesTag = "Cidades";

        app.MapGet("/cidades", GetCidades)
            .WithName("GetCidades")
            .WithTags(CidadesTag)
            .Produces<PaginatedResponse<CidadeDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar cidades")
            .WithDescription("Retorna cidades paginadas, opcionalmente filtradas por `nome` ou `estadoId`.");

        app.MapGet("/cidades/{id:int}", GetCidadeById)
            .WithName("GetCidadeById")
            .WithTags(CidadesTag)
            .Produces<CidadeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter cidade por ID")
            .WithDescription("Retorna a cidade correspondente ao `IdCidade` informado.");

        app.MapPost("/cidades", CreateCidade)
            .WithName("CreateCidade")
            .WithTags(CidadesTag)
            .Accepts<Cidade>("application/json")
            .Produces<CidadeDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar cidade")
            .WithDescription("Cria uma nova cidade vinculada a um estado existente.");

        app.MapPut("/cidades/{id:int}", UpdateCidade)
            .WithName("UpdateCidade")
            .WithTags(CidadesTag)
            .Accepts<Cidade>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar cidade")
            .WithDescription("Atualiza nome ou estado de uma cidade existente.");

        app.MapDelete("/cidades/{id:int}", DeleteCidade)
            .WithName("DeleteCidade")
            .WithTags(CidadesTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir cidade")
            .WithDescription("Remove a cidade especificada.");

        return app;
    }

    private static async Task<IResult> GetCidades(
        ApplicationDbContext db,
        int? estadoId,
        string? nome,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Cidades.AsQueryable();

        if (estadoId.HasValue)
            query = query.Where(c => c.IdEstado == estadoId.Value);

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(c => c.Nome.Contains(nome));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var cidades = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var cidadesDto = cidades.Select(c =>
        {
            var dto = ToDto(c);
            dto.Links.RemoveAll(l => l.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<CidadeDto>
        {
            Data = cidadesDto,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, nome, estadoId)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetCidadeById(int id, ApplicationDbContext db)
    {
        var cidade = await db.Cidades.FindAsync(id);
        
        return cidade is null 
            ? Results.NotFound() 
            : Results.Ok(ToDto(cidade));
    }

    private static async Task<IResult> CreateCidade(Cidade input, ApplicationDbContext db)
    {
        if (await db.Estados.FindAsync(input.IdEstado) is null)
            return Results.BadRequest($"Estado com Id {input.IdEstado} não encontrado.");

        db.Cidades.Add(input);
        await db.SaveChangesAsync();

        var cidadeDto = ToDto(input);

        return Results.Created($"/cidades/{input.IdCidade}", cidadeDto);
    }

    private static async Task<IResult> UpdateCidade(int id, Cidade input, ApplicationDbContext db)
    {
        var cidade = await db.Cidades.FindAsync(id);
        if (cidade is null)
        {
            return Results.NotFound();
        }

        if (cidade.IdEstado != input.IdEstado && await db.Estados.FindAsync(input.IdEstado) is null)
        {
            return Results.BadRequest($"Estado com Id {input.IdEstado} não encontrado.");
        }
            
        cidade.Nome = input.Nome;
        cidade.IdEstado = input.IdEstado;
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCidade(int id, ApplicationDbContext db)
    {
        var cidade = await db.Cidades.FindAsync(id);
        if (cidade is null)
        {
            return Results.NotFound();
        }

        db.Cidades.Remove(cidade);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }
    
    private static CidadeDto ToDto(Cidade cidade) => new()
    {
        IdCidade = cidade.IdCidade,
        Nome = cidade.Nome,
        IdEstado = cidade.IdEstado,
        Links = new List<Link>
        {
            new("self", $"/cidades/{cidade.IdCidade}", "GET"),
            new("update", $"/cidades/{cidade.IdCidade}", "PUT"),
            new("delete", $"/cidades/{cidade.IdCidade}", "DELETE"),
            new("all", "/cidades", "GET")
        }
    };
    
    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, string? nome, int? estadoId)
    {
        var links = new List<Link>();
        var baseUrl = "/cidades";
        
        var queryBuilder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(nome))
            queryBuilder.Append($"&nome={Uri.EscapeDataString(nome)}");
        if (estadoId.HasValue)
            queryBuilder.Append($"&estadoId={estadoId.Value}");
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


public class CidadeDto
{
    public int IdCidade { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int IdEstado { get; set; }
    public List<Link> Links { get; set; } = new();
}

