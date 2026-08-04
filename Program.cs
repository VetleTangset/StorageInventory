using StorageInventory.Application;
using StorageInventory.Infrastructure;

var repository = new JsonInventoryRepository();
var service = new InventoryService(repository);

while (true)
{
    Console.WriteLine("1. Create box");
    Console.WriteLine("2. Delete box");
    Console.WriteLine("3. Add item to box");
    Console.WriteLine("4. Remove item from box");
    Console.WriteLine("5. List boxes");
    Console.WriteLine("6. Exit");
    var choice = Console.ReadLine();
    switch (choice)
    {
        case "1":
            await CreateBox();
            break;
        case "2":
            await DeleteBox();
            break;
        case "3":
            await AddItem();
            break;
        case "4":
            await RemoveItem();
            break;
        case "5":
            await ListBoxes();
            break;
        case "6":
            return;
        default:
            Console.WriteLine("Invalid choice, try again.");
            break;
    }
}
async Task CreateBox()
{
    var name = ReadString("Enter Box Name");
    var aisle = ReadString("Enter aisle");
    var shelf = ReadString("Enter shelf");

    await service.CreateBoxAsync(name, aisle, shelf);
}
async Task AddItem()
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
var itemName =ReadString("Enter item name" );
int quantityInput = int.TryParse(ReadString("Enter quantity (default 1)"), out var q) ? q : 1;

await service.AddItemToBoxAsync(boxId, itemName, quantityInput);
}
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
async Task DeleteBox()
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
    await service.DeleteBoxAsync(boxId);
}

static string ReadString(string message)
{
    Console.Write($"{message}: ");
    return Console.ReadLine() ?? "";
}