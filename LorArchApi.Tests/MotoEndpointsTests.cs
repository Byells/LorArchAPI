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

namespace LorArchApi.Tests
{
    public class MotoInputModel
    {
        public string Modelo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int IdSetor { get; set; }
    }

    public class MotoEndpointsTests
    {
        [Fact]
        public async Task GetMotoById_Ok()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Setores.Add(new Setor { IdSetor = 1, Nome = "Setor Teste" });
                db.Motos.Add(new Moto
                {
                    IdMoto = 1,
                    Modelo = "Yamaha",
                    Placa = "ABC1234",
                    Status = "Ativa",
                    IdSetor = 1,
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/motos/1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var motoDto = await response.Content.ReadFromJsonAsync<MotoDto>();
            Assert.NotNull(motoDto);
            Assert.Equal("Yamaha", motoDto.Modelo);
        }

        [Fact]
        public async Task GetMotoById_NotFound()
        {
            await using var application = new CustomWebApplicationFactory();
            var client = await application.CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/motos/99");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetMotos_List()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Setores.Add(new Setor { IdSetor = 1, Nome = "Setor A" });
                db.Motos.Add(new Moto
                {
                    IdMoto = 1,
                    Modelo = "Honda",
                    Placa = "AAA1111",
                    Status = "Ativa",
                    IdSetor = 1,
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow
                });
                db.Motos.Add(new Moto
                {
                    IdMoto = 2,
                    Modelo = "Suzuki",
                    Placa = "BBB2222",
                    Status = "Ativa",
                    IdSetor = 1,
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();

            var response = await client.GetFromJsonAsync<PaginatedResponse<MotoDto>>("/motos");

            Assert.NotNull(response);
            Assert.Equal(2, response.Data.Count);
        }

        [Fact]
        public async Task CreateMoto_Created()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Setores.Add(new Setor { IdSetor = 1, Nome = "Setor Padrão" });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();
            var novaMoto = new MotoInputModel
            {
                Modelo = "Kawasaki",
                Placa = "CCC3333",
                Status = "Ativa",
                IdSetor = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(novaMoto), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/motos", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task UpdateMoto_NoContent()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Setores.Add(new Setor { IdSetor = 1, Nome = "Setor A" });
                db.Motos.Add(new Moto
                {
                    IdMoto = 1,
                    Modelo = "Antiga",
                    Placa = "DDD4444",
                    Status = "Ativa",
                    IdSetor = 1,
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();
            var motoAtualizada = new MotoInputModel
            {
                Modelo = "Nova",
                Placa = "EEE5555",
                Status = "Inativa",
                IdSetor = 1
            };
            var content = new StringContent(JsonSerializer.Serialize(motoAtualizada), Encoding.UTF8, "application/json");

            var response = await client.PutAsync("/motos/1", content);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMoto_NoContent()
        {
            await using var application = new CustomWebApplicationFactory();
            using (var scope = application.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Setores.Add(new Setor { IdSetor = 1, Nome = "Setor A" });
                db.Motos.Add(new Moto
                {
                    IdMoto = 1,
                    Modelo = "Para Deletar",
                    Placa = "FFF6666",
                    Status = "Ativa",
                    IdSetor = 1,
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var client = await application.CreateAuthenticatedClientAsync();

            var response = await client.DeleteAsync("/motos/1");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var getResponse = await client.GetAsync("/motos/1");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
