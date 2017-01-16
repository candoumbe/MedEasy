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

namespace MedEasy.BLL.Tests.Commands.Patient
{
    public class RunCreateDocumentForPatientCommandTests : IDisposable
    {
        private Mock<ILogger<RunCreateDocumentForPatientCommand>> _loggerMock;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private Mock<IMapper> _mapperMock;
        private RunCreateDocumentForPatientCommand _handler;
        private Mock<IValidate<ICreateDocumentForPatientCommand>> _validatorMock;
        private Mock<IExpressionBuilder> _expressionBuilderMock;
        private ITestOutputHelper _outputHelper;


        public RunCreateDocumentForPatientCommandTests(ITestOutputHelper output)
        {
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<RunCreateDocumentForPatientCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _validatorMock = new Mock<IValidate<ICreateDocumentForPatientCommand>>(Strict);
            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);

            _outputHelper = output;
            _handler = new RunCreateDocumentForPatientCommand(_validatorMock.Object, _loggerMock.Object,
                _unitOfWorkFactoryMock.Object,
               _expressionBuilderMock.Object);
        }

        public void Dispose()
        {
            _unitOfWorkFactoryMock = null;
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
                yield return new object[] {
                    new [] {
                        new Objects.Patient { Id = 1, Firstname = "Bruce", Lastname = "Wayne" }
                    },
                    new CreateDocumentForPatientInfo
                    {
                        PatientId = 1,
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
            IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            Action action = () => new RunCreateDocumentForPatientCommand(validator, logger, factory, expressionBuilder);

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
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Patient>().SingleOrDefaultAsync(It.IsNotNull<Expression<Func<Objects.Patient, bool>>>()))
                .Returns((Expression<Func<Objects.Patient, bool>> predicate) => Task.FromResult(patients.SingleOrDefault(predicate.Compile())));
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<DocumentMetadata>().Create(It.IsNotNull<DocumentMetadata>()))
                .Returns((DocumentMetadata documentMetadata) => 
                {

                    return documentMetadata;
                });

            _unitOfWorkFactoryMock.Setup(mock => mock.New().SaveChangesAsync()).ReturnsAsync(1);

            _validatorMock.Setup(mock => mock.Validate(It.IsAny<ICreateDocumentForPatientCommand>()))
                .Returns(Enumerable.Empty<Task<ErrorInfo>>());

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<CreatePatientInfo, Objects.Patient>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.CreateMapExpression<CreatePatientInfo, Objects.Patient>(parameters, membersToExpand));

            _expressionBuilderMock.Setup(mock => mock.CreateMapExpression<Objects.DocumentMetadata, DocumentMetadataInfo>(It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
               .Returns((IDictionary<string, object> parameters, MemberInfo[] membersToExpand) =>
                    AutoMapperConfig.Build().CreateMapper().ConfigurationProvider
                        .ExpressionBuilder.CreateMapExpression<Objects.DocumentMetadata, DocumentMetadataInfo>(parameters, membersToExpand));

            // Act
            CreateDocumentForPatientCommand cmd = new CreateDocumentForPatientCommand(input);
            DocumentMetadataInfo output = await _handler.RunAsync(cmd);

            // Assert
            output.Should().NotBeNull();
            output.MimeType.Should().Be(input.Document.MimeType);
            output.Title.Should().Be(input.Document.Title);
            output.Size.Should().Be(input.Document.Content.Length);
            output.PatientId.Should().Be(input.PatientId);
            
            _validatorMock.Verify(mock => mock.Validate(It.Is<ICreateDocumentForPatientCommand>(x => x.Id == cmd.Id)), Times.Once);
            _validatorMock.Verify(mock => mock.Validate(It.IsAny<ICreateDocumentForPatientCommand>()), Times.Once);
            _loggerMock.Verify(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.AtLeast(2));
        }


    }

}
