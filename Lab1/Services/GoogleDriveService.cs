using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace Lab1.Services;

/// <summary>Короткі відомості про файл на Google Drive.</summary>
public class DriveFileInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

/// <summary>
/// Робота з Google Drive через Google Drive API v3.
/// Під час першого звернення відкривається браузер для входу в Google-акаунт,
/// після чого токен зберігається локально і повторний вхід не потрібен.
///
/// Використовується дозвіл drive.file: програма бачить лише ті файли,
/// які вона сама створила, — решта вашого Drive їй недоступна.
/// </summary>
public class GoogleDriveService
{
    private const string CredentialsFileName = "credentials.json";
    private const string JsonMimeType = "application/json";

    private DriveService? service;

    public string TokenFolder { get; } = Path.Combine(FileSystem.AppDataDirectory, "GoogleDriveToken");

    /// <summary>Список таблиць (.json), створених цією програмою на Google Drive.</summary>
    public async Task<List<DriveFileInfo>> GetFilesAsync()
    {
        DriveService drive = await GetServiceAsync();

        FilesResource.ListRequest request = drive.Files.List();
        request.Q = $"mimeType = '{JsonMimeType}' and trashed = false";
        request.Fields = "files(id, name)";
        request.OrderBy = "name";
        request.PageSize = 100;

        var result = await request.ExecuteAsync();

        var files = new List<DriveFileInfo>();
        if (result.Files == null)
            return files;

        foreach (DriveFile file in result.Files)
            files.Add(new DriveFileInfo { Id = file.Id, Name = file.Name });
        return files;
    }

    /// <summary>
    /// Зберігає текст у файл на Google Drive.
    /// Якщо файл з такою назвою вже є — перезаписує його, інакше створює новий.
    /// </summary>
    public async Task SaveAsync(string fileName, string content)
    {
        DriveService drive = await GetServiceAsync();

        string? existingId = null;
        foreach (DriveFileInfo file in await GetFilesAsync())
        {
            if (file.Name == fileName)
            {
                existingId = file.Id;
                break;
            }
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        IUploadProgress progress;

        if (existingId == null)
        {
            var metadata = new DriveFile { Name = fileName, MimeType = JsonMimeType };
            progress = await drive.Files.Create(metadata, stream, JsonMimeType).UploadAsync();
        }
        else
        {
            progress = await drive.Files.Update(new DriveFile(), existingId, stream, JsonMimeType).UploadAsync();
        }

        if (progress.Status != UploadStatus.Completed)
            throw new IOException("Помилка вивантаження на Google Drive: " + progress.Exception?.Message);
    }

    /// <summary>Завантажує вміст файлу з Google Drive.</summary>
    public async Task<string> LoadAsync(string fileId)
    {
        DriveService drive = await GetServiceAsync();

        using var stream = new MemoryStream();
        IDownloadProgress progress = await drive.Files.Get(fileId).DownloadAsync(stream);

        if (progress.Status != DownloadStatus.Completed)
            throw new IOException("Помилка завантаження з Google Drive: " + progress.Exception?.Message);

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>Чи збережено вхід у Google-акаунт (є токен на диску).</summary>
    public bool IsSignedIn =>
        Directory.Exists(TokenFolder) && Directory.EnumerateFiles(TokenFolder).Any();

    /// <summary>
    /// Забуває збережений вхід (наступного разу знову відкриється браузер).
    /// Повертає false, якщо вхід і так не було виконано.
    /// </summary>
    public bool SignOut()
    {
        bool wasSignedIn = IsSignedIn;
        service = null;
        if (Directory.Exists(TokenFolder))
            Directory.Delete(TokenFolder, true);
        return wasSignedIn;
    }

    /// <summary>Створює підключення до Drive (один раз за сеанс роботи програми).</summary>
    private async Task<DriveService> GetServiceAsync()
    {
        if (service != null)
            return service;

        if (!await FileSystem.AppPackageFileExistsAsync(CredentialsFileName))
        {
            throw new InvalidOperationException(
                "Не знайдено файл credentials.json.\n" +
                "Створіть OAuth-клієнт (Desktop app) у Google Cloud Console, " +
                "покладіть файл у папку Resources/Raw проєкту і перезберіть програму. " +
                "Детальніше — у README.md.");
        }

        GoogleClientSecrets secrets;
        using (Stream stream = await FileSystem.OpenAppPackageFileAsync(CredentialsFileName))
        {
            secrets = GoogleClientSecrets.FromStream(stream);
        }

        // Якщо користувач закрив браузер і не увійшов — не чекаємо вічно
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));

        UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets.Secrets,
            new[] { DriveService.Scope.DriveFile },
            "user",
            timeout.Token,
            new FileDataStore(TokenFolder, true));

        service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Lab1 Spreadsheet"
        });

        return service;
    }
}
