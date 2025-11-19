namespace ChecklistAPI.Models.Entities;

public class TemplateItem
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string ItemText { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? StatusOptions { get; set; }
    public string? AllowedPositions { get; set; }
    public string? DefaultNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Template Template { get; set; } = null!;
}
