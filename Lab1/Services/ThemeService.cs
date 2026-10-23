namespace Lab1.Services;

/// <summary>
/// Вибір теми оформлення (світла / темна) і запам'ятовування вибору між запусками програми.
/// </summary>
public static class ThemeService
{
    private const string PreferenceKey = "AppTheme";

    /// <summary>Тема, збережена минулого разу (за замовчуванням — темна).</summary>
    public static AppTheme LoadSavedTheme()
    {
        string saved = Preferences.Default.Get(PreferenceKey, nameof(AppTheme.Dark));
        return saved == nameof(AppTheme.Light) ? AppTheme.Light : AppTheme.Dark;
    }

    /// <summary>Застосовує тему до всієї програми та запам'ятовує її.</summary>
    public static void Apply(AppTheme theme)
    {
        if (Application.Current != null)
            Application.Current.UserAppTheme = theme;
        Preferences.Default.Set(PreferenceKey, theme.ToString());
    }
}
