using MedEasy.DTO;
using FluentValidation;
using MedEasy.DAL.Interfaces;
using System;
using static FluentValidation.CascadeMode;
using static FluentValidation.Severity;

namespace MedEasy.Validators
{
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

            CascadeMode = StopOnFirstFailure;

            When(x => !string.IsNullOrWhiteSpace(x.Lastname), () =>
            {
                RuleFor(x => x.Firstname)
                    .NotEmpty()
                        .WithSeverity(Warning)
                    .MaximumLength(100);

            });
            
            When(x => string.IsNullOrWhiteSpace(x.Lastname), () =>
            {
                RuleFor(x => x.Firstname)
                    .NotEmpty()
                        .WithSeverity(Error)
                    .MaximumLength(100);

            });

            RuleFor(x => x.Lastname)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.MainDoctorId)
                .NotEqual(Guid.Empty)
                .MustAsync(async (mainDoctorId, cancellationToken) =>
                {
                        
                    using (IUnitOfWork uow = uowFactory.New())
                    {
                        return await uow.Repository<Objects.Doctor>()
                                .AnyAsync(doc => doc.UUID == mainDoctorId, cancellationToken)
                                .ConfigureAwait(false);
                    }
                })
                .When(x => x.MainDoctorId.HasValue);
        }
    }
}
