namespace Lab1.Services;

/// <summary>Таблиця, прочитана з файлу на комп'ютері.</summary>
public record LocalTableFile(string Name, string FullPath, string Content);

/// <summary>
/// Робота з таблицями на комп'ютері: відкриття та збереження файлів .json
/// у будь-якій папці через стандартні вікна Windows «Відкрити» і «Зберегти як».
/// </summary>
public class LocalFileService
{
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

    /// <summary>
    /// Показує стандартне вікно «Зберегти як» (користувач обирає папку й назву)
    /// і записує туди таблицю. Повертає повний шлях до файлу або null, якщо збереження скасовано.
    /// </summary>
    public async Task<string?> SaveAsAsync(string suggestedName, string content)
    {
#if WINDOWS
        var picker = new Windows.Storage.Pickers.FileSavePicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
            SuggestedFileName = suggestedName
        };
        picker.FileTypeChoices.Add("Таблиця (JSON)", new List<string> { ".json" });

        // У Windows-застосунку без пакування вікно вибору треба прив'язати до вікна програми
        var window = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window
            ?? throw new InvalidOperationException("Не знайдено вікно програми.");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));

        Windows.Storage.StorageFile? file = await picker.PickSaveFileAsync();
        if (file == null)
            return null;

        await File.WriteAllTextAsync(file.Path, content);
        return file.Path;
#else
        await Task.CompletedTask;
        throw new PlatformNotSupportedException("Збереження у файл підтримується лише у Windows.");
#endif
    }
}
