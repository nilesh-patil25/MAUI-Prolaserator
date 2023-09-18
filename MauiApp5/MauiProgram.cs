﻿using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;
using System.Reflection;

namespace MauiApp5;

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
            });

        using var stream = Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("MauiApp5.appsettings.json");
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        builder.Configuration.AddConfiguration(config);
        builder.Services.AddTransient<MainPage>();
        return builder.Build();
    }
}
