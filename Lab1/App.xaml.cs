namespace Lab1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage())
        {
            Title = "Електронна таблиця — Лабораторна робота №1 (варіант 73)"
        };
    }
}
