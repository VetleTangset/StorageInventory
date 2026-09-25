using StorageInventory.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace StorageInventory.Domain;

public class Box
{
    private readonly List<Item> _items;

    public Guid Id { get; }
    public string Name { get; private set; }
    public Location Location { get; private set; }

    public IReadOnlyCollection<Item> Items => _items.AsReadOnly();

    // Creates a new box with a fresh Id
    public Box(string name, Location location)
    {
        Id = Guid.NewGuid();
        Name = name;
        Location = location;
        _items = new();
    }

    // Used for deserialization, to rebuild an existing box (keeps original Id)
    [JsonConstructor]
    public Box(Guid id, string name, Location location, IReadOnlyCollection<Item>? items)
    {
        Id = id;
        Name = name;
        Location = location;
        _items = items?.ToList() ?? new List<Item>();
    }

    // Increase quantity if item already exists, otherwise add it
    public void AddItem(string name, int quantity)
    {
        var existing = _items.FirstOrDefault(i => i.Name == name);
        if (existing != null)
        {
            existing.Increase(quantity);
            return;
        }
        _items.Add(new Item(name, quantity));
    }

    // Remove an item from the box
    public void RemoveItem(string name, int quantity)
    {
        var existing = _items.FirstOrDefault(i => i.Name == name);
        if (existing == null)
            throw new InvalidOperationException($"Item '{name}' not found in box '{Name}'.");
        if (existing.Quantity < quantity)
            throw new InvalidOperationException($"Not enough quantity of item '{name}' to remove. Available: {existing.Quantity}, Requested: {quantity}.");
        existing.Decrease(quantity);
        if (existing.Quantity == 0)
            _items.Remove(existing);
    }
}
