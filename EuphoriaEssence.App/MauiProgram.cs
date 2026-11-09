using EuphoriaEssence.App.Services;
using Microsoft.Extensions.Logging;

namespace EuphoriaEssence.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(
                        "OpenSans-Regular.ttf",
                        "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddSingleton<HttpClient>(_ =>
                new HttpClient
                {
                    BaseAddress = new Uri(
                        "https://euphoriaessence-api.runasp.net/"),

                    Timeout = TimeSpan.FromSeconds(60)
                });

            builder.Services.AddSingleton<SesionService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}