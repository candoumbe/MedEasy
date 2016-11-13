using AutoMapper;
using FluentAssertions;
using GenFu;
using MedEasy.API;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Exceptions;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.Queries;
using MedEasy.RestObjects;
using MedEasy.Services;
using MedEasy.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.DAL.Repositories.SortDirection;
using static MedEasy.Validators.ErrorLevel;
using static Moq.MockBehavior;
using static Moq.Times;
using static System.StringComparison;

namespace MedEasy.WebApi.Tests
{
    public class PatientsControllerTests : IDisposable
    {
        private Mock<IUrlHelperFactory> _urlHelperFactoryMock;
        private Mock<ILogger<PatientsController>> _loggerMock;
        private PatientsController _controller;
        private ITestOutputHelper _outputHelper;
        private IActionContextAccessor _actionContextAccessor;
        private EFUnitOfWorkFactory _factory;
        private IMapper _mapper;
        private Mock<IHandleGetOnePatientInfoByIdQuery> _iHandleGetOnePatientInfoByIdQueryMock;
        private Mock<IHandleGetManyPatientInfosQuery> _iHandleGetManyPatientInfoQueryMock;
        private Mock<IRunCreatePatientCommand> _iRunCreatePatientInfoCommandMock;
        private Mock<IRunDeletePatientByIdCommand> _iRunDeletePatientInfoByIdCommandMock;
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;
        private Mock<IPhysiologicalMeasureService> _physiologicalMeasureFacadeMock;
        private Mock<IPrescriptionService> _prescriptionServiceMock;
        private Mock<IRunPatchPatientCommand> _iRunPatchPatientCommandMock;
        

