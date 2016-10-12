using AutoMapper;
using FluentAssertions;
using GenFu;
using MedEasy.API;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
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
using MedEasy.Validators;
using Microsoft.AspNetCore.Http;
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
using static Moq.MockBehavior;
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
        private Mock<IHandleGetOnePatientInfoByIdQuery> _iHandleGetOnePatientInfoByIdQueryMock;
        private Mock<IHandleGetManyPatientInfosQuery> _iHandleGetManyPatientInfoQueryMock;
        private EFUnitOfWorkFactory _factory;
        private IMapper _mapper;
        private Mock<IRunCreatePatientCommand> _iRunCreatePatientInfoCommandMock;
        private Mock<IRunDeletePatientByIdCommand> _iRunDeletePatientInfoByIdCommandMock;
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;
        private Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo>> _iRunAddNewTemperatureCommandMock;
        private Mock<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>> _iHandleGetOnePatientTemperatureMock;
        private Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo>> _iRunAddNewBloodPressureCommandMock;
        private Mock<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>> _iHandleGetOnePatientBloodPressureMock;
        private Mock<IHandleGetMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo>> _iHandleGetLastBloodPressuresMock;
        private Mock<IHandleGetMostRecentPhysiologicalMeasuresQuery<TemperatureInfo>> _iHandleGetLastTemperaturesMock;

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
            _iRunDeletePatientInfoByIdCommandMock = new Mock<IRunDeletePatientByIdCommand>(Strict);
            _iRunAddNewTemperatureCommandMock = new Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo>>(Strict);
            _iRunAddNewBloodPressureCommandMock = new Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo>>(Strict);
            _iHandleGetOnePatientTemperatureMock = new Mock<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>>(Strict);
            _iHandleGetOnePatientBloodPressureMock = new Mock<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>>(Strict);
            _iHandleGetLastBloodPressuresMock = new Mock<IHandleGetMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo>>(Strict);
            _iHandleGetLastTemperaturesMock = new Mock<IHandleGetMostRecentPhysiologicalMeasuresQuery<TemperatureInfo>>(Strict);

            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);

            _controller = new PatientsController(
                _loggerMock.Object, 
                _urlHelperFactoryMock.Object, 
                _actionContextAccessor,
                _apiOptionsMock.Object,
                _iHandleGetOnePatientInfoByIdQueryMock.Object,
                _iHandleGetManyPatientInfoQueryMock.Object,
                _iRunCreatePatientInfoCommandMock.Object,
                _iRunDeletePatientInfoByIdCommandMock.Object,
                _iRunAddNewTemperatureCommandMock.Object,
                _iHandleGetOnePatientTemperatureMock.Object,
                _iRunAddNewBloodPressureCommandMock.Object,
                _iHandleGetOnePatientBloodPressureMock.Object,
                _iHandleGetLastBloodPressuresMock.Object,
                _iHandleGetLastTemperaturesMock.Object);

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
                    new GetMostRecentPhysiologicalMeasuresInfo { Id = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new BloodPressure { PatientId = 2, CreatedDate = DateTime.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { Id = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new BloodPressure { PatientId = 1, CreatedDate = DateTime.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { Id = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => x.All(measure => measure.PatientId == 1) && x.Count() == 1))
                };
            }
        }

        public static IEnumerable<object> GetLastTemperaturesMesuresCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Temperature>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { Id = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Temperature { PatientId = 2, CreatedDate = DateTime.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { Id = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Temperature { PatientId = 1, CreatedDate = DateTime.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { Id = 1, Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => x.All(measure => measure.PatientId == 1) && x.Count() == 1))
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
                    .And.BeOfType<GenericPagedGetResponse<BrowsableResource<PatientInfo>>>();

            GenericPagedGetResponse<BrowsableResource<PatientInfo>> response = (GenericPagedGetResponse<BrowsableResource<PatientInfo>>)value;

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPagedGetResponse<BrowsableResource<PatientInfo>>)}.{nameof(GenericPagedGetResponse<BrowsableResource<PatientInfo>>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);
            
        }


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
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeOfType<BrowsableResource<PatientInfo>>().Which
                    .Location.Should()
                        .NotBeNull();

            BrowsableResource<PatientInfo> result = (BrowsableResource<PatientInfo>)((OkObjectResult)actionResult).Value;
            Link location = result.Location;
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
                    Firstname = cmd.Data.Firstname,
                    Lastname = cmd.Data.Lastname,
                    UpdatedDate = new DateTime(2012, 2, 1) }));

            //Act
            CreatePatientInfo info = new CreatePatientInfo
            {
                Firstname = "Bruce",
                Lastname = "Lastname"
            };

            IActionResult actionResult = await _controller.Post(info);

            //Assert


            actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<BrowsableResource<PatientInfo>>();


            BrowsableResource<PatientInfo> createdResource = (BrowsableResource<PatientInfo>)((OkObjectResult)actionResult).Value;


            createdResource.Should()
                .NotBeNull();

            createdResource.Location.Should()
                .NotBeNull();

            createdResource.Location.Href.Should()
                .NotBeNull().And
                .MatchEquivalentOf($"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?{nameof(PatientInfo.Id)}=*");

            createdResource.Resource.Firstname.Should()
                .Be(info.Firstname);
            createdResource.Resource.Lastname.Should()
                .Be(info.Lastname);
            createdResource.Resource.UpdatedDate.Should().Be(1.February(2012));

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


        [Fact]
        public async Task AddTemperatureMeasure()
        {
            // Arrange
            _iRunAddNewTemperatureCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo>>()))
                .Returns((IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo> localCmd) => Task.FromResult(new TemperatureInfo {
                    Id = 1,
                    DateOfMeasure = localCmd.Data.DateOfMeasure,
                    PatientId = localCmd.Data.Id,
                    Value = localCmd.Data.Value
                }));

            // Act
            CreateTemperatureInfo input = new CreateTemperatureInfo
            {
                Id = 1,
                Value = 50,
                DateOfMeasure = DateTime.UtcNow
            };

            IActionResult actionResult = await _controller.Temperatures(input);

            // Assert
            BrowsableResource<TemperatureInfo> browsableResource = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                .Value.Should()
                    .NotBeNull().And
                    .BeOfType<BrowsableResource<TemperatureInfo>>().Which;

            TemperatureInfo resource = browsableResource.Resource;
            
            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(input.Id);
            resource.Value.Should().Be(input.Value);

            Link resourceLink = browsableResource.Location;
            resourceLink.Should().NotBeNull();
            resourceLink.Href.ShouldBeEquivalentTo($"api/{PatientsController.EndpointName}/Temperatures?id={input.Id}&temperatureId={resource.Id}");

            _iRunAddNewTemperatureCommandMock.VerifyAll();
            
        }

        [Fact]
        public async Task AddBloodPressureMeasure()
        {
            // Arrange
            _iRunAddNewBloodPressureCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo>>()))
                .Returns((IAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo> localCmd) => Task.FromResult(new BloodPressureInfo
                {
                    Id = 1,
                    DateOfMeasure = localCmd.Data.DateOfMeasure,
                    PatientId = localCmd.Data.Id,
                    SystolicPressure = localCmd.Data.SystolicPressure,
                    DiastolicPressure = localCmd.Data.DiastolicPressure,

                }));

            // Act
            CreateBloodPressureInfo input = new CreateBloodPressureInfo
            {
                Id = 1,
                SystolicPressure = 150,
                DiastolicPressure = 100,
                DateOfMeasure = DateTime.UtcNow
            };

            IActionResult actionResult = await _controller.BloodPressures(input);

            // Assert
            BrowsableResource<BloodPressureInfo> browsableResource = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                .Value.Should()
                    .NotBeNull().And
                    .BeOfType<BrowsableResource<BloodPressureInfo>>().Which;

            BloodPressureInfo resource = browsableResource.Resource;

            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(input.Id);
            resource.SystolicPressure.Should().Be(input.SystolicPressure);
            resource.DiastolicPressure.Should().Be(input.DiastolicPressure);

            Link resourceLink = browsableResource.Location;
            resourceLink.Should().NotBeNull();
            resourceLink.Href.ShouldBeEquivalentTo($"api/{PatientsController.EndpointName}/BloodPressures?id={input.Id}&temperatureId={resource.Id}");

            _iRunAddNewBloodPressureCommandMock.VerifyAll();

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

            _iHandleGetLastBloodPressuresMock.Setup(mock => mock.HandleAsync(It.IsAny<IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<BloodPressureInfo>>>()))
                .Returns((IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<BloodPressureInfo>> input) => Task.Run(async () =>
                {
                    using (var uow = _factory.New())
                    {
                        IPagedResult<BloodPressureInfo> mostRecentMeasures = await uow.Repository<BloodPressure>()
                            .WhereAsync(
                                _mapper.ConfigurationProvider.ExpressionBuilder.CreateMapExpression<BloodPressure, BloodPressureInfo>(),
                                x => x.PatientId == input.Data.Id,
                                new[] { OrderClause<BloodPressureInfo>.Create(x => x.DateOfMeasure, Descending) },
                                input.Data.Count.GetValueOrDefault(15), 1
                            );


                        return mostRecentMeasures.Entries;
                    }
                }));

            // Act
            IEnumerable<BloodPressureInfo> results = await _controller.MostRecentBloodPressures(query);


            // Assert
            _iHandleGetLastBloodPressuresMock.VerifyAll();
            results.Should().NotBeNull()
                .And.Match(resultExpectation);
            
        }

        [Theory]
        [MemberData(nameof(GetLastTemperaturesMesuresCases))]
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

            _iHandleGetLastTemperaturesMock.Setup(mock => mock.HandleAsync(It.IsAny<IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TemperatureInfo>>>()))
                .Returns((IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TemperatureInfo>> input) => Task.Run(async () =>
                {
                    using (var uow = _factory.New())
                    {
                        IPagedResult<TemperatureInfo> mostRecentMeasures = await uow.Repository<Temperature>()
                            .WhereAsync(
                                _mapper.ConfigurationProvider.ExpressionBuilder.CreateMapExpression<Temperature, TemperatureInfo>(),
                                x => x.PatientId == input.Data.Id,
                                new[] { OrderClause<TemperatureInfo>.Create(x => x.DateOfMeasure, Descending) },
                                input.Data.Count.GetValueOrDefault(15), 1
                            );


                        return mostRecentMeasures.Entries;
                    }
                }));

            // Act
            IEnumerable<TemperatureInfo> results = await _controller.MostRecentTemperatures(query);


            // Assert
            _iHandleGetLastTemperaturesMock.VerifyAll();
            results.Should().NotBeNull()
                .And.Match(resultExpectation);

        }


        [Fact]
        public async Task GetTemperatureShouldReturnNotFoundResultWhenServiceReturnsNull()
        {
            // Arrange
            _iHandleGetOnePatientTemperatureMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo>>()))
                .ReturnsAsync(null)
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Temperatures(1, 12);

            //Assert
            actionResult.Should().BeOfType<NotFoundResult>();
            _iHandleGetOnePatientTemperatureMock.VerifyAll();
        }

        [Fact]
        public async Task GetBloodPressureShouldReturnNotFoundResultWhenServiceReturnsNull()
        {
            // Arrange
            _iHandleGetOnePatientBloodPressureMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, BloodPressureInfo>>()))
                .ReturnsAsync(null)
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.BloodPressures(1, 12);

            //Assert
            actionResult.Should().BeOfType<NotFoundResult>();
            _iHandleGetOnePatientBloodPressureMock.VerifyAll();
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
            _iHandleGetOnePatientTemperatureMock = null;
            _iHandleGetOnePatientBloodPressureMock = null;
            _iHandleGetLastBloodPressuresMock = null;
            _iHandleGetLastTemperaturesMock = null;

            _iRunCreatePatientInfoCommandMock = null;
            _iRunDeletePatientInfoByIdCommandMock = null;
            _iRunAddNewTemperatureCommandMock = null;
            _iRunAddNewBloodPressureCommandMock = null;

            _factory = null;
            _mapper = null;
        }
    }
}






