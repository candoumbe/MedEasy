using System;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics;
using MedEasy.Commands.Doctor;

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

        public async Task RunAsync(IDeleteDoctorByIdCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            int id = command.Data;
            Debug.Assert(id > 0);

            using (var uow = UowFactory.New())
            {
                uow.Repository<Objects.Doctor>().Delete(item => item.Id == id);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
