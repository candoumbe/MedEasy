using System;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using MedEasy.Objects;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using static MedEasy.DAL.Repositories.SortDirection;
using System.Linq;
using MedEasy.Queries.Patient;
using MedEasy.Handlers.Core.Patient.Queries;
using System.Threading;
using Optional;

namespace MedEasy.Handlers.Patient.Queries
{
    public class HandleGetDocumentsByPatientIdQuery : IHandleGetDocumentsByPatientIdQuery
    {
        private IExpressionBuilder _expressionBuilder;
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<HandleGetDocumentsByPatientIdQuery> _logger;


        /// <summary>
        /// Builds a new <see cref="HandleGetDocumentsByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetDocumentsByPatientIdQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleGetDocumentsByPatientIdQuery> logger, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        public async ValueTask<Option<IPagedResult<DocumentMetadataInfo>>> HandleAsync(IWantPageOfDocumentsByPatientIdQuery query, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Start looking for documents metadata : {query}");
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (IUnitOfWork uow = _uowFactory.New())
            {
                _logger.LogTrace($"Start querying {query}");
                GetDocumentsByPatientIdInfo input = query.Data;
                Expression<Func<DocumentMetadata, DocumentMetadataInfo>> selector = _expressionBuilder.GetMapExpression<DocumentMetadata, DocumentMetadataInfo>();

                Option<IPagedResult<DocumentMetadataInfo>> option;
                if (await uow.Repository<Objects.Patient>().AnyAsync(x => x.UUID == input.PatientId).ConfigureAwait(false))
                {
                    IPagedResult<DocumentMetadataInfo> results = await uow.Repository<DocumentMetadata>()
                        .WhereAsync(
                            selector,
                            (DocumentMetadataInfo x) => x.PatientId == input.PatientId,
                            new[] { OrderClause<DocumentMetadataInfo>.Create(x => x.UpdatedDate, Descending) },
                            input.PageConfiguration.PageSize, input.PageConfiguration.Page,
                            cancellationToken);

                    _logger.LogTrace($"Nb of results : {results.Entries.Count()}");

                    option = results.Some();
                }
                else
                {
                    option = Option.None<IPagedResult<DocumentMetadataInfo>>();
                }

                _logger.LogInformation($"Query <{query.Id}> handled successfully");
                return option;
            }
        }
    }
}
