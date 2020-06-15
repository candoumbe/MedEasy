using FluentValidation;

using Measures.CQRS.Commands.GenericMeasures;
using Measures.Objects;

using MedEasy.DAL.Interfaces;

using System.Collections.Generic;
using System.Linq;

namespace Measures.Validators.Commands.GenericMeasures
{
    public class CreateGenericMeasureFormInfoCommandValidator : AbstractValidator<CreateGenericMeasureFormInfoCommand>
    {
        public CreateGenericMeasureFormInfoCommandValidator(IUnitOfWorkFactory uowFactory)
        {
            RuleFor(x => x.Data).NotNull()
                                .WithSeverity(Severity.Error);

            RuleFor(x => x.Data.Name).NotEmpty()
                                     .WithSeverity(Severity.Error);

            RuleFor(x => x.Data.Fields).NotEmpty()
                                       .WithSeverity(Severity.Error);

            When(x => !string.IsNullOrWhiteSpace(x.Data.Name),
                () =>
                {
                    RuleFor(x => x.Data.Name)
                        .MustAsync(async (name, cancellation) => {
                            using IUnitOfWork uow = uowFactory.NewUnitOfWork();
                            return !await uow.Repository<MeasureForm>().AnyAsync(mf => mf.Name == name, cancellation)
                                              .ConfigureAwait(false);
                            })
                        .WithSeverity(Severity.Error)
                        .WithMessage(cmd => $"A form with the name '{cmd.Data.Name}' already exists");

                    When(x => x.Data.Fields.AtLeastOnce(),
                        () =>
                        {
                            RuleForEach(x => x.Data.Fields)
                                .Must((_, field) => !string.IsNullOrWhiteSpace(field.Name))
                                .WithSeverity(Severity.Error)
                                .WithMessage("The property 'name' is not set");

                            When(x => x.Data.Fields.AtLeast(2),
                                () =>
                                {
                                    RuleFor(x => x.Data.Fields)
                                        .Must((_, fields) => fields.GroupBy(f => f.Name)
                                                                    .Select(kv => (name: kv.Key, duplicate: kv.AtLeast(2)))
                                                                    .AtMost(item => item.duplicate, 0))
                                        .WithMessage((_, fields) =>
                                        {
                                            IEnumerable<string> duplicates = fields.GroupBy(f => f.Name)
                                                                    .Select(kv => (name: kv.Key, duplicate: kv.AtLeast(2)))
                                                                    .Where(tuple => tuple.duplicate)
                                                                    .Select(tuple => tuple.name);

                                            return $"Multiple fields with same name : {string.Join("," , duplicates.Select(name => $"'{name}'"))}";
                                        });
                                });
                        });
                });
        }
    }
}
