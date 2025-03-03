using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppFolderPro.Databases;

public class FolderDatabaseManager
{
    private readonly JsonDataService _jsonService;

    public FolderDatabaseManager()
    {
        _jsonService = new JsonDataService();
    }

    public AppFolder? GetFolderById(int id)
    {
        var data = _jsonService.LoadData();
        return data.Folders
            .FirstOrDefault(f => f.Id == id);
    }

    public void AddFolder(AppFolder folder)
    {
        var data = _jsonService.LoadData();
        folder.Id = data.NextFolderId++;
        data.Folders.Add(folder);
        _jsonService.SaveData(data);
    }

    public void AddFileToFolder(int folderId, ItemFile file)
    {
        var data = _jsonService.LoadData();
        var folder = data.Folders.FirstOrDefault(f => f.Id == folderId);
        
        if (folder != null)
        {
            file.Id = data.NextFileId++;
            folder.Files.Add(file);
            _jsonService.SaveData(data);
        }
    }
    
    public void RemoveFolder(int folderId)
    {
        var data = _jsonService.LoadData();
        var folderToRemove = data.Folders.FirstOrDefault(f => f.Id == folderId);
        
        if (folderToRemove != null)
        {
            data.Folders.Remove(folderToRemove);
            _jsonService.SaveData(data);
        }
    }
    public List<AppFolder> GetFolderList()
    {
        var data = _jsonService.LoadData();
        return data.Folders;
    }
    public void MoveFileToPosition(int folderId, int fileId, int newPosition)
    {
        var data = _jsonService.LoadData();
        var folder = GetFolderById(folderId);
        if (folder == null)return;
        
        var file = folder.Files.FirstOrDefault(f => f.Id == fileId);
        if (file != null)
        {
            folder.Files.Remove(file);
            folder.Files.Insert(Math.Clamp(newPosition, 0, folder.Files.Count), file);
            _jsonService.SaveData(data);
        }
    }
    public void MoveFileUp(int folderId, int fileId)
    {
        var data = _jsonService.LoadData();
        var folder = data.Folders.FirstOrDefault(f => f.Id == folderId);
        
        if (folder != null)
        {
            var files = folder.Files;
            int index = files.FindIndex(f => f.Id == fileId);
            
            if (index > 0) // 첫 번째 항목이 아닌 경우
            {
                // 순서 교체
                (files[index], files[index - 1]) = (files[index - 1], files[index]);
                _jsonService.SaveData(data);
            }
        }
    }

    // 특정 파일을 한 칸 아래로 이동
    public void MoveFileDown(int folderId, int fileId)
    {
        var data = _jsonService.LoadData();
        var folder = data.Folders.FirstOrDefault(f => f.Id == folderId);
        
        if (folder != null)
        {
            var files = folder.Files;
            int index = files.FindIndex(f => f.Id == fileId);
            
            if (index < files.Count - 1) // 마지막 항목이 아닌 경우
            {
                // 순서 교체
                (files[index], files[index + 1]) = (files[index + 1], files[index]);
                _jsonService.SaveData(data);
            }
        }
    }
    public AppFolder? FindFolderByItemId(int itemId)
    {
        var data = _jsonService.LoadData();
        return data.Folders
            .FirstOrDefault(folder => 
                folder.Files.Any(file => file.Id == itemId)
            );
    }
    public bool RemoveFileFromFolder(int folderId, int fileId)
    {
        var data = _jsonService.LoadData();
        var folder = data.Folders.FirstOrDefault(f => f.Id == folderId);
        
        if (folder != null)
        {
            var file = folder.Files.FirstOrDefault(f => f.Id == fileId);
            if (file != null)
            {
                folder.Files.Remove(file);
                _jsonService.SaveData(data);
                return true;
            }
        }
        return false;
    }
}

// JSON 데이터 구조
public class AppData
{
    public int NextFolderId { get; set; } = 1;
    public int NextFileId { get; set; } = 1;
    public List<AppFolder> Folders { get; set; } = new();
}

public class JsonDataService
{
    private static readonly string LocalApplicationData = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "AppFolderPro",
        "data.json"
    );

    public AppData LoadData()
    {
        var directory = Path.GetDirectoryName(LocalApplicationData);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        if (!File.Exists(LocalApplicationData))
        {
            return new AppData();
        }

        var json = File.ReadAllText(LocalApplicationData);
        return JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
    }

    public void SaveData(AppData data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(LocalApplicationData, json);
    }
}

public class ItemFile
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
    
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}

public class AppFolder
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("files")]
    public List<ItemFile> Files { get; set; } = new();
}