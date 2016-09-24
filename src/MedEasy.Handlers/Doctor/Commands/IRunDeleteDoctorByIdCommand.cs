using MedEasy.Commands.Doctor;
using MedEasy.Handlers.Commands;
using System;

namespace MedEasy.Handlers.Doctor.Commands
{
    public interface IRunDeleteDoctorInfoByIdCommand : IRunCommandAsync<Guid, int, IDeleteDoctorByIdCommand>
    {

    }

}
