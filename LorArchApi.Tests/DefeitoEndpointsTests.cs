using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using LorArchApi.Data;
using LorArchApi.Models;
using LorArchApi.Endpoints;
using Xunit;

namespace LorArchApi.Tests
{
    public class DefeitoInputModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
    }

    public class DefeitoEndpointsTests
    {
        [Fact]
        public async Task GetDefeitoById_Ok()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Defeitos.Add(new Defeito
                {
                    IdDefeito = 1,
                    Nome = "Motor",
                    Descricao = "Defeito no motor"
                });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/defeitos/1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var defeitoDto = await response.Content.ReadFromJsonAsync<DefeitoDto>();
            Assert.NotNull(defeitoDto);
            Assert.Equal("Motor", defeitoDto.Nome);
        }

        [Fact]
        public async Task GetDefeitoById_NotFound()
        {
            await using var application = new CustomWebApplicationFactory();
            var client = await application.CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/defeitos/99");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetDefeitos_List()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Defeitos.Add(new Defeito
                {
                    Nome = "Elétrico",
                    Descricao = "Problema elétrico"
                });
                db.Defeitos.Add(new Defeito
                {
                    Nome = "Mecânico",
                    Descricao = "Problema mecânico"
                });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();

            var response = await client.GetFromJsonAsync<PaginatedResponse<DefeitoDto>>("/defeitos");

            Assert.NotNull(response);
            Assert.Equal(2, response.Data.Count);
        }

        [Fact]
        public async Task CreateDefeito_Created()
        {
            await using var application = new CustomWebApplicationFactory();
            var client = await application.CreateAuthenticatedClientAsync();

            var novoDefeito = new DefeitoInputModel
            {
                Nome = "Suspensão",
                Descricao = "Problema na suspensão"
            };

            var content = new StringContent(JsonSerializer.Serialize(novoDefeito), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/defeitos", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task UpdateDefeito_NoContent()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Defeitos.Add(new Defeito
                {
                    IdDefeito = 1,
                    Nome = "Antigo",
                    Descricao = "Descrição antiga"
                });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();

            var defeitoAtualizado = new DefeitoInputModel
            {
                Nome = "Atualizado",
                Descricao = "Descrição atualizada"
            };

            var content = new StringContent(JsonSerializer.Serialize(defeitoAtualizado), Encoding.UTF8, "application/json");

            var response = await client.PutAsync("/defeitos/1", content);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteDefeito_NoContent()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Defeitos.Add(new Defeito
                {
                    IdDefeito = 1,
                    Nome = "Para deletar",
                    Descricao = "Será removido"
                });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();

            var response = await client.DeleteAsync("/defeitos/1");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await client.GetAsync("/defeitos/1");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
