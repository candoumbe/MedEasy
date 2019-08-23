using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.CQRS.Core.Commands
{
    /// <summary>
    /// <para>
    /// Base class for building a command that returns a result.</para>
    /// <para>
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    /// <typeparam name="TResult">Type of the result the commands outputs once handled.</typeparam>
    [DataContract]
    public abstract class CommandBase<TKey, TData, TResult> : ICommand<TKey, TData, TResult>, IEquatable<CommandBase<TKey, TData, TResult>>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Data the command carries
        /// </summary>
        [DataMember]
        public TData Data { get; }

        /// <summary>
        /// Id of the command
        /// </summary>
        [DataMember]
        public TKey Id { get; }

        /// <summary>
        /// Builds a new <see cref="CommandBase{TKey, TData}"/>
        /// </summary>
        /// <param name="id">id of the command</param>
        /// <param name="data">data the command carries</param>
        /// <exception cref="ArgumentException">if <see cref="id"/> <c>Equals(id, default <typeparamref name="TKey"/>)</c> returns <c>true</c></exception>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c></exception>
        protected CommandBase(TKey id, TData data)
        {
            if (Equals(id, default(TKey)))
            {
                throw new ArgumentException(nameof(id), $"{nameof(id)} cannot be set to the default value of {typeof(TKey).FullName}");
            }
            if (Equals(data, default(TData)))
            {
                throw new ArgumentNullException(nameof(data), $"{nameof(data)} cannot be null");
            }

            Id = id;
            Data = data;
        }


        public override string ToString() => SerializeObject(this);

        public override bool Equals(object obj) => Equals(obj as CommandBase<TKey, TData, TResult>);

        public bool Equals(CommandBase<TKey, TData, TResult> other)
            => other != null && Data.Equals(other.Data);

        public override int GetHashCode() => Data.GetHashCode();

    }


    /// <summary>
    /// <para>
    /// Base class for building a command that returns no result.</para>
    /// <para>
    /// </para>
    /// 
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    /// <typeparam name="TResult">Type of the result the commands outputs once handled.</typeparam>
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
        /// <exception cref="ArgumentException">if <see cref="id"/> <c>Equals(id, default <typeparamref name="TKey"/>)</c> returns <c>true</c></exception>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c></exception>
        protected CommandBase(TKey id, TData data)
        {
            if (Equals(id, default(TKey)))
            {
                throw new ArgumentException(nameof(id), $"{nameof(id)} cannot be set to the default value of {typeof(TKey).FullName}");
            }
            if (Equals(data, default(TData)))
            {
                throw new ArgumentException(nameof(data), $"{nameof(data)} cannot be default");
            }

            Id = id;
            Data = data;
        }


        public override string ToString() => SerializeObject(this);
    }
}
