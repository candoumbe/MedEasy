﻿using MedEasy.Commands.Patient;
using MedEasy.DTO;
using Newtonsoft.Json;
using System;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Commands
{
    /// <summary>
    /// Base class for building a command that create an new <see cref="PhysiologicalMeasurementInfo"/> .</para>
    /// </summary>
    /// <typeparam name="TKey">Type of the command identifier</typeparam>
    /// <typeparam name="TData">Type of data the command will carry</typeparam>
    [JsonObject]
    public class AddNewPhysiologicalMeasureCommand<TKey, TData, TOutput> : CommandBase<TKey, TData>, IAddNewPhysiologicalMeasureCommand<TKey, TData>
        where TKey : IEquatable<TKey>
        where TData : CreatePhysiologicalMeasureInfo
    {
        
        /// <summary>
        /// Builds a new <see cref="CommandBase{TKey, TData}"/>
        /// </summary>
        /// <param name="id">id of the command</param>
        /// <param name="data">data the command carries</param>
        /// 
        public AddNewPhysiologicalMeasureCommand(TKey id, TData data) : base(id, data)
        {
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
        where TData : CreatePhysiologicalMeasureInfo

    {
        public AddNewPhysiologicalMeasureCommand(TData data) : base(Guid.NewGuid(), data)
        {

        }
    }
}