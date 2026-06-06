namespace AfvalKalender.Application.Commands;

public record VerwerkKalenderCommand(
    string Postcode,
    string Huisnummer,
    int Jaar,
    int HerinneringUur,
    string OutputPad);
