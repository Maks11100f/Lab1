using System.Numerics;

namespace Lab1.Logic;

/// <summary>
/// Вузол дерева виразу. Парсер будує дерево (етап 1),
/// а метод Evaluate обчислює його значення (етап 2).
///
/// Логічні значення представлені числами: 1 — істина, 0 — хибність.
/// Будь-яке ненульове число в логічних операціях вважається істиною.
/// </summary>
public abstract class Node
{
    public abstract BigInteger Evaluate(Table table);

    protected static bool ToBool(BigInteger value) => !value.IsZero;

    protected static BigInteger FromBool(bool value) => value ? BigInteger.One : BigInteger.Zero;
}

/// <summary>Ціле число, наприклад 42.</summary>
public class NumberNode : Node
{
    public BigInteger Value { get; }

    public NumberNode(BigInteger value)
    {
        Value = value;
    }

    public override BigInteger Evaluate(Table table) => Value;
}

/// <summary>Посилання на клітинку, наприклад A3.</summary>
public class CellNode : Node
{
    public string CellName { get; }

    public CellNode(string cellName)
    {
        CellName = cellName;
    }

    public override BigInteger Evaluate(Table table) => table.GetValue(CellName);
}

/// <summary>Логічне заперечення: not X.</summary>
public class NotNode : Node
{
    public Node Operand { get; }

    public NotNode(Node operand)
    {
        Operand = operand;
    }

    public override BigInteger Evaluate(Table table)
    {
        return FromBool(!ToBool(Operand.Evaluate(table)));
    }
}

/// <summary>Бінарна операція: ^, =, &lt;, &gt;, and, or, eqv.</summary>
public class BinaryNode : Node
{
    /// <summary>Обмеження на показник степеня, щоб програма не "зависала" на 2^999999999.</summary>
    public const int MaxExponent = 10000;

    public string Operator { get; }
    public Node Left { get; }
    public Node Right { get; }

    public BinaryNode(string op, Node left, Node right)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    public override BigInteger Evaluate(Table table)
    {
        BigInteger a = Left.Evaluate(table);
        BigInteger b = Right.Evaluate(table);

        switch (Operator)
        {
            case "^": return Power(a, b);
            case "=": return FromBool(a == b);
            case "<": return FromBool(a < b);
            case ">": return FromBool(a > b);
            case "and": return FromBool(ToBool(a) && ToBool(b));
            case "or": return FromBool(ToBool(a) || ToBool(b));
            case "eqv": return FromBool(ToBool(a) == ToBool(b));
            default:
                throw new CalculationException($"Невідома операція '{Operator}'");
        }
    }

    private static BigInteger Power(BigInteger number, BigInteger exponent)
    {
        if (exponent < 0)
            throw new CalculationException("Від'ємний показник степеня не підтримується");

        // 0, 1 у будь-якому степені не ростуть, тому для них обмеження не потрібне
        if (number <= 1)
            return exponent.IsZero ? BigInteger.One : number;

        if (exponent > MaxExponent)
            throw new CalculationException($"Показник степеня завеликий (максимум {MaxExponent})");

        return BigInteger.Pow(number, (int)exponent);
    }
}
