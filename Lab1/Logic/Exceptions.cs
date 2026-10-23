namespace Lab1.Logic;

/// <summary>
/// Синтаксична помилка у виразі (етап 1 — перевірка синтаксису).
/// Містить позицію символу, де знайдено помилку.
/// </summary>
public class ParseException : Exception
{
    public int Position { get; }

    public ParseException(string message, int position) : base(message)
    {
        Position = position;
    }
}

/// <summary>
/// Помилка під час обчислення значення (етап 2):
/// циклічне посилання, неіснуюча клітинка, завеликий степінь тощо.
/// </summary>
public class CalculationException : Exception
{
    public CalculationException(string message) : base(message)
    {
    }
}
