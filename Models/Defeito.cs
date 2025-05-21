using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models;

public class Defeito
{
    [Key]
    public int IdDefeito { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
}