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

namespace MedEasy.Handlers.Patient.Queries
{
    public class HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery : IHandleGetOneDocumentInfoByPatientIdAndDocumentId
    {
        private IExpressionBuilder _expressionBuilder;
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery> _logger;


        /// <summary>
        /// Builds a new <see cref="HandleGetDocumentsByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery> logger, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        public async Task<DocumentMetadataInfo> HandleAsync(IWantOneDocumentByPatientIdAndDocumentIdQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation($"Start looking for documents metadata : {query}");
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (IUnitOfWork uow = _uowFactory.New())
            {
                _logger.LogTrace($"Start querying {query}");
                GetOneDocumentInfoByPatientIdAndDocumentIdInfo input = query.Data;
                Expression<Func<DocumentMetadata, DocumentMetadataInfo>> selector = _expressionBuilder.CreateMapExpression<DocumentMetadata, DocumentMetadataInfo>();
                DocumentMetadataInfo result = await uow.Repository<DocumentMetadata>()
                    .SingleOrDefaultAsync(
                        selector,
                        x => x.Patient.UUID == input.PatientId && x.UUID == input.DocumentMetadataId);

                _logger.LogTrace($"Document <{input.DocumentMetadataId}> for patient <{input.PatientId}>{(result == null ? " not" : string.Empty)} found");
                _logger.LogInformation($"Handling query {query.Id} successfully");
                return result;
            }
        }
    }
}
