using System.Collections.Generic;
using MedEasy.DTO;
using System.Threading.Tasks;
using FluentValidation;
using System;
using static FluentValidation.Severity;
using static FluentValidation.CascadeMode;
using MedEasy.DAL.Interfaces;
using MedEasy.Objects;

namespace MedEasy.Validators.Doctor.DTO
{
    /// <summary>
    /// Validator of <see cref="CreateDoctorInfo"/> instances.
    /// </summary>
    public class CreateDoctorInfoValidator : AbstractValidator<CreateDoctorInfo>
    {
        /// <summary>
        /// Builds a new <see cref="CreateDoctorInfoValidator"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        public CreateDoctorInfoValidator(IUnitOfWorkFactory uowFactory)
        {
            CascadeMode = StopOnFirstFailure;

            When(x => !string.IsNullOrWhiteSpace(x.Lastname), () =>
            {
                RuleFor(x => x.Firstname)
                    .NotEmpty().WithSeverity(Warning);
            });

            RuleFor(x => x.Firstname)
                .MaximumLength(100).When(x => x.Firstname != null);

            RuleFor(x => x.Lastname)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(x => x.SpecialtyId)
                .NotEqual(Guid.Empty)
                .MustAsync(async (specialtyId, cancellationToken) =>
                {
                    using (IUnitOfWork uow = uowFactory.New())
                    {
                        return await uow.Repository<Specialty>()
                            .AnyAsync(x => x.UUID == specialtyId)
                            .ConfigureAwait(false);
                    }
                })
                .When(x => x.SpecialtyId.HasValue);
        }
    }
}
