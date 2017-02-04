using System;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics;
using MedEasy.Commands.Appointment;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Appointment.Commands;

namespace MedEasy.Handlers.Appointment.Commands
{


    /// <summary>
    /// An instance of this class process process <see cref="IRunDeleteAppointmentByIdCommand"/>
    /// </summary>
    public class RunDeleteAppointmentByIdCommand : IRunDeleteAppointmentInfoByIdCommand
    {

        public RunDeleteAppointmentByIdCommand(IUnitOfWorkFactory factory)
        {
            UowFactory = factory;
        }

        private IUnitOfWorkFactory UowFactory { get; }

        public async Task<Nothing> RunAsync(IDeleteAppointmentByIdCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            int id = command.Data;
            Debug.Assert(id > 0);

            using (var uow = UowFactory.New())
            {
                uow.Repository<Objects.Appointment>().Delete(item => item.Id == id);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                return Nothing.Value;
            }

            
        }
    }
}
