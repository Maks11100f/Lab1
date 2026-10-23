namespace Lab1.Services;

/// <summary>Таблиця, прочитана з файлу на комп'ютері.</summary>
public record LocalTableFile(string Name, string FullPath, string Content);

/// <summary>
/// Робота з таблицями на комп'ютері:
/// збереження — у папку "Документи\Lab1Tables",
/// відкриття — файлу .json з будь-якої папки (через стандартне вікно вибору файлу).
/// </summary>
public class LocalFileService
{
    public string FolderPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Lab1Tables");

    /// <summary>Зберігає текст у файл name.json і повертає повний шлях.</summary>
    public string Save(string name, string content)
    {
        Directory.CreateDirectory(FolderPath);
        string path = Path.Combine(FolderPath, name + ".json");
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Показує стандартне вікно вибору файлу (.json) і читає вибраний файл.
    /// Повертає null, якщо користувач нічого не вибрав.
    /// </summary>
    public async Task<LocalTableFile?> PickAndReadAsync()
    {
        var jsonFiles = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".json" } }
        });

        FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Відкрити таблицю",
            FileTypes = jsonFiles
        });

        if (file == null)
            return null;

        string content = await File.ReadAllTextAsync(file.FullPath);
        return new LocalTableFile(Path.GetFileNameWithoutExtension(file.FileName), file.FullPath, content);
    }
}
