using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AfvalKalender.DesktopUI.ViewModels;
using AfvalKalender.DesktopUI.Views;
using Microsoft.Extensions.DependencyInjection;
using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Infrastructure.Api;
using AfvalKalender.Infrastructure.Cache;
using AfvalKalender.Infrastructure.Ics;
using AfvalKalender.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AfvalKalender.DesktopUI;

public partial class App : Avalonia.Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        // Ensure database is created
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AfvalDbContext>();
            db.Database.EnsureCreated();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddDbContext<AfvalDbContext>(options =>
            options.UseSqlite("Data Source=afvalkalender.db"));
        
        services.AddHttpClient<TwenteMilieuApi>()
            .ConfigurePrimaryHttpMessageHandler(() => new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        services.AddScoped<IAfvalApi>(sp =>
            new CacherendeAfvalApi(sp.GetRequiredService<TwenteMilieuApi>(), "apicache"));
            
        services.AddScoped<IAfvalRepository, EfAfvalRepository>();
        services.AddScoped<IIcsExporter, IcsExporter>();

        // Application
        services.AddScoped<ICommandValidator<VerwerkKalenderCommand>, VerwerkKalenderCommandValidator>();
        services.AddScoped<VerwerkKalenderCommandHandler>();
        services.AddScoped<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>>(sp =>
            new ValidatingCommandHandlerDecorator<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>(
                sp.GetRequiredService<VerwerkKalenderCommandHandler>(),
                sp.GetRequiredService<ICommandValidator<VerwerkKalenderCommand>>()));

        // ViewModel
        services.AddTransient<MainWindowViewModel>();
    }
}
