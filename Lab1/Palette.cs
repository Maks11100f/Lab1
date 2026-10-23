namespace Lab1;

/// <summary>
/// Кольори програми для світлої та темної теми (теплі жовто-бурштинові відтінки).
/// Кожен колір задано парою «…Light / …Dark»: у XAML їх обирає AppThemeBinding,
/// а в коді — властивості без суфікса (вони повертають колір для поточної теми).
/// </summary>
public static class Palette
{
    public static bool IsDark => Application.Current?.RequestedTheme == AppTheme.Dark;

    private static Color Pick(Color light, Color dark) => IsDark ? dark : light;

    // ---------- Вікно та панелі ----------
    public static readonly Color PageLight = Color.FromArgb("#FAF7F2");
    public static readonly Color PageDark = Color.FromArgb("#1C1A17");

    public static readonly Color PanelLight = Color.FromArgb("#FFFFFF");
    public static readonly Color PanelDark = Color.FromArgb("#26231F");

    public static readonly Color PanelBorderLight = Color.FromArgb("#DDD5C8");
    public static readonly Color PanelBorderDark = Color.FromArgb("#3A352E");

    public static readonly Color TextLight = Color.FromArgb("#2B2620");
    public static readonly Color TextDark = Color.FromArgb("#EDE7DD");

    public static readonly Color MutedTextLight = Color.FromArgb("#6E655A");
    public static readonly Color MutedTextDark = Color.FromArgb("#A39A8C");

    public static readonly Color AccentLight = Color.FromArgb("#B97A00");
    public static readonly Color AccentDark = Color.FromArgb("#F2B33D");

    // ---------- Кнопки ----------
    // Основні (бурштинові)
    public static readonly Color PrimaryLight = Color.FromArgb("#F2B33D");
    public static readonly Color PrimaryDark = Color.FromArgb("#E8A93A");
    public static readonly Color PrimaryHoverLight = Color.FromArgb("#F6C463");
    public static readonly Color PrimaryHoverDark = Color.FromArgb("#F2BC5A");
    public static readonly Color PrimaryPressedLight = Color.FromArgb("#DB9A1E");
    public static readonly Color PrimaryPressedDark = Color.FromArgb("#C98F28");
    public static readonly Color OnPrimary = Color.FromArgb("#2B2010");

    // Google Drive (теплий помаранчевий)
    public static readonly Color DriveLight = Color.FromArgb("#F08A4B");
    public static readonly Color DriveDark = Color.FromArgb("#D9773A");
    public static readonly Color DriveHoverLight = Color.FromArgb("#F4A26E");
    public static readonly Color DriveHoverDark = Color.FromArgb("#E88C52");
    public static readonly Color DrivePressedLight = Color.FromArgb("#D9733A");
    public static readonly Color DrivePressedDark = Color.FromArgb("#BF652F");

    // Другорядні (довідка, вихід з акаунта) та неактивна половина перемикачів
    public static readonly Color SecondaryLight = Color.FromArgb("#EDE6DA");
    public static readonly Color SecondaryDark = Color.FromArgb("#3A342C");
    public static readonly Color SecondaryHoverLight = Color.FromArgb("#E3DACB");
    public static readonly Color SecondaryHoverDark = Color.FromArgb("#4A4238");
    public static readonly Color SecondaryPressedLight = Color.FromArgb("#D8CDBB");
    public static readonly Color SecondaryPressedDark = Color.FromArgb("#2F2A23");

    // Недоступні кнопки (поки триває робота з Google Drive)
    public static readonly Color DisabledLight = Color.FromArgb("#E5E0D8");
    public static readonly Color DisabledDark = Color.FromArgb("#2E2A25");
    public static readonly Color DisabledTextLight = Color.FromArgb("#A39A8C");
    public static readonly Color DisabledTextDark = Color.FromArgb("#6E655A");

    // ---------- Таблиця ----------
    public static readonly Color GridLinesLight = Color.FromArgb("#D9D2C5");
    public static readonly Color GridLinesDark = Color.FromArgb("#3A352E");

    // ---------- Кольори для коду (залежать від поточної теми) ----------
    public static Color Text => Pick(TextLight, TextDark);
    public static Color Primary => Pick(PrimaryLight, PrimaryDark);
    public static Color Secondary => Pick(SecondaryLight, SecondaryDark);

    public static Color HeaderBackground => Pick(Color.FromArgb("#F1ECE3"), Color.FromArgb("#2A2622"));
    public static Color HeaderText => Pick(Color.FromArgb("#4A443C"), Color.FromArgb("#A39A8C"));
    public static Color SelectedHeaderBackground => Pick(Color.FromArgb("#FBE3A6"), Color.FromArgb("#4A3B1E"));
    public static Color SelectedHeaderText => Pick(Color.FromArgb("#8A5A00"), Color.FromArgb("#F2B33D"));
    public static Color CellBackground => Pick(Color.FromArgb("#FFFFFF"), Color.FromArgb("#1F1D1A"));
    public static Color CellText => Pick(Color.FromArgb("#1F1D1A"), Color.FromArgb("#EDE7DD"));
    public static Color ErrorText => Pick(Color.FromArgb("#C62828"), Color.FromArgb("#FF7A6B"));
    public static Color SelectionBorder => Pick(Color.FromArgb("#D08C00"), Color.FromArgb("#F2B33D"));
}
