namespace ChecklistAPI.Models.Enums;

/// <summary>
/// Template type - determines how checklist instances are created
/// </summary>
public enum TemplateType
{
    /// <summary>
    /// Manual creation - user manually creates checklist from template library
    /// This is the default behavior.
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Auto-create - automatically creates checklist when event category matches
    /// Template specifies which incident categories trigger auto-creation.
    /// When event's incident type changes to matching category, system auto-creates instance.
    /// </summary>
    AutoCreate = 1,

    /// <summary>
    /// Recurring - automatically creates checklists on a schedule
    /// Future feature for daily, per-shift, or per-operational-period checklists.
    /// Requires recurrence configuration (frequency, start/end dates).
    /// </summary>
    Recurring = 2
}