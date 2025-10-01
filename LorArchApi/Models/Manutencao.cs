
using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models
{
    public class Manutencao
    {
        [Key]
        public int       IdManutencao    { get; set; }
        public int       IdMoto          { get; set; }
        public string    Descricao       { get; set; } = null!;
        public DateTime  DataManutencao  { get; set; }
        public double    CustoEstimado   { get; set; }
        public string    Tipo            { get; set; } = null!;
    }
}