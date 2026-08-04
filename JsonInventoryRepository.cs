using System.Text.Json;
using StorageInventory.Domain;

namespace StorageInventory.Infrastructure;

public class JsonInventoryRepository : IInventoryRepository
{
    private const string FileName = "inventory.json";

    public async Task AddAsync(Box box)
    {
        var boxes = (await GetAllAsync()).ToList();
        boxes.Add(box);
        var json = JsonSerializer.Serialize(boxes, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(FileName, json);
    }

    public async Task UpdateAsync(Box box)
    {
        var boxes = (await GetAllAsync()).ToList();
        var index = boxes.FindIndex(b => b.Id == box.Id);
        if (index != -1)
        {
            boxes[index] = box;
            var json = JsonSerializer.Serialize(boxes, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FileName, json);
        }
    }

    public async Task DeleteAsync(Guid boxId)
    {
        var boxes = (await GetAllAsync()).ToList();
        var removed = boxes.RemoveAll(b => b.Id == boxId);

        if (removed > 0)
        {
            var json = JsonSerializer.Serialize(boxes, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FileName, json);
        }
    }

    public async Task<IEnumerable<Box>> GetAllAsync()
    {
        if (!File.Exists(FileName))
            return new List<Box>();

        var json = await File.ReadAllTextAsync(FileName);
        return JsonSerializer.Deserialize<List<Box>>(json) ?? new List<Box>();
    }
}