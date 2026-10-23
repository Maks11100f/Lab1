namespace Lab1.Logic;

/// <summary>
/// Лексичний аналізатор: розбиває рядок виразу на список лексем.
/// Наприклад, "not (A1 > 2^3)" -> [not] [(] [A1] [>] [2] [^] [3] [)] [End]
/// </summary>
public static class Lexer
{
    public static List<Token> Tokenize(string text)
    {
        var tokens = new List<Token>();
        int i = 0;

        while (i < text.Length)
        {
            char c = text[i];
            int start = i;

            if (char.IsWhiteSpace(c))
            {
                i++;
            }
            else if (IsDigit(c))
            {
                // Ціле число довільної довжини
                while (i < text.Length && IsDigit(text[i]))
                    i++;
                tokens.Add(new Token(TokenType.Number, text.Substring(start, i - start), start + 1));
            }
            else if (IsLetter(c))
            {
                // Слово: ключове слово (not, and, or, eqv) або ім'я клітинки (A1)
                while (i < text.Length && (IsLetter(text[i]) || IsDigit(text[i])))
                    i++;
                string word = text.Substring(start, i - start);
                tokens.Add(MakeWordToken(word, start + 1));
            }
            else
            {
                tokens.Add(MakeSymbolToken(c, start + 1));
                i++;
            }
        }

        tokens.Add(new Token(TokenType.End, "", text.Length + 1));
        return tokens;
    }

    private static Token MakeWordToken(string word, int position)
    {
        switch (word.ToLower())
        {
            case "not": return new Token(TokenType.Not, word, position);
            case "and": return new Token(TokenType.And, word, position);
            case "or": return new Token(TokenType.Or, word, position);
            case "eqv": return new Token(TokenType.Eqv, word, position);
        }

        if (CellAddress.TryParse(word, out _, out _))
            return new Token(TokenType.CellName, word.ToUpper(), position);

        throw new ParseException(
            $"Невідоме слово '{word}'. Очікувалось not, and, or, eqv або ім'я клітинки (наприклад, A1)",
            position);
    }

    private static Token MakeSymbolToken(char c, int position)
    {
        switch (c)
        {
            case '^': return new Token(TokenType.Power, "^", position);
            case '=': return new Token(TokenType.Equal, "=", position);
            case '<': return new Token(TokenType.Less, "<", position);
            case '>': return new Token(TokenType.Greater, ">", position);
            case '(': return new Token(TokenType.LeftParen, "(", position);
            case ')': return new Token(TokenType.RightParen, ")", position);
            default:
                throw new ParseException(
                    $"Недопустимий символ '{c}' (дозволено: числа, клітинки, ( ) ^ = < > not and or eqv)",
                    position);
        }
    }

    private static bool IsDigit(char c) => c >= '0' && c <= '9';

    private static bool IsLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
}
