using MedEasy.DTO;
using Newtonsoft.Json;
using System;

namespace MedEasy.Commands
{
    
    /// <summary>
    /// Command to patch a <see cref="TResourceId"/> resource
    /// </summary>
    /// <typeparam name="TResourceId">Type of the identifier of the resource to PATCH</typeparam>
    /// <typeparam name="TResource">Type of the resource to PATCH</typeparam>
    [JsonObject]
    public class PatchCommand<TResourceId, TResource> : CommandBase<Guid, PatchInfo<TResourceId, TResource>>, IPatchCommand<TResourceId, TResource>
        where TResource : class
    {
        /// <summary>
        /// Builds a new <see cref="PatchCommand{TResourceId}"/> instance
        /// </summary>
        /// <param name="data">Set of change the command carries</param>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c>.</exception>
        public PatchCommand(PatchInfo<TResourceId, TResource> data) : base(Guid.NewGuid(), data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}
