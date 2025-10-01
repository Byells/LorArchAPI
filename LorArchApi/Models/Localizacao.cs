using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LorArchApi.Models;

public class Localizacao
{
    [Key]
    public int IdLocalizacao { get; set; }
    [Precision(12, 8)]
    public decimal Latitude { get; set; }
    [Precision(12, 8)]
    public decimal Longitude { get; set; }
    public int IdMoto  { get; set; }
    public int IdSetor { get; set; }
    
    
    
}