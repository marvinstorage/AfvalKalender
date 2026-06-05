using AfvalKalender.Domain.Entities;
using AfvalKalender.Domain.Interfaces;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace AfvalKalender.Infrastructure.Ics;

public class IcsExporter : IIcsExporter
{
    public Task ExporteerAsync(IEnumerable<AfvalOphaalMoment> momenten, string bestandspad, int herinneringUurVooraf)
    {
        var calendar = new Calendar();

        foreach (var moment in momenten)
        {
            var e = new CalendarEvent
            {
                Start = new CalDateTime(moment.Datum.Date.AddHours(8)), // Start om 8:00
                End = new CalDateTime(moment.Datum.Date.AddHours(9)),   // Einde om 9:00
                Summary = moment.Omschrijving,
                Uid = $"{moment.Type}_{moment.Datum:yyyyMMdd}_{moment.Postcode}",
            };

            var alarm = new Alarm
            {
                Action = AlarmAction.Display,
                Description = $"Herinnering: {moment.Omschrijving}",
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
