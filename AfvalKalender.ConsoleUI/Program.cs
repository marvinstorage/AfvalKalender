using AfvalKalender.Application.Services;
using AfvalKalender.ConsoleUI;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Infrastructure.Api;
using AfvalKalender.Infrastructure.Ics;
using AfvalKalender.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Infrastructure
        services.AddDbContext<AfvalDbContext>(options =>
            options.UseSqlite("Data Source=afvalkalender.db"));
        
        services.AddHttpClient<IAfvalApi, TwenteMilieuApi>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        services.AddScoped<IAfvalRepository, EfAfvalRepository>();
        services.AddScoped<IIcsExporter, IcsExporter>();

        // Application
        services.AddScoped<AfvalService>();

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
