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
using MedEasy.DTO;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using MedEasy.Objects;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;

namespace MedEasy.BLL.Tests.Commands.Patient
{
    public class RunCreateDocumentForPatientCommandTests : IDisposable
    {
        private Mock<ILogger<RunCreateDocumentForPatientCommand>> _loggerMock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private Mock<IMapper> _mapperMock;
        private RunCreateDocumentForPatientCommand _handler;
        private Mock<IValidate<ICreateDocumentForPatientCommand>> _validatorMock;
        private IMapper _mapper;
        private ITestOutputHelper _outputHelper;


        public RunCreateDocumentForPatientCommandTests(ITestOutputHelper output)
        {
            DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>()
                .UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(builder.Options);
            
            _loggerMock = new Mock<ILogger<RunCreateDocumentForPatientCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _validatorMock = new Mock<IValidate<ICreateDocumentForPatientCommand>>(Strict);
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _outputHelper = output;
            _handler = new RunCreateDocumentForPatientCommand(_validatorMock.Object, _loggerMock.Object,
                _unitOfWorkFactory,
               _mapper);
        }

        public void Dispose()
        {
            _unitOfWorkFactory = null;
            _loggerMock = null;
            _mapperMock = null;
            _outputHelper = null;
            _handler = null;

        }


        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    null,
                    null,
                    null
                };

            }
        }

        public static IEnumerable<object> ValidCreateDocumentForPatientCases
        {
            get
            {
                Guid patientId = Guid.NewGuid();
                yield return new object[] {
                    new [] {
                        new Objects.Patient { Id = 1, Firstname = "Bruce", Lastname = "Wayne", UUID = patientId }
                    },
                    new CreateDocumentForPatientInfo
                    {
                        PatientId = patientId,
                        Document = new CreateDocumentInfo
                        {
                            Title = "Doc 1",
                            MimeType = "application/pdf",
                            Content = new byte[] {1, 2, 3, 4, 5}
                        }
                    }
                };


            }
        }


        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IValidate<ICreateDocumentForPatientCommand> validator, ILogger<RunCreateDocumentForPatientCommand> logger,
            IUnitOfWorkFactory factory, IMapper mapper)
        {
            Action action = () => new RunCreateDocumentForPatientCommand(validator, logger, factory, mapper);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Theory]
        [MemberData(nameof(ValidCreateDocumentForPatientCases))]
        public async Task ShouldCreateResource(IEnumerable<Objects.Patient> patients, CreateDocumentForPatientInfo input)
        {
            _outputHelper.WriteLine($"input : {input}");

            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Patient>().Create(patients);

                await uow.SaveChangesAsync();
            }

            
            _validatorMock.Setup(mock => mock.Validate(It.IsAny<ICreateDocumentForPatientCommand>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>());

            
            // Act
            CreateDocumentForPatientCommand cmd = new CreateDocumentForPatientCommand(input);
            DocumentMetadataInfo output = await _handler.RunAsync(cmd);

            // Assert
            output.Should().NotBeNull();
            output.MimeType.Should().Be(input.Document.MimeType);
            output.Title.Should().Be(input.Document.Title);
            output.Size.Should().Be(input.Document.Content.Length);
            output.PatientId.Should().Be(input.PatientId);
            output.Id.Should().NotBeEmpty();

            _validatorMock.Verify(mock => mock.Validate(It.Is<ICreateDocumentForPatientCommand>(x => x.Id == cmd.Id)), Times.Once);
            _validatorMock.Verify(mock => mock.Validate(It.IsAny<ICreateDocumentForPatientCommand>()), Times.Once);
            _loggerMock.Verify(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeast(2));
        }


    }

}
