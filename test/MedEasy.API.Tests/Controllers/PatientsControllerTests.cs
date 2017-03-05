using MedEasy.API.Models;
using AutoMapper;
using FluentAssertions;
using GenFu;
using MedEasy.API;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Exceptions;
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
using static Moq.MockBehavior;
using static System.StringComparison;
using MedEasy.Queries.Search;
using static Newtonsoft.Json.JsonConvert;
using static System.StringSplitOptions;
using MedEasy.Queries.Patient;
using MedEasy.Handlers.Core.Patient.Queries;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.Handlers.Core.Search.Queries;
using System.IO;
using MedEasy.DTO.Search;
using MedEasy.DAL.Interfaces;

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
        private Mock<IHandleSearchQuery> _iHandleSearchQueryMock;
        private Mock<IHandleGetDocumentsByPatientIdQuery> _iHandleGetDocumentsByPatientIdQueryMock;
        private Mock<IRunCreateDocumentForPatientCommand> _iRunCreateDocumentForPatientCommandMock;
        private Mock<IHandleGetOneDocumentInfoByPatientIdAndDocumentId> _iHandleGetOneDocumentInfoByPatientIdAndDocumentIdMock;

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

            _iHandleGetOnePatientInfoByIdQueryMock = new Mock<IHandleGetOnePatientInfoByIdQuery>(Strict);
            _iHandleGetManyPatientInfoQueryMock = new Mock<IHandleGetManyPatientInfosQuery>(Strict);
            _iRunCreatePatientInfoCommandMock = new Mock<IRunCreatePatientCommand>(Strict);
            _iRunPatchPatientCommandMock = new Mock<IRunPatchPatientCommand>(Strict);
            _iRunDeletePatientInfoByIdCommandMock = new Mock<IRunDeletePatientByIdCommand>(Strict);
            _physiologicalMeasureFacadeMock = new Mock<IPhysiologicalMeasureService>(Strict);

            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);
            _prescriptionServiceMock = new Mock<IPrescriptionService>(Strict);
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _iHandleSearchQueryMock = new Mock<IHandleSearchQuery>(Strict);

            _iHandleGetDocumentsByPatientIdQueryMock = new Mock<IHandleGetDocumentsByPatientIdQuery>(Strict);
            _iRunCreateDocumentForPatientCommandMock = new Mock<IRunCreateDocumentForPatientCommand>(Strict);
            _iHandleGetOneDocumentInfoByPatientIdAndDocumentIdMock = new Mock<IHandleGetOneDocumentInfoByPatientIdAndDocumentId>(Strict);

            _controller = new PatientsController(
                _loggerMock.Object,
                _urlHelperFactoryMock.Object,
                _actionContextAccessor,
                _apiOptionsMock.Object,
                _mapper,
                _iHandleSearchQueryMock.Object,
                _iHandleGetOnePatientInfoByIdQueryMock.Object,
                _iHandleGetManyPatientInfoQueryMock.Object,
                _iRunCreatePatientInfoCommandMock.Object,
                _iRunDeletePatientInfoByIdCommandMock.Object,
                _physiologicalMeasureFacadeMock.Object,
                _prescriptionServiceMock.Object,
                _iHandleGetDocumentsByPatientIdQueryMock.Object,
                _iRunPatchPatientCommandMock.Object,
                _iRunCreateDocumentForPatientCommandMock.Object,
                _iHandleGetOneDocumentInfoByPatientIdAndDocumentIdMock.Object);

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
            _iHandleSearchQueryMock = null;

            _iHandleGetDocumentsByPatientIdQueryMock = null;
            _iHandleGetOneDocumentInfoByPatientIdAndDocumentIdMock = null;

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
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
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
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=14".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
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
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize=10&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize=10&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize=10&page=40".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }

                yield return new object[]
                    {
                        new [] {
                            new Patient { Id = 1, Firstname = "Bruce",  Lastname = "Wayne" }
                        },
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        1,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == "first"
                            && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
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
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new BloodPressure { PatientId = 2, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },

                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any()))
                };

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new []
                        {
                            new BloodPressure { PatientId = 1, CreatedDate = DateTimeOffset.UtcNow, Patient = new Patient { UUID = patientId } }
                        },
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => x.All(measure => measure.PatientId == patientId) && x.Count() == 1))
                    };
                }
            }
        }

        public static IEnumerable<object> GetMostRecentTemperaturesMeasuresCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Temperature>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Temperature { PatientId = 2, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any()))
                };
                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new []
                        {
                            new Temperature { PatientId = 1, CreatedDate = DateTimeOffset.UtcNow, Patient = new Patient { UUID = patientId } }
                        },
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => x.All(measure => measure.PatientId == patientId) && x.Count() == 1))
                    };
                }
            }
        }

        public static IEnumerable<object> GetMostRecentPrescriptionsCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<PrescriptionHeaderInfo>(),
                    new GetMostRecentPrescriptionsInfo { PatientId = Guid.NewGuid(), Count = 10 }
                };
                {
                    yield return new object[]
                    {
                        new []
                        {
                            new PrescriptionHeaderInfo { Id = Guid.NewGuid(), DeliveryDate = 23.June(1988), PatientId = Guid.NewGuid(), PrescriptorId = Guid.NewGuid(), UpdatedDate = DateTimeOffset.UtcNow }
                        },
                        new GetMostRecentPrescriptionsInfo { PatientId = Guid.NewGuid(), Count = 10 }
                    };
                }
                {
                    yield return new object[]
                    {
                        new []
                        {
                            new PrescriptionHeaderInfo { Id = Guid.NewGuid(), DeliveryDate = 23.June(1988), PatientId = Guid.NewGuid(), PrescriptorId = Guid.NewGuid(), UpdatedDate = DateTimeOffset.UtcNow },
                            new PrescriptionHeaderInfo { Id = Guid.NewGuid(), DeliveryDate = 15.August(2006), PatientId = Guid.NewGuid(), PrescriptorId = Guid.NewGuid(), UpdatedDate = DateTimeOffset.UtcNow }

                        },
                        new GetMostRecentPrescriptionsInfo { PatientId = Guid.NewGuid(), Count = 10 }
                    };
                }
            }
        }


        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Patient> items, int pageSize, int page,
            int expectedCount,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.GetAll)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _factory.New())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync();
            }

            _iHandleGetManyPatientInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, PatientInfo>>()))
                .Returns((IWantManyResources<Guid, PatientInfo> getQuery) => Task.Run(async () =>
                {


                    using (IUnitOfWork uow = _factory.New())
                    {
                        PaginationConfiguration queryConfig = getQuery.Data ?? new PaginationConfiguration();

                        IPagedResult<PatientInfo> results = await uow.Repository<Patient>()
                            .ReadPageAsync(x => _mapper.Map<PatientInfo>(x), getQuery.Data.PageSize, getQuery.Data.Page);

                        return results;
                    }
                }));
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });

            // Act
            IActionResult actionResult = await _controller.Get(page, pageSize);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(PatientsController)}.{nameof(PatientsController.GetAll)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");

            actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<IGenericPagedGetResponse<PatientInfo>>();

            IGenericPagedGetResponse<PatientInfo> response = (IGenericPagedGetResponse<PatientInfo>)value;

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<PatientInfo>)}.{nameof(IGenericPagedGetResponse<PatientInfo>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);

        }


        public static IEnumerable<object> SearchCases
        {
            get
            {
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };
                    yield return new object[]
                    {
                        Enumerable.Empty<PatientInfo>(),
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation == "first" &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.PageSize)}={searchInfo.PageSize}")  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Sort)}={searchInfo.Sort}" )

                           )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };

                }
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Firstname = "!bruce",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };
                    yield return new object[]
                    {
                        new [] {
                            new PatientInfo { Firstname = "Bruce", Lastname = "Wayne" }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation == "first" &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.PageSize)}={searchInfo.PageSize}")  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Sort)}={searchInfo.Sort}" )

                           )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };

                }
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                    };
                    yield return new object[]
                    {
                        new[] {
                            new PatientInfo { Firstname = "bruce" }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation == "first" &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 3 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.PageSize)}={searchInfo.PageSize}")

                            )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };
                }

                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                        BirthDate = 31.July(2010)
                    };
                    yield return new object[]
                    {
                        new[] {
                            new PatientInfo { Firstname = "bruce", BirthDate = 31.July(2010) }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation == "first" &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.BirthDate)}={searchInfo.BirthDate.Value.ToString("s")}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.PageSize)}={searchInfo.PageSize}")

                            )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };
                }

            }
        }


        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<PatientInfo> entries, SearchPatientInfo searchRequest,
        Expression<Func<Link, bool>> firstPageLinkExpectation, Expression<Func<Link, bool>> previousPageLinkExpectation, Expression<Func<Link, bool>> nextPageLinkExpectation, Expression<Func<Link, bool>> lastPageLinkExpectation)
        {
            _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
            _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");


            // Arrange
            MedEasyApiOptions apiOptions = new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 50 };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);
            _iHandleSearchQueryMock.Setup(mock => mock.Search<Patient, PatientInfo>(It.IsNotNull<SearchQuery<PatientInfo>>()))
                    .Returns((SearchQuery<PatientInfo> query) => Task.Run(() =>
                    {
                        SearchQueryInfo<PatientInfo> data = query.Data;
                        Expression<Func<PatientInfo, bool>> filter = data.Filter.ToExpression<PatientInfo>();
                        int page = query.Data.Page;
                        int pageSize = query.Data.PageSize;
                        Func<PatientInfo, bool> fnFilter = filter.Compile();

                        IEnumerable<PatientInfo> result = entries.Where(fnFilter)
                            .Skip(page * pageSize)
                            .Take(pageSize);

                        IPagedResult<PatientInfo> pageOfResult = new PagedResult<PatientInfo>(result, entries.Count(fnFilter), pageSize);
                        return pageOfResult;
                    })
                    );


            // Act
            IActionResult actionResult = await _controller.Search(searchRequest);

            // Assert
            IGenericPagedGetResponse<PatientInfo> content = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IGenericPagedGetResponse<PatientInfo>>().Which;


            content.Items.Should()
                .NotBeNull();

            content.Links.Should().NotBeNull();
            PagedRestResponseLink links = content.Links;

            links.First.Should().Match(firstPageLinkExpectation);
            links.Previous.Should().Match(previousPageLinkExpectation);
            links.Next.Should().Match(nextPageLinkExpectation);
            links.Last.Should().Match(nextPageLinkExpectation);
        }





        public static IEnumerable<object> PatchCases
        {
            get
            {
                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Firstname, "Bruce");
                    yield return new object[]
                    {
                        new Patient { Id = 1, },
                        patchDocument.Operations,
                        ((Expression<Func<Patient, bool>>)(x => x.Id == 1 && x.Firstname == "Bruce"))
                    };
                }
            }
        }




        [Theory]
        [MemberData(nameof(PatchCases))]
        public async Task Patch(Patient source, IEnumerable<Operation<PatientInfo>> operations, Expression<Func<Patient, bool>> patchResultExpectation)
        {

            // Arrange


            _iRunPatchPatientCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IPatchCommand<int, Patient>>()))
                .Returns((IPatchCommand<int, Patient> command) => Task.Run(() =>
                {
                    command.Data.PatchDocument.ApplyTo(source);
                    return Nothing.Value;
                }));


            // Act
            JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
            patchDocument.Operations.AddRange(operations);
            IActionResult actionResult = await _controller.Patch(1, patchDocument);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<OkResult>();

            _iRunPatchPatientCommandMock.Verify();

            source.Should().Match(patchResultExpectation);

        }

        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            //Arrange
            _iHandleGetOnePatientInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, PatientInfo>>()))
                .ReturnsAsync(null);

            //Act
            IActionResult actionResult = await _controller.Get(Guid.NewGuid());

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

            Guid patientId = Guid.NewGuid();
            PatientInfo expectedResource = new PatientInfo
            {
                Id = patientId,
                Firstname = "Bruce",
                Lastname = "Wayne",
                MainDoctorId = Guid.NewGuid()
            };
            _iHandleGetOnePatientInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, PatientInfo>>()))
                .ReturnsAsync(expectedResource)
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Get(patientId);

            //Assert

            IBrowsableResource<PatientInfo> result = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<IBrowsableResource<PatientInfo>>().Which;

            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .Contain(x => x.Relation == "self").And
                .Contain(x => x.Relation == "delete").And
                .Contain(x => x.Relation == "main-doctor").And
                .Contain(x => x.Relation == "documents").And
                .Contain(x => x.Relation == "most-recent-temperatures").And
                .Contain(x => x.Relation == "most-recent-blood-pressures");

            Link location = links.Single(x => x.Relation == "self");
            location.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{PatientsController.EndpointName}/{nameof(PatientsController.Get)}?{nameof(PatientInfo.Id)}={patientId}");
            location.Relation.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo("self");


            Link linkMainDoctor = links.Single(x => x.Relation == nameof(Patient.MainDoctor).ToLowerKebabCase());
            linkMainDoctor.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?{nameof(DoctorInfo.Id)}={expectedResource.MainDoctorId}");

            Link linkDelete = links.Single(x => x.Relation == "delete");
            linkDelete.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{PatientsController.EndpointName}/{nameof(PatientsController.Delete)}?{nameof(PatientInfo.Id)}={expectedResource.Id}");

            PatientInfo actualResource = result.Resource;
            actualResource.Should().NotBeNull();
            actualResource.Id.Should().Be(expectedResource.Id);
            actualResource.Firstname.Should().Be(expectedResource.Firstname);
            actualResource.Lastname.Should().Be(expectedResource.Lastname);
            actualResource.MainDoctorId.Should().Be(expectedResource.MainDoctorId);

            _iHandleGetOnePatientInfoByIdQueryMock.Verify();
            _urlHelperFactoryMock.Verify();

        }

        [Fact]
        public async Task Post()
        {
            //Arrange
            Guid patientId = Guid.NewGuid();
            _iRunCreatePatientInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreatePatientCommand>()))
                .Returns((ICreatePatientCommand cmd) => Task.Run(()
                => new PatientInfo
                {
                    Id = patientId,
                    Firstname = cmd.Data.Firstname,
                    Lastname = cmd.Data.Lastname,
                    UpdatedDate = new DateTimeOffset(2012, 2, 1, 0, 0, 0, TimeSpan.Zero),
                    MainDoctorId = cmd.Data.MainDoctorId
                }));

            //Act
            CreatePatientInfo info = new CreatePatientInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne",
                MainDoctorId = Guid.NewGuid()
            };

            IActionResult actionResult = await _controller.Post(info);

            //Assert
            CreatedAtActionResult createdActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;

            IBrowsableResource<PatientInfo> browsableResource = createdActionResult.Value.Should()
                .NotBeNull().And
                .BeAssignableTo<IBrowsableResource<PatientInfo>>().Which;


            PatientInfo createdResource = browsableResource.Resource;

            createdResource.Should()
                .NotBeNull();
            createdResource.Should()
                .NotBeNull();
            createdResource.Firstname.Should()
                .Be(info.Firstname);
            createdResource.Lastname.Should()
                .Be(info.Lastname);
            createdResource.MainDoctorId.Should()
                .Be(info.MainDoctorId);

            createdResource.UpdatedDate.Should().HaveDay(1);
            createdResource.UpdatedDate.Should().HaveMonth(2);
            createdResource.UpdatedDate.Should().HaveYear(2012);

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeNullOrEmpty().And
                .Contain(x => x.Relation == "main-doctor");

            Link locationMainDoctor = links.Single(x => x.Relation == "main-doctor");
            locationMainDoctor.Href.Should()
                .BeEquivalentTo($"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?{nameof(DoctorInfo.Id)}={createdResource.MainDoctorId}");

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
            _prescriptionServiceMock.Setup(mock => mock.GetOnePrescriptionByPatientIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(null);

            // Act
            Guid patientId = Guid.NewGuid();
            Guid prescriptionId = Guid.NewGuid();

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
            Guid patientId = Guid.NewGuid();
            Guid prescriptionId = Guid.NewGuid();
            PrescriptionHeaderInfo expectedOutput = new PrescriptionHeaderInfo
            {
                Id = prescriptionId,
                PatientId = patientId,
                PrescriptorId = Guid.NewGuid(),
                DeliveryDate = new DateTimeOffset(1983, 6, 23, 0, 0, 0, TimeSpan.Zero)
            };
            _prescriptionServiceMock.Setup(mock => mock.GetOnePrescriptionByPatientIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(expectedOutput);

            // Act

            IActionResult actionResult = await _controller.Prescriptions(patientId, prescriptionId);

            // Assert
            IBrowsableResource<PrescriptionHeaderInfo> browsableResource = actionResult.Should().NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IBrowsableResource<PrescriptionHeaderInfo>>().Which;


            browsableResource.Should().NotBeNull();

            PrescriptionHeaderInfo resource = browsableResource.Resource;

            IEnumerable<Link> links = browsableResource.Links;

            links.Should()
                .NotBeNull().And
                .Contain(x => x.Relation == "self").And
                .Contain(x => x.Relation == nameof(Prescription.Items));

            Link location = links.Single(x => x.Relation == "self");
            location.Href.Should().Be($"api/{PatientsController.EndpointName}/{nameof(PatientsController.Prescriptions)}?id={expectedOutput.PatientId}&prescriptionId={expectedOutput.Id}");

            Link locationItems = links.Single(x => x.Relation == nameof(Prescription.Items));
            locationItems.Href.Should().Be($"api/{PrescriptionsController.EndpointName}/{nameof(PrescriptionsController.Details)}?{nameof(resource.Id)}={resource.Id}");

            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(expectedOutput.PatientId);
            resource.Id.Should().Be(expectedOutput.Id);
            resource.PrescriptorId.Should().Be(expectedOutput.PrescriptorId);
            resource.DeliveryDate.Should().Be(expectedOutput.DeliveryDate);

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
            _iHandleGetOnePatientInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, PatientInfo>>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.Get(Guid.NewGuid());

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
            Func<Task> action = async () => await _controller.GetAll(new PaginationConfiguration());

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
            Func<Task> action = async () => await _controller.Delete(Guid.NewGuid());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iHandleGetManyPatientInfoQueryMock.Verify();
        }

        [Fact]
        public async Task DeleteMustRelyOnDeleteCommandHandler()
        {

            //Arrange
            _iRunDeletePatientInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeletePatientByIdCommand>()))
                .Returns(Nothing.Task)
                .Verifiable();


            //Act
            await _controller.Delete(Guid.NewGuid());

            //Assert
            _iRunDeletePatientInfoByIdCommandMock.Verify();
        }

        [Fact]
        public async Task DeletePatientByNegativeOrZeroReturnsBadRequest()
        {
            //Arrange

            //Act
            IActionResult actionResult = await _controller.Delete(Guid.Empty);

            //Assert
            actionResult.Should().BeAssignableTo<BadRequestResult>();
        }




        [Fact]
        public async Task AddTemperatureMeasure()
        {
            // Arrange
            _physiologicalMeasureFacadeMock.Setup(mock => mock.AddNewMeasureAsync<Temperature, TemperatureInfo>(It.IsAny<ICommand<Guid, CreatePhysiologicalMeasureInfo<Temperature>>>()))
                .Returns((ICommand<Guid, CreatePhysiologicalMeasureInfo<Temperature>> localCmd) => Task.FromResult(new TemperatureInfo
                {
                    Id = Guid.NewGuid(),
                    DateOfMeasure = localCmd.Data.Measure.DateOfMeasure,
                    PatientId = localCmd.Data.PatientId,
                    Value = localCmd.Data.Measure.Value
                }));

            // Act
            CreateTemperatureInfo input = new CreateTemperatureInfo
            {
                Value = 50,
                DateOfMeasure = DateTimeOffset.UtcNow
            };

            Guid patientId = Guid.NewGuid();
            IActionResult actionResult = await _controller.Temperatures(patientId, input);

            // Assert
            CreatedAtActionResult createdAtActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;

            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.Temperatures));
            createdAtActionResult.RouteValues.Should()
                .HaveCount(2).And
                .ContainKey("id").WhichValue.Should().Be(patientId);
            createdAtActionResult.RouteValues.Should()
                .ContainKey("temperatureId").WhichValue.Should()
                .BeOfType<Guid>().Which.Should()
                .NotBeEmpty();


            TemperatureInfo resource = createdAtActionResult.Value.Should()
                .BeOfType<TemperatureInfo>().Which;

            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(patientId);
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
            _physiologicalMeasureFacadeMock.Setup(mock => mock.AddNewMeasureAsync<BloodPressure, BloodPressureInfo>(It.IsAny<ICommand<Guid, CreatePhysiologicalMeasureInfo<BloodPressure>>>()))
                .Returns((ICommand<Guid, CreatePhysiologicalMeasureInfo<BloodPressure>> localCmd) => Task.FromResult(new BloodPressureInfo
                {
                    Id = Guid.NewGuid(),
                    DateOfMeasure = localCmd.Data.Measure.DateOfMeasure,
                    PatientId = localCmd.Data.PatientId,
                    SystolicPressure = localCmd.Data.Measure.SystolicPressure,
                    DiastolicPressure = localCmd.Data.Measure.DiastolicPressure,

                }));

            // Act

            Guid patientId = Guid.NewGuid();
            IActionResult actionResult = await _controller.BloodPressures(patientId, input);

            // Assert
            CreatedAtActionResult createdAtActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;
            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.BloodPressures));
            createdAtActionResult.RouteValues.Should()
                .HaveCount(2).And
                .ContainKey("id").WhichValue.Should().Be(patientId);
            createdAtActionResult.RouteValues.Should()
                .ContainKey("bloodPressureId").WhichValue.Should()
                .BeOfType<Guid>().And
                .NotBe(Guid.Empty);

            BloodPressureInfo resource = createdAtActionResult.Value.Should().BeOfType<BloodPressureInfo>().Which;

            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(patientId);
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
            Guid measureId = Guid.NewGuid();
            _physiologicalMeasureFacadeMock.Setup(mock => mock.AddNewMeasureAsync<BodyWeight, BodyWeightInfo>(It.IsAny<ICommand<Guid, CreatePhysiologicalMeasureInfo<BodyWeight>>>()))
                .Returns((ICommand<Guid, CreatePhysiologicalMeasureInfo<BodyWeight>> localCmd) => Task.FromResult(new BodyWeightInfo
                {
                    Id = measureId,
                    DateOfMeasure = localCmd.Data.Measure.DateOfMeasure,
                    PatientId = localCmd.Data.PatientId,
                    Value = localCmd.Data.Measure.Value
                }));

            // Act

            Guid patientId = Guid.NewGuid();
            IActionResult actionResult = await _controller.BodyWeights(patientId, input);

            // Assert
            CreatedAtActionResult createdAtActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;
            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.BodyWeights));
            createdAtActionResult.RouteValues.Should()
                .HaveCount(2).And
                .ContainKey("id").WhichValue.Should()
                .BeOfType<Guid>().And
                .Be(patientId);
            createdAtActionResult.RouteValues.Should()
                .ContainKey("bodyWeightId").WhichValue.Should()
                .BeOfType<Guid>().And
                .Be(measureId);

            BodyWeightInfo resource = createdAtActionResult.Value.Should().BeOfType<BodyWeightInfo>().Which;

            resource.Should().NotBeNull();
            resource.PatientId.Should().Be(patientId);
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
            using (IUnitOfWork uow = _factory.New())
            {
                uow.Repository<BloodPressure>().Create(measuresInStore);
                await uow.SaveChangesAsync();
            }

            _physiologicalMeasureFacadeMock.Setup(mock => mock.GetMostRecentMeasuresAsync<BloodPressure, BloodPressureInfo>(It.IsAny<IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<BloodPressureInfo>>>()))
                .Returns((IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<BloodPressureInfo>> input) => Task.Run(async () =>
                {
                    using (IUnitOfWork uow = _factory.New())
                    {
                        IPagedResult<BloodPressureInfo> mostRecentMeasures = await uow.Repository<BloodPressure>()
                            .WhereAsync(
                                _mapper.ConfigurationProvider.ExpressionBuilder.CreateMapExpression<BloodPressure, BloodPressureInfo>(),
                                (BloodPressure x) => x.Patient.UUID == input.Data.PatientId,
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
            using (IUnitOfWork uow = _factory.New())
            {
                uow.Repository<Temperature>().Create(measuresInStore);
                await uow.SaveChangesAsync();
            }

            _physiologicalMeasureFacadeMock.Setup(mock => mock.GetMostRecentMeasuresAsync<Temperature, TemperatureInfo>(It.IsAny<IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TemperatureInfo>>>()))
                .Returns((IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TemperatureInfo>> input) => Task.Run(async () =>
                {
                    using (IUnitOfWork uow = _factory.New())
                    {
                        IPagedResult<TemperatureInfo> mostRecentMeasures = await uow.Repository<Temperature>()
                            .WhereAsync(
                                _mapper.ConfigurationProvider.ExpressionBuilder.CreateMapExpression<Temperature, TemperatureInfo>(),
                                (TemperatureInfo x) => x.PatientId == input.Data.PatientId,
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

        /// <summary>
        /// Tests that <see cref="PatientsController.MostRecentPrescriptions(Guid, int?)"/> 
        /// </summary>
        /// <param name="prescriptions">Results returned by <see cref="PrescriptionService.GetMostRecentPrescriptionsAsync(IQuery{Guid, GetMostRecentPrescriptionsInfo, IEnumerable{PrescriptionHeaderInfo}})"/></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(GetMostRecentPrescriptionsCases))]
        public async Task GetMostRecentPrescriptionsMesures(IEnumerable<PrescriptionHeaderInfo> prescriptions, GetMostRecentPrescriptionsInfo query)
        {
            _outputHelper.WriteLine($"Current store state : {prescriptions}");
            _outputHelper.WriteLine($"Query : {query}");

            

            _prescriptionServiceMock.Setup(mock => mock.GetMostRecentPrescriptionsAsync(It.IsAny<IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>>>()))
                .ReturnsAsync(prescriptions);

            // Act
            IEnumerable<PrescriptionHeaderInfo> results = await _controller.MostRecentPrescriptions(query.PatientId, query.Count);

            // Assert
            _prescriptionServiceMock.VerifyAll();
            results.Should().BeEquivalentTo(prescriptions);
        }


        [Fact]
        public async Task CreatePrescriptionForPatient()
        {
            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.CreatePrescriptionForPatientAsync(It.IsAny<Guid>(), It.IsAny<CreatePrescriptionInfo>()))
                .Returns((Guid id, CreatePrescriptionInfo input) => Task.Run(() =>
                {
                    return new PrescriptionHeaderInfo
                    {
                        Id = Guid.NewGuid(),
                        DeliveryDate = input.DeliveryDate,
                        PatientId = id,
                        PrescriptorId = input.PrescriptorId
                    };
                }));

            // Act
            Guid patientId = Guid.NewGuid();
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                PrescriptorId = Guid.NewGuid(),
                DeliveryDate = DateTimeOffset.UtcNow,
                Duration = 60,
                Items = new PrescriptionItemInfo[] {
                    new PrescriptionItemInfo { CategoryId = Guid.NewGuid(), Code = "Prescription CODE" }
                }
            };

            IActionResult actionResult = await _controller.Prescriptions(patientId, newPrescription);

            // Assert
            CreatedAtActionResult createdAtActionResult = actionResult.Should().NotBeNull().And
                .BeAssignableTo<CreatedAtActionResult>().Which;

            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.Prescriptions));
            createdAtActionResult.RouteValues.Should().HaveCount(2);
            createdAtActionResult.RouteValues.Should()
                .ContainKey("Id").WhichValue.Should()
                .BeOfType<Guid>().Which.Should()
                .Be(patientId);

            createdAtActionResult.RouteValues.Should()
                .ContainKey("prescriptionId").WhichValue.Should()
                .BeOfType<Guid>().Which.Should()
                .NotBeEmpty();


            IBrowsableResource<PrescriptionHeaderInfo> browsableResource = createdAtActionResult.Value.Should()
                .NotBeNull().And
                .BeAssignableTo<IBrowsableResource<PrescriptionHeaderInfo>>().Which;

            browsableResource.Links.Should()
                .NotBeNullOrEmpty().And
                .Contain(x => x.Relation == nameof(Prescription.Items));

            Link linkToItems = browsableResource.Links.Single(x => x.Relation == nameof(Prescription.Items));

            linkToItems.Should()
                .NotBeNull();
            linkToItems.Relation.Should().BeEquivalentTo(nameof(Prescription.Items));
            linkToItems.Method.Should().BeEquivalentTo("get");
            linkToItems.Href.Should().MatchRegex($@"api\/{PrescriptionsController.EndpointName}\/{nameof(PrescriptionsController.Details)}\?[iI]d=\d+");



            PrescriptionHeaderInfo resource = browsableResource.Resource;
            resource.PatientId.Should().Be(patientId);
            resource.PrescriptorId.Should().Be(newPrescription.PrescriptorId);
            resource.DeliveryDate.Should().Be(newPrescription.DeliveryDate);

            _prescriptionServiceMock.Verify(mock => mock.CreatePrescriptionForPatientAsync(patientId, newPrescription), Times.Once);
        }

        [Fact]
        public void CreatePrescriptionForPatientShouldNotSwallowArgumentNullException()
        {
            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.CreatePrescriptionForPatientAsync(It.IsAny<Guid>(), It.IsAny<CreatePrescriptionInfo>()))
                .Throws<ArgumentNullException>();

            // Act
            Guid patientId = Guid.NewGuid();
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                PrescriptorId = Guid.NewGuid(),
                DeliveryDate = DateTimeOffset.UtcNow,
                Duration = 60,
                Items = new PrescriptionItemInfo[] {
                    new PrescriptionItemInfo { CategoryId = Guid.NewGuid(), Code = "Prescription CODE" }
                }
            };

            Func<Task> action = async () => await _controller.Prescriptions(patientId, newPrescription);

            // Assert
            action.ShouldThrow<ArgumentNullException>();

            _prescriptionServiceMock.Verify(mock => mock.CreatePrescriptionForPatientAsync(patientId, newPrescription), Times.Once);
        }


        /// <summary>
        /// Verifies that <see cref="PatientsController.Prescriptions(Guid, CreatePrescriptionInfo)"/> does not swallow <see cref="ArgumentOutRangeException"/>
        /// </summary>
        [Fact]
        public void CreatePrescriptionForPatientShouldNotSwallowArgumentOutOfRangeException()
        {
            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.CreatePrescriptionForPatientAsync(It.IsAny<Guid>(), It.IsAny<CreatePrescriptionInfo>()))
                .Throws<ArgumentOutOfRangeException>();

            // Act
            Guid patientId = Guid.NewGuid();
            CreatePrescriptionInfo newPrescription = new CreatePrescriptionInfo
            {
                PrescriptorId = Guid.NewGuid(),
                DeliveryDate = DateTimeOffset.UtcNow,
                Duration = 60,
                Items = new PrescriptionItemInfo[] {
                    new PrescriptionItemInfo { CategoryId = Guid.NewGuid(), Code = "Prescription CODE" }
                }
            };

            Func<Task> action = async () => await _controller.Prescriptions(patientId, newPrescription);

            // Assert
            action.ShouldThrow<ArgumentOutOfRangeException>();

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
            IActionResult actionResult = await _controller.Temperatures(Guid.NewGuid(), Guid.NewGuid());

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
            IActionResult actionResult = await _controller.BloodPressures(Guid.NewGuid(), Guid.NewGuid());

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
            IActionResult actionResult = await _controller.BodyWeights(Guid.NewGuid(), Guid.NewGuid());

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
            IActionResult actionResult = await _controller.BloodPressures(new DeletePhysiologicalMeasureInfo { Id = Guid.NewGuid(), MeasureId = Guid.NewGuid() });

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
            IActionResult actionResult = await _controller.Temperatures(new DeletePhysiologicalMeasureInfo { Id = Guid.NewGuid(), MeasureId = Guid.NewGuid() });

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
            IActionResult actionResult = await _controller.BodyWeights(new DeletePhysiologicalMeasureInfo { Id = Guid.NewGuid(), MeasureId = Guid.NewGuid() });

            // Assert
            actionResult.Should().BeOfType<OkResult>();
            _physiologicalMeasureFacadeMock.Verify(mock => mock.DeleteOnePhysiologicalMeasureAsync<BodyWeight>(It.IsAny<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>()), Times.Once);
        }

        public static IEnumerable<object> GetAllDocumentsCases
        {
            get
            {
                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        Enumerable.Empty<DocumentMetadataInfo>(),
                        patientId,
                        new PaginationConfiguration { Page = 1, PageSize = 10 },
                        ((Expression<Func<IEnumerable<DocumentMetadataInfo>, bool>>) (x => x != null && !x.Any())), // expected link to first page
                        ((Expression<Func<Link, bool>>)(first =>
                                first != null &&
                                first.Relation == "first" &&
                                first.Href != null &&
                                first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                                first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[0] == $"api/{PatientsController.EndpointName}/{nameof(PatientsController.Documents)}" &&
                                first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 3 &&
                                first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => $"{nameof(PaginationConfiguration.Page)}=1".Equals(x, CurrentCultureIgnoreCase) )  &&
                                first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => $"{nameof(PaginationConfiguration.PageSize)}=10".Equals(x, CurrentCultureIgnoreCase) )  &&
                                first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => $"{nameof(PatientInfo.Id)}={patientId}".Equals(x, CurrentCultureIgnoreCase))
                               )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))


                    };
                }
            }
        }

        /// <summary>
        /// Tests getting all the documents of a patient
        /// </summary>
        /// <param name="items"></param>
        /// <param name="patientId"></param>
        /// <param name="getQuery"></param>
        /// <param name="pageOfResultExpectation"></param>
        /// <param name="firstPageUrlExpectation"></param>
        /// <param name="previousPageUrlExpectation"></param>
        /// <param name="nextPageUrlExpectation"></param>
        /// <param name="lastPageUrlExpectation"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(GetAllDocumentsCases))]
        public async Task GetDocuments(IEnumerable<DocumentMetadataInfo> items, Guid patientId, PaginationConfiguration getQuery,
            Expression<Func<IEnumerable<DocumentMetadataInfo>, bool>> pageOfResultExpectation,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {



            // Arrange
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });
            _iHandleGetDocumentsByPatientIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantDocumentsByPatientIdQuery>()))
                .Returns((IWantDocumentsByPatientIdQuery query) => Task.Run(() =>
               {
                   Func<DocumentMetadataInfo, bool> filter = x => x.PatientId == query.Data.PatientId;
                   PaginationConfiguration pageConfiguration = query.Data.PageConfiguration;
                   IEnumerable<DocumentMetadataInfo> documents = items
                       .Where(filter)
                       .Skip(pageConfiguration.PageSize * (pageConfiguration.Page < 1 ? 1 : pageConfiguration.Page))
                       .Take(pageConfiguration.PageSize);

                   int total = items.Count(filter);

                   return (IPagedResult<DocumentMetadataInfo>)new PagedResult<DocumentMetadataInfo>(items, total, pageConfiguration.PageSize);
               }));

            // Act
            IActionResult actionResult = await _controller.Documents(patientId, getQuery.Page, getQuery.PageSize);


            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(PatientsController)}.{nameof(PatientsController.Documents)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");

            actionResult.Should()
                    .NotBeNull()
                    .And.BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            IGenericPagedGetResponse<DocumentMetadataInfo> pageOfResult = okObjectResult.Value.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<IGenericPagedGetResponse<DocumentMetadataInfo>>().Which;

            pageOfResult.Items.Should().Match(pageOfResultExpectation);

            pageOfResult?.Links?.First?.Should().Match(firstPageUrlExpectation);
            pageOfResult?.Links?.Previous?.Should().Match(previousPageUrlExpectation);
            pageOfResult?.Links?.Next?.Should().Match(nextPageUrlExpectation);
            pageOfResult?.Links?.Last?.Should().Match(lastPageUrlExpectation);
        }

        /// <summary>
        /// Test creating a new document
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostDocument()
        {
            // Arrange
            Mock<IFormFile> fileMock = new Mock<IFormFile>();
            //Setup mock file using a memory stream
            string content = "Hello World from a Fake File";
            MemoryStream ms = new MemoryStream();
            StreamWriter writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(mock => mock.OpenReadStream()).Returns(ms);
            fileMock.SetupGet(mock => mock.FileName).Returns(@"C:\Users\Batman\dummy-file.txt");
            fileMock.SetupGet(mock => mock.Name).Returns(@"Dummy file");
            fileMock.SetupGet(mock => mock.Length).Returns(ms.Length);



            _iRunCreateDocumentForPatientCommandMock.Setup(mock => mock.RunAsync(It.IsNotNull<ICreateDocumentForPatientCommand>()))
                .Returns((ICreateDocumentForPatientCommand cmd) => Task.Run(() =>
                {
                    DocumentMetadataInfo createdDocument = new DocumentMetadataInfo
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = Guid.NewGuid(),
                        MimeType = cmd.Data.Document.MimeType,
                        Size = 500,
                        Title = cmd.Data.Document.Title,
                        PatientId = cmd.Data.PatientId
                    };
                    createdDocument.PatientId = cmd.Data.PatientId;
                    return createdDocument;
                }));

            // Act
            IFormFile file = fileMock.Object;

            IActionResult actionResult = await _controller.Documents(Guid.NewGuid(), file);


            // Assert
            CreatedAtActionResult createdAtActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;


            createdAtActionResult.ControllerName.Should().Be(PatientsController.EndpointName);
            createdAtActionResult.ActionName.Should().Be(nameof(PatientsController.Documents));
            createdAtActionResult.RouteValues.Should()
                .ContainKey(nameof(PatientInfo.Id)).And
                .ContainKey("documentId");

            IBrowsableResource<DocumentMetadataInfo> browsableResource = createdAtActionResult.Value.Should()
                .NotBeNull().And
                .BeAssignableTo<IBrowsableResource<DocumentMetadataInfo>>().Which;

            IEnumerable<Link> links = browsableResource.Links;

            links?.Should()
                .NotBeNullOrEmpty().And
                .Contain(x => x.Relation == "direct-link", "link to get the document should be provided").And
                .Contain(x => x.Relation == "file", "link to download the file must be provided");

            DocumentMetadataInfo resource = browsableResource.Resource;

            resource?.Should()
                .NotBeNull();

            resource.DocumentId.Should().NotBeEmpty();
            resource.PatientId.Should().NotBeEmpty();
            resource.Title.Should().Be(file.Name);

            _iRunCreateDocumentForPatientCommandMock.Verify(mock => mock.RunAsync(It.IsAny<ICreateDocumentForPatientCommand>()), Times.Once);

        }


        /// <summary>
        /// Tests <see cref="PatientsController.Documents(int, int)"/>
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetOneDocumentByPatientIdAndDocumentId()
        {
            // Arrange
            _iHandleGetOneDocumentInfoByPatientIdAndDocumentIdMock.Setup(mock => mock.HandleAsync(It.IsNotNull<IWantOneDocumentByPatientIdAndDocumentIdQuery>()))
                .Returns((IWantOneDocumentByPatientIdAndDocumentIdQuery query) => Task.Run(() =>
                {
                    DocumentMetadataInfo documentMetadataInfo = new DocumentMetadataInfo
                    {
                        Id = query.Data.DocumentMetadataId,
                        DocumentId = query.Data.DocumentMetadataId,
                        MimeType = "application/pdf",
                        Size = 500,
                        Title = "Document 1",
                        PatientId = query.Data.PatientId
                    };

                    return documentMetadataInfo;
                }));


            // Act
            Guid patientId = Guid.NewGuid(),
                documentMetadataId = Guid.NewGuid();
            IActionResult actionResult = await _controller.Documents(patientId, documentMetadataId);


            // Assert
            IBrowsableResource<DocumentMetadataInfo> browsableResource = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IBrowsableResource<DocumentMetadataInfo>>().Which;


            DocumentMetadataInfo actualResource = browsableResource.Resource;
            actualResource.Should().NotBeNull();
            actualResource.PatientId.Should().NotBeEmpty();
            actualResource.Id.Should().Be(documentMetadataId);
            actualResource.MimeType.Should().Be("application/pdf");
            actualResource.Size.Should().Be(500);


            IEnumerable<Link> links = browsableResource.Links;

            links.Should()
                .NotBeNullOrEmpty().And
                .Contain(x => x.Relation == "direct-link", "link to get the document should be provided").And
                .Contain(x => x.Relation == "file", "link to download the file must be provided");

            Link directLink = links.Single(x => x.Relation == "direct-link");
            directLink.Method.Should().BeNullOrEmpty();
            directLink.Template.Should().NotHaveValue();
            directLink.Href.Should()
                .BeEquivalentTo($"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?{nameof(DocumentMetadataInfo.Id)}={documentMetadataId}");

            Link fileLink = links.Single(x => x.Relation == "file");
            fileLink.Method.Should().BeNullOrEmpty();
            fileLink.Template.Should().NotHaveValue();
            fileLink.Href.Should()
                .BeEquivalentTo($"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.File)}?{nameof(DocumentInfo.Id)}={documentMetadataId}");


            _iHandleGetOneDocumentInfoByPatientIdAndDocumentIdMock.Verify(mock => mock.HandleAsync(It.IsAny<IWantOneDocumentByPatientIdAndDocumentIdQuery>()), Times.Once);
        }

        /// <summary>
        /// Tests <see cref="PatientsController.Documents(int, int)"/>
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetOneDocumentByPatientIdAndDocumentId_Returns_Not_Found()
        {
            // Arrange
            _iHandleGetOneDocumentInfoByPatientIdAndDocumentIdMock.Setup(mock => mock.HandleAsync(It.IsNotNull<IWantOneDocumentByPatientIdAndDocumentIdQuery>()))
                .ReturnsAsync(null);


            // Act
            Guid patientId = Guid.NewGuid(),
                documentMetadataId = Guid.NewGuid();
            IActionResult actionResult = await _controller.Documents(patientId, documentMetadataId);


            // Assert
            actionResult.Should()
                 .NotBeNull().And
                 .BeOfType<NotFoundResult>();

            _iHandleGetOneDocumentInfoByPatientIdAndDocumentIdMock.Verify(mock => mock.HandleAsync(It.IsAny<IWantOneDocumentByPatientIdAndDocumentIdQuery>()), Times.Once);
        }

    }
}