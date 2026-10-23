namespace Lab1.Logic;

/// <summary>
/// Типи лексем (токенів), з яких складається вираз.
/// </summary>
public enum TokenType
{
    Number,      // ціле число: 123
    CellName,    // ім'я клітинки: A1, BC12
    Not,         // not
    And,         // and
    Or,          // or
    Eqv,         // eqv
    Power,       // ^
    Equal,       // =
    Less,        // <
    Greater,     // >
    LeftParen,   // (
    RightParen,  // )
    End          // кінець виразу
}

/// <summary>
/// Одна лексема виразу: тип, текст і позиція (з 1) у рядку.
/// </summary>
public class Token
{
    public TokenType Type { get; }
    public string Text { get; }
    public int Position { get; }

    public Token(TokenType type, string text, int position)
    {
        Type = type;
        Text = text;
        Position = position;
    }

    public override string ToString() => $"{Type} '{Text}' (позиція {Position})";
}
