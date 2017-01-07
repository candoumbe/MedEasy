using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Queries;
using Microsoft.Extensions.Logging;
using MedEasy.Objects;
using AutoMapper.QueryableExtensions;
using System.Linq.Expressions;
using MedEasy.Handlers.Exceptions;
using MedEasy.DAL.Repositories;

namespace MedEasy.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly ILogger<PrescriptionService> _logger;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="PrescriptionService"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory that can build new <see cref="IUnitOfWork"/> instances</param>
        /// <param name="logger">logger</param>
        public PrescriptionService(IUnitOfWorkFactory uowFactory, ILogger<PrescriptionService> logger, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _logger = logger;
            _expressionBuilder = expressionBuilder;
        }

        public async Task<PrescriptionHeaderInfo> CreatePrescriptionForPatientAsync(int patientId, CreatePrescriptionInfo newPrescription)
        {
            if (patientId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patientId), $"{nameof(patientId)} cannot be negative or zero");
            }

            if(newPrescription == null)
            {
                throw new ArgumentNullException(nameof(newPrescription), $"{nameof(newPrescription)} cannot be null");
            }

            using (var uow = _uowFactory.New())
            {
                Expression<Func<CreatePrescriptionInfo, Prescription>> convertCreatePrescriptionToPrescription = _expressionBuilder.CreateMapExpression<CreatePrescriptionInfo, Prescription>();
                Prescription prescription = convertCreatePrescriptionToPrescription.Compile()(newPrescription);
                prescription.PatientId = patientId;
                prescription = uow.Repository<Prescription>().Create(prescription);
                await uow.SaveChangesAsync();


                Expression<Func<Prescription, PrescriptionHeaderInfo>> convertPrescriptionToHeader = _expressionBuilder.CreateMapExpression<Prescription, PrescriptionHeaderInfo>();
                return convertPrescriptionToHeader.Compile().Invoke(prescription);
            }
        }

        public Task<IEnumerable<PrescriptionHeaderInfo>> GetMostRecentPrescriptionsAsync(IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>> query)
        {
            throw new NotImplementedException();
        }

        public async Task<PrescriptionHeaderInfo> GetOnePrescriptionAsync(int id)
        {

            using (var uow = _uowFactory.New())
            {
                Expression<Func<Prescription, PrescriptionHeaderInfo>> selector = _expressionBuilder.CreateMapExpression<Prescription, PrescriptionHeaderInfo>();
                PrescriptionHeaderInfo result = await uow.Repository<Prescription>()
                    .SingleOrDefaultAsync(selector, x => x.Id == id);

                return result;
            }
        }


        /// <summary>
        /// Gets one <see cref="PrescriptionHeaderInfo"/> with the specified <paramref name="patientId"/> 
        /// and <paramref name="prescriptionId"/>.
        /// </summary>
        /// <param name="patientId">id of the patient for which the prescription should be retrieved</param>
        /// <param name="prescriptionId">id of the prescription to get</param>
        /// <returns>a <see cref="PrescriptionHeaderInfo"/> or <c>null</c> if no <see cref="Prescription"/> found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     if either <paramref name="patientId"/> or <paramref name="prescriptionId"/> is negative or null
        /// </exception>
        public async Task<PrescriptionHeaderInfo> GetOnePrescriptionByPatientIdAsync(int patientId, int prescriptionId)
        {
            if (patientId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patientId));
            }

            if (prescriptionId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(prescriptionId));
            }

            using (var uow = _uowFactory.New())
            {
                Expression<Func<Prescription, PrescriptionHeaderInfo>> selector = _expressionBuilder.CreateMapExpression<Prescription, PrescriptionHeaderInfo>();
                return await uow.Repository<Prescription>().SingleOrDefaultAsync(selector, x => x.Id == prescriptionId && x.PatientId == patientId);
            }
        }

        public async Task<IEnumerable<PrescriptionItemInfo>> GetItemsByPrescriptionIdAsync(int id)
        {
            using (var uow = _uowFactory.New())
            {
                Prescription prescription = await uow.Repository<Prescription>()
                    .SingleOrDefaultAsync(x => x.Id == id, new[] { IncludeClause<Prescription>.Create(x => x.Items) });

                if (prescription == null)
                {
                    throw new NotFoundException($"Prescription <{id}> not found");
                }

                var converterExpression =_expressionBuilder.CreateMapExpression<IEnumerable<PrescriptionItem>, IEnumerable<PrescriptionItemInfo>>();
                IEnumerable<PrescriptionItemInfo> results = converterExpression.Compile()(prescription.Items);


                return results;
            }
        }
    }
}
