using Lab1.Logic;
using Lab1.Services;

namespace Lab1;

/// <summary>
/// Головне вікно. Тут лише робота з інтерфейсом:
/// вся логіка обчислень — у класах папки Logic,
/// робота з файлами та Google Drive — у класах папки Services.
/// </summary>
public partial class MainPage : ContentPage
{
    private const int DefaultRows = 10;
    private const int DefaultColumns = 6;

    // Кольори таблиці (у стилі Excel: світло-сірі заголовки, зелена рамка вибраної клітинки)
    private static readonly Color HeaderColor = Color.FromArgb("#F3F3F3");
    private static readonly Color HeaderTextColor = Color.FromArgb("#333333");
    private static readonly Color SelectedHeaderColor = Color.FromArgb("#D2E6D9");
    private static readonly Color SelectedHeaderTextColor = Color.FromArgb("#1F6E43");
    private static readonly Color CellColor = Colors.White;
    private static readonly Color SelectionBorderColor = Color.FromArgb("#1F6E43");
    private static readonly Color ErrorTextColor = Color.FromArgb("#C62828");
    private static readonly Color ActiveModeColor = Color.FromArgb("#B26A00");
    private static readonly Color InactiveModeColor = Color.FromArgb("#E8E8E8");

    /// <summary>Поточна сторінка (потрібна для питання при закритті вікна).</summary>
    public static MainPage? Current { get; private set; }

    public bool HasUnsavedChanges { get; set; }

    private readonly LocalFileService localFiles = new();
    private readonly GoogleDriveService googleDrive = new();

    private Table table = new(DefaultRows, DefaultColumns);
    private CellEntry[,] cellEntries = new CellEntry[0, 0];
    private Border[,] cellBorders = new Border[0, 0];     // рамка навколо клітинки (видима лише у вибраної)
    private Label[] rowHeaders = Array.Empty<Label>();
    private Label[] columnHeaders = Array.Empty<Label>();
    private int selectedRow;
    private int selectedColumn;
    private bool showExpressions;          // false — режим ЗНАЧЕННЯ, true — режим ВИРАЗ
    private string fileName = "table1";

    public MainPage()
    {
        InitializeComponent();
        Current = this;
        BuildGrid();
        SelectCell(0, 0);
        UpdateModeButtons();
    }

    // =====================================================================
    //  Побудова та оновлення таблиці на екрані
    // =====================================================================

