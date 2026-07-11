using System.Text.Json;
using Spectre.Console;

const string createLinkChoice = "1. Создать ссылку";
const string findLinkChoice = "2. Найти ссылку";
const string showAllLinksChoice = "3. Посмотреть все ссылки";
const string deleteLinkChoice = "4. Удалить ссылку";
const string exitChoice = "5. Выход";
const string cancelChoice = "! Отмена";

const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

var dbPath = Path.Combine(AppContext.BaseDirectory, "db.links.json");

const int codeLength = 7;

var linksDb = Load(dbPath);

while (true)
{
    AnsiConsole.Clear();
    RenderHeader(linksDb);
    
    var userChoice = AnsiConsole.Prompt(new SelectionPrompt<string>()
        .Title("[underline blue]Что делаем?[/]")
        .PageSize(6)
        .HighlightStyle(new Style(Color.SpringGreen1))
        .AddChoices(
            createLinkChoice,
            findLinkChoice,
            showAllLinksChoice,
            deleteLinkChoice,
            exitChoice)
    );
    
    AnsiConsole.Clear();
    RenderHeader(linksDb);
    
    switch (userChoice)
    {
        case createLinkChoice:
            CreateLink(linksDb, dbPath);
            break;
        case findLinkChoice:
            GetLink(linksDb);
            break;
        case showAllLinksChoice:
            GetAllLinks(linksDb);
            break;
        case deleteLinkChoice:
            RemoveLink(linksDb, dbPath);
            break;
        case exitChoice:
            AnsiConsole.MarkupLine("[blue italic]Пока пока...[/]");
            return;
    }
    
    AnsiConsole.MarkupLine("Нажмите что-нибудь...");
    Console.ReadKey(true);
}

static void RenderHeader(Dictionary<string, string> db)
{
    AnsiConsole.Write(new FigletText("Shortener")
        .Centered()
        .Color(Color.Orchid));

    var rule = new Rule($"[grey] ссылко в базе:[/] [springgreen1]{db.Count}[/]")
    {
        Style = Style.Parse("grey"),
        Justification = Justify.Center
    };
    
    AnsiConsole.Write(rule);
}

static void CreateLink(Dictionary<string, string> db, string dbPath)
{
    var url = AnsiConsole.Prompt(
        new TextPrompt<string>("[springgreen1]>[/] Введите свой [bold]URL[/]:")
        .Validate(u => ValidateUserLinkInput(db, u, out var errorMessage) ? 
            ValidationResult.Success() : ValidationResult.Error(errorMessage)));

    var urlCode = GenerateLinkCode(db.ContainsKey);
    db.Add(urlCode, url.Trim());
    Save(db, dbPath);

    var panel = new Panel(new Markup($"Короткий код: [springgreen1]{urlCode}[/]\n" +
                                     $"Оригинальный url: {url.Trim().EscapeMarkup()}"))
    {
        Header = new PanelHeader("Ссылка создана", Justify.Center),
        Border = BoxBorder.Rounded,
        BorderStyle = Style.Parse("springgreen2"),
        Padding = new Padding(2, 1)
    };
    AnsiConsole.Write(panel);
}

static bool ValidateUserLinkInput(Dictionary<string, string> db, string input, out string? errorMessage)
{
    try
    {
        var uri = new Uri(input);
    }
    catch (UriFormatException)
    {
        errorMessage = "Переданный инпут [red]не ссылка[/]";
        return false;
    }
    
    if (db.ContainsValue(input))
    {
        var value = db.Keys.First(k => db[k] == input);
        errorMessage = $"Такая ссылка [red]уже есть[/] в базе. Её код {value}";
        return false;
    }

    if (input.Trim().Length == 0)
    {
        errorMessage = "Ссылка должна быть [red]непустой[/]";
        return false;
    }
    
    errorMessage = null;
    return true;
}

static void GetLink(Dictionary<string, string> db)
{
    var code = AnsiConsole.Ask<string>("[springgreen1]>[/] Введите [bold]код[/]: ").Trim();

    if (db.TryGetValue(code, out var url))
    {
        var panel = new Panel(new Markup($"[bold springgreen1]{code}[/] -> {url.EscapeMarkup()}"))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("springgreen2"),
            Padding = new Padding(2,1)
        };
        AnsiConsole.Write(panel);
    }
    else
    {
        AnsiConsole.Write(new Panel($"[red]Ссылка с кодом {code} не найдена[/]")
        {
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("red"),
            Padding = new Padding(2,1)
        });
    }
}

static void GetAllLinks(Dictionary<string, string> db)
{
    if (db.Count == 0)
    {
        AnsiConsole.MarkupLine("БД [red]пустая[/]");
        return;
    }

    var linksTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Grey)
        .Title("[bold springgreen1]Все ссылки[/]")
        .ShowRowSeparators()
        .Expand();
    
    linksTable.AddColumns("[bold]код ссылки[/]", "[bold]URL[/]");

    foreach (var keyValuePair in db)
    {
        linksTable.AddRow($"{keyValuePair.Key}", keyValuePair.Value);
    }
    
    AnsiConsole.Write(linksTable);
}

static void RemoveLink(Dictionary<string, string> db, string dbPath)
{
    if (db.Count == 0)
    {
        AnsiConsole.MarkupLine("БД [red]пустая[/]");
        return;
    }

    var codes = new List<string>(db.Keys)
    {
        cancelChoice
    };

    var code = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Выеберите ссылку для [red]удаления[/]")
            .HighlightStyle(new Style(Color.Red1))
            .PageSize(12)
            .AddChoices(codes)
    );

    if (code == cancelChoice)
    {
        AnsiConsole.MarkupLine("[red1]Отменено.[/]");
        return;
    }

    var confirm = AnsiConsole.Confirm($"Уверен в удалении [springgreen1]{code}[/]?", false);

    if (!confirm)
    {
        AnsiConsole.MarkupLine("[red1]Отменено.[/]");
        return;
    }

    db.Remove(code);
    Save(db, dbPath);
    AnsiConsole.MarkupLine($"Удалено: [red]{code}[/]");
}

static string GenerateLinkCode(Predicate<string> isDuplicate)
{
    var rng = new Random();
    string result;

    do
    {
        var chars = new char[codeLength];
    
        foreach (var i in Enumerable.Range(0, codeLength))
        {
            chars[i] = alphabet[rng.Next(alphabet.Length)];
        }

        result = new string(chars);
    } while (isDuplicate(result));
    
    return result;
}

static Dictionary<string, string> Load(string dbPath)
{
    if (!File.Exists(dbPath)) return new Dictionary<string, string>();

    try
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(dbPath)) ?? new Dictionary<string, string>();
    }
    catch
    {
        return new Dictionary<string, string>();
    }
}

static void Save(Dictionary<string, string> db, string dbPath)
{
    File.WriteAllText(dbPath, JsonSerializer.Serialize(db, new JsonSerializerOptions { WriteIndented = true}));
}
