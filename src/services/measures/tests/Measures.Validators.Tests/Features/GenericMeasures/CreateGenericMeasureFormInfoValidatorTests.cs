using FluentAssertions;

using FluentValidation.Results;

using Forms;

using Measures.Context;
using Measures.CQRS.Commands.GenericMeasures;
using Measures.DTO;
using Measures.Objects;
using Measures.Validators.Commands.GenericMeasures;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

using static FluentValidation.Severity;

namespace Measures.Validators.Tests.Features.GenericMeasures
{
    public class CreateGenericMeasureFormInfoCommandValidatorTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly CreateGenericMeasureFormInfoCommandValidator _sut;
        private readonly IUnitOfWorkFactory _uowFactory;

        public CreateGenericMeasureFormInfoCommandValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MeasuresContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbContextOptionsBuilder.Options, (options) => new MeasuresContext(options));
            _sut = new CreateGenericMeasureFormInfoCommandValidator(_uowFactory);
        }

        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<MeasureForm>(),
                    new CreateGenericMeasureFormInfoCommand(new CreateGenericMeasureFormInfo()),
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(2)
                        && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Name)}")
                        && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)}")
                    ),
                    $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Name)} cannot be null/empty/whitespace " +
                    $"and {nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)} is empty."
                };

                yield return new object[]
                {
                    Enumerable.Empty<MeasureForm>(),
                    new CreateGenericMeasureFormInfoCommand(new CreateGenericMeasureFormInfo { Name = "blood-pressure" }),
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                        && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)}")
                    ),
                    $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)} cannot be empy."
                };

                yield return new object[]
                {
                    Enumerable.Empty<MeasureForm>(),
                    new CreateGenericMeasureFormInfoCommand(new CreateGenericMeasureFormInfo {
                        Fields = new []
                        {
                            new FormField { Name = "Diastolic" }
                        }
                    }),
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                        && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Name)}")
                    ),
                    $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Name)} is empty."
                };

                yield return new object[]
                {
                    Enumerable.Empty<MeasureForm>(),
                    new CreateGenericMeasureFormInfoCommand(new CreateGenericMeasureFormInfo {
                        Name = "blood-pressure",
                        Fields = new []
                        {
                            new FormField ()
                        }
                    }),
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                        && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)}[0]"
                            && err.Severity == Error && err.ErrorMessage == "The property 'name' is not set")
                    ),
                    $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)} contains only one field and its name is not set."
                };

                yield return new object[]
                {
                    Enumerable.Empty<MeasureForm>(),
                    new CreateGenericMeasureFormInfoCommand(new CreateGenericMeasureFormInfo {
                        Name = "blood-pressure",
                        Fields = new []
                        {
                            new FormField (),
                            new FormField {Name = "a-random-name"}
                        }
                    }),
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                        && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)}[0]"
                            && err.Severity == Error && err.ErrorMessage == "The property 'name' is not set")
                    ),
                    $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)} contains only two fields and one of them has the name property not set."
                };

                yield return new object[]
                {
                    Enumerable.Empty<MeasureForm>(),
                    new CreateGenericMeasureFormInfoCommand(new CreateGenericMeasureFormInfo {
                        Name = "blood-pressure",
                        Fields = new []
                        {
                            new FormField {Name = "a-random-name"},
                            new FormField {Name = "a-random-name"}
                        }
                    }),
                    (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                        && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)}"
                            && err.Severity == Error && err.ErrorMessage == "Multiple fields with same name : 'a-random-name'")
                    ),
                    $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Fields)} contains only two fields and one of them has the name property not set."
                };
                {
                    MeasureForm bloodPressureForm = new MeasureForm(Guid.NewGuid(), "blood-pressure");
                    bloodPressureForm.AddFloatField("systolic", "Systolic pressure", min: 0, max: 100, required: true);
                    bloodPressureForm.AddFloatField("diastolic", "Diastolic pressure", min: 0, max: 100, required: true);

                    yield return new object[]
                    {
                        new []{ bloodPressureForm },
                        new CreateGenericMeasureFormInfoCommand(new CreateGenericMeasureFormInfo {
                            Name = "blood-pressure",
                            Fields = new []
                            {
                                new FormField {Name = "prop1"},
                                new FormField {Name = "prop2"}
                            }
                        }),
                        (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                            && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureFormInfoCommand.Data)}.{nameof(CreateGenericMeasureFormInfo.Name)}"
                               && err.Severity == Error && err.ErrorMessage == "A form with the name 'blood-pressure' already exists")
                        ),
                        "A form with the same name already exists."
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ValidateCases))]
        public async Task Validate(IEnumerable<MeasureForm> forms, CreateGenericMeasureFormInfoCommand cmd, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            _outputHelper.WriteLine($"Command : {cmd.Jsonify()}");

            // Arrange
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<MeasureForm>().Create(forms);

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);

            // Act
            var validationResult = await _sut.ValidateAsync(cmd, default)
                                             .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Errors : {validationResult.Errors.Jsonify()}");

            validationResult.Should()
                            .Match(validationResultExpectation, reason);
        }
    }
}
