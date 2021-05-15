namespace Agenda.Objects
{
    using System;

    [Flags]
    public enum AppointmentStatus
    {
        NotStarted = 0x0,
        Started = 0x1,
        Ended = 0x2
    }
}
