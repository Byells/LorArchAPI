using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models;

public class Estado
{
    [Key]
    public int IdEstado { get; set; }
    public string Nome { get; set; }
    public string Sigla { get; set; }
}