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
    /// Process <see cref="IRunDeleteAppointmentByIdCommand"/> instances
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

            Guid id = command.Data;
           
            using (IUnitOfWork uow = UowFactory.New())
            {
                uow.Repository<Objects.Appointment>().Delete(item => item.UUID == id);
                await uow.SaveChangesAsync();

                return Nothing.Value;
            }

            
        }
    }
}
