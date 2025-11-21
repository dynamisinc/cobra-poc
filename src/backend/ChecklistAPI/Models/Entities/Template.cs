using ChecklistAPI.Models.Enums;

namespace ChecklistAPI.Models.Entities;

public class Template
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty; // JSON array
    public bool IsActive { get; set; } = true;
    public bool IsArchived { get; set; } = false;
    public string? ArchivedBy { get; set; }
    public DateTime? ArchivedAt { get; set; }

    // Template type and auto-creation configuration
    public TemplateType TemplateType { get; set; } = TemplateType.Manual;
    public string? AutoCreateForCategories { get; set; } // JSON array of incident categories (e.g., ["Hurricane", "Flood"])
    public string? RecurrenceConfig { get; set; } // JSON configuration for recurring templates (future feature)

    // Audit fields
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByPosition { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
    public string? LastModifiedByPosition { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    
    // Navigation
    public ICollection<TemplateItem> Items { get; set; } = new List<TemplateItem>();
}
