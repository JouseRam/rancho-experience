namespace RanchoApi.Models;

public class GalleryImage
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
