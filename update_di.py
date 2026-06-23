import re
import os
import glob

files = [
    "AfvalKalender.AndroidUI/MauiProgram.cs",
    "AfvalKalender.ConsoleUI/Program.cs",
    "AfvalKalender.DesktopUI/App.axaml.cs"
]

for file in files:
    with open(file, "r") as f:
        content = f.read()
    
    # Add KalenderSynchronisatieService using
    if "AfvalKalender.Domain.Services" not in content:
        content = content.replace("using AfvalKalender.Domain.Interfaces;", "using AfvalKalender.Domain.Interfaces;\nusing AfvalKalender.Domain.Services;")
        content = content.replace("using AfvalKalender.Application.Commands;", "using AfvalKalender.Application.Commands;\nusing AfvalKalender.Domain.Services;")
    
    # Replace WebDavSyncAdapter registration with all 3 and the service
    old_reg = "services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()"
    # Some use builder.Services.AddHttpClient
    old_reg_builder = "builder.Services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()"

    new_reg = """services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
        services.AddHttpClient<IAfvalKalenderSynchronisator, GoogleCalendarSyncAdapter>();
        services.AddHttpClient<IAfvalKalenderSynchronisator, MicrosoftGraphSyncAdapter>();
        services.AddTransient<KalenderSynchronisatieService>();"""
        
    new_reg_builder = """builder.Services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
        builder.Services.AddHttpClient<IAfvalKalenderSynchronisator, GoogleCalendarSyncAdapter>();
        builder.Services.AddHttpClient<IAfvalKalenderSynchronisator, MicrosoftGraphSyncAdapter>();
        builder.Services.AddTransient<KalenderSynchronisatieService>();"""

    if "ConfigurePrimaryHttpMessageHandler" in content and "services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()" in content:
        # Regex replacement might be safer
        import re
        content = re.sub(r'services\.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>\(\).*?\}\);', new_reg, content, flags=re.DOTALL)
    elif "ConfigurePrimaryHttpMessageHandler" in content and "builder.Services.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>()" in content:
        content = re.sub(r'builder\.Services\.AddHttpClient<IAfvalKalenderSynchronisator, WebDavSyncAdapter>\(\).*?\}\);', new_reg_builder, content, flags=re.DOTALL)
    else:
        content = content.replace(old_reg, new_reg).replace(old_reg_builder, new_reg_builder)

    with open(file, "w") as f:
        f.write(content)

print("DI updated.")
