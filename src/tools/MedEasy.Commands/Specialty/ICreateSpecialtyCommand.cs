using System;
using MedEasy.DTO;

namespace MedEasy.Commands.Specialty
{
    public interface ICreateSpecialtyCommand : ICommand<Guid, CreateSpecialtyInfo, SpecialtyInfo>
    {    }
}