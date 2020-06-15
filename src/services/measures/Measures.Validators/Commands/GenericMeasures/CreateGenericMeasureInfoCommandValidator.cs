using FluentValidation;

using Forms;

using Measures.CQRS.Commands;
using Measures.DTO;
using Measures.Objects;

using MedEasy.DAL.Interfaces;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Measures.Validators.Commands.GenericMeasures
{
    public class CreateGenericMeasureInfoCommandValidator : AbstractValidator<CreateGenericMeasureInfoCommand>
    {
        public CreateGenericMeasureInfoCommandValidator(IUnitOfWorkFactory unitOfWorkFactory)
        {
            RuleFor(x => x.Data.FormId)
                .NotEmpty()
                .WithMessage($"{nameof(CreateGenericMeasureInfoCommand.Data)}.{nameof(CreateGenericMeasureInfo.FormId)} cannot be empty");
            RuleFor(x => x.Data.PatientId)
                .NotEmpty()
                .WithMessage($"{nameof(CreateGenericMeasureInfoCommand.Data)}.{nameof(CreateGenericMeasureInfo.PatientId)} cannot be empty");

            When(x => x.Data.Values?.AtLeastOnce() ?? false,
                 () =>
                 {
                     When(x => x.Data.FormId != Guid.Empty  && x.Data.PatientId != Guid.Empty,
                         () =>
                         {
                             RuleFor(x => x.Data.Values)
                                .MustAsync(async (cmd, values, cancellation) =>
                                     {
                                         using IUnitOfWork uow = unitOfWorkFactory.NewUnitOfWork();
                                         MeasureForm form = await uow.Repository<MeasureForm>()
                                                                     .SingleAsync(form => form.Id == cmd.Data.FormId, cancellation)
                                                                     .ConfigureAwait(false);

                                         IEnumerable<string> requiredFields = form.Fields.Where(f => f.Required ?? false)
                                                                                            .Select(f => f.Name);

                                         return !requiredFields.Except(values.Select(f => f.Key)).Any();
                                     })
                                .WithMessage(cmd => $"Missing one or more required values for form <{cmd.Data.FormId}>");

                             RuleFor(x => x.Data.Values)
                                .MustAsync(async (cmd, values, cancellation) =>
                                {
                                    using IUnitOfWork uow = unitOfWorkFactory.NewUnitOfWork();
                                    MeasureForm form = await uow.Repository<MeasureForm>()
                                                                .SingleAsync(form => form.Id == cmd.Data.FormId, cancellation)
                                                                .ConfigureAwait(false);

                                    IEnumerable<string> requiredFields = form.Fields.Where(f => !f.Required.HasValue || !f.Required.Value)
                                                                                    .Select(f => f.Name);

                                    return !requiredFields.Except(values.Select(f => f.Key)).Any();
                                })
                                .WithSeverity(Severity.Warning)
                                .WithMessage(cmd => $"Missing one or more non mandatory values for form <{cmd.Data.FormId}>");
                         });
                 });
        }
    }
}
