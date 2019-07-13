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

        private IList<PhysiologicalMeasurement> _measures;

        public IEnumerable<PhysiologicalMeasurement> Measures => _measures;

        /// <summary>
        /// Builds a new <see cref="Patient"/> instance.
        /// </summary>
        /// <param name="id">instance unique identitifer</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is <see cref="Guid.Empty"/></exception>
        public Patient(Guid id) : base(id)
        {
            if (id == default)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            Name = $"patient-{id}";
            _measures = new List<PhysiologicalMeasurement>();
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
            if (_measures.AtLeastOnce(m => m.Id == measureId))
            {
                throw new DuplicateIdException();
            }
            _measures.Add(new BloodPressure(measureId, Id, dateOfMeasure, diastolicPressure: diastolic, systolicPressure: systolic));
        }

        /// <summary>
        /// Removes the <see cref="PhysiologicalMeasurement"/> with the specified id
        /// </summary>
        /// <param name="measureId">id of the measure to delete</param>
        public void DeleteMeasure(Guid measureId)
        {
            Option<PhysiologicalMeasurement> optionalMeasureToDelete = _measures.SingleOrDefault(m => m.Id == measureId)
                .SomeNotNull();

            optionalMeasureToDelete.MatchSome(measure => _measures.Remove(measure));
        }

        public Patient WasBornIn(DateTime? birthDate)
        {
            BirthDate = birthDate;

            return this;
        }
    }
}