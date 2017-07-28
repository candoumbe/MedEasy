using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using Xunit;
using AutoMapper;
using FluentAssertions;
using MedEasy.Validators;
using MedEasy.Commands.Patient;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.Mapping;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using MedEasy.Handlers.Core.Exceptions;
using Optional;
using MedEasy.CQRS.Core;

namespace MedEasy.Handlers.Tests.Commands.Patient
{
    public class HandleDeletePatientByIdCommandTests : IDisposable
    {
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private RunDeletePatientByIdCommand _handler;
        private ITestOutputHelper _outputHelper;

        public HandleDeletePatientByIdCommandTests(ITestOutputHelper outputHelper)
        {
            IMapper mapper = AutoMapperConfig.Build().CreateMapper();
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());
            
            _outputHelper = outputHelper;

            _handler = new RunDeletePatientByIdCommand(_unitOfWorkFactoryMock.Object);
        }


        [Fact]
        public void Ctor_With_Null_Parameter_Throws_ArgumentNullException()
        {
            
            // Act
            Action action = () => new RunDeletePatientByIdCommand(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Delete()
        {
            // Arrange 
            
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().Delete(It.IsAny<Expression<Func<Objects.Patient, bool>>>()));
            _unitOfWorkFactoryMock.Setup(mock => mock.New().SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            Option<Nothing, CommandException> result = await _handler.RunAsync(new DeletePatientByIdCommand(Guid.NewGuid()));

            // Assert
            _unitOfWorkFactoryMock.Verify();
            
        }

        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactoryMock = null;
            _handler = null;
        }
    }
}
