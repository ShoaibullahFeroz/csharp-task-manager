# C# Task Manager

A small console task manager written in C# (.NET 8). Tasks are stored in a local `tasks.json` file, so they survive between runs. No database, no web server, no setup beyond the .NET SDK.

## Features

- Add tasks with a priority (low / medium / high), an optional due date, and notes
- List tasks — all, open, done, by priority, or overdue only
- Complete and delete tasks (delete asks for confirmation)
- Search tasks by title or notes
- Overdue tasks are flagged in the list

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build and run

```bash
dotnet run -- add "Finish quarterly report" --priority high --due 2026-12-05
dotnet run -- list
```

Or build once and run the binary:

```bash
dotnet build
./bin/Debug/net8.0/task-manager list
```

## Example commands

```bash
# Add tasks
dotnet run -- add "Buy groceries" --priority low
dotnet run -- add "Call the bank" --priority high --due 2026-11-28 --notes "Ask about the new rate"
dotnet run -- add "Read chapter 4"

# List open tasks, highest priority first
dotnet run -- list --open

# Only high-priority tasks
dotnet run -- list --priority high

# Overdue tasks
dotnet run -- list --overdue

# Completed tasks
dotnet run -- list --done

# Search by text (title or notes)
dotnet run -- search bank

# Complete task 2, then delete task 1
dotnet run -- complete 2
dotnet run -- delete 1
```

## Example output

```
  2. [ ] Call the bank (High, due 2026-11-28)
      Ask about the new rate
  1. [ ] Buy groceries (Low, no due date)

2 open, 0 done.
```

## Data

Tasks are saved to `tasks.json` in the folder you run the program from.

## Project layout

| File | Purpose |
| --- | --- |
| `Program.cs` | Command-line parsing and output |
| `TaskItem.cs` | The task model, priorities, and overdue logic |
| `TaskStore.cs` | Loading, saving, searching, and filtering tasks in JSON |
| `csharp-task-manager.csproj` | .NET 8 project file |
