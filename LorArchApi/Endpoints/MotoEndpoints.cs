using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class MotoEndpoints
{
    public static WebApplication MapMotoEndpoints(this WebApplication app)
    {
        const string MotosTag = "Motos";

        app.MapGet("/motos", GetAllMotos)
            .WithName("GetAllMotos")
            .WithTags(MotosTag)
            .Produces<PaginatedResponse<MotoDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar todas as motos")
            .WithDescription("Retorna uma lista paginada de todas as motos cadastradas, com filtros opcionais.")
            .RequireAuthorization();

        app.MapGet("/motos/{id:int}", GetMotoById)
            .WithName("GetMotoById")
            .WithTags(MotosTag)
            .Produces<MotoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter moto por ID")
            .WithDescription("Retorna a moto correspondente ao `IdMoto` informado.")
            .RequireAuthorization();

        app.MapPost("/motos", CreateMoto)
            .WithName("CreateMoto")
            .WithTags(MotosTag)
            .Accepts<Moto>("application/json")
            .Produces<MotoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar nova moto")
            .WithDescription("Cria uma nova moto com os campos `Modelo`, `Placa`, `Status` e `IdSetor`.")
            .RequireAuthorization();

        app.MapPut("/motos/{id:int}", UpdateMoto)
            .WithName("UpdateMoto")
            .WithTags(MotosTag)
            .Accepts<Moto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar moto")
            .WithDescription("Atualiza os campos `Modelo`, `Placa`, `Status` e `IdSetor` de uma moto existente.")
            .RequireAuthorization();

        app.MapDelete("/motos/{id:int}", DeleteMoto)
            .WithName("DeleteMoto")
            .WithTags(MotosTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir moto")
            .WithDescription("Remove a moto correspondente ao `IdMoto` informado.")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetAllMotos(
        ApplicationDbContext db,
        string? placa,
        string? modelo,
        string? status,
        int? setorId,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Motos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(placa))
            query = query.Where(m => m.Placa.Contains(placa));
        if (!string.IsNullOrWhiteSpace(modelo))
            query = query.Where(m => m.Modelo.Contains(modelo));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(m => m.Status.Contains(status));
        if (setorId.HasValue)
            query = query.Where(m => m.IdSetor == setorId.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var motos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = motos.Select(m =>
        {
            var dto = ToDto(m);
            dto.Links.RemoveAll(link => link.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<MotoDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, placa, modelo, status, setorId)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetMotoById(int id, ApplicationDbContext db)
    {
        var moto = await db.Motos.FindAsync(id);
        return moto is not null ? Results.Ok(ToDto(moto)) : Results.NotFound();
    }

    private static async Task<IResult> CreateMoto(Moto input, ApplicationDbContext db)
    {
        if (await db.Setores.FindAsync(input.IdSetor) is null)
            return Results.BadRequest($"Setor {input.IdSetor} não encontrado.");

        db.Motos.Add(input);
        await db.SaveChangesAsync();
        return Results.Created($"/motos/{input.IdMoto}", ToDto(input));
    }

    private static async Task<IResult> UpdateMoto(int id, Moto input, ApplicationDbContext db)
    {
        var moto = await db.Motos.FindAsync(id);
        if (moto is null)
        {
            return Results.NotFound();
        }

        if (moto.IdSetor != input.IdSetor && await db.Setores.FindAsync(input.IdSetor) is null)
            return Results.BadRequest($"Setor {input.IdSetor} não encontrado.");

        moto.Modelo = input.Modelo;
        moto.Placa = input.Placa;
        moto.Status = input.Status;
        moto.IdSetor = input.IdSetor;
        moto.DataAtualizacao = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteMoto(int id, ApplicationDbContext db)
    {
        var moto = await db.Motos.FindAsync(id);
        if (moto is null)
        {
            return Results.NotFound();
        }

        db.Motos.Remove(moto);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static MotoDto ToDto(Moto moto) => new()
    {
        IdMoto = moto.IdMoto,
        Modelo = moto.Modelo,
        Placa = moto.Placa,
        Status = moto.Status,
        IdSetor = moto.IdSetor,
        DataCadastro = moto.DataCadastro,
        DataAtualizacao = moto.DataAtualizacao,
        Links = new List<Link>
        {
            new("self", $"/motos/{moto.IdMoto}", "GET"),
            new("update", $"/motos/{moto.IdMoto}", "PUT"),
            new("delete", $"/motos/{moto.IdMoto}", "DELETE"),
            new("all", "/motos", "GET")
        }
    };

    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, string? placa, string? modelo, string? status, int? setorId)
    {
        var links = new List<Link>();
        var baseUrl = "/motos";
        
        var queryBuilder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(placa)) queryBuilder.Append($"&placa={Uri.EscapeDataString(placa)}");
        if (!string.IsNullOrWhiteSpace(modelo)) queryBuilder.Append($"&modelo={Uri.EscapeDataString(modelo)}");
        if (!string.IsNullOrWhiteSpace(status)) queryBuilder.Append($"&status={Uri.EscapeDataString(status)}");
        if (setorId.HasValue) queryBuilder.Append($"&setorId={setorId.Value}");
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


public class MotoDto
{
    public int IdMoto { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int IdSetor { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime DataAtualizacao { get; set; }
    public List<Link> Links { get; set; } = new();
}
