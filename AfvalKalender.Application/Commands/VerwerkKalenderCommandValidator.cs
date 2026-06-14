using System;
using System.Text.RegularExpressions;

namespace AfvalKalender.Application.Commands;

public class VerwerkKalenderCommandValidator : ICommandValidator<VerwerkKalenderCommand>
{
    private static readonly Regex PostcodeRegex = new(@"^[1-9][0-9]{3}[A-Z]{2}$", RegexOptions.Compiled);

    public void Validate(VerwerkKalenderCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command), "Command mag niet null zijn.");

        if (string.IsNullOrWhiteSpace(command.Postcode))
            throw new ArgumentException("Postcode is verplicht.", nameof(command.Postcode));

        if (!PostcodeRegex.IsMatch(command.Postcode))
            throw new ArgumentException("Postcode is ongeldig. Moet formaat 1234AB hebben.", nameof(command.Postcode));

        if (string.IsNullOrWhiteSpace(command.Huisnummer))
            throw new ArgumentException("Huisnummer is verplicht.", nameof(command.Huisnummer));

        if (command.Jaar < 2000 || command.Jaar > 2100)
            throw new ArgumentException("Jaar moet tussen 2000 en 2100 liggen.", nameof(command.Jaar));

        if (command.HerinneringUur < 0 || command.HerinneringUur > 23)
            throw new ArgumentException("HerinneringUur moet tussen 0 en 23 liggen.", nameof(command.HerinneringUur));

        if (string.IsNullOrWhiteSpace(command.OutputPad))
            throw new ArgumentException("OutputPad is verplicht.", nameof(command.OutputPad));

        if (!Guid.TryParse(command.CompanyCode, out _))
            throw new ArgumentException("CompanyCode is geen geldige GUID.", nameof(command.CompanyCode));
    }
}
