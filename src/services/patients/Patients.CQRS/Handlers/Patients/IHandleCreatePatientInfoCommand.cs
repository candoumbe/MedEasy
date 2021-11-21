namespace Patients.CQRS.Handlers.Patients
{

    using MediatR;

    using global::Patients.CQRS.Commands;
    using global::Patients.DTO;
    using MedEasy.CQRS.Core.Commands.Results;
    using Optional;

    /// <summary>
    /// Contract for handling creation of new <see cref="PatientInfo"/> resources.
    /// </summary>
    public interface IHandleCreatePatientInfoCommand : IRequestHandler<CreatePatientInfoCommand, Option<PatientInfo, CreateCommandFailure>> { }
}