    /// <summary>Створює заголовки та поля введення для всіх клітинок.</summary>
    private void BuildGrid()
    {
        TableGrid.Children.Clear();
        TableGrid.RowDefinitions.Clear();
        TableGrid.ColumnDefinitions.Clear();

        int rows = table.RowCount;
        int columns = table.ColumnCount;
        cellEntries = new CellEntry[rows, columns];
        cellBorders = new Border[rows, columns];
        rowHeaders = new Label[rows];
        columnHeaders = new Label[columns];

        TableGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(50)));
        for (int c = 0; c < columns; c++)
            TableGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(130)));

        for (int r = 0; r <= rows; r++)
            TableGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        // Лівий верхній кут і заголовки стовпчиків
        TableGrid.Add(CreateHeader(""), 0, 0);
        for (int c = 0; c < columns; c++)
        {
            columnHeaders[c] = CreateHeader(CellAddress.ColumnName(c));
            TableGrid.Add(columnHeaders[c], c + 1, 0);
        }

        for (int r = 0; r < rows; r++)
        {
            rowHeaders[r] = CreateHeader((r + 1).ToString());
            TableGrid.Add(rowHeaders[r], 0, r + 1);

            for (int c = 0; c < columns; c++)
            {
                CellEntry entry = CreateCellEntry(r, c);
                var border = new Border
                {
                    // Прямокутна рамка без заокруглень; прозора, доки клітинку не вибрано
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle(),
                    StrokeThickness = 2,
                    Stroke = Colors.Transparent,
                    BackgroundColor = CellColor,
                    Padding = 0,
                    Content = entry
                };

                cellEntries[r, c] = entry;
                cellBorders[r, c] = border;
                TableGrid.Add(border, c + 1, r + 1);
            }
        }

        RefreshCells();
    }

    private static Label CreateHeader(string text)
    {
        return new Label
        {
            Text = text,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = HeaderColor,
            TextColor = HeaderTextColor,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Padding = new Thickness(4, 6)
        };
    }

    private CellEntry CreateCellEntry(int row, int column)
    {
        var entry = new CellEntry
        {
            FontSize = 14,
            BackgroundColor = Colors.Transparent    // фон дає рамка-контейнер клітинки
        };

        entry.Focused += (sender, e) => SelectCell(row, column);
        entry.Completed += (sender, e) => CommitCellEntry(entry, row, column);
        entry.Unfocused += (sender, e) => CommitCellEntry(entry, row, column);
        return entry;
    }

    /// <summary>Показує в клітинках вирази або значення (залежно від режиму).</summary>
    private void RefreshCells()
    {
        for (int r = 0; r < table.RowCount; r++)
        {
            for (int c = 0; c < table.ColumnCount; c++)
            {
                TableCell cell = table.GetCell(r, c);
                CellEntry entry = cellEntries[r, c];

                entry.Text = showExpressions ? cell.Expression : cell.ValueText;
                entry.IsReadOnly = !showExpressions;
                entry.TextColor = cell.HasError ? ErrorTextColor : Colors.Black;
                // Як в Excel: значення вирівняні праворуч, вирази — ліворуч
                entry.HorizontalTextAlignment = showExpressions ? TextAlignment.Start : TextAlignment.End;
            }
        }
    }

    /// <summary>Показує або прибирає виділення клітинки (рамка + підсвічені заголовки рядка і стовпчика).</summary>
    private void SetCellHighlight(int row, int column, bool selected)
    {
        if (row >= table.RowCount || column >= table.ColumnCount)
            return;

        cellBorders[row, column].Stroke = selected ? SelectionBorderColor : Colors.Transparent;

        foreach (Label header in new[] { rowHeaders[row], columnHeaders[column] })
        {
            header.BackgroundColor = selected ? SelectedHeaderColor : HeaderColor;
            header.TextColor = selected ? SelectedHeaderTextColor : HeaderTextColor;
        }
    }

    // =====================================================================
    //  Вибір і редагування клітинок
    // =====================================================================

    private void SelectCell(int row, int column)
    {
        SetCellHighlight(selectedRow, selectedColumn, false);

        selectedRow = row;
        selectedColumn = column;
        SetCellHighlight(row, column, true);

        CellNameLabel.Text = CellAddress.GetName(row, column);
        FormulaEntry.Text = table.GetExpression(row, column);
        ShowSelectedCellInfo();
    }

    /// <summary>Виводить у рядку стану значення або опис помилки вибраної клітинки.</summary>
    private void ShowSelectedCellInfo()
    {
        TableCell cell = table.GetCell(selectedRow, selectedColumn);
        string name = CellAddress.GetName(selectedRow, selectedColumn);

        if (cell.IsEmpty)
            StatusLabel.Text = $"{name}: порожня клітинка (значення 0)";
        else if (cell.HasError)
            StatusLabel.Text = $"{name}: {cell.Error}";
        else
            StatusLabel.Text = $"{name} = {cell.Value}   (вираз: {cell.Expression})";
    }

    /// <summary>Введення виразу прямо в клітинці (лише в режимі ВИРАЗ).</summary>
    private void CommitCellEntry(Entry entry, int row, int column)
    {
        if (!showExpressions)
            return;

        // Таблицю могли перебудувати (змінили розмір) — тоді це поле вже не актуальне
        if (row >= table.RowCount || column >= table.ColumnCount || cellEntries[row, column] != entry)
            return;

        string text = (entry.Text ?? "").Trim();
        if (text == table.GetExpression(row, column))
            return;

        ChangeExpression(row, column, text);
    }

    /// <summary>Введення виразу в рядку над таблицею (працює в обох режимах).</summary>
    private void FormulaEntry_Completed(object? sender, EventArgs e)
    {
        ChangeExpression(selectedRow, selectedColumn, FormulaEntry.Text ?? "");
    }

    private void ChangeExpression(int row, int column, string expression)
    {
        table.SetExpression(row, column, expression);   // таблиця сама все перерахує
        HasUnsavedChanges = true;
        RefreshCells();

        if (row == selectedRow && column == selectedColumn)
            FormulaEntry.Text = table.GetExpression(row, column);
        ShowSelectedCellInfo();
    }

    private void ValuesMode_Clicked(object? sender, EventArgs e) => SetDisplayMode(false);

    private void ExpressionsMode_Clicked(object? sender, EventArgs e) => SetDisplayMode(true);

    /// <summary>Перемикає режим відображення: ЗНАЧЕННЯ (false) або ВИРАЗ (true).</summary>
    private void SetDisplayMode(bool expressions)
    {
        if (showExpressions == expressions)
            return;

        showExpressions = expressions;
        UpdateModeButtons();
        RefreshCells();
        StatusLabel.Text = showExpressions
            ? "Режим ВИРАЗ: у клітинках показано вирази, їх можна редагувати."
            : "Режим ЗНАЧЕННЯ: у клітинках показано обчислені значення.";
    }

    /// <summary>Підсвічує кнопку поточного режиму.</summary>
    private void UpdateModeButtons()
    {
        ValuesModeButton.BackgroundColor = showExpressions ? InactiveModeColor : ActiveModeColor;
        ValuesModeButton.TextColor = showExpressions ? Colors.Black : Colors.White;
        ExpressionsModeButton.BackgroundColor = showExpressions ? ActiveModeColor : InactiveModeColor;
        ExpressionsModeButton.TextColor = showExpressions ? Colors.White : Colors.Black;
    }

    // =====================================================================
    //  Рядки та стовпчики
    // =====================================================================

    private async void AddRow_Clicked(object? sender, EventArgs e)
    {
        if (!table.AddRow())
        {
            await DisplayAlertAsync("Увага", $"Максимальна кількість рядків — {Table.MaxRows}.", "OK");
            return;
        }
        OnTableSizeChanged();
    }

    private async void AddColumn_Clicked(object? sender, EventArgs e)
    {
        if (!table.AddColumn())
        {
            await DisplayAlertAsync("Увага", $"Максимальна кількість стовпчиків — {Table.MaxColumns}.", "OK");
            return;
        }
        OnTableSizeChanged();
    }

    private async void RemoveRow_Clicked(object? sender, EventArgs e)
    {
        if (table.RowCount <= 1)
        {
            await DisplayAlertAsync("Увага", "У таблиці має залишитися хоча б один рядок.", "OK");
            return;
        }

        int last = table.RowCount - 1;
        if (!table.IsRowEmpty(last))
        {
            bool confirm = await DisplayAlertAsync("Видалення рядка",
                $"Рядок {last + 1} містить дані. Видалити його?", "Так", "Ні");
            if (!confirm)
                return;
        }

        table.RemoveLastRow();
        OnTableSizeChanged();
    }

    private async void RemoveColumn_Clicked(object? sender, EventArgs e)
    {
        if (table.ColumnCount <= 1)
        {
            await DisplayAlertAsync("Увага", "У таблиці має залишитися хоча б один стовпчик.", "OK");
            return;
        }

        int last = table.ColumnCount - 1;
        if (!table.IsColumnEmpty(last))
        {
            bool confirm = await DisplayAlertAsync("Видалення стовпчика",
                $"Стовпчик {CellAddress.ColumnName(last)} містить дані. Видалити його?", "Так", "Ні");
            if (!confirm)
                return;
        }

        table.RemoveLastColumn();
        OnTableSizeChanged();
    }

    private void OnTableSizeChanged()
    {
        HasUnsavedChanges = true;
        int row = Math.Min(selectedRow, table.RowCount - 1);
        int column = Math.Min(selectedColumn, table.ColumnCount - 1);
        selectedRow = row;
        selectedColumn = column;
        BuildGrid();
        SelectCell(row, column);
    }

    // =====================================================================
    //  Нова таблиця, локальні файли
    // =====================================================================

    private async void New_Clicked(object? sender, EventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;

        LoadTable(new Table(DefaultRows, DefaultColumns), "table1");
        StatusLabel.Text = "Створено нову таблицю.";
    }

    private async void SaveLocal_Clicked(object? sender, EventArgs e)
    {
        string? name = await AskFileNameAsync();
        if (name == null)
            return;

        try
        {
            string path = localFiles.Save(name, TableSerializer.ToJson(table));
            MarkAsSaved(name);
            StatusLabel.Text = $"Таблицю збережено: {path}";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", "Не вдалося зберегти файл.\n" + ex.Message, "OK");
        }
    }

    private async void OpenLocal_Clicked(object? sender, EventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;

        List<string> names = localFiles.GetFileNames();
        if (names.Count == 0)
        {
            await DisplayAlertAsync("Відкриття", $"У папці {localFiles.FolderPath} ще немає збережених таблиць.", "OK");
            return;
        }

        string? choice = await DisplayActionSheetAsync("Оберіть таблицю", "Скасувати", null, names.ToArray());
        if (choice == null || !names.Contains(choice))
            return;

        try
        {
            LoadTable(TableSerializer.FromJson(localFiles.Load(choice)), choice);
            StatusLabel.Text = $"Відкрито таблицю «{choice}».";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", "Не вдалося відкрити файл.\n" + ex.Message, "OK");
        }
    }

    // =====================================================================
    //  Google Drive
    // =====================================================================

    private async void SaveDrive_Clicked(object? sender, EventArgs e)
    {
        string? name = await AskFileNameAsync();
        if (name == null)
            return;

        SetBusy(true, "Збереження на Google Drive... (при першому вході відкриється браузер)");
        try
        {
            await googleDrive.SaveAsync(name + ".json", TableSerializer.ToJson(table));
            MarkAsSaved(name);
            StatusLabel.Text = $"Таблицю збережено на Google Drive: {name}.json";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Не вдалося зберегти на Google Drive.";
            await DisplayAlertAsync("Google Drive", GetDriveErrorText(ex), "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OpenDrive_Clicked(object? sender, EventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
            return;

        SetBusy(true, "Отримання списку файлів з Google Drive... (при першому вході відкриється браузер)");
        try
        {
            List<DriveFileInfo> files = await googleDrive.GetFilesAsync();
            SetBusy(false);

            if (files.Count == 0)
            {
                await DisplayAlertAsync("Google Drive", "На Google Drive ще немає таблиць, збережених цією програмою.", "OK");
                StatusLabel.Text = "";
                return;
            }

            string[] names = new string[files.Count];
            for (int i = 0; i < files.Count; i++)
                names[i] = files[i].Name;

            string? choice = await DisplayActionSheetAsync("Таблиці на Google Drive", "Скасувати", null, names);
            DriveFileInfo? selected = files.Find(f => f.Name == choice);
            if (selected == null)
            {
                StatusLabel.Text = "";
                return;
            }

            SetBusy(true, $"Завантаження «{selected.Name}»...");
            string json = await googleDrive.LoadAsync(selected.Id);
            string tableName = Path.GetFileNameWithoutExtension(selected.Name);
            LoadTable(TableSerializer.FromJson(json), tableName);
            StatusLabel.Text = $"Відкрито з Google Drive: {selected.Name}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Не вдалося відкрити таблицю з Google Drive.";
            await DisplayAlertAsync("Google Drive", GetDriveErrorText(ex), "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void SignOut_Clicked(object? sender, EventArgs e)
    {
        try
        {
            if (googleDrive.SignOut())
            {
                StatusLabel.Text = "Ви вийшли з Google-акаунта.";
                await DisplayAlertAsync("Google Drive",
                    "Ви вийшли з Google-акаунта.\n" +
                    "Під час наступного звернення до Google Drive програма знову відкриє браузер для входу.",
                    "OK");
            }
            else
            {
                StatusLabel.Text = "Вхід у Google-акаунт не виконано.";
                await DisplayAlertAsync("Google Drive",
                    "Ви ще не входили в Google-акаунт у цій програмі, тому виходити не потрібно.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", ex.Message, "OK");
        }
    }

    private static string GetDriveErrorText(Exception ex)
    {
        if (ex is OperationCanceledException)
            return "Вхід у Google-акаунт не виконано (минув час очікування або вікно браузера закрито).";
        if (ex is FormatException)
            return "Файл з Google Drive має неправильний формат.\n" + ex.Message;
        if (ex is System.Net.Http.HttpRequestException)
            return "Немає з'єднання з інтернетом або сервер Google недоступний.\n" + ex.Message;
        return ex.Message;
    }

    // =====================================================================
    //  Довідка
    // =====================================================================

    private async void Help_Clicked(object? sender, EventArgs e)
    {
        const string help =
            "ЯК КОРИСТУВАТИСЯ\n" +
            "• Виберіть клітинку і введіть вираз у рядку над таблицею, потім Enter або «Застосувати вираз».\n" +
            "• У режимі ВИРАЗ можна писати вирази прямо в клітинках.\n" +
            "• «Показувати: Значення / Вирази» перемикає режим відображення таблиці.\n" +
            "• Група «Таблиця» — зміна кількості рядків і стовпчиків, група «Файл» — " +
            "створення, відкриття та збереження таблиць (на комп'ютері або на Google Drive).\n" +
            "• Помилкові клітинки показують #ПОМИЛКА, опис помилки — у рядку стану внизу.\n\n" +
            "ЩО МОЖНА ПИСАТИ У ВИРАЗІ\n" +
            "• Цілі невід'ємні числа будь-якої довжини: 7, 123456789012345678901234567890\n" +
            "• Посилання на клітинки: A1, B12, AA3 (порожня клітинка = 0)\n" +
            "• Круглі дужки ( )\n" +
            "• a ^ b — піднесення до степеня (2^3^2 = 2^(3^2) = 512)\n" +
            "• a = b, a < b, a > b — порівняння\n" +
            "• not a, a and b, a or b, a eqv b — логічні операції\n\n" +
            "ЛОГІЧНІ ЗНАЧЕННЯ\n" +
            "Істина = 1, хибність = 0. Будь-яке ненульове число вважається істиною.\n" +
            "a eqv b — істина, коли a і b одночасно істинні або одночасно хибні.\n\n" +
            "ПРІОРИТЕТ (від найвищого)\n" +
            "( )  →  ^  →  = < >  →  not  →  and  →  or  →  eqv\n\n" +
            "ПРИКЛАДИ\n" +
            "2^10 > 1000            → 1\n" +
            "not (A1 = 0) and B1 > 5\n" +
            "(A1 > 3) eqv (B1 > 3)";

        await DisplayAlertAsync("Довідка", help, "Зрозуміло");
    }

    private async void About_Clicked(object? sender, EventArgs e)
    {
        const string about =
            "Електронна таблиця\n" +
            "Лабораторна робота №1* з дисципліни «Об'єктно-орієнтоване програмування».\n" +
            "Варіант 73: операції ^, = < >, not, and, or, eqv; цілі числа довільної довжини.\n\n" +
            "Можливості: введення та обчислення виразів (синтаксична перевірка з позицією помилки, " +
            "пошук циклічних посилань), режими ВИРАЗ / ЗНАЧЕННЯ, зміна кількості рядків і стовпчиків, " +
            "збереження у файл та на Google Drive.\n\n" +
            "C#, .NET MAUI.";

        await DisplayAlertAsync("Про програму", about, "OK");
    }

    // =====================================================================
    //  Допоміжні методи
    // =====================================================================

    private void LoadTable(Table newTable, string name)
    {
        table = newTable;
        fileName = name;
        HasUnsavedChanges = false;
        selectedRow = 0;
        selectedColumn = 0;
        BuildGrid();
        SelectCell(0, 0);
    }

    private void MarkAsSaved(string name)
    {
        fileName = name;
        HasUnsavedChanges = false;
    }

    /// <summary>Питає назву файлу. Повертає null, якщо користувач скасував.</summary>
    private async Task<string?> AskFileNameAsync()
    {
        string? name = await DisplayPromptAsync("Збереження", "Введіть назву таблиці:",
            "Зберегти", "Скасувати", initialValue: fileName, maxLength: 60);

        if (name == null)
            return null;

        name = name.Trim();
        if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            name = name.Substring(0, name.Length - 5);

        if (name == "" || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            await DisplayAlertAsync("Помилка", "Назва порожня або містить недопустимі символи (\\ / : * ? \" < > |).", "OK");
            return null;
        }

        return name;
    }

    private async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (!HasUnsavedChanges)
            return true;

        return await DisplayAlertAsync("Незбережені зміни",
            "У таблиці є незбережені зміни. Продовжити без збереження?", "Так", "Ні");
    }

    /// <summary>Викликається при закритті вікна (див. MauiProgram.cs).</summary>
    public Task<bool> AskExitAsync()
    {
        return DisplayAlertAsync("Вихід",
            "У таблиці є незбережені зміни. Вийти без збереження?", "Вийти", "Скасувати");
    }

    private void SetBusy(bool busy, string message = "")
    {
        BusyIndicator.IsRunning = busy;
        BusyIndicator.IsVisible = busy;
        ButtonsPanel.IsEnabled = !busy;
        if (busy)
            StatusLabel.Text = message;
    }
}
