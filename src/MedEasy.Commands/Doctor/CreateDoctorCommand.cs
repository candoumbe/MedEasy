using System;
using MedEasy.DTO;

namespace MedEasy.Commands.Doctor
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class CreateDoctorCommand : CommandBase<Guid, CreateDoctorInfo>, ICreateDoctorCommand
    {
        
        /// <summary>
        /// Builds a new <see cref="CreateDoctorCommand"/> instance with default validation
        /// </summary>
        /// <param name="data">data to process</param>
        public CreateDoctorCommand(CreateDoctorInfo data) : base(Guid.NewGuid(), data)
        { 
        }
        
    }


    
}
