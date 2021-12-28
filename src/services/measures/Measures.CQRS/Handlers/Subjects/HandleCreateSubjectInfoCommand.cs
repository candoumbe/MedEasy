namespace Measures.CQRS.Handlers.Subjects
{
    using AutoMapper.QueryableExtensions;

    using Measures.CQRS.Commands.Patients;
    using Measures.CQRS.Events;
    using Measures.DTO;
    using Measures.Ids;
    using Measures.Objects;

    using MedEasy.DAL.Interfaces;

    using MediatR;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="CreateSubjectInfoCommand"/>s
    /// </summary>
    public class HandleCreateSubjectInfoCommand : IRequestHandler<CreateSubjectInfoCommand, SubjectInfo>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;

        /// <summary>
        /// Builds a new <see cref="HandleCreateSubjectInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator</param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"/> or 
        /// <paramref name="mediator"/> is <c>null</c>.
        /// </exception>
        public HandleCreateSubjectInfoCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        ///<inheritdoc/>
        public async Task<SubjectInfo> Handle(CreateSubjectInfoCommand cmd, CancellationToken ct)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            NewSubjectInfo data = cmd.Data;
            data.Id = data.Id is null || data.Id == SubjectId.Empty
                ? SubjectId.New()
                : data.Id;
            Subject entity = new Subject(data.Id , data.Name).WasBornOn(data.BirthDate);

            uow.Repository<Subject>().Create(entity);
            await uow.SaveChangesAsync(ct).ConfigureAwait(false);

            Expression<Func<Subject, SubjectInfo>> mapEntityToDtoExpression = _expressionBuilder.GetMapExpression<Subject, SubjectInfo>();

            SubjectInfo patientInfo = mapEntityToDtoExpression.Compile().Invoke(entity);

            await _mediator.Publish(new PatientCreated(patientInfo), ct).ConfigureAwait(false);

            return patientInfo;
        }
    }
}
