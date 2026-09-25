using StorageInventory.Domain;

namespace StorageInventory.Infrastructure;

// Interface for the inventory repository, defining methods for adding, updating, deleting, and retrieving boxes
public interface IInventoryRepository
{
    Task AddAsync(Box box);
    Task UpdateAsync(Box box);
    Task DeleteAsync(Guid boxId);
    Task<IEnumerable<Box>> GetAllAsync();
}