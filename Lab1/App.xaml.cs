using Lab1.Services;

namespace Lab1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = ThemeService.LoadSavedTheme();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage())
        {
            Title = MainPage.AppTitle,
            // Менше вікно не дозволяємо, щоб групи кнопок на панелі не обрізалися
            MinimumWidth = 1120,
            MinimumHeight = 600
        };
    }
}
