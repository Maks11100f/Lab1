using Microsoft.Maui.LifecycleEvents;

namespace Lab1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if WINDOWS
        // Питаємо користувача перед закриттям вікна, якщо є незбережені зміни
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(windows => windows.OnWindowCreated(window =>
            {
                window.AppWindow.Closing += async (sender, args) =>
                {
                    MainPage? page = MainPage.Current;
                    if (page == null || !page.HasUnsavedChanges)
                        return;

                    args.Cancel = true;
                    if (await page.AskExitAsync())
                    {
                        page.HasUnsavedChanges = false;
                        Application.Current?.Quit();
                    }
                };
            }));
        });
#endif

        return builder.Build();
    }
}
