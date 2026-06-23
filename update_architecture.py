import os
import re

domain_dir = "AfvalKalender.Domain"
app_dir = "AfvalKalender.Application"
infra_dir = "AfvalKalender.Infrastructure"
tests_dir = "AfvalKalender.UnitTests"

# 1. SyncProvider
os.makedirs(f"{domain_dir}/ValueObjects", exist_ok=True)
with open(f"{domain_dir}/ValueObjects/SyncProvider.cs", "w") as f:
    f.write("""namespace AfvalKalender.Domain.ValueObjects;

public enum SyncProvider
{
    Geen,
    WebDav,
    GoogleCalendar,
    MicrosoftGraph
}
""")

# 2. SyncConfiguratie
with open(f"{domain_dir}/ValueObjects/SyncConfiguratie.cs", "w") as f:
    f.write("""namespace AfvalKalender.Domain.ValueObjects;

public record SyncConfiguratie(
    SyncProvider Provider,
    string DoelUrlOfToken,
    string Gebruikersnaam,
    string Wachtwoord
);
""")

# 3. IAfvalKalenderSynchronisator
with open(f"{domain_dir}/Interfaces/IAfvalKalenderSynchronisator.cs", "w") as f:
    f.write("""using System.Collections.Generic;
using System.Threading.Tasks;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Domain.Interfaces;

public interface IAfvalKalenderSynchronisator
{
    bool Ondersteunt(SyncProvider provider);
    Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, SyncConfiguratie configuratie, int herinneringUur);
}
""")

# 4. KalenderSynchronisatieService
os.makedirs(f"{domain_dir}/Services", exist_ok=True)
with open(f"{domain_dir}/Services/KalenderSynchronisatieService.cs", "w") as f:
    f.write("""using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Domain.Services;

public class KalenderSynchronisatieService
{
    private readonly IEnumerable<IAfvalKalenderSynchronisator> _synchronisatoren;

    public KalenderSynchronisatieService(IEnumerable<IAfvalKalenderSynchronisator> synchronisatoren)
    {
        _synchronisatoren = synchronisatoren;
    }

    public async Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, SyncConfiguratie configuratie, int herinneringUur)
    {
        if (configuratie.Provider == SyncProvider.Geen)
        {
            return;
        }

        var synchronisator = _synchronisatoren.FirstOrDefault(s => s.Ondersteunt(configuratie.Provider));
        if (synchronisator == null)
        {
            throw new System.NotSupportedException($"Geen synchronisator gevonden voor provider {configuratie.Provider}.");
        }

        await synchronisator.SynchroniseerAsync(momenten, configuratie, herinneringUur);
    }
}
""")

# 5. WebDavSyncAdapter
webdav_path = f"{infra_dir}/Sync/WebDavSyncAdapter.cs"
with open(webdav_path, "r") as f:
    webdav_code = f.read()

webdav_code = webdav_code.replace("using AfvalKalender.Domain.Interfaces;", "using AfvalKalender.Domain.Interfaces;\nusing AfvalKalender.Domain.ValueObjects;")
webdav_code = webdav_code.replace("public async Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, string webDavUrl, string gebruikersnaam, string wachtwoord, int herinneringUur)", 
"""public bool Ondersteunt(SyncProvider provider) => provider == SyncProvider.WebDav;

    public async Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, SyncConfiguratie configuratie, int herinneringUur)""")
webdav_code = webdav_code.replace("webDavUrl", "configuratie.DoelUrlOfToken")
webdav_code = webdav_code.replace("gebruikersnaam", "configuratie.Gebruikersnaam")
webdav_code = webdav_code.replace("wachtwoord", "configuratie.Wachtwoord")

with open(webdav_path, "w") as f:
    f.write(webdav_code)

# 6. GoogleCalendarSyncAdapter
with open(f"{infra_dir}/Sync/GoogleCalendarSyncAdapter.cs", "w") as f:
    f.write("""using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Infrastructure.Sync;

public class GoogleCalendarSyncAdapter : IAfvalKalenderSynchronisator
{
    private readonly HttpClient _httpClient;

    public GoogleCalendarSyncAdapter(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool Ondersteunt(SyncProvider provider) => provider == SyncProvider.GoogleCalendar;

    public async Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, SyncConfiguratie configuratie, int herinneringUur)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuratie.DoelUrlOfToken);
        // Note: Implementation omitted for demonstration. Here we would map `momenten` to Google Calendar JSON
        // and POST to https://www.googleapis.com/calendar/v3/calendars/primary/events
        await Task.CompletedTask;
    }
}
""")

# 7. MicrosoftGraphSyncAdapter
with open(f"{infra_dir}/Sync/MicrosoftGraphSyncAdapter.cs", "w") as f:
    f.write("""using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Infrastructure.Sync;

public class MicrosoftGraphSyncAdapter : IAfvalKalenderSynchronisator
{
    private readonly HttpClient _httpClient;

    public MicrosoftGraphSyncAdapter(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool Ondersteunt(SyncProvider provider) => provider == SyncProvider.MicrosoftGraph;

    public async Task SynchroniseerAsync(IEnumerable<AfvalOphaalMoment> momenten, SyncConfiguratie configuratie, int herinneringUur)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuratie.DoelUrlOfToken);
        // Note: Implementation omitted for demonstration. Here we would map `momenten` to Microsoft Graph JSON
        // and POST to https://graph.microsoft.com/v1.0/me/events
        await Task.CompletedTask;
    }
}
""")

