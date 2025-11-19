namespace ChecklistAPI.Models.Entities;

public class ChecklistInstance
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    
    // Event context
    public string EventId { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string? OperationalPeriodId { get; set; }
    public string? OperationalPeriodName { get; set; }
    public string? AssignedPositions { get; set; }
    
    // Progress tracking
    public decimal ProgressPercentage { get; set; } = 0;
    public int TotalItems { get; set; } = 0;
    public int CompletedItems { get; set; } = 0;
    public int RequiredItems { get; set; } = 0;
    public int RequiredItemsCompleted { get; set; } = 0;
    
    public bool IsArchived { get; set; } = false;
    public string? ArchivedBy { get; set; }
    public DateTime? ArchivedAt { get; set; }
    
    // Audit
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByPosition { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
    public string? LastModifiedByPosition { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    
    // Navigation
    public Template Template { get; set; } = null!;
    public ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
}
