using MedEasy.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

using Measures.Objects.Exceptions;
using Optional;
using Optional.Collections;
using System.Text.Json;

namespace Measures.Objects
{
    /// <summary>
    /// Patient that owns measures data
    /// </summary>
    public class Patient : AuditableEntity<Guid, Patient>
    {
        /// <summary>
        /// Name of the patient
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Patient's date of birth
        /// </summary>
        public DateTime? BirthDate { get; private set; }

        private readonly IList<BloodPressure> _bloodPressures;

        public IEnumerable<BloodPressure> BloodPressures => _bloodPressures;

        private readonly IList<Temperature> _temperatures;

        public IEnumerable<Temperature> Temperatures => _temperatures;

        private IList<GenericMeasure> _measures;

        /// <summary>
        /// Measures that don't fit in provided measures.
        /// </summary>
        public IEnumerable<GenericMeasure> Measures => _measures;
        
        /// <summary>
        /// Builds a new <see cref="Patient"/> instance.
        /// </summary>
        /// <param name="id">instance unique identitifer</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is <see cref="Guid.Empty"/></exception>
        public Patient(Guid id, string name, DateTime? birthDate = null) : base(id)
        {
            Name = name?.Trim().ToTitleCase();
            BirthDate = birthDate;
            _bloodPressures = new List<BloodPressure>();
            _temperatures = new List<Temperature>();
            _measures = new List<GenericMeasure>();
        }

        /// <summary>
        /// Change the name of the patient.
        /// </summary>
        /// <param name="newName">new name to set.</param>
        public Patient ChangeNameTo(string newName)
        {
            if (newName is null)
            {
                throw new ArgumentNullException(nameof(newName));
            }
            Name = newName.Trim().ToTitleCase();

            return this;
        }

        /// <summary>
        /// Adds a new <see cref="BloodPressure"/> measure
        /// </summary>
        /// <param name="measureId">id of the measure. this could later be used to retrieve the created measure.</param>
        /// <param name="dateOfMeasure"></param>
        /// <param name="systolic"></param>
        /// <param name="diastolic"></param>
        /// <exception cref="DuplicateIdException">if <paramref name="measureId"/> is already present</exception>
        public void AddBloodPressure(Guid measureId, DateTime dateOfMeasure, float systolic, float diastolic)
        {
            if (_bloodPressures.AtLeastOnce(m => m.Id == measureId))
            {
                throw new DuplicateIdException();
            }
            _bloodPressures.Add(new BloodPressure(Id, measureId, dateOfMeasure, systolicPressure: systolic, diastolicPressure: diastolic));
        }

        /// <summary>
        /// Removes the <see cref="BloodPressure"/> with the specified id
        /// </summary>
        /// <param name="measureId">id of the measure to delete</param>
        public void DeleteBloodPressure(Guid measureId)
        {
            Option<BloodPressure> optionalMeasureToDelete = _bloodPressures.SingleOrNone(m => m.Id == measureId);
            optionalMeasureToDelete.MatchSome(measure => _bloodPressures.Remove(measure));
        }

        /// <summary>
        /// Adds a new <see cref="Temperature"/> measure
        /// </summary>
        /// <param name="measureId">id of the measure. this could later be used to retrieve the created measure.</param>
        /// <param name="dateOfMeasure"></param>
        /// <param name="value">The new temperature value to add</param>
        public void AddTemperature(Guid measureId, DateTime dateOfMeasure, float value)
        {
            if (_bloodPressures.AtLeastOnce(m => m.Id == measureId))
            {
                throw new DuplicateIdException();
            }
            _temperatures.Add(new Temperature(measureId, Id, dateOfMeasure, value));
        }

        /// <summary>
        /// Removes the <see cref="BloodPressure"/> with the specified id
        /// </summary>
        /// <param name="measureId">id of the measure to delete</param>
        public void DeleteTemperature(Guid measureId)
        {
            Option<BloodPressure> optionalMeasureToDelete = _bloodPressures.SingleOrDefault(m => m.Id == measureId)
                .SomeNotNull();

            optionalMeasureToDelete.MatchSome(measure => _bloodPressures.Remove(measure));
        }

        public Patient WasBornIn(DateTime? birthDate)
        {
            BirthDate = birthDate;

            return this;
        }

        /// <summary>
        /// Adds a <see cref="GenericMeasure"/>
        /// </summary>
        /// <param name="formId">id of the form which <paramref name="values"/> should be validated against</param>
        /// <param name="measureId">id of the measure to create</param>
        /// <param name="dateOfMeasure">date and time when the measure was taken</param>
        /// <param name="values">values associated with the measure</param>
        /// <exception cref="ArgumentNullException">if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="name"/> is empty or whitespace only</exception>
        public void AddMeasure(Guid formId, Guid measureId, DateTime dateOfMeasure, IDictionary<string, object> values)
        {
            if (formId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(formId), formId, $"{nameof(formId)} is empty");
            }

            if (measureId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(measureId), measureId, $"{nameof(measureId)} is empty");
            }

            if (dateOfMeasure == DateTime.MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(dateOfMeasure), dateOfMeasure, $"{nameof(dateOfMeasure)} cannot be DateTime.MinValue");
            }

            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            _measures.Add(new GenericMeasure(id : measureId, patientId : Id, dateOfMeasure, formId, JsonDocument.Parse(values.Jsonify())));
        }
    }
}