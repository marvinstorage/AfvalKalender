import os

def fix_tests():
    path = "AfvalKalender.Infrastructure.Tests/Sync/WebDavSyncAdapterTests.cs"
    with open(path, "r") as f:
        code = f.read()
    
    code = code.replace("using AfvalKalender.Domain.Interfaces;", "using AfvalKalender.Domain.Interfaces;\nusing AfvalKalender.Domain.ValueObjects;")
    code = code.replace("_sut.SynchroniseerAsync(momenten, \"http://dav\", \"user\", \"pass\", 12)", "_sut.SynchroniseerAsync(momenten, new SyncConfiguratie(SyncProvider.WebDav, \"http://dav\", \"user\", \"pass\"), 12)")
    code = code.replace("_sut.SynchroniseerAsync(momenten, \"http://dav\", \"\", \"\", 12)", "_sut.SynchroniseerAsync(momenten, new SyncConfiguratie(SyncProvider.WebDav, \"http://dav\", \"\", \"\"), 12)")
    code = code.replace("_sut.SynchroniseerAsync(momenten, \"\", \"\", \"\", 12)", "_sut.SynchroniseerAsync(momenten, new SyncConfiguratie(SyncProvider.WebDav, \"\", \"\", \"\"), 12)")

    with open(path, "w") as f:
        f.write(code)

def fix_viewmodels():
    # Desktop
    path = "AfvalKalender.DesktopUI/ViewModels/MainWindowViewModel.cs"
    with open(path, "r") as f:
        code = f.read()
    code = code.replace("using AfvalKalender.Application.Commands;", "using AfvalKalender.Application.Commands;\nusing AfvalKalender.Domain.ValueObjects;")
    # The old command was:
    # new VerwerkKalenderCommand(Postcode, Huisnummer, Jaar, HerinneringUur, _outputPad, SelectedVerwerker.CompanyCode, ForceerVernieuwen, WebDavUrl, WebDavGebruiker, WebDavWachtwoord);
    # The new is:
    # new VerwerkKalenderCommand(..., ForceerVernieuwen, SyncProvider, SyncDoelUrlOfToken, SyncGebruiker, SyncWachtwoord)
    
    code = code.replace("ForceerVernieuwen, WebDavUrl, WebDavGebruiker, WebDavWachtwoord", "ForceerVernieuwen, string.IsNullOrWhiteSpace(WebDavUrl) ? SyncProvider.Geen : SyncProvider.WebDav, WebDavUrl, WebDavGebruiker, WebDavWachtwoord")
    with open(path, "w") as f:
        f.write(code)

    # Android
    path = "AfvalKalender.AndroidUI/ViewModels/MainPageViewModel.cs"
    if os.path.exists(path):
        with open(path, "r") as f:
            code = f.read()
        code = code.replace("using AfvalKalender.Application.Commands;", "using AfvalKalender.Application.Commands;\nusing AfvalKalender.Domain.ValueObjects;")
        code = code.replace("ForceerVernieuwen, WebDavUrl, WebDavGebruiker, WebDavWachtwoord", "ForceerVernieuwen, string.IsNullOrWhiteSpace(WebDavUrl) ? SyncProvider.Geen : SyncProvider.WebDav, WebDavUrl, WebDavGebruiker, WebDavWachtwoord")
        with open(path, "w") as f:
            f.write(code)

def fix_console():
    path = "AfvalKalender.ConsoleUI/ConsoleApp.cs"
    with open(path, "r") as f:
        code = f.read()
    code = code.replace("using AfvalKalender.Application.Commands;", "using AfvalKalender.Application.Commands;\nusing AfvalKalender.Domain.ValueObjects;")
    code = code.replace("ForceerVernieuwen, webDavUrl, webDavGebruiker, webDavWachtwoord", "ForceerVernieuwen, string.IsNullOrWhiteSpace(webDavUrl) ? SyncProvider.Geen : SyncProvider.WebDav, webDavUrl, webDavGebruiker, webDavWachtwoord")
    with open(path, "w") as f:
        f.write(code)

def fix_desktop_app():
    path = "AfvalKalender.DesktopUI/App.axaml.cs"
    with open(path, "r") as f:
        code = f.read()
    if "using System.Net.Http;" not in code:
        code = code.replace("using Avalonia.Markup.Xaml;", "using Avalonia.Markup.Xaml;\nusing System.Net.Http;")
    with open(path, "w") as f:
        f.write(code)

def fix_console_di():
    path = "AfvalKalender.ConsoleUI/Program.cs"
    with open(path, "r") as f:
        code = f.read()
    if "using System.Net.Http;" not in code:
        code = code.replace("using Microsoft.Extensions.Hosting;", "using Microsoft.Extensions.Hosting;\nusing System.Net.Http;")
    with open(path, "w") as f:
        f.write(code)

fix_tests()
fix_viewmodels()
fix_console()
fix_desktop_app()
fix_console_di()

print("Files fixed.")
