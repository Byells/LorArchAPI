using LorArchApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace LorArchApi.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { }

    public DbSet<Moto>               Motos               { get; set; }
    public DbSet<Setor>              Setores             { get; set; }
    public DbSet<Unidade>            Unidades            { get; set; }
    public DbSet<Cidade>             Cidades             { get; set; }
    public DbSet<Defeito>            Defeitos            { get; set; }
    public DbSet<DefeitoMoto>        DefeitoMotos        { get; set; }
    public DbSet<Estado>             Estados             { get; set; }
    public DbSet<HistoricoManutencao>HistoricoManutencoes{ get; set; }
    public DbSet<Manutencao>         Manutencoes         { get; set; }
    
    public DbSet<Localizacao>        Localizacoes        { get; set; }
    public DbSet<Lora>               Loras           { get; set; }
    public DbSet<Rfid>               Rfids          { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(bool))
                {
                    property.SetValueConverter(new BoolToZeroOneConverter<int>());
                }
            }
        }
    }

    
}       