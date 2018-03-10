using FluentValidation;
using MedEasy.DAL.Interfaces;
using Measures.DTO;
using System;
using static FluentValidation.CascadeMode;
using static FluentValidation.Severity;

namespace Measures.Validators.Features.Patients.DTO
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

            When(x => !string.IsNullOrWhiteSpace(x.Lastname), () =>
            {
                RuleFor(x => x.Firstname)
                    .NotEmpty().WithSeverity(Warning);
            });
            When(x => x.Id.HasValue, () =>
            {
                RuleFor(x => x.Id)
                    .NotEqual(default(Guid));

                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) =>
                    {
                        using (IUnitOfWork uow = uowFactory.NewUnitOfWork())
                        {
                            return !await uow.Repository<Objects.Patient>()
                                .AnyAsync(p => p.UUID == id, ct)
                                .ConfigureAwait(false);
                        }
                    })
                    .WithErrorCode("BAD_REQUEST")
                    .When(x => x.Id.Value != default);
            });
            

            RuleFor(x => x.Firstname)
                .MaximumLength(255);

            RuleFor(x => x.Lastname)
                .NotEmpty()
                .MaximumLength(255);
        }
    }
}
