using System.Numerics;

namespace Lab1.Logic;

/// <summary>
/// Модель електронної таблиці: зберігає клітинки, змінює розмір
/// і перераховує значення всіх клітинок.
/// </summary>
public class Table
{
    public const int MaxRows = 200;
    public const int MaxColumns = 52;

    private readonly List<List<TableCell>> rows = new();
    private int columnCount;

    public int RowCount => rows.Count;
    public int ColumnCount => columnCount;

    public Table(int rowCount, int columnCount)
    {
        if (rowCount < 1 || columnCount < 1 || rowCount > MaxRows || columnCount > MaxColumns)
            throw new ArgumentException("Неправильний розмір таблиці");

        this.columnCount = columnCount;
        for (int r = 0; r < rowCount; r++)
            rows.Add(CreateRow());
    }

    public TableCell GetCell(int row, int column) => rows[row][column];

    public string GetExpression(int row, int column) => rows[row][column].Expression;

    /// <summary>Змінює вираз клітинки і перераховує всю таблицю.</summary>
    public void SetExpression(int row, int column, string expression)
    {
        rows[row][column].Expression = expression.Trim();
        Recalculate();
    }

    // ---------- Зміна розміру ----------

    public bool AddRow()
    {
        if (RowCount >= MaxRows)
            return false;
        rows.Add(CreateRow());
        Recalculate();
        return true;
    }

    public bool AddColumn()
    {
        if (ColumnCount >= MaxColumns)
            return false;
        columnCount++;
        foreach (List<TableCell> row in rows)
            row.Add(new TableCell());
        Recalculate();
        return true;
    }

    public bool RemoveLastRow()
    {
        if (RowCount <= 1)
            return false;
        rows.RemoveAt(RowCount - 1);
        Recalculate();
        return true;
    }

    public bool RemoveLastColumn()
    {
        if (ColumnCount <= 1)
            return false;
        columnCount--;
        foreach (List<TableCell> row in rows)
            row.RemoveAt(row.Count - 1);
        Recalculate();
        return true;
    }

    public bool IsRowEmpty(int row)
    {
        for (int c = 0; c < ColumnCount; c++)
        {
            if (!rows[row][c].IsEmpty)
                return false;
        }
        return true;
    }

    public bool IsColumnEmpty(int column)
    {
        for (int r = 0; r < RowCount; r++)
        {
            if (!rows[r][column].IsEmpty)
                return false;
        }
        return true;
    }

    // ---------- Обчислення ----------

    /// <summary>Перераховує значення всіх клітинок таблиці.</summary>
    public void Recalculate()
    {
        foreach (List<TableCell> row in rows)
            foreach (TableCell cell in row)
                cell.State = CellState.NotCalculated;

        foreach (List<TableCell> row in rows)
            foreach (TableCell cell in row)
                CalculateCell(cell);
    }

    /// <summary>
    /// Повертає значення клітинки за іменом (викликається з CellNode під час обчислення).
    /// </summary>
    public BigInteger GetValue(string cellName)
    {
        if (!CellAddress.TryParse(cellName, out int row, out int column)
            || row >= RowCount || column >= ColumnCount)
        {
            throw new CalculationException($"Клітинки {cellName} не існує в таблиці");
        }

        TableCell cell = rows[row][column];

        if (cell.State == CellState.InProgress)
            throw new CalculationException($"Циклічне посилання (через клітинку {cellName})");

        CalculateCell(cell);

        if (cell.Error != null)
        {
            // Про цикл повідомляємо всім клітинкам, що від нього залежать
            if (cell.Error.StartsWith("Циклічне"))
                throw new CalculationException(cell.Error);
            throw new CalculationException($"Клітинка {cellName} містить помилку");
        }

        return cell.Value; // порожня клітинка має значення 0
    }

    private void CalculateCell(TableCell cell)
    {
        if (cell.State != CellState.NotCalculated)
            return;

        cell.State = CellState.InProgress;
        cell.Value = BigInteger.Zero;
        cell.Error = null;

        if (!cell.IsEmpty)
        {
            try
            {
                Node tree = Parser.Parse(cell.Expression);   // етап 1: синтаксис
                cell.Value = tree.Evaluate(this);            // етап 2: обчислення
            }
            catch (ParseException ex)
            {
                cell.Error = $"Синтаксична помилка (позиція {ex.Position}): {ex.Message}";
            }
            catch (CalculationException ex)
            {
                cell.Error = ex.Message;
            }
        }

        cell.State = CellState.Calculated;
    }

    private List<TableCell> CreateRow()
    {
        var row = new List<TableCell>();
        for (int c = 0; c < columnCount; c++)
            row.Add(new TableCell());
        return row;
    }
}
