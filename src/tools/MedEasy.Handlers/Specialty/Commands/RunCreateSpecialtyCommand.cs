using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using MedEasy.Commands.Specialty;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Specialty.Commands;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.Handlers.Specialty.Commands
{


    /// <summary>
    /// An instance of this class can run <see cref="IRunCreateSpecialtyCommand"/> commands
    /// </summary>
    public class RunCreateSpecialtyCommand : GenericCreateCommandRunner<Guid, Objects.Specialty, CreateSpecialtyInfo, SpecialtyInfo, ICreateSpecialtyCommand>, IRunCreateSpecialtyCommand
    {

        /// <summary>
        /// Builds a new <see cref="RunCreateSpecialtyCommand"/> instance.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="expressionBuilder"></param>
        /// 
        /// 
        public RunCreateSpecialtyCommand(IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder) : base(factory, expressionBuilder)
        {
        }
        

    }
}
