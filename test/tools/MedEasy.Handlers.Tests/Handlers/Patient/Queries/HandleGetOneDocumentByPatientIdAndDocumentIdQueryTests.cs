using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using AutoMapper;
using Xunit;
using FluentAssertions;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Mapping;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Queries.Patient;
using MedEasy.RestObjects;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MedEasy.Objects;
using MedEasy.Handlers.Core.Patient.Queries;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;
using System.Linq;
using static Newtonsoft.Json.JsonConvert;
using Newtonsoft.Json;
using Optional;

namespace MedEasy.Handlers.Tests.Patient.Queries
{
    public class HandleGetOneDocumentByPatientIdAndDocumentIdQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery _handler;

        private Mock<ILogger<HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery>> _loggerMock;
        private IMapper _mapper;

        public HandleGetOneDocumentByPatientIdAndDocumentIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            DbContextOptionsBuilder<MedEasyContext> dbOptions = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptions.UseInMemoryDatabase($"InMemoryMedEasyDb_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(dbOptions.Options);

            _loggerMock = new Mock<ILogger<HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _handler = new HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery(_unitOfWorkFactory, _loggerMock.Object, _mapper.ConfigurationProvider.ExpressionBuilder);
        }


        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    null,
                    null
                };

            }
        }




        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(ILogger<HandleGetDocumentsByPatientIdQuery> logger,
           IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Logger : {logger}");
            _outputHelper.WriteLine($"Unit of work factory : {factory}");
            _outputHelper.WriteLine($"expression builder : {expressionBuilder}");
            Action action = () => new HandleGetDocumentsByPatientIdQuery(factory, logger, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                IHandleGetDocumentsByPatientIdQuery handler = new HandleGetDocumentsByPatientIdQuery(
                    Mock.Of<IUnitOfWorkFactory>(),
                    Mock.Of<ILogger<HandleGetDocumentsByPatientIdQuery>>(),
                    Mock.Of<IExpressionBuilder>());

                await handler.HandleAsync(null);
            };

            action.ShouldThrow<ArgumentNullException>("because the query to handle is null")
                .And.ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        public static IEnumerable<object> GetOneDocumentByPatientIdAndDocumentIdCases
        {
            get
            {
                {
                    Guid patientId = Guid.NewGuid();
                    Guid documentId = Guid.NewGuid();
                    yield return new object[]
                    {
                        Enumerable.Empty<Objects.Patient>(),
                        new WantOneDocumentByPatientIdAndDocumentIdQuery(patientId, documentId),
                        ((Expression<Func<Option<DocumentMetadataInfo>, bool>>)(x => !x.HasValue))
                    };
                }
                {
                    Guid patientId = Guid.NewGuid();
                    Guid documentId = Guid.NewGuid();

                    yield return new object[]
                    {
                        new [] {
                            new Objects.Patient { Id = 1, Firstname = "Bruce", Lastname = "Wayne", UUID = patientId }
                        },
                    new WantOneDocumentByPatientIdAndDocumentIdQuery(patientId, documentId),
                    ((Expression<Func<Option<DocumentMetadataInfo>, bool>>)(x => !x.HasValue))
                    };
                }

                {
                    Guid patientId = Guid.NewGuid();
                    Guid documentId = Guid.NewGuid();

                    yield return new object[]
                    {

                    new [] {
                        new Objects.Patient {
                            Id = 1,
                            Firstname = "Bruce",
                            Lastname = "Wayne",
                            UUID = patientId,
                            Documents  = new []
                            {
                                new DocumentMetadata
                                {
                                    Id = 4,
                                    MimeType = "application/pdf",
                                    Title = "Secret weapon",
                                    Size = 512,
                                    Document = new Objects.Document {
                                        Content = new byte[512],
                                        UUID = Guid.NewGuid()
                                    },
                                    UUID = documentId
                                }
                            }

                        }
                    },
                    new WantOneDocumentByPatientIdAndDocumentIdQuery(patientId, documentId),
                    ((Expression<Func<Option<DocumentMetadataInfo>, bool>>)(x => x.HasValue && x.Exists( doc => doc.Id == documentId && doc.PatientId == patientId
                        && doc.MimeType == "application/pdf"
                        && doc.Title == "Secret weapon"
                        && doc.Size == 512
                    )))
                    };
                }
                {
                    Guid patientId = Guid.NewGuid();
                    Guid documentId = Guid.NewGuid();

                    yield return new object[]
                    {
                        new [] {
                            new Objects.Patient {
                                Id = 1,
                                Firstname = "Bruce",
                                Lastname = "Wayne",
                                UUID = patientId,
                                Documents  = new []
                                {
                                    new DocumentMetadata
                                    {
                                        MimeType = "application/pdf",
                                        Title = "Secret weapon",
                                        Size = 512,
                                        Document = new Objects.Document {
                                            Content = new byte[0],
                                            UUID = Guid.NewGuid()
                                        },
                                        UUID = Guid.NewGuid()
                                    }
                                }

                            }
                        },
                        new WantOneDocumentByPatientIdAndDocumentIdQuery(patientId, documentId),
                        ((Expression<Func<Option<DocumentMetadataInfo>, bool>>)(x => !x.HasValue))
                    };
                }

            }
        }

        [Theory]
        [MemberData(nameof(GetOneDocumentByPatientIdAndDocumentIdCases))]
        public async Task GetOneDocumentByPatientIdAndDocumentId(IEnumerable<Objects.Patient> patients, IWantOneDocumentByPatientIdAndDocumentIdQuery query, Expression<Func<Option<DocumentMetadataInfo>, bool>> resultExpectation)
        {
            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Patient>().Create(patients);
                await uow.SaveChangesAsync();
            }
            _outputHelper.WriteLine($"Patients  : {SerializeObject(patients, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
            
            // Act
            Option<DocumentMetadataInfo> documentMetadataInfo = await _handler.HandleAsync(query);

            // Assert
            _loggerMock.Verify();
            documentMetadataInfo.Should().Match(resultExpectation);

        }

        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _handler = null;
            _mapper = null;
        }
    }
}
