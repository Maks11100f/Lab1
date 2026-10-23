using System.Numerics;
using Lab1.Logic;
using Xunit;

namespace Lab1.Tests;

public class TableTests
{
    [Fact]
    public void CellNames_AreConverted()
    {
        Assert.Equal("A", CellAddress.ColumnName(0));
        Assert.Equal("Z", CellAddress.ColumnName(25));
        Assert.Equal("AA", CellAddress.ColumnName(26));
        Assert.Equal("C5", CellAddress.GetName(4, 2));

        Assert.True(CellAddress.TryParse("aa10", out int row, out int column));
        Assert.Equal(9, row);
        Assert.Equal(26, column);

        Assert.False(CellAddress.TryParse("A0", out _, out _));
        Assert.False(CellAddress.TryParse("12", out _, out _));
        Assert.False(CellAddress.TryParse("AB", out _, out _));
    }

    [Fact]
    public void References_AreCalculated()
    {
        var table = new Table(3, 3);
        table.SetExpression(0, 0, "2^5");          // A1 = 32
        table.SetExpression(0, 1, "A1 > 30");      // B1 = 1
        table.SetExpression(0, 2, "not B1");       // C1 = 0

        Assert.Equal(new BigInteger(32), table.GetCell(0, 0).Value);
        Assert.Equal(BigInteger.One, table.GetCell(0, 1).Value);
        Assert.Equal(BigInteger.Zero, table.GetCell(0, 2).Value);

        // Зміна A1 перераховує залежні клітинки
        table.SetExpression(0, 0, "3");
        Assert.Equal(BigInteger.Zero, table.GetCell(0, 1).Value);
        Assert.Equal(BigInteger.One, table.GetCell(0, 2).Value);
    }

    [Fact]
    public void EmptyCell_IsZero()
    {
        var table = new Table(3, 3);
        table.SetExpression(0, 0, "B2 = 0");
        Assert.Equal(BigInteger.One, table.GetCell(0, 0).Value);
        Assert.Null(table.GetCell(0, 0).Error);
    }

    [Fact]
    public void Cycle_IsDetected()
    {
        var table = new Table(3, 3);
        table.SetExpression(0, 0, "B1");
        table.SetExpression(0, 1, "A1");
        table.SetExpression(1, 0, "A2 > 1");   // сама на себе

        Assert.True(table.GetCell(0, 0).HasError);
        Assert.True(table.GetCell(0, 1).HasError);
        Assert.True(table.GetCell(1, 0).HasError);
        Assert.Contains("Циклічне", table.GetCell(0, 0).Error);
        Assert.Equal("#ПОМИЛКА", table.GetCell(0, 0).ValueText);
    }

    [Fact]
    public void ReferenceToMissingCell_IsError()
    {
        var table = new Table(3, 3);
        table.SetExpression(0, 0, "Z99 > 1");
        Assert.True(table.GetCell(0, 0).HasError);
    }

    [Fact]
    public void SyntaxErrorInCell_IsShown()
    {
        var table = new Table(2, 2);
        table.SetExpression(0, 0, "1 >");
        Assert.True(table.GetCell(0, 0).HasError);
        Assert.Contains("Синтаксична", table.GetCell(0, 0).Error);
    }

    [Fact]
    public void AddAndRemove_RowsAndColumns()
    {
        var table = new Table(2, 2);
        table.AddRow();
        table.AddColumn();
        Assert.Equal(3, table.RowCount);
        Assert.Equal(3, table.ColumnCount);

        table.SetExpression(0, 0, "C3");
        table.SetExpression(2, 2, "5");
        Assert.Equal(new BigInteger(5), table.GetCell(0, 0).Value);

        table.RemoveLastRow();                  // C3 більше не існує
        Assert.True(table.GetCell(0, 0).HasError);

        Assert.True(table.RemoveLastRow());
        Assert.False(table.RemoveLastRow());    // хоча б один рядок має лишитись
    }

    [Fact]
    public void Json_SaveAndLoad()
    {
        var table = new Table(4, 3);
        table.SetExpression(0, 0, "2^10");
        table.SetExpression(3, 2, "A1 > 1000 and not 0");

        string json = TableSerializer.ToJson(table);
        Table loaded = TableSerializer.FromJson(json);

        Assert.Equal(4, loaded.RowCount);
        Assert.Equal(3, loaded.ColumnCount);
        Assert.Equal("2^10", loaded.GetExpression(0, 0));
        Assert.Equal(BigInteger.One, loaded.GetCell(3, 2).Value);
    }

    [Fact]
    public void Json_BadFile_Throws()
    {
        Assert.Throws<FormatException>(() => TableSerializer.FromJson("це не json"));
        Assert.Throws<FormatException>(() => TableSerializer.FromJson("{\"Rows\":0,\"Columns\":5}"));
        Assert.Throws<FormatException>(() => TableSerializer.FromJson("{\"Rows\":2,\"Columns\":2,\"Cells\":{\"Z9\":\"1\"}}"));
    }
}
