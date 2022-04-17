namespace MedEasy.Wasm.Apis.Agenda.v1;

using MedEasy.Attributes;

using NodaTime;

public record SearchAppointmentModel
{
    /// <summary>
    /// Min start or end date
    /// </summary>
    public ZonedDateTime? From { get; set; }

    /// <summary>
    /// Max start or end date
    /// </summary>
    public ZonedDateTime? To { get; set; }

    /// <summary>
    /// Subject of the appointments
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Location of the appointment
    /// </summary>
    public string Location { get; set; }

    [Minimum(1)]
    public int Page { get; set; }

    [Minimum(1)]
    public int PageSize { get; set; }


    public string Sort { get; set; }
}
