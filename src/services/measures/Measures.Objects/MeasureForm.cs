using System;
using System.Collections.Generic;
using System.Text.Json;

using MedEasy.Objects;

using Forms;


namespace Measures.Objects
{
    public class MeasureForm : AuditableEntity<Guid, MeasureForm>
    {
        /// <summary>
        /// Name of the measure. 
        /// </summary>
        public string Name { get;  }

        public Form Form { get; }

        public MeasureForm(Guid id, string name) : base(id)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
