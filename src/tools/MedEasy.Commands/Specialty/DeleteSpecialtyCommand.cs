using System;

namespace MedEasy.Commands.Specialty
{
    /// <summary>
    /// Command to delete a specialty by its id.
    /// </summary>
    public class DeleteSpecialtyByIdCommand : IDeleteSpecialtyByIdCommand
    {
        /// <summary>
        /// Id of the command
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Id of the resource to delete
        /// </summary>
        public Guid Data { get; }

        /// <summary>
        /// Builds a new <see cref="DeleteSpecialtyByIdCommand"/> instance
        /// </summary>
        /// <param name="id">Id of the resource to delete</param>
        public DeleteSpecialtyByIdCommand(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            Id = Guid.NewGuid();
            Data = id;
        }
    }



}
