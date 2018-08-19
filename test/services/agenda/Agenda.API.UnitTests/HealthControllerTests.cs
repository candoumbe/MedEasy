using System;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using static Moq.MockBehavior;
using Moq;
using System.Threading;
using Agenda.Objects;
using Agenda.API.Resources;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;
using static Microsoft.AspNetCore.Http.StatusCodes;
using System.Collections.Generic;
using Xunit.Categories;

namespace Agenda.API.UnitTests
{
    [UnitTest]
    [Feature("Health")]
    public class HealthControllerTests : IDisposable
    {
        private Mock<IUnitOfWorkFactory> _uowFactoryMock;
        private HealthController _sut;

        public HealthControllerTests()
        {
            _uowFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _uowFactoryMock.Setup(mock => mock.NewUnitOfWork().Dispose());

            _sut = new HealthController(_uowFactoryMock.Object);
        }

        public void Dispose()
        {
            _uowFactoryMock = null;
            _sut = null;
        }

        [Fact]
        public async Task GivenNoError_Status_Returns_NoContent()
        {
            // Arrange
            _uowFactoryMock.Setup(mock => mock.NewUnitOfWork().Repository<Participant>().AnyAsync(It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<bool>(true));
            // Act
            IActionResult actionResult = await _sut.Status(ct : default)
                .ConfigureAwait(false);


            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<StatusCodeResult>().Which
                .StatusCode.Should()
                .Be(Status204NoContent);

        }

        public static IEnumerable<object[]> ServiceNotAvailableCases
        {
            get
            {
                Exception[] exceptions = {
                    new ArgumentNullException(),
                    new Exception(),
                    new IndexOutOfRangeException(),
                    new InvalidOperationException()
                };

                foreach (Exception exception in exceptions)
                {
                    yield return new object[] { exception };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ServiceNotAvailableCases))]
        public async Task GivenError_Status_Returns_InternalServerError(Exception exception)
        {
            // Arrange
            _uowFactoryMock.Setup(mock => mock.NewUnitOfWork().Repository<Participant>().AnyAsync(It.IsAny<CancellationToken>()))
                .Throws(exception);

            // Act
            IActionResult actionResult = await _sut.Status(ct: default)
                .ConfigureAwait(false);


            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<StatusCodeResult>().Which
                .StatusCode.Should()
                .BeGreaterOrEqualTo(Status500InternalServerError);

        }
    }
}
