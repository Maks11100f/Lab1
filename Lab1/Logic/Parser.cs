using System.Globalization;
using System.Numerics;

namespace Lab1.Logic;

/// <summary>
/// Синтаксичний аналізатор (метод рекурсивного спуску).
/// Кожне правило граматики (БНФ) — це окремий метод.
///
///   вираз       ::= диз'юнкція { "eqv" диз'юнкція }
///   диз'юнкція  ::= кон'юнкція { "or" кон'юнкція }
///   кон'юнкція  ::= заперечення { "and" заперечення }
///   заперечення ::= "not" заперечення | порівняння
///   порівняння  ::= степінь { ("=" | "&lt;" | "&gt;") степінь }
///   степінь     ::= первинний [ "^" степінь ]
///   первинний   ::= число | клітинка | "(" вираз ")"
///
/// Пріоритет (від найвищого): ( ), ^, порівняння, not, and, or, eqv.
/// Операція ^ правоасоціативна: 2^3^2 = 2^(3^2) = 512.
/// </summary>
public class Parser
{
    private readonly List<Token> tokens;
    private int position;

    private Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        position = 0;
    }

    /// <summary>
    /// Перевіряє синтаксис і будує дерево виразу.
    /// У разі помилки кидає ParseException з позицією помилки.
    /// </summary>
    public static Node Parse(string text)
    {
        var parser = new Parser(Lexer.Tokenize(text));

        if (parser.Current.Type == TokenType.End)
            throw new ParseException("Порожній вираз", 1);

        Node result = parser.ParseEqv();

        if (parser.Current.Type != TokenType.End)
            throw new ParseException($"Зайвий символ '{parser.Current.Text}'", parser.Current.Position);

        return result;
    }

    private Token Current => tokens[position];

    private Token MoveNext()
    {
        Token token = tokens[position];
        if (position < tokens.Count - 1)
            position++;
        return token;
    }

    private Node ParseEqv()
    {
        Node left = ParseOr();
        while (Current.Type == TokenType.Eqv)
        {
            MoveNext();
            left = new BinaryNode("eqv", left, ParseOr());
        }
        return left;
    }

    private Node ParseOr()
    {
        Node left = ParseAnd();
        while (Current.Type == TokenType.Or)
        {
            MoveNext();
            left = new BinaryNode("or", left, ParseAnd());
        }
        return left;
    }

    private Node ParseAnd()
    {
        Node left = ParseNot();
        while (Current.Type == TokenType.And)
        {
            MoveNext();
            left = new BinaryNode("and", left, ParseNot());
        }
        return left;
    }

    private Node ParseNot()
    {
        if (Current.Type == TokenType.Not)
        {
            MoveNext();
            return new NotNode(ParseNot());
        }
        return ParseComparison();
    }

    private Node ParseComparison()
    {
        Node left = ParsePower();
        while (Current.Type == TokenType.Equal || Current.Type == TokenType.Less || Current.Type == TokenType.Greater)
        {
            string op = MoveNext().Text;
            left = new BinaryNode(op, left, ParsePower());
        }
        return left;
    }

    private Node ParsePower()
    {
        Node baseNode = ParsePrimary();
        if (Current.Type == TokenType.Power)
        {
            MoveNext();
            // Рекурсія праворуч дає правоасоціативність
            return new BinaryNode("^", baseNode, ParsePower());
        }
        return baseNode;
    }

    private Node ParsePrimary()
    {
        Token token = Current;

        switch (token.Type)
        {
            case TokenType.Number:
                MoveNext();
                return new NumberNode(BigInteger.Parse(token.Text, CultureInfo.InvariantCulture));

            case TokenType.CellName:
                MoveNext();
                return new CellNode(token.Text);

            case TokenType.LeftParen:
                MoveNext();
                Node inner = ParseEqv();
                if (Current.Type != TokenType.RightParen)
                    throw new ParseException("Очікувалась закриваюча дужка ')'", Current.Position);
                MoveNext();
                return inner;

            case TokenType.End:
                throw new ParseException("Вираз обірвано: очікувалось число, клітинка або '('", token.Position);

            default:
                throw new ParseException($"Неочікуваний символ '{token.Text}': тут має бути число, клітинка або '('", token.Position);
        }
    }
}
