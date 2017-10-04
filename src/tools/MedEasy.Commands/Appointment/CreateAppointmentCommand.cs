using System;
using MedEasy.DTO;

namespace MedEasy.Commands.Appointment
{

    /// <summary>
    /// 
    /// </summary>
    public class CreateAppointmentCommand : CommandBase<Guid, CreateAppointmentInfo>, ICreateAppointmentCommand
    {
        
        /// <summary>
        /// Builds a new <see cref="CreateAppointmentCommand"/> instance with default validation
        /// </summary>
        /// <param name="data">data to process</param>
        public CreateAppointmentCommand(CreateAppointmentInfo data) : base(Guid.NewGuid(), data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
        
    }


    
}