# 8. VerwerkKalenderCommand
with open(f"{app_dir}/Commands/VerwerkKalenderCommand.cs", "w") as f:
    f.write("""using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Application.Commands;

public record VerwerkKalenderCommand(
    string Postcode,
    string Huisnummer,
    int Jaar,
    int HerinneringUur,
    string OutputPad,
    string CompanyCode = "8d97bb56-5afd-4cbc-a651-b4f7314264b4",
    bool ForceerVernieuwen = false,
    SyncProvider SyncProvider = SyncProvider.Geen,
    string? SyncDoelUrlOfToken = null,
    string? SyncGebruiker = null,
    string? SyncWachtwoord = null);
""")

# 9. VerwerkKalenderCommandHandler
handler_code = """using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.Services;
using AfvalKalender.Domain.ValueObjects;

namespace AfvalKalender.Application.Commands;

public class VerwerkKalenderCommandHandler
    : ICommandHandler<VerwerkKalenderCommand, IReadOnlyList<AfvalOphaalMoment>>
{
    private readonly IAfvalApi _afvalApi;
    private readonly IAfvalRepository _afvalRepository;
    private readonly IIcsExporter _icsExporter;
    private readonly KalenderSynchronisatieService _synchronisatieService;

    public VerwerkKalenderCommandHandler(
        IAfvalApi afvalApi,
        IAfvalRepository afvalRepository,
        IIcsExporter icsExporter,
        KalenderSynchronisatieService synchronisatieService)
    {
        _afvalApi = afvalApi;
        _afvalRepository = afvalRepository;
        _icsExporter = icsExporter;
        _synchronisatieService = synchronisatieService;
    }

    public async Task<IReadOnlyList<AfvalOphaalMoment>> HandleAsync(
        VerwerkKalenderCommand command, CancellationToken ct = default)
    {
        var uniekId = await _afvalApi.HaalUniekAdresIdOpAsync(command.Postcode, command.Huisnummer, command.CompanyCode, command.ForceerVernieuwen);
        var momenten = await _afvalApi.HaalKalenderOpAsync(uniekId, command.Postcode, command.Huisnummer, command.Jaar, command.CompanyCode, command.ForceerVernieuwen);

        await _afvalRepository.SlaOpOfUpdateAsync(momenten);

        var opgeslagenMomenten = await _afvalRepository.HaalOpVoorAdresEnJaarAsync(
            command.Postcode, command.Huisnummer, command.Jaar);

        await _icsExporter.ExporteerAsync(opgeslagenMomenten, command.OutputPad, command.HerinneringUur);

        var config = new SyncConfiguratie(command.SyncProvider, command.SyncDoelUrlOfToken ?? "", command.SyncGebruiker ?? "", command.SyncWachtwoord ?? "");
        await _synchronisatieService.SynchroniseerAsync(opgeslagenMomenten, config, command.HerinneringUur);

        return opgeslagenMomenten.ToList().AsReadOnly();
    }
}
"""
with open(f"{app_dir}/Commands/VerwerkKalenderCommandHandler.cs", "w") as f:
    f.write(handler_code)

# 10. Update Tests
tests_file = f"{tests_dir}/Application/AfvalServiceTests.cs"
with open(tests_file, "r") as f:
    t_code = f.read()

t_code = t_code.replace("using AfvalKalender.Domain.Interfaces;", "using AfvalKalender.Domain.Interfaces;\nusing AfvalKalender.Domain.Services;")
t_code = t_code.replace("Mock<IAfvalKalenderSynchronisator> _mockSync", "Mock<IAfvalKalenderSynchronisator> _mockSync")
t_code = t_code.replace("new(_mockApi.Object, _mockRepo.Object, _mockIcs.Object, _mockSync.Object);", "new(_mockApi.Object, _mockRepo.Object, _mockIcs.Object, new KalenderSynchronisatieService(new[] { _mockSync.Object }));")

t_code = t_code.replace("13, \"test.ics\", \"company\", false, \"https://dav\", \"user\", \"pass\"", "13, \"test.ics\", \"company\", false, SyncProvider.WebDav, \"https://dav\", \"user\", \"pass\"")
t_code = t_code.replace("_mockSync.Verify(x => x.SynchroniseerAsync(momenten, \"https://dav\", \"user\", \"pass\", 13), Times.Once);", "_mockSync.Verify(x => x.SynchroniseerAsync(momenten, It.Is<SyncConfiguratie>(c => c.Provider == SyncProvider.WebDav && c.DoelUrlOfToken == \"https://dav\"), 13), Times.Once);")

t_code = t_code.replace("It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),", "It.IsAny<SyncConfiguratie>(),")
t_code = t_code.replace("_mockSync.Setup(x => x.Ondersteunt(SyncProvider.WebDav)).Returns(true);", "") # we will add it manually
t_code = t_code.replace("var command = new VerwerkKalenderCommand(\"1234AB\", \"10\", 2026, 13, \"test.ics\", \"company\", false, SyncProvider.WebDav", "_mockSync.Setup(x => x.Ondersteunt(SyncProvider.WebDav)).Returns(true);\n        var command = new VerwerkKalenderCommand(\"1234AB\", \"10\", 2026, 13, \"test.ics\", \"company\", false, SyncProvider.WebDav")

with open(tests_file, "w") as f:
    f.write(t_code)

print("Architecture updated successfully.")
