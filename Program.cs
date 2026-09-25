using Spectre.Console;
using StorageInventory.Application;
using StorageInventory.Infrastructure;
using StorageInventory.Domain;

// Entry point of the Storage Inventory application
var repository = new JsonInventoryRepository();
var service = new InventoryService(repository);

while (true)
{
    Console.Clear();

    AnsiConsole.Write(
        new FigletText("Storage Inventory")
            .Centered()
            .Color(Color.Gold1));

    // Display the main menu and prompt the user for an action
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[blue]What would you like to do?[/]")
            .PageSize(10)
            .HighlightStyle(new Style(foreground: Color.Gray))
            .AddChoices(new[]
            {
                "View all boxes",
                "Add a new box",
                "Add item to box",
                "Remove item from box",
                "Remove box",
                "Exit"
            }));

    // Handle the user's choice and call the appropriate method
    switch (choice)
    {
        case "View all boxes":
            await ListBoxes();
            Pause();
            break;
        case "Add a new box":
            await CreateBox();
            Pause();
            break;
        case "Add item to box":
            await AddItem();
            Pause();
            break;
        case "Remove item from box":
            await RemoveItem();
            Pause();
            break;
        case "Remove box":
            await DeleteBox();
            Pause();
            break;
        case "Exit":
            AnsiConsole.MarkupLine("[red]Exiting...[/]");
            return;
    }
}
// Pauses the console until the user presses Enter
void Pause()
{
    AnsiConsole.MarkupLine("Press [yellow]Enter[/] to continue...");
    Console.ReadLine();
}

// Prompts the user to create a new box and adds it to the inventory
async Task CreateBox()
{
    var boxes = (await service.GetAllAsync()).ToList();

    var name = ReadString("Enter Box Name");

    if (boxes.Any(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine($"A box named '{name}' already exists.");
        return;
    }

    var aisle = ReadString("Enter aisle");
    var shelf = ReadString("Enter shelf");

    await service.CreateBoxAsync(name, aisle, shelf);
}

// Adds an item to a selected box
async Task AddItem()
{
    var box = await SelectBoxByNameAsync();
    if (box is null)
        return;

    var itemName = ReadString("Enter item name");
    int quantityInput = int.TryParse(ReadString("Enter quantity (default 1)"), out var q) ? q : 1;

    await service.AddItemToBoxAsync(box.Id, itemName, quantityInput);
}

// Lists all boxes and their items
async Task ListBoxes()
{
    var boxes = await service.GetAllAsync();

    foreach (var box in boxes)
    {
        Console.WriteLine($"Box: {box.Name} ({box.Location.Aisle}-{box.Location.Shelf})");

        foreach (var item in box.Items)
        {
            Console.WriteLine($"  - {item.Name}: {item.Quantity}");
        }
    }
}

// Removes an item from a selected box
async Task RemoveItem()
{
    var boxes = await service.GetAllAsync();
    foreach (var box in boxes)
    {
        Console.WriteLine($"{box.Name} - {box.Id}");
    }
    var idInput = ReadString("Enter box id");
    if (!Guid.TryParse(idInput, out var boxId))
    {
        Console.WriteLine("Invalid box id");
        return;
    }
    var itemName = ReadString("Enter item name");
    int quantityInput = int.TryParse(ReadString("Enter quantity (default 1)"), out var q) ? q : 1;
    try
    {
        await service.RemoveItemFromBoxAsync(boxId, itemName, quantityInput);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine(ex.Message);
    }
}

// Deletes a selected box from the inventory
async Task DeleteBox()
{
    var box = await SelectBoxByNameAsync();
    if (box is null)
        return;

    await service.DeleteBoxAsync(box.Id);
}

// Prompts the user to select a box by name and returns the selected box or null if not found
async Task<Box?> SelectBoxByNameAsync()
{
    var boxes = (await service.GetAllAsync()).ToList();

    if (boxes.Count == 0)
    {
        Console.WriteLine("No boxes found.");
        return null;
    }

    foreach (var box in boxes)
    {
        Console.WriteLine($"{box.Name}");
    }

    var name = ReadString("Enter box name");
    var selectedBox = boxes.FirstOrDefault(b =>
        b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    if (selectedBox == null)
    {
        Console.WriteLine($"No box named '{name}' was found.");
    }

    return selectedBox;
}

// Reads a string input from the console with a prompt message
static string ReadString(string message)
{
    Console.Write($"{message}: ");
    return Console.ReadLine() ?? "";
}