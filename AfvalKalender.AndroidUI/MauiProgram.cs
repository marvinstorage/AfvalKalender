using Microsoft.Extensions.Logging;
using AfvalKalender.Application.Commands;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Infrastructure.Api;
using AfvalKalender.Infrastructure.Cache;
using AfvalKalender.Infrastructure.Ics;
using AfvalKalender.Infrastructure.Persistence;
using AfvalKalender.Infrastructure.Sync;
using AfvalKalender.AndroidUI.ViewModels;
using AfvalKalender.AndroidUI.Views;
using Microsoft.EntityFrameworkCore;

namespace AfvalKalender.AndroidUI;

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

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Infrastructure
		string dbPath = Path.Combine(FileSystem.AppDataDirectory, "afvalkalender.db");
		builder.Services.AddDbContext<AfvalDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		builder.Services.AddHttpClient<TwenteMilieuApi>()
			.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
			});

		builder.Services.AddScoped<IAfvalApi>(sp =>
			new CacherendeAfvalApi(sp.GetRequiredService<TwenteMilieuApi>(), Path.Combine(FileSystem.CacheDirectory, "apicache")));
		
		builder.Services.AddScoped<IAfvalRepository, EfAfvalRepository>();
		builder.Services.AddScoped<IIcsExporter, IcsExporter>();
		builder.Services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()
			.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
			});

		// Application
		builder.Services.AddScoped<ICommandValidator<VerwerkKalenderCommand>, VerwerkKalenderCommandValidator>();
		builder.Services.AddScoped<VerwerkKalenderCommandHandler>();
		builder.Services.AddScoped<ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>>(sp =>
			new ValidatingCommandHandlerDecorator<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>(
				sp.GetRequiredService<VerwerkKalenderCommandHandler>(),
				sp.GetRequiredService<ICommandValidator<VerwerkKalenderCommand>>()));

		// ViewModels
		builder.Services.AddTransient<MainPageViewModel>();

		// Views
		builder.Services.AddTransient<MainPage>();

		var app = builder.Build();

		// Ensure Database created
		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AfvalDbContext>();
			db.Database.EnsureCreated();
		}

		return app;
	}
}
