namespace StorageInventory.Domain.ValueObjects;

// Represents the location of a box in the storage, defined by aisle and shelf
public record Location(string Aisle, string Shelf);
