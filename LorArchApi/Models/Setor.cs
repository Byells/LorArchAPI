using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models;

public class Setor
{
    [Key]
    public int IdSetor { get; set; }
    public string Nome { get; set; } = null!;
    public int IdUnidade { get; set; }
}