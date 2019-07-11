using FluentValidation;
using MedEasy.DAL.Interfaces;
using Measures.DTO;
using System;
using static FluentValidation.CascadeMode;
using static FluentValidation.Severity;

namespace Measures.Validators.Features.Patients.DTO
{
    /// <summary>
    /// Validates <see cref="NewPatientInfo"/> instances.
    /// </summary>
    public class CreatePatientInfoValidator : AbstractValidator<NewPatientInfo>
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
                                .AnyAsync(p => p.Id == id, ct)
                                .ConfigureAwait(false);
                        }
                    })
                    .WithErrorCode("BAD_REQUEST")
                    .When(x => x.Id.Value != default);
            });

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(255);
        }
    }
}
