using MedEasy.DTO.Autocomplete;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.Queries.Autocomplete;
using MedEasy.Handlers.Core.Autocomplete;
using System.Threading;

namespace MedEasy.Handlers.Doctor.Queries
{
    /// <summary>
    /// An instance of this class can handle autocomplete query for doctor name.
    /// </summary>
    public class HandleAutocompleteDoctorNameQuery : IHandleAutocompleteDoctorNameQuery
    {
        private readonly IMapper _mapper;

        private IUnitOfWorkFactory Factory { get; }
        private ILogger Logger { get; }

        public HandleAutocompleteDoctorNameQuery(IUnitOfWorkFactory uowFactory, ILoggerFactory loggerFactory, IMapper mapper)
        {
            Factory = uowFactory;
            Logger = loggerFactory.CreateLogger<HandleAutocompleteDoctorNameQuery>();
            _mapper = mapper;
        }


        public async ValueTask<IEnumerable<DoctorAutocompleteInfo>> HandleAsync(IAutocompleteDoctorNameQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (IUnitOfWork uow = Factory.New())
            {
                IEnumerable<DoctorAutocompleteInfo> results = (await uow.Repository<Objects.Doctor>()
                    .WhereAsync(
                        _mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<Objects.Doctor, DoctorAutocompleteInfo>(),
                        item => (item.Firstname != null && item.Firstname.Contains(query.Data)) || (item.Lastname != null && item.Lastname.Contains(query.Data)),
                        orderBy: new[]
                        {
                            OrderClause<DoctorAutocompleteInfo>.Create(item => item.Firstname),
                            OrderClause<DoctorAutocompleteInfo>.Create(item => item.Lastname),
                        },
                        cancellationToken : cancellationToken
                        ));

                return results;
            }
        }
    }
}
