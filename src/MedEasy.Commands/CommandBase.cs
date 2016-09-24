using Newtonsoft.Json;
using System;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Commands
{
    /// <summary>
    /// <para>
    /// Base class for building a command.</para>
    /// <para>
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    [JsonObject]
    public abstract class CommandBase<TKey, TData> : ICommand<TKey, TData>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Data the command carries
        /// </summary>
        [JsonProperty]
        public TData Data { get; }

        /// <summary>
        /// Id of the command
        /// </summary>
        [JsonProperty]
        public TKey Id { get; }

        /// <summary>
        /// Builds a new <see cref="CommandBase{TKey, TData}"/>
        /// </summary>
        /// <param name="id">id of the command</param>
        /// <param name="data">data the command carries</param>
        /// 
        protected CommandBase(TKey id, TData data)
        {
            
            Id = id;
            Data = data;
        }


        public override string ToString() => SerializeObject(this);
    }
}
