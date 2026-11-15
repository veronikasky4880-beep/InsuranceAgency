using System.Text.Json;

namespace InsuranceAgency.Core.Services;

public class JsonFileService
{
    private readonly string _filePath;

    public JsonFileService(string filePath)
    {
        _filePath = filePath;
    }

    public AppData Load()
    {
        if (!File.Exists(_filePath))
            return new AppData();

        string json = File.ReadAllText(_filePath);
        AppData data = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();

        data.Clients ??= new();
        data.Agents ??= new();
        data.Policies ??= new();
        data.Claims ??= new();
        return data;
    }

    public void Save(AppData data)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(_filePath, json);
    }
}
