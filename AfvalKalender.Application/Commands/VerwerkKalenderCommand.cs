namespace AfvalKalender.Application.Commands;

public record VerwerkKalenderCommand(
    string Postcode,
    string Huisnummer,
    int Jaar,
    int HerinneringUur,
    string OutputPad,
    string CompanyCode = "8d97bb56-5afd-4cbc-a651-b4f7314264b4");
