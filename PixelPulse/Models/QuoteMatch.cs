using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PixelPulse.Models;

[Table("QuoteMatches")]
public class QuoteMatch
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int PrimaryQuoteId { get; set; }
    
    [Required]
    public int SecondaryQuoteId { get; set; }
    
    [Required]
    public QuoteCategory PrimaryCategory { get; set; }
    
    [Required]
    public QuoteCategory SecondaryCategory { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string MatchType { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [ForeignKey(nameof(PrimaryQuoteId))]
    public virtual Quote? PrimaryQuote { get; set; }
    
    [ForeignKey(nameof(SecondaryQuoteId))]
    public virtual Quote? SecondaryQuote { get; set; }
}
