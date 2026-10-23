namespace Lab1.Services;

/// <summary>
/// Збереження таблиць у локальну папку "Документи\Lab1Tables".
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

    public string Load(string name)
    {
        return File.ReadAllText(Path.Combine(FolderPath, name + ".json"));
    }

    /// <summary>Назви (без .json) усіх збережених таблиць.</summary>
    public List<string> GetFileNames()
    {
        var names = new List<string>();
        if (!Directory.Exists(FolderPath))
            return names;

        foreach (string path in Directory.GetFiles(FolderPath, "*.json"))
            names.Add(Path.GetFileNameWithoutExtension(path));

        names.Sort();
        return names;
    }
}
