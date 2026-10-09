using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using AfvalKalender.Domain.ValueObjects;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace AfvalKalender.Infrastructure.Ics;

public class IcsExporter : IIcsExporter
{
    public Task ExporteerAsync(IEnumerable<AfvalOphaalMoment> momenten, string bestandspad, int herinneringUurVooraf, Taal taal)
    {
        var calendar = new Calendar();

        foreach (var moment in momenten)
        {
            var omschrijving = taal == Taal.Nederlands
                ? moment.Omschrijving
                : AfvalTypeVertaling.Omschrijving(moment.Type, taal);

            var e = new CalendarEvent
            {
                Start = new CalDateTime(moment.Datum.Date.AddHours(8)), // Start om 8:00
                End = new CalDateTime(moment.Datum.Date.AddHours(9)),   // Einde om 9:00
                Summary = omschrijving,
                Uid = $"{moment.Type}_{moment.Datum:yyyyMMdd}_{moment.Postcode}",
            };

            var alarm = new Alarm
            {
                Action = AlarmAction.Display,
                Description = $"{AfvalTypeVertaling.HerinneringVoorvoegsel(taal)} {omschrijving}",
                Trigger = new Trigger($"-PT{herinneringUurVooraf}H")
            };

            e.Alarms.Add(alarm);
            calendar.Events.Add(e);
        }

        var serializer = new CalendarSerializer();
        var icsString = serializer.SerializeToString(calendar);
        File.WriteAllText(bestandspad, icsString);

        return Task.CompletedTask;
    }
}
