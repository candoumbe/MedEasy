using FluentValidation;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using System;
using System.Threading;
using System.Threading.Tasks;
using static FluentValidation.CascadeMode;
using static FluentValidation.Severity;

namespace MedEasy.Validators.Appointment
{
    /// <summary>
    /// Validates <see cref="CreateAppointmentInfo"/> instances.
    /// </summary>
    public class CreateAppointmentInfoValidator : AbstractValidator<CreateAppointmentInfo>
    {

        /// <summary>
        /// Builds a new <see cref="CreateAppointmentInfoValidator"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory to build <see cref="IUnitOfWork"/> instances.</param>
        public CreateAppointmentInfoValidator(IUnitOfWorkFactory uowFactory)
        {

            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }

            CascadeMode = StopOnFirstFailure;

            async Task<bool> IsDoctorIdKnown(Guid doctorId, CancellationToken cancellationToken)
            {
                using (IUnitOfWork uow = uowFactory.New())
                {
                    return await uow.Repository<Objects.Doctor>()
                        .AnyAsync(x => x.UUID == doctorId, cancellationToken)
                        .ConfigureAwait(false);
                };
            }

            async Task<bool> IsPatientIdKnown(Guid patientId, CancellationToken cancellationToken)
            {
                using (IUnitOfWork uow = uowFactory.New())
                {
                    return await uow.Repository<Objects.Patient>()
                        .AnyAsync(x => x.UUID == patientId, cancellationToken)
                        .ConfigureAwait(false);
                };
            }


            RuleFor(x => x.DoctorId)
                .NotEmpty()
                .MustAsync(async (doctorId, cancellationToken) =>
                {
                    using (IUnitOfWork uow = uowFactory.New())
                    {
                        return await IsDoctorIdKnown(doctorId, cancellationToken)
                            .ConfigureAwait(false);
                    }
                })
                .WithSeverity(Error);

            RuleFor(x => x.PatientId)
                .NotEmpty()
                .MustAsync(async (patientId, cancellationToken) =>
                {
                    using (IUnitOfWork uow = uowFactory.New())
                    {
                        return await IsPatientIdKnown(patientId, cancellationToken)
                            .ConfigureAwait(false);
                    }
                })
                .WithSeverity(Error)
                .WithMessage((info) => $"{nameof(Objects.Patient)} <{info.PatientId}> not found.");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithSeverity(Error);

            RuleFor(x => x.Duration)
                .GreaterThan(0)
                    .WithSeverity(Error)
                    .WithErrorCode($"Err{nameof(Objects.Appointment)}Bad{nameof(CreateAppointmentInfo.Duration)}");



            WhenAsync(async (info) =>
            {

                bool canApplyRule = info.Duration > 0 && info.StartDate != default
                    && info.DoctorId != Guid.Empty && info.PatientId != Guid.Empty;
                bool rulePreconditionOk = false;

                if (canApplyRule)
                {
                    Task<bool> doctorKnown = IsDoctorIdKnown(info.DoctorId, CancellationToken.None);
                    Task<bool> patientKnown = IsPatientIdKnown(info.PatientId, CancellationToken.None);

                    await Task.WhenAll(doctorKnown, patientKnown);

                    rulePreconditionOk = (await doctorKnown) && (await patientKnown);
                }


                return canApplyRule && rulePreconditionOk;
            },
            () =>
            {
                RuleFor(x => x.Duration)
                    .MustAsync(async (info, duration, cancellationToken) =>
                    {
                        DateTimeOffset startDate = info.StartDate;
                        DateTimeOffset endDate = startDate.AddMinutes(duration);
                        using (IUnitOfWork uow = uowFactory.New())
                        {
                            
                            return !(await uow.Repository<Objects.Appointment>()
                                .AnyAsync(x => x.Doctor.UUID == info.DoctorId
                                    && ( (startDate <= x.StartDate && x.StartDate < endDate) || (startDate < x.EndDate && x.EndDate <= endDate)), cancellationToken));
                            

                        }
                    })
                    .WithSeverity(Warning)
                    .WithMessage((info) => $"{nameof(Objects.Appointment)} overlaps one or more appointments");
            });


        }
    }
}
