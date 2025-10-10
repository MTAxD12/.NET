using System;
using System.Collections.Generic;
using System.Linq;

Manager manager = new Manager
{
    Name = "Alex",
    Team = "Dev",
    Email = "a@a.com"
};

Console.WriteLine($"Manager: {manager.Name} ({manager.Team}) - {manager.Email}\n");

var task1 = new Task { Title = "Implementeaza AI", IsCompleted = true, DueDate = new DateTime(2025, 10, 5) };
var task2 = new Task { Title = "Testeaza", IsCompleted = false, DueDate = new DateTime(2025, 10, 8) };
var task3 = new Task { Title = "Spala", IsCompleted = false, DueDate = new DateTime(2025, 10, 15) };

var project = new Project
{
    Name = "Web Application",
    Tasks = new List<Task> { task1, task2 }
};

Console.WriteLine("Proiect initial:");
Utils.Display(project);
Console.WriteLine();

var updatedProject = project with
{
    Tasks = new List<Task>(project.Tasks) { task3 }
};

Console.WriteLine("Primul proiect:");
Utils.Display(project);
Console.WriteLine($"  Taskurile primului proiect: {project.Tasks.Count}\n");

Console.WriteLine("Al doilea proiect (copiat): ");
Utils.Display(updatedProject);
Console.WriteLine($"  Taskurile la al doilea proiect: {updatedProject.Tasks.Count}");
Console.WriteLine();

Console.WriteLine("Toate taskurile");
foreach (var task in updatedProject.Tasks)
{
    Utils.Display(task);
}
Console.WriteLine();

Console.WriteLine("Taskuri overdue sau necompletate");
var overdueTasks = updatedProject.Tasks.Where(Utils.IsOverdueOrNotCompleted).ToList();

if (overdueTasks.Count > 0)
{
    foreach (var task in overdueTasks)
    {
        Console.WriteLine($"  '{task.Title}' - Pana la data: {task.DueDate:yyyy-MM-dd}");
    }
}
else
{
    Console.WriteLine("  Fara taskuri overdue!");
}
Console.WriteLine();


//Userul adauga taskuld
Console.Write("Adaugati task \n Title: ");
string userTitle = Console.ReadLine() ?? "task fara nume";

Console.Write("Completat? (y/n)");
bool userCompleted = Console.ReadLine()?.ToLower() == "y";

Console.Write("Due date (aaaa-ll-zz): ");
DateTime userDueDate;
if (!DateTime.TryParse(Console.ReadLine(), out userDueDate))
{
    userDueDate = DateTime.Now.AddDays(7);
}

var userTask = new Task
{
    Title = userTitle,
    IsCompleted = userCompleted,
    DueDate = userDueDate
};

var finalProject = updatedProject with
{
    Tasks = new List<Task>(updatedProject.Tasks) { userTask }
};

Console.WriteLine("\n Proiectul final al userului");
Utils.Display(finalProject);
Console.WriteLine();

Console.WriteLine("Lista taskurilor actualizata:");
foreach (var task in finalProject.Tasks)
{
    Utils.Display(task);
}

record Task
{
    public string Title { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime DueDate { get; init; }
}

record Project
{
    public string Name { get; init; }
    public List<Task> Tasks { get; init; }
}

class Manager
{
    public string Name { get; init; }
    public string Team { get; init; }
    public string Email { get; init; }
}

static class Utils
{
    public static void Display(object item)
    {
        switch (item)
        {
            case Task t:
                Console.WriteLine($"Task: '{t.Title}' - Status: {(t.IsCompleted ? "Complet" : "Nu e complet")}");
                break;
            case Project p:
                Console.WriteLine($"Project: '{p.Name}' - Nr taskuri: {p.Tasks.Count}");
                break;
            default:
                Console.WriteLine("Tip necunoscut");
                break;
        }
    }

    public static readonly Func<Task, bool> IsOverdueOrNotCompleted = static task =>
        !task.IsCompleted && task.DueDate < DateTime.Now;
}
