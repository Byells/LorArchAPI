using System.ComponentModel.DataAnnotations;

namespace LorArchApi.Models;

public class Rfid
{
    [Key]    
    public int IdRfid { get; set; }
    public int NumeroRfid { get; set; }
    public int IdMoto { get; set; }
}