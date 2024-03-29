﻿namespace Agenda.Validators
{
    using Agenda.DTO;
    using Agenda.Ids;
    using Agenda.Objects;

    using FluentValidation;

    using MedEasy.DAL.Interfaces;
    using MedEasy.DTO;
    using MedEasy.Validators.Patch;

    using Microsoft.AspNetCore.JsonPatch.Operations;

    using NodaTime;

    using System;
    using System.Linq;

    using static FluentValidation.CascadeMode;
    using static Microsoft.AspNetCore.JsonPatch.Operations.OperationType;

    /// <summary>
    /// Validates <see cref="PatchInfo{Guid, AppointmentInfo}"/> instances.
    /// </summary>
    public class PatchAppointmentInfoValidator : AbstractValidator<PatchInfo<AppointmentId, AppointmentInfo>>
    {
        /// <summary>
        /// Builds a <see cref="PatchAppointmentInfoValidator"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory">Factory for building <see cref="IUnitOfWork"/> instances.</param>
        public PatchAppointmentInfoValidator(IClock datetimeService, IUnitOfWorkFactory unitOfWorkFactory)
        {
            static bool IsFieldOperation(Operation op, string path) => string.Compare(path, op.path, ignoreCase: true) == 0;

            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            if (datetimeService == null)
            {
                throw new ArgumentNullException(nameof(datetimeService));
            }
            CascadeMode = Stop;

            RuleFor(x => x.Id)
                .NotEqual(AppointmentId.Empty);

            RuleFor(x => x.PatchDocument)
                .NotNull();

            When(
                x => x.Id != AppointmentId.Empty,
                () =>
                {
                    RuleFor(x => x.PatchDocument)
                        .SetValidator(new JsonPatchDocumentValidator<AppointmentInfo>() { CascadeMode = CascadeMode })

                        // The patch document should not replace or remove the resource identifier 
                        .Must(patchDocument => !patchDocument.Operations.Any(op => new[] { Replace, Remove }.Contains(op.OperationType)
                                                                                   && string.Compare($"/{nameof(AppointmentInfo.Id)}", op.path, true) == 0))
                            .OverridePropertyName(nameof(PatchInfo<AppointmentId, AppointmentInfo>.PatchDocument))

                        .MustAsync(async (context, patchInfo, cancellationToken) =>
                        {
                            bool valid = false;
                            Operation<AppointmentInfo> replaceStartDate = patchInfo.Operations
                                .SingleOrDefault(op => op.OperationType == Replace
                                 && IsFieldOperation(op, $"/{nameof(AppointmentInfo.StartDate)}"));

                            Operation<AppointmentInfo> replaceEndDate = patchInfo.Operations
                                .SingleOrDefault(op => op.OperationType == Replace
                                                       && IsFieldOperation(op, $"/{nameof(AppointmentInfo.EndDate)}"));

                            if (replaceStartDate != default || replaceEndDate != default)
                            {
                                if (replaceStartDate != default && replaceEndDate != default)
                                {
                                    valid = replaceStartDate.value is Instant newStartDate
                                            && replaceEndDate.value is Instant newEndDate
                                            && newStartDate <= newEndDate;
                                }
                                else
                                {
                                    using IUnitOfWork uow = unitOfWorkFactory.NewUnitOfWork();
                                    if (replaceStartDate?.value is Instant newStartDate)
                                    {
                                        valid = !await uow.Repository<Appointment>()
                                            .AnyAsync(x => x.Id == context.Id && x.EndDate <= newStartDate, cancellationToken)
                                            .ConfigureAwait(false);
                                    }
                                    else if (replaceEndDate?.value is Instant newEndDate)
                                    {
                                        valid = !await uow.Repository<Appointment>()
                                            .AnyAsync(x => x.Id == context.Id && x.StartDate >= newEndDate, cancellationToken)
                                            .ConfigureAwait(false);
                                    }
                                }

                            }

                            return valid;
                        })
                        .WithMessage($"{nameof(AppointmentInfo.StartDate)} cannot be greater than {nameof(AppointmentInfo.EndDate)}");

                });




        }
    }
}
