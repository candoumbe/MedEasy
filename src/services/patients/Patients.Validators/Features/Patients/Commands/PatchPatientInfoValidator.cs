namespace Patients.Validators.Features.Patients.Commands
{
    using FluentValidation;

    using MedEasy.DAL.Interfaces;
    using MedEasy.DTO;
    using MedEasy.Validators.Patch;

    using Microsoft.AspNetCore.JsonPatch.Operations;

    using global::Patients.DTO;
    using global::Patients.Ids;
    using global::Patients.Objects;

    using System;
    using System.Linq;

    using static FluentValidation.CascadeMode;
    using static Microsoft.AspNetCore.JsonPatch.Operations.OperationType;

    /// <summary>
    /// Validates <see cref="PatchInfo{Guid, PatientInfo}"/> instances.
    /// </summary>
    public class PatchPatientInfoValidator : AbstractValidator<PatchInfo<PatientId, PatientInfo>>
    {
        /// <summary>
        /// Builds a <see cref="PatchPatientInfoValidator"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        public PatchPatientInfoValidator(IUnitOfWorkFactory unitOfWorkFactory)
        {
            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            CascadeMode = Stop;

            RuleFor(x => x.Id)
                .NotEqual(PatientId.Empty);

            RuleFor(x => x.PatchDocument)
                .NotNull();

            When(
                x => x.Id != PatientId.Empty,
                () =>
                {
                    RuleFor(x => x.PatchDocument)
                        .SetValidator(new JsonPatchDocumentValidator<PatientInfo>() { CascadeMode = CascadeMode })

                        // The patch document should not replace or remove the resource identifier 
                        .Must(patchDocument => !patchDocument.Operations.Any(op => new[] { Replace, Remove }.Contains(op.OperationType) && string.Compare($"/{nameof(PatientInfo.Id)}", op.path, true) == 0))
                            .OverridePropertyName(nameof(PatchInfo<PatientId, PatientInfo>.PatchDocument))

                        // Cannot set lastname to null or empty string
                        .Must(patchDocument => !patchDocument.Operations.Any(op => op.OperationType == Replace
                            && string.Compare($"/{nameof(PatientInfo.Lastname)}", op.path, true) == 0
                            && op.value is string lastname && string.IsNullOrWhiteSpace(lastname)))

                        // Cannot set doctorId to empty guid
                        .Must(patchDocument => !patchDocument.Operations.Any(op => op.OperationType == Replace
                            && string.Compare($"/{nameof(PatientInfo.MainDoctorId)}", op.path, true) == 0
                            && op.value is Guid mainDoctorId && mainDoctorId == Guid.Empty))

                        .MustAsync(async (patchInfo, cancellationToken) =>
                        {
                            bool valid = false;
                            Operation<PatientInfo> replaceMainDoctorIdOperation = patchInfo.Operations
                                .SingleOrDefault(op => op.OperationType == Replace
                                 && string.Compare($"/{nameof(PatientInfo.MainDoctorId)}", op.path, true) == 0);

                            switch (replaceMainDoctorIdOperation?.value)
                            {
                                case DoctorId mainDoctorId:
                                    using (IUnitOfWork uow = unitOfWorkFactory.NewUnitOfWork())
                                    {
                                        valid = await uow.Repository<Doctor>()
                                            .AnyAsync(x => x.Id == mainDoctorId)
                                            .ConfigureAwait(false);
                                    }
                                    break;
                                default:
                                    valid = false;
                                    break;
                            }

                            return valid;
                        })
                        .WithMessage((patchInfo) =>
                        {
                            string errorMessage;
                            Operation<PatientInfo> replaceMainDoctorIdOperation = patchInfo.PatchDocument.Operations
                                .SingleOrDefault(op => op.OperationType == Replace
                                 && string.Compare($"/{nameof(PatientInfo.MainDoctorId)}", op.path, true) == 0);

                            switch (replaceMainDoctorIdOperation?.value)
                            {
                                case Guid mainDoctorId:
                                    errorMessage = $"{nameof(Doctor)} <{mainDoctorId}> not found.";
                                    break;
                                default:
                                    errorMessage = $"{nameof(Doctor)} not found.";
                                    break;
                            }

                            return errorMessage;
                        });
                });
        }
    }
}
