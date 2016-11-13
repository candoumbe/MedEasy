using System;


namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// An instance of this class can be used to check if a <see cref="DTO.SpecialtyInfo.Code"/> can be used for a new 
    /// <see cref="DTO.SpecialtyInfo"/>
    /// </summary>
    public class IsNameAvailableForNewSpecialtyQuery : IIsNameAvailableForNewSpecialtyQuery
    {
        public Guid Id { get; }

        /// <summary>
        /// The code to test
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// Builds a new <see cref="IsNameAvailableForNewSpecialtyQuery"/> instance
        /// </summary>
        /// <param name="name">the name to check availabilty for</param>
        /// <exception cref="ArgumentNullException">if <paramref name="name"/> is <c>null</c></exception>
        public IsNameAvailableForNewSpecialtyQuery(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException($"{nameof(name)}");
            }

            Id = Guid.NewGuid();
            Data = name;
        }
    }
}
