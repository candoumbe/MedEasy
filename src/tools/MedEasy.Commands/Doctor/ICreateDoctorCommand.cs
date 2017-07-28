using MedEasy.DTO;
using System;

namespace MedEasy.Commands.Doctor
{
    public interface ICreateDoctorCommand : ICommand<Guid, CreateDoctorInfo, DoctorInfo>
    {
    }
}
