
using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models
{
    public class Moto
    {
        [Key]
        public int       IdMoto          { get; set; }
        public string    Modelo          { get; set; } = null!;
        public string    Placa           { get; set; } = null!;
        public string    Status          { get; set; } = null!;
        public DateTime  DataCadastro    { get; set; }
        public DateTime  DataAtualizacao { get; set; }
        public int       IdSetor         { get; set; }
    }
}