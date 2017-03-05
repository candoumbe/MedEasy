using System;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics;
using MedEasy.Commands.Doctor;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Doctor.Commands;

namespace MedEasy.Handlers.Doctor.Commands
{


    /// <summary>
    /// An instance of this class process process <see cref="IRunDeleteDoctorByIdCommand"/>
    /// </summary>
    public class RunDeleteDoctorByIdCommand : IRunDeleteDoctorInfoByIdCommand
    {

        public RunDeleteDoctorByIdCommand(IUnitOfWorkFactory factory)
        {
            UowFactory = factory;
        }

        private IUnitOfWorkFactory UowFactory { get; }

        public async Task<Nothing> RunAsync(IDeleteDoctorByIdCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Guid id = command.Data;
            Debug.Assert(id != Guid.Empty);

            using (IUnitOfWork uow = UowFactory.New())
            {
                uow.Repository<Objects.Doctor>().Delete(item => item.UUID == id);
                await uow.SaveChangesAsync();

                return Nothing.Value;
            }

            
        }
    }
}
