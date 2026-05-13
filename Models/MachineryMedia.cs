using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backEnd.Models;

public class MachineryMedia
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long MachineryId { get; set; }

    [ForeignKey("MachineryId")]
    public Machinery? Machinery { get; set; }

    [Required]
    public string Url { get; set; } = string.Empty;

    public string PublicId { get; set; } = string.Empty;

    public string MediaType { get; set; } = "image"; // "image" or "video"

    public bool IsPrimary { get; set; } = false;
}
