using System;

namespace MedEasy.Commands.Patient
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class DeletePatientByIdCommand : IDeletePatientByIdCommand
    {
        public Guid Id => Guid.NewGuid();

        public Guid Data { get; }

        public DeletePatientByIdCommand(Guid id)
        {
            if (Guid.Empty == id)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            Data = id;
        }
    }



}
