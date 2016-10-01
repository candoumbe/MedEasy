﻿using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using MedEasy.Commands.Specialty;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Commands;
using MedEasy.Handlers.Exceptions;
using MedEasy.Handlers.Specialty.Commands;

namespace MedEasy.Handlers.Specialty.Commands
{


    /// <summary>
    /// An instance of this class can run <see cref="IRunDeleteSpecialtyByIdCommand"/> commands
    /// </summary>
    public class RunDeleteSpecialtyByIdCommand : GenericDeleteByIdCommandRunner<Guid, Objects.Specialty, int, IDeleteSpecialtyByIdCommand>, IRunDeleteSpecialtyByIdCommand
    {

        /// <summary>
        /// Builds a new <see cref="RunDeleteSpecialtyByIdCommand"/> instance
        /// </summary>
        /// <param name="validator">Validator for commands instances</param>
        /// <param name="logger">Logger used to track</param>
        /// <param name="factory"></param>
        /// <see cref="GenericDeleteByIdCommandRunner{TKey, TEntity, TData, TCommand}"/>
        public RunDeleteSpecialtyByIdCommand(IValidate<IDeleteSpecialtyByIdCommand> validator, ILogger<RunDeleteSpecialtyByIdCommand> logger, IUnitOfWorkFactory factory) : base(validator, logger, factory)
        {
        }
    }
}