using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Commands
{
    /// <summary>
    /// Command to patch a <see cref="TResourceId"/> resource
    /// </summary>
    /// <typeparam name="TResourceId">Type of the resource</typeparam>
    /// <typeparam name="TData">Type of data the command carries</typeparam>
    /// <see cref="CommandBase{TKey, TData}"/>
    /// <seealso cref="IPatchCommand{TResourceId, TData}"/>
    public class PatchCommand<TResourceId, TData> : CommandBase<Guid, TData>, IPatchCommand<TResourceId, TData>
        where TData : IPatchInfo<TResourceId>
    {
        /// <summary>
        /// Builds a new <see cref="PatchCommand{TResourceId, TData}"/>
        /// </summary>
        /// <param name="data">Data of the command</param>
        public PatchCommand(TData data) : base(Guid.NewGuid(), data)
        {
        }
    }

    /// <summary>
    /// Command to patch a <see cref="TResourceId"/> resource
    /// </summary>
    /// <typeparam name="TResourceId">Type of the resource</typeparam>
    public class PatchCommand<TResourceId> : PatchCommand<TResourceId, IPatchInfo<TResourceId>>, IPatchCommand<TResourceId>
    {
        /// <summary>
        /// Builds a new <see cref="PatchCommand{TResourceId}"/> instance
        /// </summary>
        /// <param name="data">Set of change the command carries</param>
        public PatchCommand(IPatchInfo<TResourceId> data) : base(data)
        {
        }
    }
}
