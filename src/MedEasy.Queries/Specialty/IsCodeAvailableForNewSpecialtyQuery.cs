using System;


namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// An instance of this class can be used to check if a <see cref="DTO.SpecialtyInfo.Code"/> can be used for a new 
    /// <see cref="DTO.SpecialtyInfo"/>
    /// </summary>
    public class IsCodeAvailableForNewSpecialtyQuery : IIsCodeAvailableForNewSpecialtyQuery
    {
        public Guid Id { get; }

        /// <summary>
        /// The code to test
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// Builds a new <see cref="IsCodeAvailableForNewSpecialtyQuery"/> instance
        /// </summary>
        /// <param name="code">the code to check availabilty for</param>
        /// <exception cref="ArgumentNullException">if <paramref name="code"/> is <c>null</c></exception>
        public IsCodeAvailableForNewSpecialtyQuery(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException($"{nameof(code)}");
            }

            Id = Guid.NewGuid();
            Data = code;
        }
    }
}
