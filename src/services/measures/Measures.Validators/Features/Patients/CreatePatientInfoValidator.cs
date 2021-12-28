namespace Measures.Validators.Features.Patients.DTO
{
    using FluentValidation;
    using MedEasy.DAL.Interfaces;
    using Measures.DTO;
    using System;
    using Measures.Ids;

    /// <summary>
    /// Validates <see cref="NewSubjectInfo"/> instances.
    /// </summary>
    public class CreatePatientInfoValidator : AbstractValidator<NewSubjectInfo>
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

            When(x => x.Id is not null, () =>
            {
                RuleFor(x => x.Id)
                    .NotEqual(SubjectId.Empty);

                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) =>
                    {
                        using IUnitOfWork uow = uowFactory.NewUnitOfWork();

                        return !await uow.Repository<Objects.Subject>()
                            .AnyAsync(p => p.Id == id, ct)
                            .ConfigureAwait(false);
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
