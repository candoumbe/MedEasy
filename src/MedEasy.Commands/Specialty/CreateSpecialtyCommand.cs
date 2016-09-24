using System;
using MedEasy.DTO;
using Newtonsoft.Json;

namespace MedEasy.Commands.Specialty
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    [JsonObject]
    public class CreateSpecialtyCommand : CommandBase<Guid, CreateSpecialtyInfo>, ICreateSpecialtyCommand
    {
        
        /// <summary>
        /// Builds a new <see cref="CreateSpecialtyCommand"/> instance.
        /// </summary>
        /// <param name="data">data that will be used to create</param>
        /// <see cref="CommandBase{TKey, TData}"/>
        /// <see cref="ICreateSpecialtyCommand"/>
        public CreateSpecialtyCommand(CreateSpecialtyInfo data) : base(Guid.NewGuid(), data)
        {
            
        }
    }


    
}
