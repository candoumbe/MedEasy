using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedEasy.Objects;
using MedEasy.DTO;
using MedEasy.Queries;

namespace MedEasy.Services
{
    /// <summary>
    /// Defines methods for interacting with <see cref="Prescription"/>
    /// </summary>
    public interface IPrescriptionService
    {
        /// <summary>
        /// Gets one <see cref="PrescriptionHeaderInfo"/>
        /// </summary>
        /// <param name="patientId">Id of the patient to get the prescription from</param>
        /// <param name="prescriptionId">Id of the prescription to retrieve</param>
        /// <returns>
        /// The <see cref="PrescriptionHeaderInfo"/> or <c>null</c> if no <see cref="PatientInfo"/> with 
        /// <paramref name="patientId"/> found or no <see cref="PrescriptionHeaderInfo"/> with <paramref name="prescriptionId"/>
        /// found
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="patientId"/>is negative of zero</exception>
        Task<PrescriptionHeaderInfo> GetOnePrescriptionByPatientIdAsync(Guid patientId, Guid prescriptionId);

        /// <summary>
        /// Creates a new prescription for the patient with the specified <paramref name="patientId"/>
        /// </summary>
        /// <remarks>
        /// It checks that a patient with the specified <paramref name="patientId"/> exists prior to creating 
        /// the <see cref="PrescriptionHeaderInfo"/>.
        /// </remarks>
        /// <param name="patientId">id of the patient the new prescription will be created on</param>
        /// <param name="createPrescriptionInfo">data of the new prescription</param>
        /// <returns>The created prescription's header</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="createPrescriptionInfo"/> is <c>null</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="patientId"/>is negative of zero</exception>
        Task<PrescriptionHeaderInfo> CreatePrescriptionForPatientAsync(Guid patientId, CreatePrescriptionInfo createPrescriptionInfo);

        /// <summary>
        /// Gets the most recent prescriptions
        /// </summary>
        /// <param name="query">query to get the most recent <see cref="PrescriptionInfo"/>s.</param>
        /// <returns></returns>
        Task<IEnumerable<PrescriptionHeaderInfo>> GetMostRecentPrescriptionsAsync(IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>> query);

        /// <summary>
        /// Gets the <see cref="PrescriptionHeaderInfo"/> with the specified <paramref name="id"/>
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of the prescription to get header's from</param>
        /// <returns></returns>
        Task<PrescriptionHeaderInfo> GetOnePrescriptionAsync(Guid id);

        /// <summary>
        /// Gets the <see cref="PrescriptionInfo"/>.
        /// </summary><
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of the resource</param>
        /// <returns></returns>
        Task<IEnumerable<PrescriptionItemInfo>> GetItemsByPrescriptionIdAsync(Guid id);
    }
}
