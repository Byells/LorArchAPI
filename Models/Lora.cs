using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models;

public class Lora
{
    [Key]
    public int IdLora { get; set; }
    public int NumeroLora { get; set; }
    public int Moto  { get; set; }
} 