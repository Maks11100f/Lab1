using System.Numerics;

namespace Lab1.Logic;

/// <summary>
/// Стан клітинки під час перерахунку таблиці (потрібен для пошуку циклів).
/// </summary>
public enum CellState
{
    NotCalculated,
    InProgress,
    Calculated
}

/// <summary>
/// Одна клітинка таблиці (назва TableCell, бо в MAUI вже є клас Cell): вираз, обчислене значення і текст помилки (якщо є).
/// </summary>
public class TableCell
{
    public string Expression { get; set; } = "";
    public BigInteger Value { get; set; }
    public string? Error { get; set; }
    public CellState State { get; set; } = CellState.NotCalculated;

    public bool IsEmpty => string.IsNullOrWhiteSpace(Expression);

    public bool HasError => !IsEmpty && Error != null;

    /// <summary>Текст для режиму "ЗНАЧЕННЯ".</summary>
    public string ValueText
    {
        get
        {
            if (IsEmpty)
                return "";
            if (Error != null)
                return "#ПОМИЛКА";
            return Value.ToString();
        }
    }
}
