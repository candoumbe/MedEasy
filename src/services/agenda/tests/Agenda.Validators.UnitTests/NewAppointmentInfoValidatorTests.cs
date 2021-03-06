namespace Agenda.Validators.UnitTests
{
    using Agenda.DTO;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using FluentValidation.Results;

    using Moq;

    using NodaTime;
    using NodaTime.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static FluentValidation.Severity;
    using static Moq.MockBehavior;
    using static Newtonsoft.Json.JsonConvert;

    [Feature("Agenda")]
    [UnitTest]
    public class NewAppointmentInfoValidatorTests
    {
        private static ITestOutputHelper _outputHelper;
        private readonly Mock<IClock> _datetimeServiceMock;
        private readonly NewAppointmentModelValidator _sut;

        public NewAppointmentInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _datetimeServiceMock = new(Strict);
            _sut = new NewAppointmentModelValidator(_datetimeServiceMock.Object);
        }

        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                yield return new object[]
                {
                    new NewAppointmentInfo(),
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 5
                        && vr.Errors.Once(error => error.PropertyName == nameof(NewAppointmentInfo.EndDate) && error.Severity == Error)
                        && vr.Errors.Once(error => error.PropertyName == nameof(NewAppointmentInfo.StartDate) && error.Severity == Error)
                        && vr.Errors.Once(error => error.PropertyName == nameof(NewAppointmentInfo.Location) && error.Severity == Error)
                        && vr.Errors.Once(error => error.PropertyName == nameof(NewAppointmentInfo.Subject) && error.Severity == Error)
                        && vr.Errors.Once(error => error.PropertyName == nameof(NewAppointmentInfo.Attendees) && error.Severity == Error)
                    ),
                    "no property set"
                };

                yield return new object[]
                {
                    new NewAppointmentInfo
                    {
                        Location = "Wayne Tower",
                        Subject = "Classified",
                        StartDate = 1.February(2005).Add(12.Hours().And(30.Minutes())).AsUtc().ToInstant().InUtc(),
                        EndDate = 1.February(2005).Add(12.Hours().And(30.Minutes())).AsUtc().ToInstant().InUtc(),
                        Attendees = new []
                        {
                            new AttendeeInfo { Name = "Ed Nigma" }
                        }
                    },
                    (Expression<Func<ValidationResult, bool>>)(vr => vr.IsValid),
                    $"all properties set and {nameof(NewAppointmentInfo.StartDate)} == {nameof(NewAppointmentInfo.EndDate)}"
                };

                yield return new object[]
                {
                    new NewAppointmentInfo
                    {
                        Location = "Wayne Tower",
                        Subject = "Classified",
                        StartDate = 1.February(2005).Add(12.Hours().And(30.Minutes())).AsUtc().ToInstant().InUtc(),
                        EndDate = 1.February(2005).Add(12.Hours()).AsUtc().ToInstant().InUtc(),
                        Attendees = new []
                        {
                            new AttendeeInfo { Name = "Ed Nigma" }
                        }
                    },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(error => nameof(NewAppointmentInfo.EndDate) == error.PropertyName && error.Severity == Error)
                    ),
                    $"{nameof(NewAppointmentInfo.StartDate)} > {nameof(NewAppointmentInfo.EndDate)}"
                };

                yield return new object[]
                {
                    new NewAppointmentInfo
                    {
                        Location = "Wayne Tower",
                        Subject = "Classified",
                        StartDate = 1.February(2005).Add(12.Hours()).AsUtc().ToInstant().InUtc(),
                        EndDate = 1.February(2005).Add(12.Hours()).Add(30.Minutes()).AsUtc().ToInstant().InUtc()
                    },
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(error => nameof(NewAppointmentInfo.Attendees) == error.PropertyName && error.Severity == Error)
                    ),
                    $"{nameof(NewAppointmentInfo.Attendees)} is empty"
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidateCases))]
        public async Task ValidateNewAppointmentInfo(NewAppointmentInfo newAppointmentInfo, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            _outputHelper.WriteLine($"appointment : {SerializeObject(newAppointmentInfo)}");
            // Arrange

            // Act
            ValidationResult vr = await _sut.ValidateAsync(newAppointmentInfo)
                .ConfigureAwait(false);

            // Assert
            vr.Should()
                .Match(validationResultExpectation, reason);
        }

        [Fact]
        public void GivenParameterIsNull_Ctor_ThrowsArgumentNullException()
        {
            // Act
            Action action = () => new NewAppointmentModelValidator(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>($"a {nameof(IClock)} instance is required").Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}
