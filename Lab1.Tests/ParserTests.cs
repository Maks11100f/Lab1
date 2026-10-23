using System.Numerics;
using Lab1.Logic;
using Xunit;

namespace Lab1.Tests;

public class ParserTests
{
    // Обчислює вираз у порожній таблиці 5x5
    private static BigInteger Calc(string expression)
    {
        var table = new Table(5, 5);
        return Parser.Parse(expression).Evaluate(table);
    }

    [Fact]
    public void Number_LongNumbersAreSupported()
    {
        Assert.Equal(BigInteger.Parse("123456789012345678901234567890"), Calc("123456789012345678901234567890"));
    }

    [Fact]
    public void Power_Works()
    {
        Assert.Equal(new BigInteger(1024), Calc("2^10"));
        Assert.Equal(BigInteger.Pow(2, 100), Calc("2 ^ 100"));
        Assert.Equal(BigInteger.One, Calc("5^0"));
    }

    [Fact]
    public void Power_IsRightAssociative()
    {
        Assert.Equal(new BigInteger(512), Calc("2^3^2"));   // 2^(3^2)
        Assert.Equal(new BigInteger(64), Calc("(2^3)^2"));
    }

    [Fact]
    public void Comparisons_Work()
    {
        Assert.Equal(BigInteger.One, Calc("3 > 2"));
        Assert.Equal(BigInteger.Zero, Calc("3 < 2"));
        Assert.Equal(BigInteger.One, Calc("2^3 = 8"));
        Assert.Equal(BigInteger.Zero, Calc("2^3 = 9"));
    }

    [Fact]
    public void Not_Works()
    {
        Assert.Equal(BigInteger.Zero, Calc("not 1"));
        Assert.Equal(BigInteger.One, Calc("not 0"));
        Assert.Equal(BigInteger.One, Calc("not not 5"));
        Assert.Equal(BigInteger.One, Calc("NOT (2 > 3)"));
    }

    [Fact]
    public void AndOrEqv_TruthTables()
    {
        Assert.Equal(BigInteger.One, Calc("1 and 1"));
        Assert.Equal(BigInteger.Zero, Calc("1 and 0"));
        Assert.Equal(BigInteger.One, Calc("0 or 1"));
        Assert.Equal(BigInteger.Zero, Calc("0 or 0"));
        Assert.Equal(BigInteger.One, Calc("0 eqv 0"));
        Assert.Equal(BigInteger.One, Calc("1 eqv 7"));
        Assert.Equal(BigInteger.Zero, Calc("1 eqv 0"));
    }

    [Fact]
    public void Priorities_AreCorrect()
    {
        Assert.Equal(BigInteger.One, Calc("1 or 0 and 0"));        // and вище за or
        Assert.Equal(BigInteger.Zero, Calc("0 eqv 0 or 1"));       // or вище за eqv
        Assert.Equal(BigInteger.One, Calc("not 1 = 2"));           // not (1 = 2)
        Assert.Equal(BigInteger.One, Calc("2^2 = 4 and 3 > 1"));   // ^ і порівняння вище за and
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1 +")]
    [InlineData("(1 > 2")]
    [InlineData("1 > 2)")]
    [InlineData("1 2")]
    [InlineData("1 >")]
    [InlineData("A0")]
    [InlineData("hello")]
    [InlineData("1 <= 2")]
    [InlineData("and 1")]
    public void SyntaxErrors_AreDetected(string expression)
    {
        Assert.Throws<ParseException>(() => Parser.Parse(expression));
    }

    [Fact]
    public void SyntaxError_HasPosition()
    {
        var ex = Assert.Throws<ParseException>(() => Parser.Parse("1 > 2 + 3"));
        Assert.Equal(7, ex.Position);
    }

    [Fact]
    public void TooBigExponent_IsCalculationError()
    {
        Assert.Throws<CalculationException>(() => Calc("2^100000"));
        Assert.Equal(BigInteger.One, Calc("1^100000000000"));
    }
}
