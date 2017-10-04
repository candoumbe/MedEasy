using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MedEasy.API.Stores;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Validators.Patch;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FluentValidation.Severity;
using static Newtonsoft.Json.JsonConvert;
using MedEasy.Validators.Patient.Commands;

namespace MedEasy.Validators.Tests.Patient.Commands
{
    /// <summary>
    /// Unit tests collection for <see cref="PatchPatientInfoValidator"/> class.
    /// </summary>
    public class PatchPatientInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        private PatchPatientInfoValidator Validator { get; set; }

        public PatchPatientInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>()
                .UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(builder.Options);


            Validator = new PatchPatientInfoValidator(_unitOfWorkFactory);
        }

        public void Dispose()
        {
            _outputHelper = null;
            Validator = null;
            _unitOfWorkFactory = null;
        }


        public static IEnumerable<object[]> InvalidPatchInfoCases
        {
            get
            {
                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo>(),
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 2
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.Id) && x.Severity == Error)
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.Id)} and {nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} not set"
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo> { Id = Guid.Empty },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 2
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.Id) && x.Severity == Error)
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.Id)} == Guid.Empty and {nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} not set"
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo> { PatchDocument = null },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 2
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.Id) && x.Severity == Error)
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.Id)} and {nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} set to null"
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo> {Id = Guid.Empty, PatchDocument = null },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 2
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.Id) && x.Severity == Error)
                        && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.Id)} set to Guid.Empty and {nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} set to null"
                };

                yield return new object[]
                {
                    new PatchInfo<Guid, PatientInfo> {Id = Guid.NewGuid(), PatchDocument = new JsonPatchDocument<PatientInfo>() },
                    ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                        && vr.Errors.Count == 1
                        && vr.Errors.Once(x => x.ErrorMessage == "Operations must have at least one item." && x.Severity == Error)
                    )),
                    $"{nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} has zero operation."
                };

                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Id, Guid.Empty);

                    yield return new object[]
                    {
                        new PatchInfo<Guid, PatientInfo>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<ValidationResult, bool>>)(
                            vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                        )),
                        $"{nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} contains an operation which replace {nameof(PatientInfo.Id)} with Guid.Empty."
                    };
                }
                
                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Lastname, string.Empty);

                    yield return new object[]
                    {
                        new PatchInfo<Guid, PatientInfo>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                        )),
                        $"{nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} contains an operation which update {nameof(PatientInfo.Lastname)} to string.Empty"
                    };
                }

                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.MainDoctorId, Guid.Empty);

                    yield return new object[]
                    {
                        new PatchInfo<Guid, PatientInfo>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                        )),
                        $"{nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} contains an operation which update {nameof(PatientInfo.MainDoctorId)} to Guid.Empty"
                    };
                }

                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Fullname, string.Empty);

                    yield return new object[]
                    {
                        new PatchInfo<Guid, PatientInfo>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                        )),
                        $"{nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} contains an operation which update {nameof(PatientInfo.Fullname)}"
                    };
                }

                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Fullname, "  ");

                    yield return new object[]
                    {
                        new PatchInfo<Guid, PatientInfo>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = patchDocument
                        },
                        ((Expression<Func<ValidationResult, bool>>)(vr => !vr.IsValid
                            && vr.Errors.Count == 1
                            && vr.Errors.Once(x => x.PropertyName == nameof(PatchInfo<Guid, PatientInfo>.PatchDocument) && x.Severity == Error)
                        )),
                        $"{nameof(PatchInfo<Guid, PatientInfo>.PatchDocument)} contains an operation which update {nameof(PatientInfo.Fullname)}"
                    };
                }
            }
        }

        /// <summary>
        /// Tests that invalid <c><see cref="PatchInfo{TResourceId, TResource}.Id"/>s cases.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(InvalidPatchInfoCases))]
        public async Task Should_Fails(PatchInfo<Guid, PatientInfo> info, Expression<Func<ValidationResult, bool>> expectation, string because)
        {
            _outputHelper.WriteLine($"{nameof(info)} : {SerializeObject(info)}");

            // Act
            ValidationResult vr = await Validator.ValidateAsync(info);

            // Assert
            vr.Should().Match(expectation, because);
        }

        /// <summary>
        /// Tests that providing a <see cref="PatchInfo{Guid, PatientInfo}"/> with an unknown <see cref="PatchInfo{Guid, PatientInfo}"/>
        /// should not pass validation.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Fails_When_MainDoctorId_Is_Unknown_Resource()
        {

            // Arrange
            Guid patientId = Guid.NewGuid();
            Guid mainDoctorId = Guid.NewGuid();

            Objects.Patient patientToUpdate = new Objects.Patient
            {
                Firstname = string.Empty,
                Lastname = "STRANGE",
                UUID = patientId
            };

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Patient>().Create(patientToUpdate);
                await uow.SaveChangesAsync();
            }

            JsonPatchDocument<PatientInfo> patch = new JsonPatchDocument<PatientInfo>();
            patch.Replace(x => x.MainDoctorId, mainDoctorId);

            // Act
            PatchInfo<Guid, PatientInfo> info = new PatchInfo<Guid, PatientInfo>
            {
                Id = Guid.NewGuid(),
                PatchDocument = patch      
            };

            ValidationResult vr = await Validator.ValidateAsync(info);

            // Assert
            vr.IsValid.Should().BeFalse();
            vr.Errors.Should()
                .HaveCount(1).And
                .Contain(x => x.Severity == Error && x.ErrorMessage == $"{nameof(Objects.Doctor)} <{mainDoctorId}> not found.");

        }


        [Fact]
        public void Throws_ArgumentNullException_When_Ctor_Parameter_Is_Null()
        {
            // Act
            Action action = () => new PatchPatientInfoValidator(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        
    }
}
