using System.Text.Encodings.Web;
using System.Text.Json;

namespace Lab1.Logic;

/// <summary>
/// Дані таблиці у файлі. Зберігаються лише розмір і непорожні вирази:
/// {
///   "Rows": 10,
///   "Columns": 6,
///   "Cells": { "A1": "2^10", "B1": "A1 > 1000" }
/// }
/// </summary>
public class TableData
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public Dictionary<string, string> Cells { get; set; } = new();
}

/// <summary>
/// Перетворення таблиці у JSON-текст і навпаки (власний формат файлу).
/// </summary>
public static class TableSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // щоб символи <, > не перетворювались на <
    };

    public static string ToJson(Table table)
    {
        var data = new TableData { Rows = table.RowCount, Columns = table.ColumnCount };

        for (int r = 0; r < table.RowCount; r++)
        {
            for (int c = 0; c < table.ColumnCount; c++)
            {
                string expression = table.GetExpression(r, c);
                if (expression != "")
                    data.Cells[CellAddress.GetName(r, c)] = expression;
            }
        }

        return JsonSerializer.Serialize(data, Options);
    }

    public static Table FromJson(string json)
    {
        TableData? data;
        try
        {
            data = JsonSerializer.Deserialize<TableData>(json);
        }
        catch (JsonException)
        {
            throw new FormatException("Файл пошкоджений або має неправильний формат");
        }

        if (data == null)
            throw new FormatException("Файл порожній");

        if (data.Rows < 1 || data.Columns < 1 || data.Rows > Table.MaxRows || data.Columns > Table.MaxColumns)
            throw new FormatException($"Неправильний розмір таблиці у файлі: {data.Rows} x {data.Columns}");

        var table = new Table(data.Rows, data.Columns);

        foreach (KeyValuePair<string, string> pair in data.Cells)
        {
            if (!CellAddress.TryParse(pair.Key, out int row, out int column)
                || row >= table.RowCount || column >= table.ColumnCount)
            {
                throw new FormatException($"У файлі є неправильна клітинка: {pair.Key}");
            }
            table.GetCell(row, column).Expression = (pair.Value ?? "").Trim();
        }

        table.Recalculate();
        return table;
    }
}
