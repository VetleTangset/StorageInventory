namespace StorageInventory.Domain;

// Represents an item stored in a box, with a name and quantity
public class Item
{
    public string Name { get; }
    public int Quantity { get; private set; }

    public Item(string name, int quantity)
    {
        Name = name;
        Quantity = quantity;
    }

    public void Increase(int amount) => Quantity += amount;

    public void Decrease(int amount)
    {
        if (amount > Quantity)
            throw new InvalidOperationException("Not enough items");
        Quantity -= amount;
    }
}