using FluentAssertions;
using FluentAssertions.Extensions;

using FluentValidation.Results;

using Measures.Context;
using Measures.CQRS.Commands;
using Measures.DTO;
using Measures.Objects;
using Measures.Validators.Commands.GenericMeasures;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

using static FluentValidation.Severity;

namespace Measures.Validators.Tests.Features.GenericMeasures
{
    public class CreateGenericMeasureInfoCommandValidatorTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly CreateGenericMeasureInfoCommandValidator _sut;
        private readonly IUnitOfWorkFactory _uowFactory;

        public CreateGenericMeasureInfoCommandValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MeasuresContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbContextOptionsBuilder.Options, (options) => new MeasuresContext(options));
            _sut = new CreateGenericMeasureInfoCommandValidator(_uowFactory);
        }

        public static IEnumerable<object[]> ValidateCases
        {
            get
            {
                {
                    MeasureForm bloodPressureForm = new MeasureForm(Guid.NewGuid(), "blood-pressure");
                    bloodPressureForm.AddFloatField("systolic", "Systolic pressure", min: 0, max: 100, required: true);
                    bloodPressureForm.AddFloatField("diastolic", "Diastolic pressure", min: 0, max: 100, required: true);

                    Patient patient = new Patient(Guid.NewGuid(), "Bruce Wayne");

                    yield return new object[]
                    {
                        new []{ bloodPressureForm },
                        new []{ patient },
                        new CreateGenericMeasureInfoCommand(new CreateGenericMeasureInfo {
                            FormId = Guid.Empty,
                            DateOfMeasure = 13.July(2012),
                            PatientId = patient.Id,
                            Values = new Dictionary<string, object>
                            {
                                ["systolic"] = 13.4f,
                                ["diastolic"] = 8
                            }
                        }),
                        (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                            && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureInfoCommand.Data)}.{nameof(CreateGenericMeasureInfo.FormId)}"
                                && err.Severity == Error
                                && err.ErrorMessage == $"{nameof(CreateGenericMeasureInfoCommand.Data)}.{nameof(CreateGenericMeasureInfo.FormId)} cannot be empty"
                            )
                        ),
                        "FormID cannot be empty"
                    };
                }

                {
                    MeasureForm bloodPressureForm = new MeasureForm(Guid.NewGuid(), "blood-pressure");
                    bloodPressureForm.AddFloatField("systolic", "Systolic pressure", min: 0, max: 100, required: true);
                    bloodPressureForm.AddFloatField("diastolic", "Diastolic pressure", min: 0, max: 100, required: true);

                    Patient patient = new Patient(Guid.NewGuid(), "Bruce Wayne");

                    yield return new object[]
                    {
                        new []{ bloodPressureForm },
                        new []{ patient },
                        new CreateGenericMeasureInfoCommand(new CreateGenericMeasureInfo {
                            FormId = bloodPressureForm.Id,
                            DateOfMeasure = 13.December(2010).Add(15.Hours()),
                            PatientId = Guid.Empty,
                            Values = new Dictionary<string, object>
                            {
                                ["systolic"] = 13.4f,
                                ["diastolic"] = 8
                            }
                        }),
                        (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                            && vr.Errors.Once(err => err.PropertyName == "Data.PatientId")
                        ),
                        "PatientId cannot be empty"
                    };
                }

                {
                    MeasureForm bloodPressureForm = new MeasureForm(Guid.NewGuid(), "blood-pressure");
                    bloodPressureForm.AddFloatField("systolic", "Systolic pressure", min: 0, max: 100, required: true);
                    bloodPressureForm.AddFloatField("diastolic", "Diastolic pressure", min: 0, max: 100, required: true);

                    Patient patient = new Patient(Guid.NewGuid(), "Bruce Wayne");
                    yield return new object[]
                    {
                        new []{ bloodPressureForm },
                        new []{ patient },
                        new CreateGenericMeasureInfoCommand(new CreateGenericMeasureInfo {
                            FormId = bloodPressureForm.Id,
                            DateOfMeasure = 13.December(2010).Add(15.Hours()),
                            PatientId = patient.Id,
                            Values = new Dictionary<string, object>
                            {
                                ["systolic"] = 13.4f
                            }
                        }),
                        (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                            && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureInfoCommand.Data)}.{nameof(CreateGenericMeasureInfo.Values)}"
                                              && err.ErrorMessage == $"Missing one or more required values for form <{bloodPressureForm.Id}>"
                            )
                        ),
                        "'diastolic' field was not submitted"
                    };
                }

                {
                    MeasureForm bloodPressureForm = new MeasureForm(Guid.NewGuid(), "blood-pressure");
                    bloodPressureForm.AddFloatField("systolic", "Systolic pressure", min: 0, max: 100, required: true);
                    bloodPressureForm.AddFloatField("diastolic", "Diastolic pressure", min: 0, max: 100, required: true);
                    bloodPressureForm.AddTextField(name :"Comments" );

                    Patient patient = new Patient(Guid.NewGuid(), "Bruce Wayne");
                    yield return new object[]
                    {
                        new []{ bloodPressureForm },
                        new []{ patient },
                        new CreateGenericMeasureInfoCommand(new CreateGenericMeasureInfo {
                            FormId = bloodPressureForm.Id,
                            DateOfMeasure = 13.December(2010).Add(15.Hours()),
                            PatientId = patient.Id,
                            Values = new Dictionary<string, object>
                            {
                                ["systolic"] = 13.4f,
                                ["diastolic"] = 8f
                            }
                        }),
                        (Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid && vr.Errors.Exactly(1)
                            && vr.Errors.Once(err => err.PropertyName == $"{nameof(CreateGenericMeasureInfoCommand.Data)}.{nameof(CreateGenericMeasureInfo.Values)}"
                                              && err.ErrorMessage == $"Missing one or more non mandatory values for form <{bloodPressureForm.Id}>"
                                              && err.Severity == Warning
                            )
                        ),
                        "'Comments' field was not submitted but is not mandatory"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ValidateCases))]
        public async Task Validate(IEnumerable<MeasureForm> forms, IEnumerable<Patient> patients, CreateGenericMeasureInfoCommand cmd, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            _outputHelper.WriteLine($"Command : {cmd.Jsonify()}");

            // Arrange
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<MeasureForm>().Create(forms);
            uow.Repository<Patient>().Create(patients);

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);

            // Act
            ValidationResult validationResult = await _sut.ValidateAsync(cmd, default)
                                             .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Errors : {validationResult.Errors.Jsonify()}");

            validationResult.Should()
                            .Match(validationResultExpectation, reason);
        }
    }
}
