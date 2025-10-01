using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class ManutencaoEndpoints
{
    public static WebApplication MapManutencaoEndpoints(this WebApplication app)
    {
        const string ManutencoesTag = "Manutencoes";

        app.MapGet("/manutencoes", GetAllManutencoes)
            .WithName("GetManutencoes")
            .WithTags(ManutencoesTag)
            .Produces<PaginatedResponse<ManutencaoDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar manutenções")
            .WithDescription("Retorna todas as manutenções paginadas, com filtros opcionais.")
            .RequireAuthorization();

        app.MapGet("/manutencoes/{id:int}", GetManutencaoById)
            .WithName("GetManutencaoById")
            .WithTags(ManutencoesTag)
            .Produces<ManutencaoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter manutenção por ID")
            .WithDescription("Retorna a manutenção correspondente ao `IdManutencao` informado.")
            .RequireAuthorization();

        app.MapPost("/manutencoes", CreateManutencao)
            .WithName("CreateManutencao")
            .WithTags(ManutencoesTag)
            .Accepts<Manutencao>("application/json")
            .Produces<ManutencaoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar manutenção")
            .WithDescription("Cria um novo registro de manutenção para uma moto.")
            .RequireAuthorization();

        app.MapPut("/manutencoes/{id:int}", UpdateManutencao)
            .WithName("UpdateManutencao")
            .WithTags(ManutencoesTag)
            .Accepts<Manutencao>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar manutenção")
            .WithDescription("Atualiza um registro de manutenção existente.")
            .RequireAuthorization();

        app.MapDelete("/manutencoes/{id:int}", DeleteManutencao)
            .WithName("DeleteManutencao")
            .WithTags(ManutencoesTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir manutenção")
            .WithDescription("Remove o registro de manutenção especificado.")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetAllManutencoes(
        ApplicationDbContext db,
        int? motoId,
        string? tipo,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Manutencoes.AsQueryable();

        if (motoId.HasValue)
            query = query.Where(m => m.IdMoto == motoId.Value);
        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(m => m.Tipo.Contains(tipo));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var manutencoes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = manutencoes.Select(m =>
        {
            var dto = ToDto(m);
            dto.Links.RemoveAll(link => link.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<ManutencaoDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, motoId, tipo)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetManutencaoById(int id, ApplicationDbContext db)
    {
        var manutencao = await db.Manutencoes.FindAsync(id);

        return manutencao is not null
            ? Results.Ok(ToDto(manutencao))
            : Results.NotFound();
    }

    private static async Task<IResult> CreateManutencao(Manutencao input, ApplicationDbContext db)
    {
        if (await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");

        db.Manutencoes.Add(input);
        await db.SaveChangesAsync();

        var dto = ToDto(input);

        return Results.Created($"/manutencoes/{input.IdManutencao}", dto);
    }

    private static async Task<IResult> UpdateManutencao(int id, Manutencao input, ApplicationDbContext db)
    {
        var manutencao = await db.Manutencoes.FindAsync(id);
        if (manutencao is null)
        {
            return Results.NotFound();
        }

        if (manutencao.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");

        manutencao.Descricao = input.Descricao;
        manutencao.DataManutencao = input.DataManutencao;
        manutencao.CustoEstimado = input.CustoEstimado;
        manutencao.Tipo = input.Tipo;
        manutencao.IdMoto = input.IdMoto;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteManutencao(int id, ApplicationDbContext db)
    {
        var manutencao = await db.Manutencoes.FindAsync(id);
        if (manutencao is null)
        {
            return Results.NotFound();
        }

        db.Manutencoes.Remove(manutencao);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static ManutencaoDto ToDto(Manutencao manutencao) => new()
    {
        IdManutencao = manutencao.IdManutencao,
        Descricao = manutencao.Descricao,
        DataManutencao = manutencao.DataManutencao,
        CustoEstimado = manutencao.CustoEstimado,
        Tipo = manutencao.Tipo,
        IdMoto = manutencao.IdMoto,
        Links = new List<Link>
        {
            new("self", $"/manutencoes/{manutencao.IdManutencao}", "GET"),
            new("update", $"/manutencoes/{manutencao.IdManutencao}", "PUT"),
            new("delete", $"/manutencoes/{manutencao.IdManutencao}", "DELETE"),
            new("all", "/manutencoes", "GET")
        }
    };

    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, int? motoId, string? tipo)
    {
        var links = new List<Link>();
        var baseUrl = "/manutencoes";

        var queryBuilder = new StringBuilder();
        if (motoId.HasValue)
            queryBuilder.Append($"&motoId={motoId.Value}");
        if (!string.IsNullOrWhiteSpace(tipo))
            queryBuilder.Append($"&tipo={Uri.EscapeDataString(tipo)}");
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


public class ManutencaoDto
{
    public int IdManutencao { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataManutencao { get; set; }
    public double CustoEstimado { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int IdMoto { get; set; }
    public List<Link> Links { get; set; } = new();
}
