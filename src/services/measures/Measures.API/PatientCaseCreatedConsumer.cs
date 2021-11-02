namespace Measures.API
{

    using MassTransit;

    using Measures.CQRS.Commands.Patients;
    using Measures.Ids;

    using MediatR;

    using Microsoft.Extensions.Logging;

    using Patients.Events;

    using System.Threading.Tasks;

    internal class PatientCaseCreatedConsumer : IConsumer<PatientCaseCreated>
    {
        private readonly ILogger<PatientCaseCreated> _logger;
        private readonly IMediator _mediator;

        public PatientCaseCreatedConsumer(ILogger<PatientCaseCreated> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        ///<inheritdoc/>
        public async Task Consume(ConsumeContext<PatientCaseCreated> context)
        {
            _logger.LogInformation("Received {EventName} : {@Event}", nameof(PatientCaseCreated), context.Message);

            CreatePatientInfoCommand cmd = new(new DTO.NewPatientInfo { Id = new PatientId(context.Message.Id.Value), Name = context.Message.Name, BirthDate = context.Message.BirthDate });
            await _mediator.Send(cmd, context.CancellationToken)
                            .ConfigureAwait(false);
        }
    }
}
