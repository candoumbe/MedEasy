using FluentAssertions;
using MedEasy.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Core.UnitTests.Attributes
{
    public class RequireNonDefaultAttributeTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private RequireNonDefaultAttribute _sut;

        public RequireNonDefaultAttributeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _sut = new RequireNonDefaultAttribute();
        }

        public void Dispose()
        {
            _outputHelper = null;
            _sut = null;
        }


        [Fact]
        [Trait("Category", "Unit test")]
        public void CtorCreateAValidAttribute()
        {
            // Assert
            _sut.Should()
                .BeAssignableTo<ValidationAttribute>();
        }


        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                yield return new object[] { default(Guid), false, "default(Guid) must not be valid" };
                yield return new object[] { Guid.Empty, false, "Guid.Rmpty must not be valid" };
                yield return new object[] { default(int?), false, "default(int?) must not be valid" };
                yield return new object[] { default(long?), false, "default(long?) must not be valid" };
                yield return new object[] { default(float?), false, "default(float?) must not be valid" };
                yield return new object[] { default(decimal?), false, "default(decimal?) must not be valid" };
                yield return new object[] { default(decimal), false, "default(decimal) must not be valid" };

            }
        }

        [Theory]
        [InlineData(default(string), false, "default string is not valid")]
        [InlineData(default(int), false, "default int is not valid")]
        [InlineData(default(long), false, "default long is not valid")]
        [InlineData(default(float), false, "default float is not valid")]
        [InlineData(default(short), false, "default short is not valid")]
        [MemberData(nameof(ValidateCases))]

        public void Validate(object value, bool expectedResult, string reason)
        {
            _outputHelper.WriteLine($"Parameters : {SerializeObject(new { value, expectedResult })}");

            // Assert
            _sut.IsValid(value).Should()
                .Be(expectedResult, reason);
        }

        public static IEnumerable<object[]> ValidateWithContextCases
        {
            get
            {
                yield return new object[]
                {
                    default(int),
                    ((Expression<Func<ValidationResult, bool>>)(vr => "the field must have a non default value".Equals(vr.ErrorMessage))),
                    "default(int) is not valid"
                };

                

                yield return new object[]
                {
                    3, 
                    ((Expression<Func<ValidationResult, bool>>)(vr => vr == ValidationResult.Success)),
                    "3 is a valid value"
                };


            }

        }

        [Theory]
        [MemberData(nameof(ValidateWithContextCases))]

        public void ValidateWithContext(object value, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            _outputHelper.WriteLine($"Value to validate : {value}");

            // Act
            ValidationResult vr = _sut.GetValidationResult(value, new ValidationContext(value));

            // Assert
            vr.Should()
                .Match(validationResultExpectation, reason);
        }
    }
}
