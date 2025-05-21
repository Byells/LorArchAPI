// Models/DefeitoMoto.cs
using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models
{
    public class DefeitoMoto
    {
        [Key]
        public int       IdDefeitoMoto   { get; set; }
        public int       IdMoto          { get; set; }
        public int       IdDefeito       { get; set; }
        public DateTime  DataRegistro    { get; set; }
        public DateTime  DataAtualizacao { get; set; }
    }
}