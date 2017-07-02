using System;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using MedEasy.Commands.Appointment;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Appointment.Commands;
using System.Threading;

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

        public async Task<Nothing> RunAsync(IDeleteAppointmentByIdCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Guid id = command.Data;
           
            using (IUnitOfWork uow = UowFactory.New())
            {
                uow.Repository<Objects.Appointment>().Delete(item => item.UUID == id);
                await uow.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                return Nothing.Value;
            }

            
        }
    }
}
