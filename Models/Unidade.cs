using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models;

public class Unidade
{
    [Key]
    public int IdUnidade { get; set; }
    public string Nome { get; set; }
    public int IdCidade { get; set; }
}