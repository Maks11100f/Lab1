using Microsoft.Maui.LifecycleEvents;

namespace Lab1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if WINDOWS
        // Клітинки таблиці — пласкі прямокутники без рамки та заокруглених кутів (як в Excel).
        // Межі клітинок малює сама сітка, а вибрану клітинку — рамка навколо поля (див. MainPage).
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("FlatTableCell", (handler, view) =>
        {
            if (view is not CellEntry)
                return;

            var textBox = handler.PlatformView;
            var noBorder = new Microsoft.UI.Xaml.Thickness(0);
            textBox.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(0);
            textBox.BorderThickness = noBorder;
            textBox.Resources["TextControlBorderThemeThickness"] = noBorder;
            textBox.Resources["TextControlBorderThemeThicknessFocused"] = noBorder;
            // Фон поля прозорий (BackgroundColor = Transparent у MainPage), тож видно фон рамки-контейнера.
            textBox.MinHeight = 0;
            textBox.MinWidth = 0;
            textBox.Padding = new Microsoft.UI.Xaml.Thickness(6, 4, 6, 4);
        });

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
