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
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.DAL.Repositories;
using AutoMapper;

namespace MedEasy.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private IUnitOfWorkFactory UowFactory { get; }
        private readonly ILogger<PrescriptionService> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Builds a new <see cref="PrescriptionService"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory that can build new <see cref="IUnitOfWork"/> instances</param>
        /// <param name="logger">logger</param>
        public PrescriptionService(IUnitOfWorkFactory uowFactory, ILogger<PrescriptionService> logger, IMapper mapper)
        {
            UowFactory = uowFactory;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PrescriptionHeaderInfo> CreatePrescriptionForPatientAsync(Guid patientId, CreatePrescriptionInfo newPrescription)
        {
            if (patientId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(patientId), $"{nameof(patientId)} cannot be empty");
            }

            if (newPrescription == null)
            {
                throw new ArgumentNullException(nameof(newPrescription), $"{nameof(newPrescription)} cannot be null");
            } 

            using (IUnitOfWork uow = UowFactory.New())
            {
                var patient = await uow.Repository<Patient>().SingleOrDefaultAsync(x => new { x.Id }, x => x.UUID == patientId);
                if (patient == null)
                {
                    throw new NotFoundException($"Patient <{patientId}> not found");
                }

                var prescriptor = await uow.Repository<Doctor>().SingleOrDefaultAsync(x => new { x.Id }, x => x.UUID == newPrescription.PrescriptorId);
                if (prescriptor == null)
                {
                    throw new NotFoundException($"Prescriptor <{newPrescription.PrescriptorId}> not found");
                }

                Prescription prescription = _mapper.Map<CreatePrescriptionInfo, Prescription>(newPrescription);
                prescription.PatientId = patient.Id;
                prescription.PrescriptorId = prescriptor.Id;
                prescription = uow.Repository<Prescription>().Create(prescription);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                PrescriptionHeaderInfo output = _mapper.Map<Prescription, PrescriptionHeaderInfo>(prescription);
                output.PatientId = patientId;

                return output;
            }
        }

        public Task<IEnumerable<PrescriptionHeaderInfo>> GetMostRecentPrescriptionsAsync(IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>> query)
        {
            throw new NotImplementedException();
        }

        public async Task<PrescriptionHeaderInfo> GetOnePrescriptionAsync(Guid id)
        {

            using (IUnitOfWork uow = UowFactory.New())
            {
                Prescription p = await uow.Repository<Prescription>().SingleOrDefaultAsync(
                    x => x.UUID == id, 
                    new[] {
                        IncludeClause<Prescription>.Create(x => x.Patient),
                        IncludeClause<Prescription>.Create(x => x.Prescriptor) });
                return _mapper.Map<PrescriptionHeaderInfo>(p);
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
        ///     if either <paramref name="patientId"/> or <paramref name="prescriptionId"/> is <see cref="Guid.Empty"/>
        /// </exception>
        public async Task<PrescriptionHeaderInfo> GetOnePrescriptionByPatientIdAsync(Guid patientId, Guid prescriptionId)
        {
            if (patientId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(patientId));
            }

            if (prescriptionId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(prescriptionId));
            }

            using (IUnitOfWork uow = UowFactory.New())
            {
                Prescription p = await uow.Repository<Prescription>().SingleOrDefaultAsync(
                    x => x.UUID == prescriptionId,
                    new[] {
                        IncludeClause<Prescription>.Create(x => x.Patient),
                        IncludeClause<Prescription>.Create(x => x.Prescriptor) });

                PrescriptionHeaderInfo header = _mapper.Map<PrescriptionHeaderInfo>(p);

                return header?.PatientId == patientId 
                    ? header
                    : null;
            }
        }

        public async Task<IEnumerable<PrescriptionItemInfo>> GetItemsByPrescriptionIdAsync(Guid id)
        {
            using (var uow = UowFactory.New())
            {
                Prescription prescription = await uow.Repository<Prescription>()
                    .SingleOrDefaultAsync(x => x.UUID == id, new[] { IncludeClause<Prescription>.Create(x => x.Items) });

                if (prescription == null)
                {
                    throw new NotFoundException($"Prescription <{id}> not found");
                }

                IEnumerable<PrescriptionItemInfo> results = _mapper.Map<IEnumerable<PrescriptionItem>, IEnumerable<PrescriptionItemInfo>>(prescription.Items);
                return results;
            }
        }
    }
}
