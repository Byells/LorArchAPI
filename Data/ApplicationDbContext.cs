using LorArchApi.Models;
using Microsoft.EntityFrameworkCore;


namespace LorArchApi.Data;

public class ApplicationDbContext : DbContext
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

    
}       