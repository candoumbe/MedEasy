using System;
using System.Collections.Generic;
using System.Text.Json;

using MedEasy.Objects;

using Forms;
using System.Linq;
using Optional;
using Optional.Collections;

namespace Measures.Objects
{
    /// <summary>
    /// Wraps the name of a mesurement and the associated data needed to describe it.
    /// </summary>
    public class MeasureForm : AuditableEntity<Guid, MeasureForm>
    {
        /// <summary>
        /// Name of the measure. 
        /// </summary>
        public string Name { get;  }

        private readonly IList<FormField> _fields;

        /// <summary>
        /// Wraps all fields needed to register one instance of measurement.
        /// </summary>
        public IEnumerable<FormField> Fields => _fields;

        public MeasureForm(Guid id, string name) : base(id)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _fields = new List<FormField>();
        }

        /// <summary>
        /// Adds a field to the current instance
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="description">Description of the form</param>
        /// <param name="min">minimun value of the field.</param>
        /// <param name="max">maximum value of the field.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="name"/> is <c>null</c>.</exception>
        public void AddFloatField(string name, string description = null, float? min = null, float? max = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            AddField(new FormField { Name = name, Min = min, Max = max, Description = description, Type = FormFieldType.Decimal });
        }

        /// <summary>
        /// Removes the fielld <paramref name="name"/>
        /// </summary>
        /// <param name="name">Name of the field to remove</param>
        /// <exception cref="ArgumentNullException">if <paramref name="name"/> is <c>null</c>.</exception>
        public void RemoveField(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Option<FormField> maybeField = _fields.SingleOrNone(f => f.Name == name.Slugify());

            maybeField.MatchSome(field => _fields.Remove(field));
        }

        private void AddField(FormField field)
        {
            field.Name = field?.Name?.Slugify();

            if (_fields.AtLeastOnce(f => f.Name == field.Name))
            {
                throw new InvalidOperationException($"A field with the name '{field.Name}' already sets for the form '{Name}'");
            }

            _fields.Add(field);
        }
    }
}
