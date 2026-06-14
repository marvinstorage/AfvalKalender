using AfvalKalender.Application.Commands;
using AfvalKalender.ConsoleUI;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Infrastructure.Api;
using AfvalKalender.Infrastructure.Cache;
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

        services.AddHttpClient<TwenteMilieuApi>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
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
