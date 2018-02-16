using AutoMapper.QueryableExtensions;
using Measures.CQRS.Commands;
using Measures.DTO;
using Measures.Objects;
using MedEasy.DAL.Interfaces;
using MediatR;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers
{
    /// <summary>
    /// Handles <see cref="CreateBloodPressureInfoCommand"/>s
    /// </summary>
    public class HandleCreateBloodPressureInfoCommand : IRequestHandler<CreateBloodPressureInfoCommand, BloodPressureInfo>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleCreateBloodPressureInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleCreateBloodPressureInfoCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }


        public async Task<BloodPressureInfo> Handle(CreateBloodPressureInfoCommand cmd, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.New())
            {
                Expression<Func<CreateBloodPressureInfo, BloodPressure>> mapBloodPressureInfoToEntity = _expressionBuilder.GetMapExpression<CreateBloodPressureInfo, BloodPressure>();
                Expression<Func<PatientInfo, Patient>> mapPatientInfoToEntity = _expressionBuilder.GetMapExpression<PatientInfo, Patient>();
                BloodPressure newEntity = mapBloodPressureInfoToEntity.Compile().Invoke(cmd.Data);

                newEntity.Patient.UUID = newEntity.Patient.UUID == default
                    ? Guid.NewGuid()
                    : newEntity.Patient.UUID;

                uow.Repository<BloodPressure>().Create(newEntity);
                await uow.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);

                Expression<Func<BloodPressure, BloodPressureInfo>> mapEntityToBloodPressureInfo = _expressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                BloodPressureInfo createdResource = mapEntityToBloodPressureInfo.Compile().Invoke(newEntity);

                return createdResource;

            }
        }
    }
}
