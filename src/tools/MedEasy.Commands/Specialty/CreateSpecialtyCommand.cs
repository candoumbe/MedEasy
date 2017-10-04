using System;
using MedEasy.DTO;
using Newtonsoft.Json;

namespace MedEasy.Commands.Specialty
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    [JsonObject]
    public class CreateSpecialtyCommand : CommandBase<Guid, CreateSpecialtyInfo, SpecialtyInfo>, ICreateSpecialtyCommand
    {
        
        /// <summary>
        /// Builds a new <see cref="CreateSpecialtyCommand"/> instance.
        /// </summary>
        /// <param name="data">data that will be used to create</param>
        /// <see cref="CommandBase{TKey, TData}"/>
        /// <see cref="ICreateSpecialtyCommand"/>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c>.</exception>
        public CreateSpecialtyCommand(CreateSpecialtyInfo data) : base(Guid.NewGuid(), data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }


    
}
