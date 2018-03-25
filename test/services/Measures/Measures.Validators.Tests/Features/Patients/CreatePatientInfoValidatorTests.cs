

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Measures.Context;
using Measures.DTO;
using Measures.Objects;
using Measures.Validators.Features.Patients.DTO;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static FluentValidation.Severity;
using static Moq.MockBehavior;
using static Moq.Times;
using static Newtonsoft.Json.JsonConvert;

namespace Measures.Validators.Tests.Features.Patients
{

    /// <summary>
    /// Unit tests for <see cref="CreatePatientInfoValidator"/> class.
    /// </summary>
    [Feature("Measures")]
    [UnitTest]
    public class CreatePatientInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private CreatePatientInfoValidator _validator;
        private IUnitOfWorkFactory _uowFactory;

        public CreatePatientInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MeasuresContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbContextOptionsBuilder.Options, (options) => new MeasuresContext(options));
            _validator = new CreatePatientInfoValidator(_uowFactory);

        }

        public void Dispose()
        {
            _outputHelper = null;
            _validator = null;
            _uowFactory = null;
        }

        [Fact]
        public void Should_Implements_AbstractValidator() => _validator.Should()
                .BeAssignableTo<AbstractValidator<CreatePatientInfo>>();


        [Fact]
        public void Ctor_Throws_ArgumentNullException_When_Arguments_Null()
        {
            // Act
            Action action = () => new CreatePatientInfoValidator(null);

            action.Should().Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        public static IEnumerable<object[]> ValidateTestCases
        {
            get
            {

                yield return new object[]
                {
                    new CreatePatientInfo(),
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because no {nameof(CreatePatientInfo)}'s data set."
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Firstname = "Bruce" },
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Lastname).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because {nameof(CreatePatientInfo.Firstname)} is set and {nameof(CreatePatientInfo.Lastname)} is not"
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Lastname = "Wayne" },
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Firstname).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                    )),
                    $"because {nameof(CreatePatientInfo.Lastname)} is set and {nameof(CreatePatientInfo.Firstname)} is not"
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Lastname = "Wayne" },
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Firstname).Equals(errorItem.PropertyName) && errorItem.Severity == Warning)
                    )),
                    $"because {nameof(CreatePatientInfo.Lastname)} is set and {nameof(CreatePatientInfo.Firstname)} is not"
                };

                yield return new object[]
                {
                    new CreatePatientInfo() { Lastname = "Wayne", Firstname = "Bruce", Id = Guid.Empty },
                    ((Expression<Func<ValidationResult, bool>>)
                        (vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(errorItem => nameof(CreatePatientInfo.Id).Equals(errorItem.PropertyName) && errorItem.Severity == Error)
                    )),
                    $"because {nameof(CreatePatientInfo.Id)} is set to {Guid.Empty}"
                };

            }
        }

        [Theory]
        [MemberData(nameof(ValidateTestCases))]
        public async Task ValidateTest(CreatePatientInfo info,
            Expression<Func<ValidationResult, bool>> errorMatcher,
            string because = "")
        {
            _outputHelper.WriteLine($"{nameof(info)} : {SerializeObject(info)}");

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);

            // Assert
            vr.Should()
                .Match(errorMatcher, because);
        }

        [Fact]
        public async Task Should_Fails_When_Id_AlreadyExists()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(new Patient { Lastname = "Grundy", UUID = patientId });
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            CreatePatientInfo info = new CreatePatientInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne",
                Id = patientId
            };

            // Act
            ValidationResult vr = await _validator.ValidateAsync(info);

            // Assert
            vr.IsValid.Should().BeFalse($"{nameof(Patient)} <{info.Id}> already exists");
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.PropertyName == nameof(CreatePatientInfo.Id));
        }



    }

}

