using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class SetorEndpoints
{
    public static WebApplication MapSetorEndpoints(this WebApplication app)
    {
        const string SetoresTag = "Setores";

        app.MapGet("/setores", GetAllSetores)
            .WithName("GetSetores")
            .WithTags(SetoresTag)
            .Produces<PaginatedResponse<SetorDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar setores")
            .WithDescription("Retorna todos os setores paginados, opcionalmente filtrados por `unidadeId` ou `nome`.");

        app.MapGet("/setores/{id:int}", GetSetorById)
            .WithName("GetSetorById")
            .WithTags(SetoresTag)
            .Produces<SetorDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter setor por ID")
            .WithDescription("Retorna o setor correspondente ao `IdSetor` informado.");

        app.MapPost("/setores", CreateSetor)
            .WithName("CreateSetor")
            .WithTags(SetoresTag)
            .Accepts<Setor>("application/json")
            .Produces<SetorDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar setor")
            .WithDescription("Cria um novo setor vinculado a uma unidade existente.");

        app.MapPut("/setores/{id:int}", UpdateSetor)
            .WithName("UpdateSetor")
            .WithTags(SetoresTag)
            .Accepts<Setor>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar setor")
            .WithDescription("Atualiza o nome ou unidade de um setor existente.");

        app.MapDelete("/setores/{id:int}", DeleteSetor)
            .WithName("DeleteSetor")
            .WithTags(SetoresTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir setor")
            .WithDescription("Remove o setor especificado.");

        return app;
    }

    private static async Task<IResult> GetAllSetores(
        ApplicationDbContext db,
        int? unidadeId,
        string? nome,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Setores.AsQueryable();

        if (unidadeId.HasValue)
            query = query.Where(s => s.IdUnidade == unidadeId.Value);
        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(s => s.Nome.Contains(nome));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var setores = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = setores.Select(s =>
        {
            var dto = ToDto(s);
            dto.Links.RemoveAll(link => link.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<SetorDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, unidadeId, nome)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetSetorById(int id, ApplicationDbContext db)
    {
        var setor = await db.Setores.FindAsync(id);
        return setor is not null ? Results.Ok(ToDto(setor)) : Results.NotFound();
    }

    private static async Task<IResult> CreateSetor(Setor input, ApplicationDbContext db)
    {
        if (await db.Unidades.FindAsync(input.IdUnidade) is null)
            return Results.BadRequest($"Unidade com Id {input.IdUnidade} não encontrada.");

        db.Setores.Add(input);
        await db.SaveChangesAsync();
        return Results.Created($"/setores/{input.IdSetor}", ToDto(input));
    }

    private static async Task<IResult> UpdateSetor(int id, Setor input, ApplicationDbContext db)
    {
        var setor = await db.Setores.FindAsync(id);
        if (setor is null)
        {
            return Results.NotFound();
        }

        if (setor.IdUnidade != input.IdUnidade && await db.Unidades.FindAsync(input.IdUnidade) is null)
            return Results.BadRequest($"Unidade com Id {input.IdUnidade} não encontrada.");

        setor.Nome = input.Nome;
        setor.IdUnidade = input.IdUnidade;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteSetor(int id, ApplicationDbContext db)
    {
        var setor = await db.Setores.FindAsync(id);
        if (setor is null)
        {
            return Results.NotFound();
        }

        db.Setores.Remove(setor);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static SetorDto ToDto(Setor setor) => new()
    {
        IdSetor = setor.IdSetor,
        Nome = setor.Nome,
        IdUnidade = setor.IdUnidade,
        Links = new List<Link>
        {
            new("self", $"/setores/{setor.IdSetor}", "GET"),
            new("update", $"/setores/{setor.IdSetor}", "PUT"),
            new("delete", $"/setores/{setor.IdSetor}", "DELETE"),
            new("all", "/setores", "GET")
        }
    };

    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, int? unidadeId, string? nome)
    {
        var links = new List<Link>();
        var baseUrl = "/setores";

        var queryBuilder = new StringBuilder();
        if (unidadeId.HasValue)
            queryBuilder.Append($"&unidadeId={unidadeId.Value}");
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


public class SetorDto
{
    public int IdSetor { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int IdUnidade { get; set; }
    public List<Link> Links { get; set; } = new();
}
