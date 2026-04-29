public class ChatMessageDto
{
    public string SenderEmail { get; set; } = "";
    public string ReceiverEmail { get; set; } = "";
    public string? Content { get; set; }
    public bool IsVoice { get; set; }
    public string? VoiceUrl { get; set; }
}