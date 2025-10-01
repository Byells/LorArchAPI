using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models;

public class Cidade
{
    [Key]
    public int IdCidade { get; set; }
    public string Nome { get; set; }
    public int IdEstado { get; set; }
    
}