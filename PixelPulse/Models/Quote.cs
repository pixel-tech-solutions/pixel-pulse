using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PixelPulse.Models;

[Table("Quotes")]
public class Quote
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Author { get; set; }
    
    [Required]
    public QuoteCategory Category { get; set; }
    
    [MaxLength(100)]
    public string? Genre { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
