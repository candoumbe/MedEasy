namespace Measures.CQRS.Handlers.Subjects
{
    using AutoMapper.QueryableExtensions;

    using Measures.CQRS.Queries.Subjects;
    using Measures.DTO;
    using Measures.Objects;

    using MedEasy.DAL.Interfaces;

    using MediatR;

    using Optional;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetSubjectInfoByIdQuery"/>s
    /// </summary>
    public class HandleGetOneSubjectInfoByIdQuery : IRequestHandler<GetSubjectInfoByIdQuery, Option<SubjectInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleCreateSubjectInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/> or <paramref name="expressionBuilder"/> is <c>null</c></exception>
        public HandleGetOneSubjectInfoByIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }


        public async Task<Option<SubjectInfo>> Handle(GetSubjectInfoByIdQuery query, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            Expression<Func<Subject, SubjectInfo>> selector = _expressionBuilder.GetMapExpression<Subject, SubjectInfo>();
            Option<SubjectInfo> result = await uow.Repository<Subject>()
                .SingleOrDefaultAsync(
                    selector,
                    (Subject x) => x.Id == query.Data,
                    cancellationToken)
                .ConfigureAwait(false);


            return result;
        }
    }
}
