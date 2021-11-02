namespace Patients.Validators.Features.Patients.DTO
{
    using FluentValidation;

    using MedEasy.DAL.Interfaces;

    using global::Patients.DTO;

    using System;

    using static FluentValidation.CascadeMode;
    using static FluentValidation.Severity;
    using global::Patients.Ids;

    /// <summary>
    /// Validates <see cref="CreatePatientInfo"/> instances.
    /// </summary>
    public class CreatePatientInfoValidator : AbstractValidator<CreatePatientInfo>
    {
        /// <summary>
        /// Builds a new <see cref="CreatePatientInfoValidator"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory to create <see cref="IUnitOfWork"/> instances.</param>
        public CreatePatientInfoValidator(IUnitOfWorkFactory uowFactory)
        {
            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }

            CascadeMode = Stop;

            When(x => !string.IsNullOrWhiteSpace(x.Lastname), () =>
            {
                RuleFor(x => x.Firstname)
                    .NotEmpty().WithSeverity(Warning);
            });

            RuleFor(x => x.Id)
                .MustAsync(async (id, cancellationToken) =>
                {
                    using IUnitOfWork uow = uowFactory.NewUnitOfWork();

                    return !await uow.Repository<Objects.Patient>()
                        .AnyAsync(p => p.Id == id, cancellationToken)
                        .ConfigureAwait(false);
                })
                .When(x => x.Id != default);

            RuleFor(x => x.Firstname)
                .MaximumLength(100);

            RuleFor(x => x.Lastname)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.MainDoctorId)
                .MustAsync(async (mainDoctorId, cancellationToken) =>
                {
                    using IUnitOfWork uow = uowFactory.NewUnitOfWork();
                    return await uow.Repository<Objects.Doctor>()
                            .AnyAsync(doc => doc.Id == mainDoctorId, cancellationToken)
                            .ConfigureAwait(false);
                })
                .When(x => x.MainDoctorId != default);
        }
    }
}
