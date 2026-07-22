using System.Text.Json;

namespace RentalBlazorApp.Services;


public class JsonDataService
{
    private readonly string _dataDir;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonDataService(IWebHostEnvironment env)
    {
        _dataDir = Path.Combine(env.ContentRootPath, "Data");
        if (!Directory.Exists(_dataDir))
            Directory.CreateDirectory(_dataDir);
    }

    public async Task<List<T>> LoadAsync<T>(string fileName)
    {
        var path = Path.Combine(_dataDir, fileName);
        if (!File.Exists(path))
            return new List<T>();

        await _lock.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync<T>(string fileName, List<T> data)
    {
        var path = Path.Combine(_dataDir, fileName);
        await _lock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(path, json);
        }
        finally
        {
            _lock.Release();
        }
    }
}