        public PatientsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<PatientsController>>(Strict);
            _urlHelperFactoryMock = new Mock<IUrlHelperFactory>(Strict);
            _urlHelperFactoryMock.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>()).Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}");

            _actionContextAccessor = new ActionContextAccessor()
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            DbContextOptionsBuilder<MedEasyContext> dbOptions = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptions.UseInMemoryDatabase($"InMemoryMedEasyDb_{Guid.NewGuid()}");
            _factory = new EFUnitOfWorkFactory(dbOptions.Options);
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _iHandleGetOnePatientInfoByIdQueryMock = new Mock<IHandleGetOnePatientInfoByIdQuery>(Strict);
            _iHandleGetManyPatientInfoQueryMock = new Mock<IHandleGetManyPatientInfosQuery>(Strict);
            _iRunCreatePatientInfoCommandMock = new Mock<IRunCreatePatientCommand>(Strict);
            _iRunPatchPatientCommandMock = new Mock<IRunPatchPatientCommand>(Strict);
            _iRunDeletePatientInfoByIdCommandMock = new Mock<IRunDeletePatientByIdCommand>(Strict);
            _physiologicalMeasureFacadeMock = new Mock<IPhysiologicalMeasureService>(Strict);

            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);
            _prescriptionServiceMock = new Mock<IPrescriptionService>(Strict);

            _controller = new PatientsController(
                _loggerMock.Object, 
                _urlHelperFactoryMock.Object, 
                _actionContextAccessor,
                _apiOptionsMock.Object,
                _iHandleGetOnePatientInfoByIdQueryMock.Object,
                _iHandleGetManyPatientInfoQueryMock.Object,
                _iRunCreatePatientInfoCommandMock.Object,
                _iRunDeletePatientInfoByIdCommandMock.Object,
                _physiologicalMeasureFacadeMock.Object,
                _prescriptionServiceMock.Object,
                _iRunPatchPatientCommandMock.Object);

        }

        public void Dispose()
        {
            _loggerMock = null;
            _urlHelperFactoryMock = null;
            _controller = null;
            _outputHelper = null;
            _actionContextAccessor = null;
            _apiOptionsMock = null;

            _iHandleGetOnePatientInfoByIdQueryMock = null;
            _iHandleGetManyPatientInfoQueryMock = null;
            _physiologicalMeasureFacadeMock = null;

            _iRunCreatePatientInfoCommandMock = null;
            _iRunDeletePatientInfoByIdCommandMock = null;
            _iRunPatchPatientCommandMock = null;

            _prescriptionServiceMock = null;
            _factory = null;
            _mapper = null;
        }

        public static IEnumerable<object> GetAllTestCases
        {
            get
            {
                int[] pageSizes = { 0, int.MinValue, int.MaxValue };
                int[] pages = { 0, int.MinValue, int.MaxValue };


                foreach (var pageSize in pageSizes)
                {
                    foreach (var page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<Patient>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            ((Expression<Func<Link, bool>>) (x => x == null))  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<Patient> items = A.ListOf<Patient>(400);
                    items.ForEach(item => item.Id = default(int));
                    yield return new object[]
                    {
                        items,
                        GenericGetQuery.DefaultPageSize, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "next" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=14".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }
                {
                    IEnumerable<Patient> items = A.ListOf<Patient>(400);
                    items.ForEach(item => item.Id = default(int));

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize=10&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "next" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize=10&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize=10&page=40".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }

                yield return new object[]
                    {
                        new [] {
                            new Patient { Id = 1, Firstname = "Bruce",  Lastname = "Wayne" }
                        },
                        GenericGetQuery.DefaultPageSize, 1, // request
                        1,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
                    };
            }
        }

        public static IEnumerable<object> GetLastBloodPressuresMesuresCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<BloodPressure>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new BloodPressure { PatientId = 2, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new BloodPressure { PatientId = 1, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => x.All(measure => measure.PatientId == 1) && x.Count() == 1))
                };
            }
        }

        public static IEnumerable<object> GetMostRecentTemperaturesMeasuresCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Temperature>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Temperature { PatientId = 2, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Temperature { PatientId = 1, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => x.All(measure => measure.PatientId == 1) && x.Count() == 1))
                };
            }
        }

        public static IEnumerable<object> GetMostRecentPrescriptionsCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Prescription>(),
                    new GetMostRecentPrescriptionsInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<PrescriptionHeaderInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Prescription { PatientId = 2, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPrescriptionsInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<PrescriptionHeaderInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Prescription { PatientId = 1, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPrescriptionsInfo { PatientId = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<PrescriptionHeaderInfo>, bool>>) (x => x.All(prescription => prescription.PatientId == 1) && x.Count() == 1))
                };
            }
        }


        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Patient> items, int pageSize, int page,
            int expectedCount,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.GetAll)}({nameof(GenericGetQuery)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (var uow = _factory.New())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync();
            }

            _iHandleGetManyPatientInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, PatientInfo>>()))
                .Returns((IWantManyResources<Guid, PatientInfo> getQuery) => Task.Run(async () =>
                {


                    using (var uow = _factory.New())
                    {
                        GenericGetQuery queryConfig = getQuery.Data ?? new GenericGetQuery();

                        IPagedResult<PatientInfo> results = await uow.Repository<Patient>()
                            .ReadPageAsync(x => _mapper.Map<PatientInfo>(x), getQuery.Data.PageSize, getQuery.Data.Page);

                        return results;
                    }
                }));
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });
            
            // Act
            IActionResult actionResult = await _controller.Get(page, pageSize);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(PatientsController)}.{nameof(PatientsController.GetAll)} must always check that {nameof(GenericGetQuery.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");

            actionResult.Should()
                    .NotBeNull()
                    .And.BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull()
                    .And.BeOfType<GenericPagedGetResponse<PatientInfo>>();

            GenericPagedGetResponse<PatientInfo> response = (GenericPagedGetResponse<PatientInfo>)value;

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPagedGetResponse<PatientInfo>)}.{nameof(GenericPagedGetResponse<PatientInfo>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);
            
        }


        [Fact]
        public async Task Patch()
        {

            // Arrange
            _iRunPatchPatientCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IPatchCommand<int>>()))
                .Returns(Task.FromResult(new PatientInfo()));


            // Act
            IEnumerable<ChangeInfo> changes = new[]
            {
                new ChangeInfo { Op = ChangeInfoType.Update, Path = nameof(PatientInfo.BirthDate), From =  null, Value = "Paris 13e" }
            };
            IActionResult actionResult = await _controller.Patch(1,  changes);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<OkObjectResult>().Which
                    .Value.Should().BeOfType<PatientInfo>();

            _iRunPatchPatientCommandMock.Verify();
            
        }


        //[Theory]
        //[InlineData(0, 0)]
        //[InlineData(0, null)]
        //[InlineData(0, -1)]
        //[InlineData(-1, -1)]
        //[InlineData(-1, 0)]
        //public async Task PatchMainDoctorWithInvalidInputsShouldReturnBadRequest(int id, int? newDoctorId)
        //{

        //    // Arrange
        //    IActionResult actionResult = await _controller.Patch(id, newDoctorId);

        //    // Assert

        //    _iRunChangeMainDoctorCommandMock.Verify(mock => mock.RunAsync(It.IsAny<IChangeMainDoctorCommand>()), Never, "the controller must validate arguments by itself");

        //    actionResult.Should()
        //        .NotBeNull().And
        //        .BeAssignableTo<BadRequestObjectResult>().Which
        //            .Value.Should()
        //                .NotBeNull().And
        //                .BeAssignableTo<IEnumerable<ErrorInfo>>().Which.Should()
        //                    .NotBeNullOrEmpty().And
        //                    .NotContainNulls().And
        //                    .OnlyContain(x => x.Severity == Error);
        //}


        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            //Arrange
            _iHandleGetOnePatientInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, int, PatientInfo>>()))
                .ReturnsAsync(null);

            //Act
            IActionResult actionResult = await _controller.Get(1);

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<NotFoundResult>().Which
                    .StatusCode.Should().Be(404);

        }

        [Fact]
        public async Task Get()
        {
            //Arrange
            _urlHelperFactoryMock.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>()).Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}");

            _iHandleGetOnePatientInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, int, PatientInfo>>()))
                .ReturnsAsync(new PatientInfo { Id = 1, Firstname = "Bruce", Lastname = "Wayne" })
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Get(1);

            //Assert

            IBrowsableResource<PatientInfo> result = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<IBrowsableResource<PatientInfo>>().Which;

            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .Contain(x => x.Rel == "self");

            Link location = links.Single(x => x.Rel == "self");
            location.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?{nameof(PatientInfo.Id)}=1");
            location.Rel.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo("self");

            PatientInfo resource = result.Resource;
            resource.Should().NotBeNull();
            resource.Id.Should().Be(1);
            resource.Firstname.Should().Be("Bruce");
            resource.Lastname.Should().Be("Wayne");

            _iHandleGetOnePatientInfoByIdQueryMock.Verify();
            _urlHelperFactoryMock.Verify();

        }

        [Fact]
        public async Task Post()
        {
            //Arrange
            _iRunCreatePatientInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreatePatientCommand>()))
                .Returns((ICreatePatientCommand cmd) => Task.Run(()
                => new PatientInfo {
                    Id = 3,
                    Firstname = cmd.Data.Firstname,
                    Lastname = cmd.Data.Lastname,
                    UpdatedDate = new DateTimeOffset(2012, 2, 1, 0, 0, 0, TimeSpan.Zero) }));

            //Act
            CreatePatientInfo info = new CreatePatientInfo
            {
                Firstname = "Bruce",
                Lastname = "Lastname"
            };

            IActionResult actionResult = await _controller.Post(info);

            //Assert
            CreatedAtActionResult createdActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;

            PatientInfo createdResource = createdActionResult.Value.Should().NotBeNull().And
                .BeOfType<PatientInfo>().Which;

            createdResource.Should()
                .NotBeNull();

            createdResource.Should()
                .NotBeNull();

            
            createdResource.Firstname.Should()
                .Be(info.Firstname);
            createdResource.Lastname.Should()
                .Be(info.Lastname);

            createdResource.UpdatedDate.Should().HaveDay(1);
            createdResource.UpdatedDate.Should().HaveMonth(2);
            createdResource.UpdatedDate.Should().HaveYear(2012);

            _iRunCreatePatientInfoCommandMock.Verify(mock => mock.RunAsync(It.IsAny<ICreatePatientCommand>()), Times.Once);

        }

        [Fact]
        public void PostShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new CommandNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrDuplicateCode", "A specialty with the same code already exists", ErrorLevel.Error)
                });

            //Arrange

            _iRunCreatePatientInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreatePatientCommand>()))
                .Throws(exceptionFromTheHandler);

            //Act
            CreatePatientInfo info = new CreatePatientInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne"
            };

            Func<Task> action = async () => await _controller.Post(info);

            //Assert
            action.ShouldThrow<CommandNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iRunCreatePatientInfoCommandMock.Verify();

        }


        
        [Fact]
        public async Task ShouldReturnNotFoundWhenNoResourceFound()
        {
            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.GetOnePrescriptionByPatientIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(null);

            // Act
            int patientId = 1,
                prescriptionId = 12;
            IActionResult actionResult = await _controller.Prescriptions(patientId, prescriptionId);

            // Assert
            actionResult.Should().NotBeNull().And
                .BeOfType<NotFoundResult>();

            _prescriptionServiceMock.Verify(mock => mock.GetOnePrescriptionByPatientIdAsync(patientId, prescriptionId), Times.Once);
        }


        [Fact]
        public async Task ShouldReturnThePrescriptionHeaderResource()
        {
            // Arrange
            PrescriptionHeaderInfo expectedOutput = new PrescriptionHeaderInfo
            {
                Id = 1,
                PatientId = 12,
                PrescriptorId = 10,
                DeliveryDate = new DateTimeOffset(1983, 6, 23, 0,0, 0, TimeSpan.Zero)
            };
            _prescriptionServiceMock.Setup(mock => mock.GetOnePrescriptionByPatientIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(expectedOutput);

            // Act
            int patientId = 1,
                prescriptionId = 12;
            IActionResult actionResult = await _controller.Prescriptions(patientId, prescriptionId);

            // Assert
            IBrowsableResource<PrescriptionHeaderInfo> actualResource = actionResult.Should().NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IBrowsableResource<PrescriptionHeaderInfo>>().Which;


            actualResource.Should().NotBeNull();
            IEnumerable<Link> links = actualResource.Links;

            links.Should()
                .NotBeNull().And
                .Contain(x => x.Rel == "self");

            Link location = links.Single(x => x.Rel == "self");
            location.Should().NotBeNull();
            location.Rel.Should().Be("self");
            location.Href.Should().Be($"api/{PatientsController.EndpointName}/{nameof(PatientsController.Prescriptions)}?id={expectedOutput.PatientId}&prescriptionId={expectedOutput.Id}");

            actualResource.Resource.Should().NotBeNull();
            actualResource.Resource.PatientId.Should().Be(expectedOutput.PatientId);
            actualResource.Resource.Id.Should().Be(expectedOutput.Id);
            actualResource.Resource.PrescriptorId.Should().Be(expectedOutput.PrescriptorId);
            actualResource.Resource.DeliveryDate.Should().Be(expectedOutput.DeliveryDate);

            _prescriptionServiceMock.Verify(mock => mock.GetOnePrescriptionByPatientIdAsync(patientId, prescriptionId), Times.Once);
            _urlHelperFactoryMock.Verify();

        }

        [Fact]
        public void GetShouldNotSwallowQueryNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _iHandleGetOnePatientInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, int, PatientInfo>>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.Get(1);

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iHandleGetOnePatientInfoByIdQueryMock.Verify();

        }

        [Fact]
        public void GetAllShouldNotSwallowQueryNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 20, MaxPageSize = 200 });

            _iHandleGetManyPatientInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, PatientInfo>>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();

            //Act
            Func<Task> action = async () => await _controller.GetAll(new GenericGetQuery());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iHandleGetManyPatientInfoQueryMock.Verify();

        }

        [Fact]
        public void DeleteShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _iRunDeletePatientInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeletePatientByIdCommand>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.Delete(1);

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iHandleGetManyPatientInfoQueryMock.Verify();
        }

        [Fact]
        public async Task DeleteMustRelyOnDeleteCommandHandler()
        {

            //Arrange
            _iRunDeletePatientInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeletePatientByIdCommand>()))
                .Returns(Task.CompletedTask)
                .Verifiable();


            //Act
            await _controller.Delete(1);

            //Assert
            _iRunDeletePatientInfoByIdCommandMock.Verify();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeletePatientByNegativeOrZeroReturnsBadRequest(int idToDelete)
        {

            //Arrange
            
            //Act
            IActionResult actionResult = await _controller.Delete(idToDelete);

            //Assert
            actionResult.Should().BeAssignableTo<BadRequestResult>();
        }



        [Fact]
        public async Task AddTemperatureMeasure()
        {
            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.AddNewMeasureAsync<Temperature, TemperatureInfo>(It.IsAny<ICommand<Guid, Temperature>>()))
                .Returns((ICommand<Guid, Temperature> localCmd) => Task.FromResult(new TemperatureInfo {
                    Id = 1,
                    DateOfMeasure = localCmd.Data.DateOfMeasure,
                    PatientId = localCmd.Data.PatientId,
                    Value = localCmd.Data.Value
                }));

            // Act
            CreateTemperatureInfo input = new CreateTemperatureInfo
            {
                Value = 50,
                DateOfMeasure = DateTimeOffset.UtcNow
            };

            IActionResult actionResult = await _controller.Temperatures(1, input);

            // Assert
            CreatedAtActionResult createdAtActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;

            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.Temperatures));
            createdAtActionResult.RouteValues.Should()
                .HaveCount(2).And
                .ContainKey("id").WhichValue.Should().Be(1);
            createdAtActionResult.RouteValues.Should()
                .ContainKey("temperatureId").WhichValue.Should().Be(1);


            TemperatureInfo resource = createdAtActionResult.Value.Should()
                .BeOfType<TemperatureInfo>().Which;
            
            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(1);
            resource.Value.Should().Be(input.Value);

            _physiologicalMeasureFacadeMock.VerifyAll();
            
        }

        [Fact]
        public async Task AddBloodPressureMeasure()
        {
            CreateBloodPressureInfo input = new CreateBloodPressureInfo
            {
                SystolicPressure = 150,
                DiastolicPressure = 100,
                DateOfMeasure = DateTimeOffset.UtcNow
            };

            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.AddNewMeasureAsync<BloodPressure, BloodPressureInfo>(It.IsAny<ICommand<Guid, BloodPressure>>()))
                .Returns((ICommand<Guid, BloodPressure> localCmd) => Task.FromResult(new BloodPressureInfo
                {
                    Id = 1,
                    DateOfMeasure = localCmd.Data.DateOfMeasure,
                    PatientId = localCmd.Data.PatientId,
                    SystolicPressure = localCmd.Data.SystolicPressure,
                    DiastolicPressure = localCmd.Data.DiastolicPressure,

                }));

            // Act
            

            IActionResult actionResult = await _controller.BloodPressures(1, input);

            // Assert
            CreatedAtActionResult createdAtActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;
            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.BloodPressures));
            createdAtActionResult.RouteValues.Should()
                .HaveCount(2).And
                .ContainKey("id").WhichValue.Should().Be(1);
            createdAtActionResult.RouteValues.Should()
                .ContainKey("bloodPressureId").WhichValue.Should().Be(1);

            BloodPressureInfo resource = createdAtActionResult.Value.Should().BeOfType<BloodPressureInfo>().Which;

            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(1);
            resource.SystolicPressure.Should().Be(input.SystolicPressure);
            resource.DiastolicPressure.Should().Be(input.DiastolicPressure);
            
           
            _physiologicalMeasureFacadeMock.VerifyAll();

        }

        [Fact]
        public async Task AddBodyWeightMeasure()
        {
            CreateBodyWeightInfo input = new CreateBodyWeightInfo
            {
                Value = 94.6m,
                DateOfMeasure = DateTimeOffset.UtcNow
            };

            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.AddNewMeasureAsync<BodyWeight, BodyWeightInfo>(It.IsAny<ICommand<Guid, BodyWeight>>()))
                .Returns((ICommand<Guid, BodyWeight> localCmd) => Task.FromResult(new BodyWeightInfo
                {
                    Id = 1,
                    DateOfMeasure = localCmd.Data.DateOfMeasure,
                    PatientId = localCmd.Data.PatientId,
                    Value = localCmd.Data.Value
                }));

            // Act


            IActionResult actionResult = await _controller.BodyWeights(1, input);

            // Assert
            CreatedAtActionResult createdAtActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;
            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.BodyWeights));
            createdAtActionResult.RouteValues.Should()
                .HaveCount(2).And
                .ContainKey("id").WhichValue.Should().Be(1);
            createdAtActionResult.RouteValues.Should()
                .ContainKey("bodyWeightId").WhichValue.Should().Be(1);

            BodyWeightInfo resource = createdAtActionResult.Value.Should().BeOfType<BodyWeightInfo>().Which;

            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(1);
            resource.Value.Should().Be(input.Value);


            _physiologicalMeasureFacadeMock.VerifyAll();

        }

        [Theory]
        [MemberData(nameof(GetLastBloodPressuresMesuresCases))]
        public async Task GetLastBloodPressuresMesures(IEnumerable<BloodPressure> measuresInStore, GetMostRecentPhysiologicalMeasuresInfo query, Expression<Func<IEnumerable<BloodPressureInfo>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"Current store state : {measuresInStore}");
            _outputHelper.WriteLine($"Query : {query}");

            // Arrange
            using (var uow =_factory.New())
            {
                uow.Repository<BloodPressure>().Create(measuresInStore);
                await uow.SaveChangesAsync();
            }

            _physiologicalMeasureFacadeMock.Setup(mock => mock.GetMostRecentMeasuresAsync<BloodPressure, BloodPressureInfo>(It.IsAny<IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<BloodPressureInfo>>>()))
                .Returns((IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<BloodPressureInfo>> input) => Task.Run(async () =>
                {
                    using (var uow = _factory.New())
                    {
                        IPagedResult<BloodPressureInfo> mostRecentMeasures = await uow.Repository<BloodPressure>()
                            .WhereAsync(
                                _mapper.ConfigurationProvider.ExpressionBuilder.CreateMapExpression<BloodPressure, BloodPressureInfo>(),
                                x => x.PatientId == input.Data.PatientId,
                                new[] { OrderClause<BloodPressureInfo>.Create(x => x.DateOfMeasure, Descending) },
                                input.Data.Count.GetValueOrDefault(15), 1
                            );


                        return mostRecentMeasures.Entries;
                    }
                }));

            // Act
            IEnumerable<BloodPressureInfo> results = await _controller.MostRecentBloodPressures(query.PatientId, query.Count);


            // Assert
            _physiologicalMeasureFacadeMock.VerifyAll();
            results.Should().NotBeNull()
                .And.Match(resultExpectation);
            
        }

        [Theory]
        [MemberData(nameof(GetMostRecentTemperaturesMeasuresCases))]
        public async Task GetLastTemperaturesMesures(IEnumerable<Temperature> measuresInStore, GetMostRecentPhysiologicalMeasuresInfo query, Expression<Func<IEnumerable<TemperatureInfo>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"Current store state : {measuresInStore}");
            _outputHelper.WriteLine($"Query : {query}");

            // Arrange
            using (var uow = _factory.New())
            {
                uow.Repository<Temperature>().Create(measuresInStore);
                await uow.SaveChangesAsync();
            }

            _physiologicalMeasureFacadeMock.Setup(mock => mock.GetMostRecentMeasuresAsync<Temperature, TemperatureInfo>(It.IsAny<IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TemperatureInfo>>>()))
                .Returns((IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TemperatureInfo>> input) => Task.Run(async () =>
                {
                    using (var uow = _factory.New())
                    {
                        IPagedResult<TemperatureInfo> mostRecentMeasures = await uow.Repository<Temperature>()
                            .WhereAsync(
                                _mapper.ConfigurationProvider.ExpressionBuilder.CreateMapExpression<Temperature, TemperatureInfo>(),
                                x => x.PatientId == input.Data.PatientId,
                                new[] { OrderClause<TemperatureInfo>.Create(x => x.DateOfMeasure, Descending) },
                                input.Data.Count.GetValueOrDefault(15), 1
                            );


                        return mostRecentMeasures.Entries;
                    }
                }));

            // Act
            IEnumerable<TemperatureInfo> results = await _controller.MostRecentTemperatures(query.PatientId, query.Count);

            // Assert
            _physiologicalMeasureFacadeMock.VerifyAll();
            results.Should().NotBeNull()
                .And.Match(resultExpectation);
        }

        [Theory]
        [MemberData(nameof(GetMostRecentPrescriptionsCases))]
        public async Task GetMostRecentPrescriptionsMesures(IEnumerable<Prescription> prescriptionsInStore, GetMostRecentPrescriptionsInfo query, Expression<Func<IEnumerable<PrescriptionHeaderInfo>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"Current store state : {prescriptionsInStore}");
            _outputHelper.WriteLine($"Query : {query}");

            // Arrange
            using (var uow = _factory.New())
            {
                uow.Repository<Prescription>().Create(prescriptionsInStore);
                await uow.SaveChangesAsync();
            }

            _prescriptionServiceMock.Setup(mock => mock.GetMostRecentPrescriptionsAsync(It.IsAny<IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>>>()))
                .Returns((IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>> input) => Task.Run(async () =>
                {
                    using (var uow = _factory.New())
                    {
                        IPagedResult<PrescriptionHeaderInfo> mostRecentPrescriptions = await uow.Repository<Prescription>()
                            .WhereAsync(
                                _mapper.ConfigurationProvider.ExpressionBuilder.CreateMapExpression<Prescription, PrescriptionHeaderInfo>(),
                                x => x.PatientId == input.Data.PatientId,
                                new[] { OrderClause<PrescriptionHeaderInfo>.Create(x => x.DeliveryDate, Descending) },
                                input.Data.Count.GetValueOrDefault(15), 1
                            );


                        return mostRecentPrescriptions.Entries;
                    }
                }));

            // Act
            IEnumerable<PrescriptionHeaderInfo> results = await _controller.MostRecentPrescriptions(query.PatientId, query.Count);

            // Assert
            _prescriptionServiceMock.VerifyAll();
            results.Should().NotBeNull()
                .And.Match(resultExpectation);
        }


        [Fact]
        public async Task CreatePrescriptionForPatient()
        {
            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.CreatePrescriptionForPatientAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionInfo>()))
                .Returns((int id, CreatePrescriptionInfo input) => Task.Run(() =>
                {
                    return new PrescriptionHeaderInfo
                    {
                        Id = 1,
                        DeliveryDate = input.DeliveryDate,
                        PatientId = id,
                        PrescriptorId = input.PrescriptorId
                    };
                }));

            // Act
            int patientId = 1;
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                PrescriptorId = 1,
                DeliveryDate = DateTimeOffset.UtcNow,
                Duration = 60,
                Items = new PrescriptionItemInfo[] {
                    new PrescriptionItemInfo { CategoryId = 3, Code = "Prescription CODE" }
                }
            };

            IActionResult actionResult = await _controller.Prescriptions(patientId, newPrescription);

            // Assert

            CreatedAtActionResult createdAtActionResult = actionResult.Should().NotBeNull().And
                .BeAssignableTo<CreatedAtActionResult>().Which;
                    
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.Prescriptions));
            createdAtActionResult.RouteValues.Should().NotBeNull();
            createdAtActionResult.RouteValues.ToQueryString().Should().MatchRegex($@"[iI]d={patientId}&[pP]rescriptionId=[1-9](\d+)?");

            PrescriptionHeaderInfo resource = createdAtActionResult.Value.Should().NotBeNull().And
                .BeAssignableTo<PrescriptionHeaderInfo>().Which;


            resource.PatientId.Should().Be(patientId);
            resource.PrescriptorId.Should().Be(newPrescription.PrescriptorId);
            resource.DeliveryDate.Should().Be(newPrescription.DeliveryDate);

            _prescriptionServiceMock.Verify(mock => mock.CreatePrescriptionForPatientAsync(patientId, newPrescription), Times.Once);
        }

        [Fact]
        public async Task CreatePrescriptionForPatientShouldReturnBadRequestResultWhenArgumentNullException()
        {
            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.CreatePrescriptionForPatientAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionInfo>()))
                .Throws<ArgumentNullException>();

            // Act
            int patientId = 1;
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                PrescriptorId = 1,
                DeliveryDate = DateTimeOffset.UtcNow,
                Duration = 60,
                Items = new PrescriptionItemInfo[] {
                    new PrescriptionItemInfo { CategoryId = 3, Code = "Prescription CODE" }
                }
            };

           IActionResult actionResult = await _controller.Prescriptions(patientId, newPrescription);

            // Assert
            actionResult.Should().BeAssignableTo<BadRequestResult>();

            _prescriptionServiceMock.Verify(mock => mock.CreatePrescriptionForPatientAsync(patientId, newPrescription), Times.Once);
        }

        [Fact]
        public async Task CreatePrescriptionForPatientShouldReturnBadRequestResultWhenArgumentOutOfRangeException()
        {
            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.CreatePrescriptionForPatientAsync(It.IsAny<int>(), It.IsAny<CreatePrescriptionInfo>()))
                .Throws<ArgumentOutOfRangeException>();

            // Act
            int patientId = 1;
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                PrescriptorId = 1,
                DeliveryDate = DateTimeOffset.UtcNow,
                Duration = 60,
                Items = new PrescriptionItemInfo[] {
                    new PrescriptionItemInfo { CategoryId = 3, Code = "Prescription CODE" }
                }
            };

            IActionResult actionResult = await _controller.Prescriptions(patientId, newPrescription);

            // Assert
            actionResult.Should().BeAssignableTo<BadRequestResult>();

            _prescriptionServiceMock.Verify(mock => mock.CreatePrescriptionForPatientAsync(patientId, newPrescription), Times.Once);
        }


        [Fact]
        public async Task GetTemperatureShouldReturnNotFoundResultWhenServiceReturnsNull()
        {
            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.GetOneMeasureAsync<Temperature, TemperatureInfo>(It.IsAny<IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo>>()))
                .ReturnsAsync(null)
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Temperatures(1, 12);

            //Assert
            actionResult.Should().BeOfType<NotFoundResult>();
            _physiologicalMeasureFacadeMock.VerifyAll();
        }

        [Fact]
        public async Task GetBloodPressureShouldReturnNotFoundResultWhenServiceReturnsNull()
        {
            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.GetOneMeasureAsync<BloodPressure, BloodPressureInfo>(It.IsAny<IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, BloodPressureInfo>>()))
                .ReturnsAsync(null)
                .Verifiable();


            //Act
            IActionResult actionResult = await _controller.BloodPressures(1, 12);

            //Assert
            actionResult.Should().BeOfType<NotFoundResult>();
            _physiologicalMeasureFacadeMock.VerifyAll();
        }




        [Fact]
        public async Task GetBodyWeightShouldReturnNotFoundResultWhenServiceReturnsNull()
        {
            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.GetOneMeasureAsync<BodyWeight, BodyWeightInfo>(It.IsAny<IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, BodyWeightInfo>>()))
                .ReturnsAsync(null)
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.BodyWeights(1, 12);

            //Assert
            actionResult.Should().BeOfType<NotFoundResult>();
            _physiologicalMeasureFacadeMock.VerifyAll();
        }

        [Fact]
        public async Task DeleteOneBloodPressure()
        {
            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.DeleteOnePhysiologicalMeasureAsync<BloodPressure>(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()))
                .Returns(Task.CompletedTask);


            // Act
            IActionResult actionResult = await _controller.BloodPressures(new DeletePhysiologicalMeasureInfo { Id = 1, MeasureId = 4 });

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            _physiologicalMeasureFacadeMock.Verify(mock => mock.DeleteOnePhysiologicalMeasureAsync<BloodPressure>(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteOneTemperature()
        {
            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.DeleteOnePhysiologicalMeasureAsync<Temperature>(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()))
                .Returns(Task.CompletedTask);


            // Act
            IActionResult actionResult = await _controller.Temperatures(new DeletePhysiologicalMeasureInfo { Id = 1, MeasureId = 4 });

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            _physiologicalMeasureFacadeMock.Verify(mock => mock.DeleteOnePhysiologicalMeasureAsync<Temperature>(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteOneBodyWeight()
        {
            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.DeleteOnePhysiologicalMeasureAsync<BodyWeight>(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()))
                .Returns(Task.CompletedTask);


            // Act
            IActionResult actionResult = await _controller.BodyWeights(new DeletePhysiologicalMeasureInfo { Id = 1, MeasureId = 4 });

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            _physiologicalMeasureFacadeMock.Verify(mock => mock.DeleteOnePhysiologicalMeasureAsync<BodyWeight>(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()), Times.Once);
        }

       
    }
}






