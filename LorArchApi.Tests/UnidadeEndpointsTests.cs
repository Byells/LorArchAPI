using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LorArchApi.Data;
using LorArchApi.Models;
using LorArchApi.Endpoints;
using Xunit;

namespace LorArchApi.Tests;



public class UnidadeInputModel
{
    public string Nome { get; set; } = string.Empty;
    public int IdCidade { get; set; }
}


public class UnidadeEndpointsTests
{
    [Fact]
    public async Task GetUnidadeById_Ok()
    {
        await using var application = new LorArchApiApplication();
        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Unidades.Add(new Unidade { IdUnidade = 1, Nome = "Sede Teste", IdCidade = 1 });
            await db.SaveChangesAsync();
        }
        var client = application.CreateClient();

        var response = await client.GetAsync("/unidades/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var unidadeDto = await response.Content.ReadFromJsonAsync<UnidadeDto>();
        Assert.NotNull(unidadeDto);
        Assert.Equal("Sede Teste", unidadeDto.Nome);
    }

    [Fact]
    public async Task GetUnidadeById_NotFound()
    {
        await using var application = new LorArchApiApplication();
        var client = application.CreateClient();

        var response = await client.GetAsync("/unidades/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetUnidades_List()
    {
        await using var application = new LorArchApiApplication();
        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Unidades.Add(new Unidade { IdUnidade = 1, Nome = "Unidade A", IdCidade = 1 });
            db.Unidades.Add(new Unidade { IdUnidade = 2, Nome = "Unidade B", IdCidade = 1 });
            await db.SaveChangesAsync();
        }
        var client = application.CreateClient();

        var response = await client.GetFromJsonAsync<PaginatedResponse<UnidadeDto>>("/unidades");

        Assert.NotNull(response);
        Assert.Equal(2, response.Data.Count);
    }
    
    [Fact]
    public async Task CreateUnidade_Created()
    {
        await using var application = new LorArchApiApplication();
        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Cidades.Add(new Cidade { IdCidade = 1, Nome = "Cidade Padrão", IdEstado = 1 });
            await db.SaveChangesAsync();
        }
        var client = application.CreateClient();
        var novaUnidade = new UnidadeInputModel { Nome = "Nova Unidade", IdCidade = 1 };
        var content = new StringContent(JsonSerializer.Serialize(novaUnidade), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/unidades", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }
    
    [Fact]
    public async Task UpdateUnidade_NoContent()
    {
        await using var application = new LorArchApiApplication();
        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Cidades.Add(new Cidade { IdCidade = 1, Nome = "Cidade A", IdEstado = 1 });
            db.Unidades.Add(new Unidade { IdUnidade = 1, Nome = "Nome Antigo", IdCidade = 1 });
            await db.SaveChangesAsync();
        }
        var client = application.CreateClient();
        var unidadeAtualizada = new UnidadeInputModel { Nome = "Nome Novo", IdCidade = 1 };
        var content = new StringContent(JsonSerializer.Serialize(unidadeAtualizada), Encoding.UTF8, "application/json");

        var response = await client.PutAsync("/unidades/1", content);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteUnidade_NoContent()
    {
        await using var application = new LorArchApiApplication();
        using (var scope = application.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Unidades.Add(new Unidade { IdUnidade = 1, Nome = "Para Deletar", IdCidade = 1 });
            await db.SaveChangesAsync();
        }
        var client = application.CreateClient();

        var response = await client.DeleteAsync("/unidades/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await client.GetAsync("/unidades/1");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}

class LorArchApiApplication : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"BancoDeTestes-{Guid.NewGuid()}";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}