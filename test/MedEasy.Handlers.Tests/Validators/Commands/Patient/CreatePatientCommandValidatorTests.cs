using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using Xunit;
using System.Linq;
using static MedEasy.Validators.ErrorLevel;
using Xunit.Abstractions;
using MedEasy.Validators.Patient;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using System.Threading.Tasks;

namespace MedEasy.Validators.Tests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class CreatePatientCommandValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CreatePatientCommandValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> InvalidCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    null,
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors =>
                            errors.Count() == 1 &&
                            errors.Once(errorItem => string.Empty.Equals(errorItem.Key) && errorItem.Severity == Error))),
                    $"because {nameof(ICreatePatientCommand)} is null"
                };


                //
                yield return new object[]
                {
                    null,
                    new DateTime(2012, 1, 1),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => 
                            errors.Count() == 1 && 
                            errors.Once(errorItem => string.Empty.Equals(errorItem.Key) && errorItem.Severity == Error))),
                    $"because {nameof(ICreatePatientCommand)} is null"
                };

                yield return new object[]
                {
                    new CreatePatientCommand(null),
                    new DateTime(2012, 1, 1),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => errors.Count() == 1 && errors.Once(item => nameof(ICreatePatientCommand.Data).Equals(item.Key) && item.Severity == Error))),
                    $"new {nameof(CreatePatientCommand)}(null) is not valid"
                };

                yield return new object[]
                {
                    new CreatePatientCommand(new CreatePatientInfo()),
                    new DateTime(2012, 1, 1),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => errors.Count() == 2 && 
                            errors.Once(item => nameof(CreatePatientInfo.Firstname).Equals(item.Key) && item.Severity == Warning) &&
                            errors.Once(item => nameof(CreatePatientInfo.Lastname).Equals(item.Key) && item.Severity == Error))),
                    $"new {nameof(CreatePatientCommand)}(new {nameof(CreatePatientInfo)}()) is not valid"
                };

                yield return new object[]
                {
                    new CreatePatientCommand(new CreatePatientInfo { BirthDate = DateTime.MaxValue }),
                    new DateTime(2012, 1, 1),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => errors.Count() == 3 && errors.Once(item => nameof(CreatePatientInfo.BirthDate).Equals(item.Key) && item.Severity == Warning))),
                    $"{Environment.NewLine}{nameof(ICreatePatientCommand)}.{nameof(ICreatePatientCommand.Data)}.{nameof(CreatePatientInfo.Firstname)} is not set,"  +
                    $"{Environment.NewLine}{nameof(ICreatePatientCommand)}.{nameof(ICreatePatientCommand.Data)}.{nameof(CreatePatientInfo.Lastname)} is not set,"  +
                    $"{Environment.NewLine}{nameof(ICreatePatientCommand)}.{nameof(ICreatePatientCommand.Data)}.{nameof(CreatePatientInfo.BirthDate)} is after {new DateTime(2012, 1, 1)}" 
                };

                yield return new object[]
                {
                    new CreatePatientCommand(new CreatePatientInfo { BirthDate = DateTime.MaxValue }),
                    new DateTime(2012, 1, 1),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>)
                        (errors => errors.Count() == 3 && errors.Once(item => nameof(CreatePatientInfo.BirthDate).Equals(item.Key) && item.Severity == Warning))),
                    $"{Environment.NewLine}{nameof(ICreatePatientCommand)}.{nameof(ICreatePatientCommand.Data)}.{nameof(CreatePatientInfo.Firstname)} is not set,"  +
                    $"{Environment.NewLine}{nameof(ICreatePatientCommand)}.{nameof(ICreatePatientCommand.Data)}.{nameof(CreatePatientInfo.Lastname)} is not set,"  +
                    $"{Environment.NewLine}{nameof(ICreatePatientCommand)}.{nameof(ICreatePatientCommand.Data)}.{nameof(CreatePatientInfo.BirthDate)} is after {new DateTime(2012, 1, 1)}"
                };

                yield return new object[]
                {
                    new CreatePatientCommand(new CreatePatientInfo {
                        Firstname = "Bruce",
                        BirthDate = new DateTime(2012, 1, 1),
                    }),
                    new DateTime(2012, 1, 1),
                    ((Expression<Func<IEnumerable<ErrorInfo>, bool>>) (errors =>
                        errors.Count() == 1 &&
                        errors.Once(errorItem => nameof(CreatePatientInfo.Lastname).Equals(errorItem.Key) && errorItem.Severity == Error ))),
                    $"because {nameof(CreatePatientInfo.BirthDate)} is equal to {new DateTime(2012, 1, 1)}"
                };
                
            }
        }


        public static IEnumerable<object[]> ValidCreatePatientCommandCases
        {
            get
            {
                yield return new object[]
                {
                    new CreatePatientCommand(new CreatePatientInfo { Lastname = "Wayne" }),
                    null,
                };

                yield return new object[]
                {
                    new CreatePatientCommand(new CreatePatientInfo { Firstname = "Bruce", Lastname = "Wayne" }),
                    null,
                };

                yield return new object[]
                {
                    new CreatePatientCommand(new CreatePatientInfo { Firstname = "Bruce", Lastname = "Wayne", BirthDate = new DateTime(1960, 1, 1) }),
                    null,
                };

                yield return new object[]
                {
                    new CreatePatientCommand(new CreatePatientInfo { Firstname = "Bruce", Lastname = "Wayne", BirthDate = new DateTime(1960, 1, 1) }),
                    DateTime.MaxValue,
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidCreatePatientCommandCases))]
        public async Task ValidCreatePatientInfoTests(ICreatePatientCommand command, DateTime? maxBirthDate)
            => await Validate(command, maxBirthDate, errors => !errors.Any(errorItem => errorItem.Severity == Error));


        [Theory]
        [MemberData(nameof(InvalidCases))]
        public async Task InvalidCreatePatientInfoTests(ICreatePatientCommand command, DateTime? maxBirthDate,
            Expression<Func<IEnumerable<ErrorInfo>, bool>> errorsExpectation,
            string because = "")
            => await Validate(command, maxBirthDate, errorsExpectation, because);
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">The command to validate</param>
        /// <param name="maxBirthDateAllowed">Defines the date beyond which the <see cref="CreatePatientInfo.BirthDate"/> will be considered invalid</param>
        /// <param name="errorsExpectation"></param>
        /// <param name="because"></param>
        private async Task Validate(ICreatePatientCommand command, DateTime? maxBirthDateAllowed,
            Expression<Func<IEnumerable<ErrorInfo>, bool>> errorsExpectation,
            string because = "")
        {

            _outputHelper.WriteLine($"{nameof(command)} : {command}");
            _outputHelper.WriteLine($"{nameof(maxBirthDateAllowed)} : {maxBirthDateAllowed}");
            _outputHelper.WriteLine($"{nameof(errorsExpectation)} : {errorsExpectation}");
            
            //Act
            IValidate<ICreatePatientCommand> validator = new CreatePatientCommandValidator(maxBirthDateAllowed);
            IEnumerable<Task<ErrorInfo>> errorsTasks = validator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorsTasks);
            
            //Assert
            errors.Should().Match(errorsExpectation, because);
        }


        public void Dispose()
        {
            _outputHelper = null;
        }


    }
}
