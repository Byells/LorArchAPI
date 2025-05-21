using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models
{
    public class HistoricoManutencao
    {
        [Key]
        public int       IdMovimentacao  { get; set; }
        public int       IdMoto          { get; set; }
        public int       IdSetorOrigem   { get; set; }
        public int       IdSetorDestino  { get; set; }
        public DateTime  DataMovimento   { get; set; }
    }
}