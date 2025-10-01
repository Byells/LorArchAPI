using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using LorArchApi.Data;
using LorArchApi.Models;
using System.Text;

namespace LorArchApi.Endpoints;

public static class HistoricoManutencaoEndpoints
{
    public static WebApplication MapHistoricoManutencaoEndpoints(this WebApplication app)
    {
        const string HistoricosTag = "Historicos";
        
        app.MapGet("/historicos", GetAllHistoricos)
            .WithName("GetHistoricos")
            .WithTags(HistoricosTag)
            .Produces<PaginatedResponse<HistoricoManutencaoDto>>(StatusCodes.Status200OK)
            .WithSummary("Listar histórico de movimentações")
            .WithDescription("Retorna movimentações paginadas de motos entre setores, com filtros opcionais.");

        app.MapGet("/historicos/{id:int}", GetHistoricoById)
            .WithName("GetHistoricoById")
            .WithTags(HistoricosTag)
            .Produces<HistoricoManutencaoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Obter movimentação por ID")
            .WithDescription("Retorna a movimentação correspondente ao IdMovimentacao informado.");

        app.MapPost("/historicos", CreateHistorico)
            .WithName("CreateHistorico")
            .WithTags(HistoricosTag)
            .Accepts<HistoricoManutencao>("application/json")
            .Produces<HistoricoManutencaoDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Criar movimentação")
            .WithDescription("Registra a movimentação de uma moto entre setores.");

        app.MapPut("/historicos/{id:int}", UpdateHistorico)
            .WithName("UpdateHistorico")
            .WithTags(HistoricosTag)
            .Accepts<HistoricoManutencao>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Atualizar movimentação")
            .WithDescription("Atualiza dados de uma movimentação existente.");

        app.MapDelete("/historicos/{id:int}", DeleteHistorico)
            .WithName("DeleteHistorico")
            .WithTags(HistoricosTag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Excluir movimentação")
            .WithDescription("Remove a movimentação especificada.");

        return app;
    }

    private static async Task<IResult> GetAllHistoricos(
        ApplicationDbContext db,
        int? motoId,
        int? setorOrigemId,
        int? setorDestinoId,
        int page = 1,
        int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.HistoricoManutencoes.AsQueryable();

        if (motoId.HasValue)
            query = query.Where(h => h.IdMoto == motoId.Value);
        if (setorOrigemId.HasValue)
            query = query.Where(h => h.IdSetorOrigem == setorOrigemId.Value);
        if (setorDestinoId.HasValue)
            query = query.Where(h => h.IdSetorDestino == setorDestinoId.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var historicos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        var dtos = historicos.Select(h =>
        {
            var dto = ToDto(h);
            dto.Links.RemoveAll(l => l.Rel == "all");
            return dto;
        }).ToList();

        var response = new PaginatedResponse<HistoricoManutencaoDto>
        {
            Data = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
            Links = GeneratePaginationLinks(page, totalPages, pageSize, motoId, setorOrigemId, setorDestinoId)
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetHistoricoById(int id, ApplicationDbContext db)
    {
        var historico = await db.HistoricoManutencoes.FindAsync(id);
        
        return historico is not null 
            ? Results.Ok(ToDto(historico)) 
            : Results.NotFound();
    }

    private static async Task<IResult> CreateHistorico(HistoricoManutencao input, ApplicationDbContext db)
    {
        if (await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");

        if (await db.Setores.FindAsync(input.IdSetorOrigem) is null)
            return Results.BadRequest($"Setor de Origem com Id {input.IdSetorOrigem} não encontrado.");

        if (await db.Setores.FindAsync(input.IdSetorDestino) is null)
            return Results.BadRequest($"Setor de Destino com Id {input.IdSetorDestino} não encontrado.");

        db.HistoricoManutencoes.Add(input);
        await db.SaveChangesAsync();
        
        var dto = ToDto(input);
        
        return Results.Created($"/historicos/{input.IdMovimentacao}", dto);
    }

    private static async Task<IResult> UpdateHistorico(int id, HistoricoManutencao input, ApplicationDbContext db)
    {
        var historico = await db.HistoricoManutencoes.FindAsync(id);
        if (historico is null)
        {
            return Results.NotFound();
        }

        if (historico.IdMoto != input.IdMoto && await db.Motos.FindAsync(input.IdMoto) is null)
            return Results.BadRequest($"Moto com Id {input.IdMoto} não encontrada.");

        if (historico.IdSetorOrigem != input.IdSetorOrigem && await db.Setores.FindAsync(input.IdSetorOrigem) is null)
            return Results.BadRequest($"Setor de Origem com Id {input.IdSetorOrigem} não encontrado.");

        if (historico.IdSetorDestino != input.IdSetorDestino && await db.Setores.FindAsync(input.IdSetorDestino) is null)
            return Results.BadRequest($"Setor de Destino com Id {input.IdSetorDestino} não encontrado.");

        historico.IdMoto = input.IdMoto;
        historico.IdSetorOrigem = input.IdSetorOrigem;
        historico.IdSetorDestino = input.IdSetorDestino;
        historico.DataMovimento = input.DataMovimento;
        
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteHistorico(int id, ApplicationDbContext db)
    {
        var historico = await db.HistoricoManutencoes.FindAsync(id);
        if (historico is null)
        {
            return Results.NotFound();
        }

        db.HistoricoManutencoes.Remove(historico);
        await db.SaveChangesAsync();
        
        return Results.NoContent();
    }
    
    private static HistoricoManutencaoDto ToDto(HistoricoManutencao historico) => new()
    {
        IdMovimentacao = historico.IdMovimentacao,
        IdMoto = historico.IdMoto,
        IdSetorOrigem = historico.IdSetorOrigem,
        IdSetorDestino = historico.IdSetorDestino,
        DataMovimento = historico.DataMovimento,
        Links = new List<Link>
        {
            new("self", $"/historicos/{historico.IdMovimentacao}", "GET"),
            new("update", $"/historicos/{historico.IdMovimentacao}", "PUT"),
            new("delete", $"/historicos/{historico.IdMovimentacao}", "DELETE"),
            new("all", "/historicos", "GET")
        }
    };
    
    private static List<Link> GeneratePaginationLinks(int currentPage, int totalPages, int pageSize, int? motoId, int? setorOrigemId, int? setorDestinoId)
    {
        var links = new List<Link>();
        var baseUrl = "/historicos";
        
        var queryBuilder = new StringBuilder();
        if (motoId.HasValue)
            queryBuilder.Append($"&motoId={motoId.Value}");
        if (setorOrigemId.HasValue)
            queryBuilder.Append($"&setorOrigemId={setorOrigemId.Value}");
        if (setorDestinoId.HasValue)
            queryBuilder.Append($"&setorDestinoId={setorDestinoId.Value}");
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


public class HistoricoManutencaoDto
{
    public int IdMovimentacao { get; set; }
    public int IdMoto { get; set; }
    public int IdSetorOrigem { get; set; }
    public int IdSetorDestino { get; set; }
    public DateTime DataMovimento { get; set; }
    public List<Link> Links { get; set; } = new();
}
