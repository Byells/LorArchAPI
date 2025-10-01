using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class LoraEndpoints
{
    public static WebApplication MapLoraEndpoints(this WebApplication app)
    {
        const string LoraTag = "Lora";

        app.MapGet("/lora", GetAllLoras)
            .WithName("GetLoras")
            .WithTags(LoraTag)
            .Produces<PaginatedResponse<LoraDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar LoRa")
            .WithDescription("Retorna todos os registros de LoRa paginados, com filtros opcionais.");

        app.MapGet("/lora/{id:int}", GetLoraById)
            .WithName("GetLoraById")
            .WithTags(LoraTag)
            .Produces<LoraDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter LoRa por ID")
            .WithDescription("Retorna o registro de LoRa correspondente ao `IdLora` informado.");

        app.MapPost("/lora", CreateLora)
            .WithName("CreateLora")
            .WithTags(LoraTag)
            .Accepts<Lora>("application/json")
            .Produces<LoraDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar LoRa")
            .WithDescription("Registra um novo dispositivo LoRa vinculado a uma moto.");

        app.MapPut("/lora/{id:int}", UpdateLora)
            .WithName("UpdateLora")
            .WithTags(LoraTag)
            .Accepts<Lora>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar LoRa")
            .WithDescription("Atualiza dados de um dispositivo LoRa existente.");

        app.MapDelete("/lora/{id:int}", DeleteLora)
            .WithName("DeleteLora")
            .WithTags(LoraTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir LoRa")
            .WithDescription("Remove o dispositivo LoRa especificado.");

        return app;
    }

    private static async Task<IResult> GetAllLoras(
        ApplicationDbContext db,
        int? motoId,
        string? numeroLora,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Loras.AsQueryable();

        if (motoId.HasValue)
            query = query.Where(l => l.Moto == motoId.Value);
        
        if (!string.IsNullOrWhiteSpace(numeroLora))
        {
            // Corrigido: Converte a propriedade numérica para string para a busca.
            // EF Core consegue traduzir esta operação para SQL.
            query = query.Where(l => l.NumeroLora.ToString().Contains(numeroLora));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var loras = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = loras.Select(l =>
        {
            var dto = ToDto(l);
            dto.Links.RemoveAll(link => link.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<LoraDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, motoId, numeroLora)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetLoraById(int id, ApplicationDbContext db)
    {
        var lora = await db.Loras.FindAsync(id);

        return lora is not null
            ? Results.Ok(ToDto(lora))
            : Results.NotFound();
    }

    private static async Task<IResult> CreateLora(Lora input, ApplicationDbContext db)
    {
        // Corrigido: Validação para 'int' não nulo, assumindo 0 como "não atribuído".
        if (input.Moto != 0 && await db.Motos.FindAsync(input.Moto) is null)
            return Results.BadRequest($"Moto com Id {input.Moto} não encontrada.");

        db.Loras.Add(input);
        await db.SaveChangesAsync();

        var dto = ToDto(input);

        return Results.Created($"/lora/{input.IdLora}", dto);
    }

    private static async Task<IResult> UpdateLora(int id, Lora input, ApplicationDbContext db)
    {
        var lora = await db.Loras.FindAsync(id);
        if (lora is null)
        {
            return Results.NotFound();
        }

        // Corrigido: Validação para 'int' não nulo.
        if (lora.Moto != input.Moto && input.Moto != 0 && await db.Motos.FindAsync(input.Moto) is null)
            return Results.BadRequest($"Moto com Id {input.Moto} não encontrada.");

        lora.NumeroLora = input.NumeroLora;
        lora.Moto = input.Moto;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteLora(int id, ApplicationDbContext db)
    {
        var lora = await db.Loras.FindAsync(id);
        if (lora is null)
        {
            return Results.NotFound();
        }

        db.Loras.Remove(lora);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static LoraDto ToDto(Lora lora) => new()
    {
        IdLora = lora.IdLora,
        // Corrigido: Garante a conversão para string no DTO.
        NumeroLora = lora.NumeroLora.ToString(), 
        // Corrigido: Converte 0 para null no DTO para representar "não atribuído".
        Moto = lora.Moto == 0 ? null : lora.Moto,
        Links = new List<Link>
        {
            new("self", $"/lora/{lora.IdLora}", "GET"),
            new("update", $"/lora/{lora.IdLora}", "PUT"),
            new("delete", $"/lora/{lora.IdLora}", "DELETE"),
            new("all", "/lora", "GET")
        }
    };

    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, int? motoId, string? numeroLora)
    {
        var links = new List<Link>();
        var baseUrl = "/lora";

        var queryBuilder = new StringBuilder();
        if (motoId.HasValue)
            queryBuilder.Append($"&motoId={motoId.Value}");
        if (!string.IsNullOrWhiteSpace(numeroLora))
            queryBuilder.Append($"&numeroLora={Uri.EscapeDataString(numeroLora)}");
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


public class LoraDto
{
    public int IdLora { get; set; }
    public string NumeroLora { get; set; } = string.Empty;
    public int? Moto { get; set; }
    public List<Link> Links { get; set; } = new();
}

