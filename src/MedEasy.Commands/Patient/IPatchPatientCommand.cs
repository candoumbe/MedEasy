using System;
using MedEasy.Commands;
using MedEasy.DTO;

namespace MedEasy.Commands.Patient
{
    /// <summary>
    /// Command to partially update a resource.
    /// </summary>
    /// <remarks>
    /// This command embed a set of <see cref="ChangeInfo"/> that must be applied atomically.
    /// </remarks>
    public interface IPatchPatientCommand : IPatchCommand<Guid, int, IPatchInfo<int>>

    {

    }
}