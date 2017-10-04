using System;
using MedEasy.DAL.Interfaces;
using MedEasy.Commands.Specialty;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Specialty.Commands;

namespace MedEasy.Handlers.Specialty.Commands
{


    /// <summary>
    /// An instance of this class can run <see cref="IRunDeleteSpecialtyByIdCommand"/> commands
    /// </summary>
    public class RunDeleteSpecialtyByIdCommand : GenericDeleteByIdCommandRunner<Guid, Objects.Specialty, IDeleteSpecialtyByIdCommand>, IRunDeleteSpecialtyByIdCommand
    {

        /// <summary>
        /// Builds a new <see cref="RunDeleteSpecialtyByIdCommand"/> instance
        /// </summary>
        /// <param name="factory">builds <see cref="IUnitOfWork"/> instances.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="factory"/> is <c>null</c>.</exception>
        /// <see cref="GenericDeleteByIdCommandRunner{TKey, TEntity, TData, TCommand}"/>
        public RunDeleteSpecialtyByIdCommand(IUnitOfWorkFactory factory) : base(factory)
        {
        }
    }
}
