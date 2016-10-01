﻿using System;
using Newtonsoft.Json;

namespace MedEasy.Commands.Doctor
{
    /// <summary>
    /// Command to delete a doctor by its id
    /// </summary>
    [JsonObject]
    public class DeleteDoctorByIdCommand : CommandBase<Guid, int>, IDeleteDoctorByIdCommand
    {

        /// <summary>
        /// Builds a new <see cref="DeleteDoctorByIdCommand"/> with a default validator
        /// </summary>
        /// <param name="id">id of the resource to delete</param>
        public DeleteDoctorByIdCommand(int id) : base(Guid.NewGuid(), id)
        {}

        
    }


    
}