using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class RfidEndpoints
{
    public static WebApplication MapRfidEndpoints(this WebApplication app)
    {
        const string RfidTag = "Rfid";

        app.MapGet("/rfid", GetAllRfids)
            .WithName("GetRfids")
            .WithTags(RfidTag)
            .Produces<PaginatedResponse<RfidDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar RFID")
            .WithDescription("Retorna todos os registros RFID paginados, com filtros opcionais.")
            .RequireAuthorization();

        app.MapGet("/rfid/{id:int}", GetRfidById)
            .WithName("GetRfidById")
            .WithTags(RfidTag)
            .Produces<RfidDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter RFID por ID")
            .WithDescription("Retorna o registro RFID correspondente ao `IdRfid` informado.")
            .RequireAuthorization();

        app.MapPost("/rfid", CreateRfid)
            .WithName("CreateRfid")
            .WithTags(RfidTag)
            .Accepts<Rfid>("application/json")
            .Produces<RfidDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar RFID")
            .WithDescription("Registra um novo tag RFID vinculado a uma moto.")
            .RequireAuthorization();

        app.MapPut("/rfid/{id:int}", UpdateRfid)
            .WithName("UpdateRfid")
            .WithTags(RfidTag)
            .Accepts<Rfid>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar RFID")
            .WithDescription("Atualiza dados de um tag RFID existente.")
            .RequireAuthorization();

        app.MapDelete("/rfid/{id:int}", DeleteRfid)
            .WithName("DeleteRfid")
            .WithTags(RfidTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir RFID")
            .WithDescription("Remove o tag RFID especificado.")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetAllRfids(
        ApplicationDbContext db,
        int? motoId,
        string? numeroRfid,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Rfids.AsQueryable();

        if (motoId.HasValue)
            query = query.Where(r => r.IdMoto == motoId.Value);
        
        if (!string.IsNullOrWhiteSpace(numeroRfid))
            query = query.Where(r => r.NumeroRfid.ToString().Contains(numeroRfid));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var rfids = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = rfids.Select(r =>
        {
            var dto = ToDto(r);
            dto.Links.RemoveAll(link => link.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<RfidDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, motoId, numeroRfid)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetRfidById(int id, ApplicationDbContext db)
    {
        var rfid = await db.Rfids.FindAsync(id);
        return rfid is not null ? Results.Ok(ToDto(rfid)) : Results.NotFound();
    }

    private static async Task<IResult> CreateRfid(Rfid input, ApplicationDbContext db)
    {
        if (await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");

        db.Rfids.Add(input);
        await db.SaveChangesAsync();

        return Results.Created($"/rfid/{input.IdRfid}", ToDto(input));
    }

    private static async Task<IResult> UpdateRfid(int id, Rfid input, ApplicationDbContext db)
    {
        var rfid = await db.Rfids.FindAsync(id);
        if (rfid is null)
        {
            return Results.NotFound();
        }

        if (rfid.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");

        rfid.NumeroRfid = input.NumeroRfid;
        rfid.IdMoto = input.IdMoto;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteRfid(int id, ApplicationDbContext db)
    {
        var rfid = await db.Rfids.FindAsync(id);
        if (rfid is null)
        {
            return Results.NotFound();
        }

        db.Rfids.Remove(rfid);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static RfidDto ToDto(Rfid rfid) => new()
    {
        IdRfid = rfid.IdRfid,
        NumeroRfid = rfid.NumeroRfid.ToString(),
        IdMoto = rfid.IdMoto,
        Links = new List<Link>
        {
            new("self", $"/rfid/{rfid.IdRfid}", "GET"),
            new("update", $"/rfid/{rfid.IdRfid}", "PUT"),
            new("delete", $"/rfid/{rfid.IdRfid}", "DELETE"),
            new("all", "/rfid", "GET")
        }
    };

    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, int? motoId, string? numeroRfid)
    {
        var links = new List<Link>();
        var baseUrl = "/rfid";

        var queryBuilder = new StringBuilder();
        if (motoId.HasValue)
            queryBuilder.Append($"&motoId={motoId.Value}");
        if (!string.IsNullOrWhiteSpace(numeroRfid))
            queryBuilder.Append($"&numeroRfid={Uri.EscapeDataString(numeroRfid)}");
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


public class RfidDto
{
    public int IdRfid { get; set; }
    public string NumeroRfid { get; set; } = string.Empty;
    public int IdMoto { get; set; }
    public List<Link> Links { get; set; } = new();
}

