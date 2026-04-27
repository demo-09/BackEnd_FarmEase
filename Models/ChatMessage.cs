using System;
using System.ComponentModel.DataAnnotations;

namespace backEnd.Models;

public class ChatMessage
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public string SenderEmail { get; set; } = string.Empty;
    
    [Required]
    public string ReceiverEmail { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
