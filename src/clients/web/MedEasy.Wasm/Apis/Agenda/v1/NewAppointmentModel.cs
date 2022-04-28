namespace MedEasy.Wasm.Apis.Agenda.v1;

using NodaTime;

public record NewAppointmentModel
{
    public ZonedDateTime StartDate { get; set; }

    public ZonedDateTime EndDate { get; set; }

    public string Location { get; set; }

    public string Subject { get; set; }

    public IEnumerable<AttendeeModel> Attendees { get; set; }

}

public record AppointmentModel
{
    public Guid Id { get; set; }

    public ZonedDateTime StartDate { get; set; }

    public ZonedDateTime EndDate { get; set; }

    public string Location { get; set; }

    public string Subject { get; set; }

    public IEnumerable<AttendeeModel> Attendees { get; set; }
}
