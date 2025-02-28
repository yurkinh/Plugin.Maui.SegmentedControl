using Microsoft.Extensions.Logging;
using Plugin.Maui.SegmentedControl;

namespace Test.SegmentedControl;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseSegmentedControl();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
