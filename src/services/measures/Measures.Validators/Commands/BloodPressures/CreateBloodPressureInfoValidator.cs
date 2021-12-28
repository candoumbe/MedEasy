namespace Measures.Validators.Commands.BloodPressures
{
    using FluentValidation;

    using Measures.DTO;
    using Measures.Ids;
    using Measures.Objects;

    using MedEasy.DAL.Interfaces;

    using System;
    using System.Threading;

    using static FluentValidation.Severity;

    /// <summary>
    /// Validator for <see cref="CreateBloodPressureInfo"/> instances.
    /// </summary>
    public class CreateBloodPressureInfoValidator : AbstractValidator<CreateBloodPressureInfo>
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

            RuleFor(x => x.SubjectId)
                .NotNull()
                .NotEqual(SubjectId.Empty)
                .WithSeverity(Error);

            When(
                x => x.SubjectId is not null && x.SubjectId != SubjectId.Empty,
                () =>
                {
                    RuleFor(x => x.SubjectId)
                        .MustAsync(async (SubjectId patientId, CancellationToken ct) =>
                        {
                            using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();
                            return await uow.Repository<Subject>()
                                            .AnyAsync(x => x.Id == patientId, ct)
                                            .ConfigureAwait(false);
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
