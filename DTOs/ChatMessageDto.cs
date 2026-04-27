using System.ComponentModel.DataAnnotations;

namespace backEnd.DTOs;

public class ChatMessageDto
{
    [Required]
    public string ReceiverEmail { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
}
