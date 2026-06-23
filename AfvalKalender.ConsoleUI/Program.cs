using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.Services;
using AfvalKalender.ConsoleUI;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.Services;
using AfvalKalender.Infrastructure.Api;
using AfvalKalender.Infrastructure.Cache;
using AfvalKalender.Infrastructure.Ics;
using AfvalKalender.Infrastructure.Persistence;
using AfvalKalender.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Infrastructure
        // Get the absolute path to the project root
        var projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
        var dbPath = Path.Combine(projectRoot, "afvalkalender.db");

        services.AddDbContext<AfvalDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddHttpClient<TwenteMilieuApi>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        services.AddScoped<IAfvalApi>(sp =>
            new CacherendeAfvalApi(sp.GetRequiredService<TwenteMilieuApi>(), "apicache"));
        services.AddScoped<IAfvalRepository, EfAfvalRepository>();
        services.AddScoped<IIcsExporter, IcsExporter>();
        services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
        services.AddHttpClient<IAfvalKalenderSynchronisator, GoogleCalendarSyncAdapter>();
        services.AddHttpClient<IAfvalKalenderSynchronisator, MicrosoftGraphSyncAdapter>();
        services.AddTransient<KalenderSynchronisatieService>();

        // Application
        services.AddScoped<ICommandValidator<VerwerkKalenderCommand>, VerwerkKalenderCommandValidator>();
        services.AddScoped<VerwerkKalenderCommandHandler>();
        services.AddScoped<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>>(sp =>
            new ValidatingCommandHandlerDecorator<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>(
                sp.GetRequiredService<VerwerkKalenderCommandHandler>(),
                sp.GetRequiredService<ICommandValidator<VerwerkKalenderCommand>>()));

        // UI
        services.AddScoped<ConsoleApp>();
    })
    .Build();

// Zorg dat de database is aangemaakt
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AfvalDbContext>();
    db.Database.EnsureCreated();
}

var app = host.Services.GetRequiredService<ConsoleApp>();
await app.RunAsync();
