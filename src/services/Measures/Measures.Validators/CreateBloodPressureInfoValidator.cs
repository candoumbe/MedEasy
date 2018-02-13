using FluentValidation;
using Measures.DTO;
using Measures.Objects;
using MedEasy.DAL.Interfaces;
using System;
using static FluentValidation.Severity;

namespace Measures.Validators
{
    /// <summary>
    /// Validator for <see cref="CreateBloodPressureInfo"/> instances.
    /// </summary>
    public class CreateBloodPressureInfoValidator : AbstractValidator<CreateBloodPressureInfo>, IValidator
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Builds a new <see cref="CreateBloodPressureInfoValidator"/> instance. 
        /// </summary>
        /// <param name="uowFactory">Factory to build <see cref="IUnitOfWork"/>s.</param>
        public CreateBloodPressureInfoValidator(IUnitOfWorkFactory uowFactory)
        {
            _unitOfWorkFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory), $"{nameof(uowFactory)} cannot be null");

            RuleFor(x => x.DiastolicPressure)
                .GreaterThan(0)
                .WithSeverity(Warning);

            RuleFor(x => x.SystolicPressure)
                .GreaterThan(0)
                .WithSeverity(Warning);

            RuleFor(x => x.Patient)
                .NotNull()
                .WithSeverity(Error);

            When(
                x => x.Patient != null,
                () =>
                {
                    When(x => x.Patient.Id != default, () =>
                    {
                        RuleFor(x => x.Patient.Id)
                            .MustAsync(async (id, cancellationToken) =>
                            {
                                using (IUnitOfWork uow = _unitOfWorkFactory.New())
                                {
                                    return await uow.Repository<Patient>()
                                        .AnyAsync(x => x.UUID == id, cancellationToken)
                                        .ConfigureAwait(false);
                                }
                            })
                            .WithName($"{nameof(CreateBloodPressureInfo.Patient)}.{nameof(PatientInfo.Id)}")
                            .WithSeverity(Error);

                        RuleFor(x => x.Patient.Firstname)
                            .Null()
                            .WithName($"{nameof(CreateBloodPressureInfo.Patient)}.{nameof(PatientInfo.Firstname)}");

                        RuleFor(x => x.Patient.Lastname)
                            .Null()
                            .WithName($"{nameof(CreateBloodPressureInfo.Patient)}.{nameof(PatientInfo.Lastname)}");

                        RuleFor(x => x.Patient.Fullname)
                            .Null()
                            .WithName($"{nameof(CreateBloodPressureInfo.Patient)}.{nameof(PatientInfo.Fullname)}");

                        RuleFor(x => x.Patient.BirthDate)
                            .Null()
                            .WithName($"{nameof(CreateBloodPressureInfo.Patient)}.{nameof(PatientInfo.BirthDate)}");

                    });
                }
            );

            When(x => x.SystolicPressure > 0 && x.DiastolicPressure > 0, () =>
            {
                RuleFor(x => x.DiastolicPressure)
                    .LessThan((info) => info.SystolicPressure)
                    .WithSeverity(Error);
            });

        }
    }
}
