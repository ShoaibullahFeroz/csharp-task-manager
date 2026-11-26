namespace TaskManager;

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3
}

public sealed class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateOnly? DueDate { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public bool IsOverdue =>
        !IsComplete && DueDate is not null && DueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    public override string ToString()
    {
        var status = IsComplete ? "[x]" : "[ ]";
        var due = DueDate is null ? "no due date" : $"due {DueDate:yyyy-MM-dd}";
        var overdue = IsOverdue ? " OVERDUE" : string.Empty;
        return $"{Id,3}. {status} {Title} ({Priority}, {due}){overdue}";
    }
}
