using MedEasy.Commands.Patient;
using MedEasy.DTO;
using Newtonsoft.Json;
using System;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Commands
{
    /// <summary>
    /// Defines a command that delete a<see cref="PhysiologicalMeasurementInfo"/>.</para>
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    [JsonObject]
    public class DeleteOnePhysiologicalMeasureCommand<TKey> : CommandBase<TKey, DeletePhysiologicalMeasureInfo>, IDeleteOnePhysiologicalMeasureCommand<TKey, DeletePhysiologicalMeasureInfo>
        where TKey : IEquatable<TKey>
        
    {
        
        /// <summary>
        /// Builds a new <see cref="DeleteOnePhysiologicalMeasureCommand{TKey}"/>
        /// </summary>
        /// <remarks>
        /// This command instructs the system to remove exactly one <see cref="PhysiologicalMeasurementInfo"/> resource
        /// fromspecified <see cref="Patient"/>
        /// </remarks>
        /// <param name="id">id of the command</param>
        /// <param name="data">data the command carries</param>
        /// <see cref="CommandBase{TKey, TData}"/>
        public DeleteOnePhysiologicalMeasureCommand(TKey id, DeletePhysiologicalMeasureInfo data) : base(id, data)
        {
        }


        public override string ToString() => SerializeObject(this);
    }


    /// <summary>
    /// Defines a command that delete a<see cref="PhysiologicalMeasurementInfo"/>.</para>
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    [JsonObject]
    public class DeleteOnePhysiologicalMeasureCommand : DeleteOnePhysiologicalMeasureCommand<Guid>
    {

        /// <summary>
        /// Builds a new <see cref="DeleteOnePhysiologicalMeasureCommand"/>
        /// </summary>
        /// <remarks>
        /// This command instructs the system to remove exactly one <see cref="PhysiologicalMeasurementInfo"/> resource
        /// fromspecified <see cref="Patient"/>
        /// </remarks>
        /// <param name="data">data the command carries</param>
        /// <see cref="CommandBase{TKey, TData}"/>
        public DeleteOnePhysiologicalMeasureCommand(DeletePhysiologicalMeasureInfo data) : base(Guid.NewGuid(), data)
        {
        }

        
    }

}
