namespace Agenda.CQRS.Features.Appointments.Handlers
{
    using Agenda.CQRS.Features.Appointments.Commands;
    using Agenda.Objects;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.Interfaces;

    using MediatR;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handle <see cref="DeleteAppointmentInfoByIdCommand"/> commands.
    /// </summary>
    public class HandleDeleteAppointmentInfoByIdCommand : IRequestHandler<DeleteAppointmentInfoByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Builds a new <see cref="HandleDeleteAppointmentInfoByIdCommand"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HandleDeleteAppointmentInfoByIdCommand(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        ///<inheritdoc/>
        public async Task<DeleteCommandResult> Handle(DeleteAppointmentInfoByIdCommand request, CancellationToken ct)
        {
            using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();
            uow.Repository<Appointment>().Delete(x => x.Id == request.Data);
            await uow.SaveChangesAsync()
                .ConfigureAwait(false);

            return DeleteCommandResult.Done;
        }
    }
}
