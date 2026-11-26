using System.Text.Json;

namespace TaskManager;

public sealed class TaskStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private List<TaskItem> _tasks = new();
    private int _nextId = 1;

    public TaskStore(string filePath)
    {
        _filePath = filePath;
        Load();
    }

    public IReadOnlyList<TaskItem> All => _tasks.AsReadOnly();

    public TaskItem Add(string title, Priority priority, DateOnly? dueDate, string? notes)
    {
        var task = new TaskItem
        {
            Id = _nextId++,
            Title = title,
            Priority = priority,
            DueDate = dueDate,
            Notes = notes
        };
        _tasks.Add(task);
        Save();
        return task;
    }

    public bool Complete(int id)
    {
        var task = Find(id);
        if (task is null || task.IsComplete)
        {
            return false;
        }

        task.IsComplete = true;
        task.CompletedAt = DateTime.UtcNow;
        Save();
        return true;
    }

    public bool Delete(int id)
    {
        var task = Find(id);
        if (task is null)
        {
            return false;
        }

        _tasks.Remove(task);
        Save();
        return true;
    }

    public TaskItem? Find(int id) => _tasks.FirstOrDefault(t => t.Id == id);

    public IEnumerable<TaskItem> Search(string term) =>
        _tasks.Where(t =>
            t.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            (t.Notes is not null && t.Notes.Contains(term, StringComparison.OrdinalIgnoreCase)));

    public IEnumerable<TaskItem> Filter(bool? complete = null, Priority? priority = null, bool overdueOnly = false)
    {
        IEnumerable<TaskItem> result = _tasks;
        if (complete is not null)
        {
            result = result.Where(t => t.IsComplete == complete.Value);
        }
        if (priority is not null)
        {
            result = result.Where(t => t.Priority == priority.Value);
        }
        if (overdueOnly)
        {
            result = result.Where(t => t.IsOverdue);
        }
        return result;
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<StoreData>(json, _jsonOptions);
            if (data is null)
            {
                return;
            }

            _tasks = data.Tasks;
            _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;
        }
        catch (JsonException)
        {
            _tasks = new List<TaskItem>();
            _nextId = 1;
        }
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var data = new StoreData { Tasks = _tasks };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private sealed class StoreData
    {
        public List<TaskItem> Tasks { get; set; } = new();
    }
}
