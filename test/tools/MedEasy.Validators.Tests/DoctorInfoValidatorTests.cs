using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using MedEasy.DTO;
using Xunit;
using System.Threading.Tasks;

namespace MedEasy.Validators.Tests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class DoctorInfoValidatorTests
    {
        public IValidate<DoctorInfo> Validator { get; set; }
        public DoctorInfoValidatorTests()
        {
            Validator = new DoctorInfoValidator();
        }

        [Theory]
        [MemberData(nameof(ValidateTestCases))]
        public async Task ValidateTest(DoctorInfo info,
            Expression<Func<IEnumerable<ErrorInfo>, bool>> errorMatcher,
            string because = "")
            => (await Task.WhenAll(Validator.Validate(info))).Should().Match(errorMatcher, because);

        public static IEnumerable<object[]> ValidateTestCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => errors.Once(errorItem => "".Equals(errorItem.Key) && errorItem.Severity == ErrorLevel.Error))),
                    $"because {nameof(DoctorInfo)} is null"
                };

                yield return new object[]
                {
                    new DoctorInfo(),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => errors.Once(errorItem => string.Empty.Equals(errorItem.Key) && errorItem.Severity == ErrorLevel.Error))),
                    $"because {nameof(DoctorInfo)}'s is null"
                };

                yield return new object[]
                {
                    new DoctorInfo() { Firstname = "Bruce" },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => errors.Once(errorItem => nameof(DoctorInfo.Lastname).Equals(errorItem.Key) && errorItem.Severity == ErrorLevel.Error))),
                    $"because {nameof(DoctorInfo.Firstname)} is set and {nameof(DoctorInfo.Lastname)} is not"
                };

                yield return new object[]
                {
                    new DoctorInfo() { Lastname = "Wayne" },
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => errors.Once(errorItem => nameof(DoctorInfo.Firstname).Equals(errorItem.Key) && errorItem.Severity == ErrorLevel.Warning))),
                    $"because {nameof(DoctorInfo.Lastname)} is set and {nameof(DoctorInfo.Firstname)} is not"
                };
            }
        }
    }
}
