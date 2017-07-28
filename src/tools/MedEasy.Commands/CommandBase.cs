using Newtonsoft.Json;
using System;
using MedEasy.CQRS.Core;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Commands
{

    /// <summary>
    /// <para>
    /// Base class for building a command that produces a <typeparamref name="Nothing"/> output</para>
    /// <para>
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    [JsonObject]
    public abstract class CommandBase<TKey, TData> : CommandBase<TKey, TData, Nothing>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Builds a new <see cref="CommandBase{TKey, TData}"/>
        /// </summary>
        /// <param name="id">id of the command</param>
        /// <param name="data">data the command carries</param>
        /// <exception cref="ArgumentException">if <see cref="id"/> <c>Equals(id, default <typeparamref name="TKey"/>)</c> returns <c>true</c></exception>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c></exception>
        protected CommandBase(TKey id, TData data) : base(id, data)
        {
        }
    }


    /// <summary>
    /// <para>
    /// Base class for building a command that produces a <typeparamref name="TOutput"/> output</para>
    /// <para>
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="TCommandId">Type of the command identifier</typeparam>
    /// <typeparam name="TCommandData">Type of data the command will carry</typeparam>
    /// <typeparam name="TOutput">Type of the result of the execution of the command</typeparam>
    [JsonObject]
    public abstract class CommandBase<TCommandId, TCommandData, TOutput> : ICommand<TCommandId, TCommandData, TOutput>
        where TCommandId : IEquatable<TCommandId>
    {
        /// <summary>
        /// Data the command carries
        /// </summary>
        [JsonProperty]
        public TCommandData Data { get; }

        /// <summary>
        /// Id of the command
        /// </summary>
        [JsonProperty]
        public TCommandId Id { get; }

        /// <summary>
        /// Builds a new <see cref="CommandBase{TKey, TData, TOutput}"/> instance.
        /// </summary>
        /// <param name="id">id of the command</param>
        /// <param name="data">data the command carries</param>
        /// <exception cref="ArgumentException">if <see cref="id"/> <c>Equals(id, default <typeparamref name="TCommandId"/>)</c> returns <c>true</c></exception>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c></exception>
        protected CommandBase(TCommandId id, TCommandData data)
        {
            if (Equals(id, default(TCommandId)))
            {
                throw new ArgumentException(nameof(id), $"{nameof(id)} cannot be set to the default value of {typeof(TCommandId).FullName}");
            }

            Id = id;
            Data = data ;
        }


        public override string ToString() => SerializeObject(this);
    }
}
