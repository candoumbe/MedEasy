using System;
using MedEasy.DTO;
using Newtonsoft.Json;

namespace MedEasy.Commands.Prescription
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
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
        public CreatePrescriptionCommand(CreatePrescriptionInfo data) : base(Guid.NewGuid(), data)
        {
            
        }
    }


    
}
