using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Appointments.Handlers
{
    public class HandleDeleteAppointmentInfoByIdCommand : IRequestHandler<DeleteAppointmentInfoByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public HandleDeleteAppointmentInfoByIdCommand(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<DeleteCommandResult> Handle(DeleteAppointmentInfoByIdCommand request, CancellationToken ct)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Delete(x => x.UUID == request.Data);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);


                return DeleteCommandResult.Done;
            }
        }
    }
}
