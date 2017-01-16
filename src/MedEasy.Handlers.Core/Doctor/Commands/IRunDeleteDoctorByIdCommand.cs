using MedEasy.Commands.Doctor;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Doctor.Commands
{
    public interface IRunDeleteDoctorInfoByIdCommand : IRunCommandAsync<Guid, int, IDeleteDoctorByIdCommand>
    {

    }

}
