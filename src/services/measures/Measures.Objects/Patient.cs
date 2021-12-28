namespace Measures.Objects
{
    using MedEasy.Objects;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Measures.Objects.Exceptions;
    using Optional;
    using Optional.Collections;
    using NodaTime;
    using Measures.Ids;

    /// <summary>
    /// Patient that owns measures data
    /// </summary>
    public class Subject : AuditableEntity<SubjectId, Subject>
    {
        /// <summary>
        /// Name of the patient
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Patient's date of birth
        /// </summary>
        public LocalDate? BirthDate { get; private set; }

        private readonly IList<BloodPressure> _bloodPressures;

        public IEnumerable<BloodPressure> BloodPressures => _bloodPressures;

        private readonly IList<Temperature> _temperatures;

        public IEnumerable<Temperature> Temperatures => _temperatures;



        /// <summary>
        /// Builds a new <see cref="Subject"/> instance.
        /// </summary>
        /// <param name="id">instance unique identitifer</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is <see cref="SubjectId.Empty"/></exception>
        public Subject(SubjectId id, string name, LocalDate? birthDate = null) : base(id)
        {
            if (id == SubjectId.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            Name = name?.Trim().ToTitleCase();
            BirthDate = birthDate;
            _bloodPressures = new List<BloodPressure>();
            _temperatures = new List<Temperature>();
        }

        /// <summary>
        /// Change the name of the patient.
        /// </summary>
        /// <param name="newName">new name to set.</param>
        public Subject ChangeNameTo(string newName)
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="measureId"/> is <c>BloodPressureId.Empty</c>.
        /// <paramref name="systolic"/>  is less than <c>0</c>.
        /// <paramref name="diastolic"/> is less than <c>0</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="measureId"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="measureId"/> is <see cref="BloodPressureId.Empty"/>.</exception>
        public void AddBloodPressure(BloodPressureId measureId, Instant dateOfMeasure, float systolic, float diastolic)
        {
            if (measureId is null)
            {
                throw new ArgumentNullException(nameof(measureId));
            }
            if (measureId == BloodPressureId.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(measureId), measureId, $"{nameof(measureId)} cannot be null or empty");
            }

            if (_bloodPressures.AtLeastOnce(m => m.Id == measureId))
            {
                throw new DuplicateIdException();
            }
            _bloodPressures.Add(new BloodPressure(Id, measureId, dateOfMeasure, diastolicPressure: diastolic, systolicPressure: systolic));
        }


        /// <summary>
        /// Removes the <see cref="BloodPressure"/> with the specified id
        /// </summary>
        /// <param name="measureId">id of the measure to delete</param>
        public void DeleteBloodPressure(BloodPressureId measureId)
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
        public void AddTemperature(TemperatureId measureId, Instant dateOfMeasure, float value)
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
        public void DeleteTemperature(TemperatureId measureId)
        {
            Option<BloodPressure> optionalMeasureToDelete = _bloodPressures.SingleOrDefault(m => m.Id == measureId)
                                                                           .SomeNotNull();

            optionalMeasureToDelete.MatchSome(measure => _bloodPressures.Remove(measure));
        }

        public Subject WasBornOn(LocalDate? birthDate)
        {
            BirthDate = birthDate;

            return this;
        }
    }
}