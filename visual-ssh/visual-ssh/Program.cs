using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace visualSSH;

class Program
{
    public static List<Server> servers = new();
    public static bool firstRun = true;
    public static int selectedIndex = 0;
    static void Main()
    {
        if (firstRun)
        {
            try
            {
                if (!File.Exists("connections.json"))
                {
                    File.Create("connections.json").Dispose();
                }
                servers = JsonSerializer.Deserialize<List<Server>>(File.ReadAllText("connections.json"), ServersJsonContext.Default.ListServer);
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine("[bold yellow]WARN: Could not read from connections file. This may be caused by first run[/]");
            }
            firstRun = false;
        }

        bool running = true;

        while (running)
        {
            Console.Clear();

            // Top function key bar
            AnsiConsole.MarkupLine("[green]Select a server to SSH into:[/]");
            AnsiConsole.MarkupLine("[grey](Use arrow keys to move, Enter to connect)[/]");

            // Display server list with highlight
            if (servers.Count != null)
            {
                for (int i = 0; i < servers.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        AnsiConsole.MarkupLine($"[black on yellow]> {servers[i].Name}[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"  {servers[i].Name}");
                    }
                }
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[black on bold blue]ADD[[A]]   EDIT[[E]]   DELETE[[D]]   EXIT[[Q]][/]");
            // Read key
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    try
                    {
                        selectedIndex = (selectedIndex - 1 + servers.Count) % servers.Count;
                    }
                    catch (Exception e)
                    {

                    }
                    break;
                case ConsoleKey.DownArrow:
                    try
                    {
                        selectedIndex = (selectedIndex + 1) % servers.Count;
                    }
                    catch (Exception e)
                    {
                    }
                    break;
                case ConsoleKey.Enter:
                    ConnectToServer(servers[selectedIndex]);
                    break;
                case ConsoleKey.A:
                    AddMenu();
                    break;
                case ConsoleKey.E:
                    EditServer();
                    break;
                case ConsoleKey.Q:
                    running = false;
                    break;
                case ConsoleKey.D:
                    DeleteServer();
                    break;
            }
        }

        AnsiConsole.MarkupLine("[red]Exited program.[/]");
        WriteToConnectionsFile();
        Environment.Exit(0);
    }

    static void ConnectToServer(Server server)
    {
        string sshTarget = $"{server.User}@{server.Host}";
        AnsiConsole.MarkupLine($"[yellow]Connecting to[/] {sshTarget}...");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ssh",
                Arguments = sshTarget,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false
            }
        };

        process.Start();
        process.WaitForExit();
        Main();
    }

    static void AddMenu()
    {
        //AnsiConsole.Clear();
        var menuAddTitle = new Spectre.Console.Rule("[green] Add New Server[/]");
        menuAddTitle.Justification = Justify.Left;
        AnsiConsole.Write(menuAddTitle);


        var hostName = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter Hostname or IP:")
        );
        var userName = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the Username:")
        );
        var displayName = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter a Display Name for the connection: ")
        );

        AnsiConsole.Clear();
        var menuAddConfirmTitle = new Spectre.Console.Rule("[green] Confirm Connection Details[/]");
        menuAddConfirmTitle.Justification = Justify.Left;
        AnsiConsole.Write(menuAddConfirmTitle);

        AnsiConsole.Markup($"[green]Host:[/] {hostName}\n[green]User Name:[/] {userName}\n[green]Display Name:[/] {displayName}");
        AnsiConsole.WriteLine();
        var confirmation = AnsiConsole.Prompt(new ConfirmationPrompt("Is everything correct?"));

        if (confirmation)
        {
            servers.Add(new Server { Host = hostName, User = userName, Name = displayName });
            WriteToConnectionsFile();
            Main();
        }

        var tryAgain = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Edit Details or Return to Main Menu?")
            .AddChoices(new string[] { "Main Menu", "Edit" }));

        switch (tryAgain)
        {
            case "Main Menu":
                break;
            case "Edit":
                AddMenu(); break;
        }

        Main();
    }

    public static void DeleteServer()
    {
        //AnsiConsole.Clear();
        var deleteTitle = new Spectre.Console.Rule("[red]Delete Server[/]");
        deleteTitle.Justification = Justify.Left;
        AnsiConsole.Write(deleteTitle);
        AnsiConsole.Markup("[red]Are you sure you want to delete this server?[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]Display Name:[/] {servers[selectedIndex].Name}\n[green]Host:[/] {servers[selectedIndex].Host}\n[green]Username:[/] {servers[selectedIndex].User}");
        var confirmation = AnsiConsole.Prompt(new TextPrompt<bool>("Delete Server?")
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(false)
            .WithConverter(choice => choice ? "y" : "n"));

        if (confirmation)
        {
            servers.Remove(servers[selectedIndex]);
            selectedIndex = selectedIndex--;
        }
    }

    public static void EditServer()
    {
        var editTitle = new Spectre.Console.Rule("[yellow]Edit Server[/]");
        editTitle.Justification = Justify.Left;
        AnsiConsole.Write(editTitle);

        var newHostName =
            AnsiConsole.Prompt(
                new TextPrompt<string>("[green]New Host:[/] ").DefaultValue(servers[selectedIndex].Host));
        var newUserName =
            AnsiConsole.Prompt(
                new TextPrompt<string>("[green]New User:[/] ").DefaultValue(servers[selectedIndex].User));
        var newDisplayName =
            AnsiConsole.Prompt(
                new TextPrompt<string>("[green]New Display Name:[/] ").DefaultValue(servers[selectedIndex].Name));

        var confirmation = AnsiConsole.Prompt(new TextPrompt<bool>("Confirm Changes?")
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(true)
            .WithConverter(choice => choice ? "y" : "n"));

        if (confirmation)
        {
            servers[selectedIndex].User = newUserName;
            servers[selectedIndex].Host = newHostName;
            servers[selectedIndex].Name = newDisplayName;
        }
    }

    public static void WriteToConnectionsFile()
    {
        string serializedList = JsonSerializer.Serialize<List<Server>>(servers, ServersJsonContext.Default.ListServer);
        try
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, "connections.json");
            //File.WriteAllText(filePath, serializedList);
            using (var writer = new StreamWriter(filePath, false)) // false = overwrite
            {
                writer.Write(serializedList);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
