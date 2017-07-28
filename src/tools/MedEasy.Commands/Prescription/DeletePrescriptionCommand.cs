using System;

namespace MedEasy.Commands.Prescription
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class DeletePrescriptionByIdCommand : IDeletePrescriptionByIdCommand
    {
        public Guid Id => Guid.NewGuid();

        public Guid Data { get; }

        /// <summary>
        /// Builds a new <see cref="DeletePrescriptionByIdCommand"/>
        /// </summary>
        /// <param name="id">id of the prescription to delete</param>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="id"/> <see cref="Guid.Empty"/></exception>
        public DeletePrescriptionByIdCommand(Guid id)
        {
            if (Guid.Empty == id)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            Data = id;
        }
    }



}
