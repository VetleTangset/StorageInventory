using StorageInventory.Domain;

namespace StorageInventory.Infrastructure;

public interface IInventoryRepository
{
    Task AddAsync(Box box);
    Task UpdateAsync(Box box);
    Task DeleteAsync(Guid boxId);
    Task<IEnumerable<Box>> GetAllAsync();
}