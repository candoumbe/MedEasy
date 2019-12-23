using MedEasy.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Optional;
using Measures.Objects.Exceptions;

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
        }

        /// <summary>
        /// Change the name of the patient.
        /// </summary>
        /// <param name="newName">new name to set.</param>
        public Patient ChangeNameTo(string newName)
        {
            if (newName == null)
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
        public void AddBloodPressure(Guid measureId, DateTimeOffset dateOfMeasure, float systolic, float diastolic)
        {
            if (_bloodPressures.AtLeastOnce(m => m.Id == measureId))
            {
                throw new DuplicateIdException();
            }
            _bloodPressures.Add(new BloodPressure(measureId, Id, dateOfMeasure, diastolicPressure: diastolic, systolicPressure: systolic));
        }

        /// <summary>
        /// Removes the <see cref="BloodPressure"/> with the specified id
        /// </summary>
        /// <param name="measureId">id of the measure to delete</param>
        public void DeleteBloodPressure(Guid measureId)
        {
            Option<BloodPressure> optionalMeasureToDelete = _bloodPressures.SingleOrDefault(m => m.Id == measureId)
                .SomeNotNull();

            optionalMeasureToDelete.MatchSome(measure => _bloodPressures.Remove(measure));
        }
        
        /// <summary>
        /// Adds a new <see cref="Temperature"/> measure
        /// </summary>
        /// <param name="measureId">id of the measure. this could later be used to retrieve the created measure.</param>
        /// <param name="dateOfMeasure"></param>
        /// <param name="value">The new temperature value to add</param>
        public void AddTemperature(Guid measureId, DateTimeOffset dateOfMeasure, float value)
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
    }
}