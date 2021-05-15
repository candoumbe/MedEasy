namespace Patients.Validators.Features.Patients.Queries
{
    using FluentValidation;

    using MedEasy.Validators.Validators;

    using global::Patients.DTO;

    /// <summary>
    /// Validates <see cref="SearchPatientInfo"/> instances.
    /// </summary>
    public class SearchPatientInfoValidator : AbstractValidator<SearchPatientInfo>
    {
        /// <summary>
        /// List of properties that can be used to sort results found using a <see cref="SearchPatientInfo"/>
        /// </summary>
        private static string[] _sortableProperties = new[] {
        nameof(SearchPatientInfo.BirthDate),
        nameof(SearchPatientInfo.Firstname),
        nameof(SearchPatientInfo.Lastname),
    };


        public SearchPatientInfoValidator()
        {
            Include(new AbstractSearchInfoValidator<PatientInfo>());
            When(x => string.IsNullOrWhiteSpace(x.Firstname)
                    && string.IsNullOrWhiteSpace(x.Lastname)
                    && x.BirthDate == null
                    && string.IsNullOrWhiteSpace(x.Sort),
                () =>
                {
                    RuleFor(x => x.Firstname).NotEmpty();
                    RuleFor(x => x.Lastname).NotEmpty();
                    RuleFor(x => x.BirthDate).NotEmpty();
                    RuleFor(x => x.Sort).NotEmpty();
                });
        }
    }
}