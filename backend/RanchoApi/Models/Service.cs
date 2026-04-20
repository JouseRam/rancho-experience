namespace RanchoApi.Models;

public class Service
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
}
