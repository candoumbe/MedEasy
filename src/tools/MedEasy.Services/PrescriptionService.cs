using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedEasy.DTO;
using MedEasy.Objects;
using MedEasy.Queries;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.DAL.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace MedEasy.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private IUnitOfWorkFactory UowFactory { get; }
        private ILogger<PrescriptionService> Logger { get; }
        private IMapper Mapper { get; }

        /// <summary>
        /// Builds a new <see cref="PrescriptionService"/> instance.
        /// </summary>
        /// <param name="uowFactory">Factory that can build new <see cref="IUnitOfWork"/> instances</param>
        /// <param name="logger">logger</param>
        public PrescriptionService(IUnitOfWorkFactory uowFactory, ILogger<PrescriptionService> logger, IMapper mapper)
        {
            UowFactory = uowFactory;
            Logger = logger;
            Mapper = mapper;
        }

        public async Task<PrescriptionHeaderInfo> CreatePrescriptionForPatientAsync(Guid patientId, CreatePrescriptionInfo newPrescription, CancellationToken cancellationToken = default(CancellationToken))
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

                Prescription prescription = Mapper.Map<CreatePrescriptionInfo, Prescription>(newPrescription);
                prescription.PatientId = patient.Id;
                prescription.PrescriptorId = prescriptor.Id;
                prescription = uow.Repository<Prescription>().Create(prescription);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                PrescriptionHeaderInfo output = Mapper.Map<Prescription, PrescriptionHeaderInfo>(prescription);
                output.PatientId = patientId;

                return output;
            }
        }

        public Task<IEnumerable<PrescriptionHeaderInfo>> GetMostRecentPrescriptionsAsync(IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>> query, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public async Task<PrescriptionHeaderInfo> GetOnePrescriptionAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {

            using (IUnitOfWork uow = UowFactory.New())
            {
                Prescription p = await uow.Repository<Prescription>()
                    .SingleOrDefaultAsync(
                        x => x.UUID == id, 
                        new[] {
                            IncludeClause<Prescription>.Create(x => x.Patient),
                            IncludeClause<Prescription>.Create(x => x.Prescriptor) },
                        cancellationToken);
                return Mapper.Map<PrescriptionHeaderInfo>(p);
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
        public async Task<PrescriptionHeaderInfo> GetOnePrescriptionByPatientIdAsync(Guid patientId, Guid prescriptionId, CancellationToken cancellationToken = default(CancellationToken))
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
                        IncludeClause<Prescription>.Create(x => x.Prescriptor) },
                    cancellationToken);

                PrescriptionHeaderInfo header = Mapper.Map<PrescriptionHeaderInfo>(p);

                return header?.PatientId == patientId 
                    ? header
                    : null;
            }
        }

        public async Task<IEnumerable<PrescriptionItemInfo>> GetItemsByPrescriptionIdAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (IUnitOfWork uow = UowFactory.New())
            {
                Prescription prescription = await uow.Repository<Prescription>()
                    .SingleOrDefaultAsync(
                        x => x.UUID == id, new[] { IncludeClause<Prescription>.Create(x => x.Items) }, cancellationToken);

                IEnumerable<PrescriptionItemInfo> results = null;
                if (prescription != null)
                {
                    results = Mapper.Map<IEnumerable<PrescriptionItem>, IEnumerable<PrescriptionItemInfo>>(prescription.Items);
                }

                return results;
            }
        }
    }
}
