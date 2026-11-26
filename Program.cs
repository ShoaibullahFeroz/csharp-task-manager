using TaskManager;

const string DataFile = "tasks.json";

var store = new TaskStore(DataFile);

if (args.Length == 0)
{
    PrintUsage();
    return 0;
}

var command = args[0].ToLowerInvariant();

return command switch
{
    "add" => Add(store, args),
    "list" => List(store, args),
    "complete" => Complete(store, args),
    "delete" => Delete(store, args),
    "search" => Search(store, args),
    "help" or "--help" or "-h" => Help(),
    _ => Unknown(command)
};

static int Help()
{
    PrintUsage();
    return 0;
}

static int Unknown(string command)
{
    Console.WriteLine($"Unknown command: {command}");
    PrintUsage();
    return 1;
}

static void PrintUsage()
{
    Console.WriteLine("""
        task-manager — a small console task manager. Tasks are stored in tasks.json.

        Usage:
          task-manager add "Buy groceries" [--priority high|medium|low] [--due 2026-12-01] [--notes "..."]
          task-manager list [--all | --open | --done] [--priority high|medium|low] [--overdue]
          task-manager complete <id>
          task-manager delete <id>
          task-manager search <text>
          task-manager help
        """);
}

static Priority ParsePriority(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return Priority.Medium;
    }

    return value.ToLowerInvariant() switch
    {
        "low" => Priority.Low,
        "medium" => Priority.Medium,
        "high" => Priority.High,
        _ => throw new ArgumentException($"Unknown priority: {value}. Use low, medium, or high.")
    };
}

static string? Option(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == name)
        {
            return args[i + 1];
        }
    }
    return null;
}

static bool HasFlag(string[] args, string name) =>
    args.Any(a => a == name);

static int Add(TaskStore store, string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: task-manager add \"Task title\" [--priority high] [--due 2026-12-01]");
        return 1;
    }

    try
    {
        var priority = ParsePriority(Option(args, "--priority"));
        DateOnly? due = null;
        var dueText = Option(args, "--due");
        if (dueText is not null)
        {
            due = DateOnly.Parse(dueText);
        }

        var task = store.Add(args[1], priority, due, Option(args, "--notes"));
        Console.WriteLine($"Added: {task}");
        return 0;
    }
    catch (Exception ex) when (ex is ArgumentException or FormatException)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}

static void PrintTasks(IEnumerable<TaskItem> tasks)
{
    var ordered = tasks
        .OrderBy(t => t.IsComplete)
        .ThenByDescending(t => t.Priority)
        .ThenBy(t => t.DueDate ?? DateOnly.MaxValue)
        .ThenBy(t => t.Id)
        .ToList();

    if (ordered.Count == 0)
    {
        Console.WriteLine("No tasks found.");
        return;
    }

    foreach (var task in ordered)
    {
        Console.WriteLine(task);
        if (!string.IsNullOrWhiteSpace(task.Notes))
        {
            Console.WriteLine($"      {task.Notes}");
        }
    }

    var open = ordered.Count(t => !t.IsComplete);
    var done = ordered.Count - open;
    Console.WriteLine($"\n{open} open, {done} done.");
}

static int List(TaskStore store, string[] args)
{
    bool? complete = null;
    if (HasFlag(args, "--done"))
    {
        complete = true;
    }
    else if (HasFlag(args, "--open"))
    {
        complete = false;
    }

    Priority? priority = null;
    var priorityText = Option(args, "--priority");
    if (priorityText is not null)
    {
        try
        {
            priority = ParsePriority(priorityText);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    PrintTasks(store.Filter(complete, priority, HasFlag(args, "--overdue")));
    return 0;
}

static int Complete(TaskStore store, string[] args)
{
    if (args.Length < 2 || !int.TryParse(args[1], out var id))
    {
        Console.WriteLine("Usage: task-manager complete <id>");
        return 1;
    }

    if (!store.Complete(id))
    {
        Console.WriteLine($"Task {id} not found, or already complete.");
        return 1;
    }

    Console.WriteLine($"Completed task {id}.");
    return 0;
}

static int Delete(TaskStore store, string[] args)
{
    if (args.Length < 2 || !int.TryParse(args[1], out var id))
    {
        Console.WriteLine("Usage: task-manager delete <id>");
        return 1;
    }

    Console.Write($"Delete task {id}? [y/N] ");
    if (Console.ReadLine()?.Trim().ToLowerInvariant() != "y")
    {
        Console.WriteLine("Cancelled.");
        return 0;
    }

    if (!store.Delete(id))
    {
        Console.WriteLine($"Task {id} not found.");
        return 1;
    }

    Console.WriteLine($"Deleted task {id}.");
    return 0;
}

static int Search(TaskStore store, string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: task-manager search <text>");
        return 1;
    }

    PrintTasks(store.Search(args[1]));
    return 0;
}
