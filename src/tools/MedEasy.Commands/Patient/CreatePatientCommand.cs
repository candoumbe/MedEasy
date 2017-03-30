using System;
using MedEasy.DTO;
using Newtonsoft.Json;

namespace MedEasy.Commands.Patient
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    [JsonObject]
    public class CreatePatientCommand : CommandBase<Guid, CreatePatientInfo>, ICreatePatientCommand
    {
        
        /// <summary>
        /// Builds a new <see cref="CreatePatientCommand"/> instance.
        /// </summary>
        /// <param name="data">data that will be used to create the patient resource</param>
        /// <see cref="CommandBase{TKey, TData}"/>
        /// <see cref="ICreatePatientCommand"/>
        /// <see cref="CreatePatientInfo"/>
        public CreatePatientCommand(CreatePatientInfo data) : base(Guid.NewGuid(), data)
        {
            
        }
    }


    
}
