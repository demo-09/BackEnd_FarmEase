namespace backEnd.DTOs;

public class MachineryMediaDto
{
    public long Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public string MediaType { get; set; } = "image";
    public bool IsPrimary { get; set; }
}
