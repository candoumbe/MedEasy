using Agenda.DTO;
using Agenda.Objects;
using FluentAssertions;
using FluentAssertions.Extensions;
using FluentValidation.Results;
using MedEasy.Abstractions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using Microsoft.AspNetCore.JsonPatch;
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

namespace Agenda.Validators.UnitTests
{
    [UnitTest]
    [Feature("Validators")]
    [Feature("Agenda")]
    public class PatchAppointmentInfoValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private Mock<IDateTimeService> _datetimeServiceMock;
        private PatchAppointmentInfoValidator _sut;

        public PatchAppointmentInfoValidatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.NewUnitOfWork().Dispose());

            _datetimeServiceMock = new Mock<IDateTimeService>(Strict);

            _sut = new PatchAppointmentInfoValidator(_datetimeServiceMock.Object, _unitOfWorkFactoryMock.Object);
        }

        public void Dispose()
        {
            _unitOfWorkFactoryMock = null;
            _datetimeServiceMock = null;
            _sut = null;
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                yield return new object[] { null, null };
                yield return new object[] { null, Mock.Of<IDateTimeService>() };
                yield return new object[] { Mock.Of<IUnitOfWorkFactory>(), null };
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Throws_ArgumentNullException_WhenParameterIsNull(IUnitOfWorkFactory uowFactory, IDateTimeService dateTimeService)
        {

            _outputHelper.WriteLine($"Datetime service is null : {dateTimeService == null}");
            _outputHelper.WriteLine($"uowFactory service is null : {uowFactory == null}");

            // Act
            Action action = () => new PatchAppointmentInfoValidator(dateTimeService, uowFactory);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> InvalidPatchCases
        {
            get
            {
                {
                    Guid appointmentId = Guid.NewGuid();
                    yield return new object[]
                    {
                        Enumerable.Empty<Appointment>(),
                        new PatchInfo<Guid, AppointmentInfo>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = new JsonPatchDocument<AppointmentInfo>()
                                .Test(x => x.Id, appointmentId)
                                .Replace(x => x.StartDate, 8.March(2019).Add(14.Hours().Add(30.Minutes())))
                                .Replace(x => x.EndDate, 8.March(2019).Add(14.Hours()))
                         },
                        (Expression<Func<IEnumerable<ValidationFailure>, bool>>)(errors => errors.Once()
                            && errors.Once(err => err.PropertyName == nameof(PatchInfo<Guid, AppointmentInfo>.PatchDocument)
                                && err.Severity == Error
                                && $"{nameof(AppointmentInfo.StartDate)} cannot be greater than {nameof(AppointmentInfo.EndDate)}".Equals(err.ErrorMessage)
                            )
                        ),
                        $"The changes contains {nameof(AppointmentInfo.StartDate)} that greater than {nameof(AppointmentInfo.EndDate)}"
                    };
                }

                {
                    Guid appointmentId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new[]
                        {
                            new Appointment(
                                id:  appointmentId,
                                startDate: 12.January(2019).At(10.Hours()),
                                endDate : 12.January(2019).At(10.Hours().And(30.Minutes())),
                                subject: string.Empty,
                                location: string.Empty)
                        },
                        new PatchInfo<Guid, AppointmentInfo>
                        {
                            Id = Guid.NewGuid(),
                            PatchDocument = new JsonPatchDocument<AppointmentInfo>()
                                .Test(x => x.Id, appointmentId)
                                .Replace(x => x.StartDate, 8.March(2019).Add(14.Hours().Add(30.Minutes())))
                                .Replace(x => x.EndDate, 8.March(2019).Add(14.Hours()))
                         },
                        (Expression<Func<IEnumerable<ValidationFailure>, bool>>)(errors => errors.Once()
                            && errors.Once(err => err.PropertyName == nameof(PatchInfo<Guid, AppointmentInfo>.PatchDocument)
                                && err.Severity == Error
                                && $"{nameof(AppointmentInfo.StartDate)} cannot be greater than {nameof(AppointmentInfo.EndDate)}".Equals(err.ErrorMessage)
                            )
                        ),
                        $"Patch document contains operations that set a {nameof(AppointmentInfo.StartDate)} > {nameof(AppointmentInfo.EndDate)}"
                    };
                }

                {
                    Guid appointmentId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new[]
                        {
                            new Appointment(
                                id: appointmentId,
                                startDate : 12.January(2019).At(10.Hours()),
                                endDate : 12.January(2019).At(10.Hours().And(30.Minutes())),
                                location: "Somewhere in metropolis",
                                subject: "unknown")
                        },
                        new PatchInfo<Guid, AppointmentInfo>
                        {
                            Id = appointmentId,
                            PatchDocument = new JsonPatchDocument<AppointmentInfo>()
                                .Test(x => x.Id, appointmentId)
                                .Replace(x => x.StartDate, 12.January(2019).Add(11.Hours()))
                         },
                        (Expression<Func<IEnumerable<ValidationFailure>, bool>>)(errors => errors.Once()
                            && errors.Once(err => err.PropertyName == nameof(PatchInfo<Guid, AppointmentInfo>.PatchDocument)
                                && err.Severity == Error
                                && $"{nameof(AppointmentInfo.StartDate)} cannot be greater than {nameof(AppointmentInfo.EndDate)}".Equals(err.ErrorMessage)
                            )
                        ),
                        $"Patch document contains an operation on {nameof(AppointmentInfo.StartDate)} that will cause it to be greater than the registered {nameof(AppointmentInfo.EndDate)}"
                    };
                }

                {
                    Guid appointmentId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new[]
                        {
                            new Appointment
                            (
                                id: appointmentId,
                                startDate: 12.January(2019).Add(10.Hours()),
                                endDate: 12.January(2019).Add(10.Hours().Add(30.Minutes())),
                                subject: string.Empty,
                                location: string.Empty)
                        },
                        new PatchInfo<Guid, AppointmentInfo>
                        {
                            Id = appointmentId,
                            PatchDocument = new JsonPatchDocument<AppointmentInfo>()
                                .Test(x => x.Id, appointmentId)
                                .Replace(x => x.EndDate, 12.January(2019).Add(11.Hours()))
                         },
                        (Expression<Func<IEnumerable<ValidationFailure>, bool>>)(errors => !errors.Any()
                        ),
                        $"Patch document contains an operation on {nameof(AppointmentInfo.EndDate)} that will still be greater than the registered {nameof(AppointmentInfo.StartDate)}"
                    };
                }

                {
                    Guid appointmentId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new[]
                        {
                             new Appointment
                            (
                                id: appointmentId,
                                startDate: 12.January(2019).Add(10.Hours()),
                                endDate: 12.January(2019).Add(10.Hours().Add(30.Minutes())),
                                subject: string.Empty,
                                location: string.Empty)
                        },
                        new PatchInfo<Guid, AppointmentInfo>
                        {
                            Id = appointmentId,
                            PatchDocument = new JsonPatchDocument<AppointmentInfo>()
                                .Test(x => x.Id, appointmentId)
                                .Replace(x => x.StartDate, 12.January(2019).Add(10.Hours().Add(15.Minutes())))
                        },
                        (Expression<Func<IEnumerable<ValidationFailure>, bool>>)(errors => !errors.Any()),
                        $"Patch document contains an operation on {nameof(AppointmentInfo.StartDate)} that stay lower than registered {nameof(AppointmentInfo.EndDate)}"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(InvalidPatchCases))]
        public async Task ValidatePatch(IEnumerable<Appointment> appointments, PatchInfo<Guid, AppointmentInfo> patch, Expression<Func<IEnumerable<ValidationFailure>, bool>> errorsExpectation, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"Appointments : {appointments.Stringify()}");
            _outputHelper.WriteLine($"Changes : {patch.Stringify()}");

            int callCount= 0;

            _unitOfWorkFactoryMock.Setup(mock => mock.NewUnitOfWork().Repository<Appointment>().AnyAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()))
                .Callback(() => callCount++)
                .Returns((Expression<Func<Appointment, bool>> filter, CancellationToken ct) => new ValueTask<bool>(appointments.Any(filter.Compile())));

            // Act
            ValidationResult validationResult = await _sut.ValidateAsync(patch)
                .ConfigureAwait(false);

            _outputHelper.WriteLine($"Errors : {validationResult.Errors.Stringify()}");

            // Assert
            validationResult.Errors.Should()
                .Match(errorsExpectation, reason);

            _unitOfWorkFactoryMock.Verify(mock => mock.NewUnitOfWork().Repository<Appointment>().AnyAsync(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(callCount));
            _unitOfWorkFactoryMock.Verify(mock => mock.NewUnitOfWork().Dispose(), Times.Exactly(callCount));
            _unitOfWorkFactoryMock.VerifyNoOtherCalls();
        }
    }
}
