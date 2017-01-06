using MedEasy.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.API.Tests.Models
{
    public class SearchPatientRequestModelTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public SearchPatientRequestModelTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            _outputHelper = null;
        }

        public static IEnumerable<object> SearchPatientRequestModelCases
        {
            get
            {
                yield return new object[] {
                    new SearchPatientInfo(),
                    ((Expression<Func<IEnumerable<ValidationResult>, bool>>)(x =>
                        x.Once() &&     
                        x.Once(item => item.ErrorMessage == "One of the search criteria must be set."
                        )))
                };

                yield return new object[] {
                    new SearchPatientInfo {
                        Firstname = "Bruce",
                        Page = 1,
                        PageSize = 10,
                        Sort = $"{nameof(SearchPatientInfo.Firstname)},"
                    },
                    ((Expression<Func<IEnumerable<ValidationResult>, bool>>)(x =>
                        x.Once(item => item.MemberNames.Contains(nameof(SearchPatientInfo.Sort)) &&
                            item.ErrorMessage == $"<{nameof(SearchPatientInfo.Firstname)},> does not match '{SearchPatientInfo.SortPattern}'."
                        )))
                };

                yield return new object[] {
                    new SearchPatientInfo {
                        Firstname = "Bruce",
                        Page = 1,
                        PageSize = 10,
                        Sort = "Name"
                    },
                    ((Expression<Func<IEnumerable<ValidationResult>, bool>>)(x =>
                        x.Once() &&
                        x.Once(item => item.MemberNames.Contains(nameof(SearchPatientInfo.Sort)) &&
                            item.ErrorMessage == $"Unknown <Name> property."
                        )))
                };

                yield return new object[] {
                    new SearchPatientInfo {
                        Firstname = "Bruce",
                        Page = 1,
                        PageSize = 10,
                        Sort = "Name, Nickname"
                    },
                    ((Expression<Func<IEnumerable<ValidationResult>, bool>>)(x =>
                        x.Once() &&
                        x.Once(item => item.MemberNames.Contains(nameof(SearchPatientInfo.Sort)) &&
                            item.ErrorMessage == $"Unknown <Name>, <Nickname> properties."
                        )))
                };
            }
        }



        [Theory]
        [MemberData(nameof(SearchPatientRequestModelCases))]
        public void Validate(SearchPatientInfo model, Expression<Func<IEnumerable<ValidationResult>, bool>> validationExpectation)
        {
            _outputHelper.WriteLine($"Model : {SerializeObject(model)}");

            // Arrange
            ValidationContext validationContext = new ValidationContext(model);

            // Act
            IEnumerable<ValidationResult> validations = model.Validate(validationContext);

            // Assert
            validations.Should().Match(validationExpectation);


        }
    }
}
