using System;
using MedEasy.DTO;
using Newtonsoft.Json;

namespace MedEasy.Commands.Prescription
{
    /// <summary>
    /// A command to build a new <see cref="PrescriptionInfo"/> resource.
    /// </summary>
    [JsonObject]
    public class CreatePrescriptionCommand : CommandBase<Guid, CreatePrescriptionInfo, PrescriptionInfo>, ICreatePrescriptionCommand
    {
        
        /// <summary>
        /// Builds a new <see cref="CreatePrescriptionCommand"/> instance.
        /// </summary>
        /// <param name="data">data that will be used to create the patient resource</param>
        /// <see cref="CommandBase{TKey, TData}"/>
        /// <see cref="ICreatePrescriptionCommand"/>
        /// <see cref="CreatePrescriptionInfo"/>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c>.</exception>
        public CreatePrescriptionCommand(CreatePrescriptionInfo data) : base(Guid.NewGuid(), data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }


    
}
