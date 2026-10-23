namespace Lab1.Logic;

/// <summary>
/// Перетворення між іменами клітинок (A1, B7, AA3) та індексами рядка/стовпчика.
/// Індекси рахуються з нуля: A1 -> (row 0, column 0).
/// </summary>
public static class CellAddress
{
    /// <summary>0 -> "A", 25 -> "Z", 26 -> "AA", 27 -> "AB" ...</summary>
    public static string ColumnName(int column)
    {
        string name = "";
        int number = column + 1;
        while (number > 0)
        {
            int remainder = (number - 1) % 26;
            name = (char)('A' + remainder) + name;
            number = (number - 1) / 26;
        }
        return name;
    }

    public static string GetName(int row, int column)
    {
        return ColumnName(column) + (row + 1);
    }

    /// <summary>
    /// Розбирає ім'я клітинки ("B12") на індекси. Повертає false, якщо ім'я некоректне.
    /// </summary>
    public static bool TryParse(string name, out int row, out int column)
    {
        row = -1;
        column = -1;
        name = name.ToUpper();

        int i = 0;
        int columnNumber = 0;
        while (i < name.Length && name[i] >= 'A' && name[i] <= 'Z')
        {
            columnNumber = columnNumber * 26 + (name[i] - 'A' + 1);
            if (columnNumber > 100000)
                return false;
            i++;
        }

        // Мають бути і букви, і цифри
        if (i == 0 || i == name.Length)
            return false;

        string digits = name.Substring(i);
        foreach (char c in digits)
        {
            if (c < '0' || c > '9')
                return false;
        }

        if (!int.TryParse(digits, out int rowNumber) || rowNumber < 1)
            return false;

        row = rowNumber - 1;
        column = columnNumber - 1;
        return true;
    }
}
