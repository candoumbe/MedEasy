using MedEasy.Commands.Patient;
using MedEasy.DTO;
using Newtonsoft.Json;
using System;
using static Newtonsoft.Json.JsonConvert;
using MedEasy.Objects;

namespace MedEasy.Commands
{
    /// <summary>
    /// Base class for building a command that create an new <see cref="PhysiologicalMeasurementInfo"/> .</para>
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    /// <typeparam name="TOutput">Type of the result of the execution of the command</typeparam>
    [JsonObject]
    public class AddNewPhysiologicalMeasureCommand<TCommandId, TData, TOutput> : CommandBase<TCommandId, CreatePhysiologicalMeasureInfo<TData>, TOutput>, IAddNewPhysiologicalMeasureCommand<TCommandId, TData, TOutput>
        where TCommandId : IEquatable<TCommandId>
        where TData : PhysiologicalMeasurement
    {

        /// <summary>
        /// Builds a new <see cref="AddNewPhysiologicalMeasureCommand{TKey, TData, TOutput}"/>
        /// </summary>
        /// <param name="id">id of the command</param>
        /// <param name="data">data the command carries</param>
        /// 
        public AddNewPhysiologicalMeasureCommand(TCommandId id, CreatePhysiologicalMeasureInfo<TData> data) : base(id, data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }


        public override string ToString() => SerializeObject(this);
    }


    /// <summary>
    /// Base class for building a command that create an new <see cref="PhysiologicalMeasurementInfo"/> .</para>
    /// </summary>
    /// <remarks>Command's id is <see cref="System.Guid"/></remarks>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    [JsonObject]
    public class AddNewPhysiologicalMeasureCommand<TData, TOutput> : AddNewPhysiologicalMeasureCommand<Guid, TData, TOutput>
        where TData : PhysiologicalMeasurement
        where TOutput : PhysiologicalMeasurementInfo

    {
        /// <summary>
        /// Builds a new <see cref="AddNewPhysiologicalMeasureCommand{TData, TOutput}"/> instance
        /// </summary>
        /// <param name="data">data the command carries</param>
        public AddNewPhysiologicalMeasureCommand(CreatePhysiologicalMeasureInfo<TData> data) : base(Guid.NewGuid(), data)
        {

        }
    }
}
