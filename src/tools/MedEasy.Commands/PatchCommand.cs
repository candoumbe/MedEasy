﻿using MedEasy.DTO;
using Newtonsoft.Json;
using System;

namespace MedEasy.Commands
{
    /// <summary>
    /// Command to patch a <see cref="TResourceId"/> resource
    /// </summary>
    /// <typeparam name="TResourceId">Type of the resource identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource</typeparam>
    /// <typeparam name="TData">Type of data the command carries</typeparam>
    /// <see cref="CommandBase{TKey, TData}"/>
    /// <seealso cref="IPatchCommand{TResourceId, TData}"/>
    [JsonObject]
    public class PatchCommand<TResourceId, TResource, TData> : CommandBase<Guid, TData>, IPatchCommand<Guid, TResourceId, TResource, TData>
        where TData : IPatchInfo<TResourceId, TResource>
        where TResource : class
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
    /// <typeparam name="TResourceId">Type of the identifier of the resource to PATCH</typeparam>
    /// <typeparam name="TResource">Type of the resource to PATCH</typeparam>
    [JsonObject]
    public class PatchCommand<TResourceId, TResource> : PatchCommand<TResourceId, TResource, IPatchInfo<TResourceId, TResource>>, IPatchCommand<TResourceId, TResource>
        where TResource : class
    {
        /// <summary>
        /// Builds a new <see cref="PatchCommand{TResourceId}"/> instance
        /// </summary>
        /// <param name="data">Set of change the command carries</param>
        public PatchCommand(IPatchInfo<TResourceId, TResource> data) : base(data)
        {
        }
    }
}