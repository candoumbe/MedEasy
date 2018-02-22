using FluentAssertions;
using GenFu;
using Measures.API.Controllers;
using Measures.API.Routing;
using Measures.Context;
using Measures.DTO;
using Measures.Mapping;
using Measures.Objects;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using static System.StringComparison;
using FluentAssertions.Extensions;
using Xunit.Categories;

namespace Measures.API.Tests
{
    [Feature("Patients")]
    public class PatientsControllerTests : IDisposable
    {
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<ILogger<PatientsController>> _loggerMock;
        private PatientsController _controller;
        private ITestOutputHelper _outputHelper;
        private IActionContextAccessor _actionContextAccessor;
        private Mock<IOptionsSnapshot<MeasuresApiOptions>> _apiOptionsMock;
        private const string _baseUrl = "http://host/api";
        private IUnitOfWorkFactory _uowFactory;

        public PatientsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<PatientsController>>(Strict);
            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routename, object routeValues) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _actionContextAccessor = new ActionContextAccessor()
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            DbContextOptionsBuilder<MeasuresContext> dbOptions = new DbContextOptionsBuilder<MeasuresContext>();
            string dbName = $"InMemoryMedEasyDb_{Guid.NewGuid()}";
            dbOptions.UseInMemoryDatabase(dbName);
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbOptions.Options, (options) => new MeasuresContext(options));

            _apiOptionsMock = new Mock<IOptionsSnapshot<MeasuresApiOptions>>(Strict);
            
            _controller = new PatientsController(
                _loggerMock.Object,
                _urlHelperMock.Object,
                _apiOptionsMock.Object,
                AutoMapperConfig.Build().ExpressionBuilder,
                _uowFactory);

        }

        public void Dispose()
        {
            _loggerMock = null;
            _urlHelperMock = null;
            _controller = null;
            _outputHelper = null;
            _actionContextAccessor = null;
            _apiOptionsMock = null;
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

        public static IEnumerable<object[]> GetMostRecentTemperaturesMeasuresCases
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

        
        public static IEnumerable<object[]> GetAllTestCases
        {
            get
            {
                int[] pageSizes = { 1, 10, 20};
                int[] pages = { 1, 5, 10 };


                foreach (int pageSize in pageSizes)
                {
                    foreach (int page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<Patient>(), // Current store state
                            (pageSize, page), // request,
                            (defaultPageSize : 30, maxPageSize : 200),
                            0,    //expected total
                            (
                                first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                next :(Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase))
                            )  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<Patient> items = A.ListOf<Patient>(400);
                    items.ForEach(item => item.Id = default);
                    yield return new object[]
                    {
                        items,
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)))
                        )  // expected link to last page
                    };
                }
                {
                    IEnumerable<Patient> items = A.ListOf<Patient>(400);
                    items.ForEach(item => item.Id = default);

                    yield return new object[]
                    {
                        items,
                        (pageSize : 10, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                        )
                    };
                }

                yield return new object[]
                    {
                        new [] {
                            new Patient { Id = 1, Firstname = "Bruce",  Lastname = "Wayne" }
                        },
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        1,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous :((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)))
                        ), // expected link to last page
                    };
            }
        }
        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Patient> items, (int pageSize, int page) request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {request.pageSize}");
            _outputHelper.WriteLine($"Page : {request.page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync();
            }

            
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { Page = request.page, PageSize = request.pageSize });

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(PatientsController)}.{nameof(PatientsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MeasuresApiOptions.MaxPageSize)} value");

            actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<IGenericPagedGetResponse<BrowsableResource<PatientInfo>>>();

            IGenericPagedGetResponse<BrowsableResource<PatientInfo>> response = (IGenericPagedGetResponse<BrowsableResource<PatientInfo>>)value;

            _outputHelper.WriteLine($"response : {response}");

            response.Items.Should()
                .NotBeNull();

            if (response.Items.Any())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self));
            }

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<PatientInfo>)}.{nameof(IGenericPagedGetResponse<PatientInfo>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);

        }


        public static IEnumerable<object[ ] > SearchCases
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
                        Enumerable.Empty<Patient>(),
                        searchInfo,
                        ((
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1" +
                                $"&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))))
                        )

                    };

                }
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Lastname = "!wayne",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };
                    yield return new object[]
                    {
                        new [] {
                            new Patient { Firstname = "Bruce", Lastname = "Wayne" }
                        },
                        searchInfo,
                        (
                           ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))),
                            ((Expression<Func<Link, bool>>)(previous => previous == null)),
                            ((Expression<Func<Link, bool>>)(next => next == null)),
                            ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                    $"&page=1&pageSize=30" +
                                    $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase)))
                        )
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
                            new Patient{ Firstname = "bruce" }
                        },
                        searchInfo,
                        (
                            ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                    $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>)(previous => previous == null)),
                            ((Expression<Func<Link, bool>>)(next => next == null)),
                            ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                    $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase)))
                        )

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
                            new Patient { Firstname = "bruce", BirthDate = 31.July(2010) }
                        },
                        searchInfo,
                        ( ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value.ToString("s")}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value.ToString("s")}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))))

                    };
                }

            }
        }


        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<Patient> entries, SearchPatientInfo searchRequest,
        (Expression<Func<Link, bool>> firstPageLink, Expression<Func<Link, bool>> previousPageLink, Expression<Func<Link, bool>> nextPageLink, Expression<Func<Link, bool>> lastPageLink) linksExpectation)
        {
            _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
            _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");


            // Arrange
            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<Patient>().Create(entries);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            // Act
            IActionResult actionResult = await _controller.Search(searchRequest)
                    .ConfigureAwait(false);

            // Assert
            IGenericPagedGetResponse<BrowsableResource<PatientInfo>> content = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IGenericPagedGetResponse<BrowsableResource<PatientInfo>>>().Which;


            content.Items.Should()
                .NotBeNull($"{nameof(IGenericPagedGetResponse<object>.Items)} must not be null").And
                .NotContainNulls($"{nameof(IGenericPagedGetResponse<object>.Items)} must not contains null").And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            content.Links.Should()
                .NotBeNull();
            PagedRestResponseLink links = content.Links;

            links.First.Should().Match(linksExpectation.firstPageLink);
            links.Previous.Should().Match(linksExpectation.previousPageLink);
            links.Next.Should().Match(linksExpectation.nextPageLink);
            links.Last.Should().Match(linksExpectation.lastPageLink);
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
                        patchDocument,
                        ((Expression<Func<Patient, bool>>)(x => x.Id == 1 && x.Firstname == "Bruce"))
                    };
                }
            }
        }




        
        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            
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
            Guid patientId = Guid.NewGuid();
            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<Patient>().Create(new Patient
                {
                    UUID = patientId,
                    Firstname = "Bruce",
                    Lastname = "Wayne"
                });
                await uow.SaveChangesAsync();
            }
            PatientInfo expectedResource = new PatientInfo
            {
                Id = patientId,
                Firstname = "Bruce",
                Lastname = "Wayne"
            };
            
            //Act
            IActionResult actionResult = await _controller.Get(patientId);

            //Assert

            BrowsableResource<PatientInfo> result = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<BrowsableResource<PatientInfo>>().Which;

            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href), $"{nameof(BrowsableResource<PatientInfo>)}{nameof(BrowsableResource<PatientInfo>.Links)} cannot contain any element " +
                    $"with null/empty/whitespace {nameof(Link.Href)}s" ).And
                .ContainSingle(x => x.Relation == LinkRelation.Self).And
                .ContainSingle(x => x.Relation == "delete").And
                .ContainSingle(x => x.Relation == BloodPressuresController.EndpointName.ToLowerKebabCase());

            Link self = links.Single(x => x.Relation == LinkRelation.Self);
            self.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={patientId}");
            self.Relation.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo(LinkRelation.Self);
            self.Method.Should()
                .Be("GET");


            Link linkDelete = links.Single(x => x.Relation == "delete");
            linkDelete.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={expectedResource.Id}");
            linkDelete.Method.Should().Be("DELETE");


            Link bloodPressuresLink = links.Single(x => x.Relation == BloodPressuresController.EndpointName.ToLowerKebabCase());
            bloodPressuresLink.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?Controller={BloodPressuresController.EndpointName}&{nameof(BloodPressureInfo.PatientId)}={expectedResource.Id}");
            bloodPressuresLink.Method.Should().Be("GET");


            PatientInfo actualResource = result.Resource;
            actualResource.Should().NotBeNull();
            actualResource.Id.Should().Be(expectedResource.Id);
            actualResource.Firstname.Should().Be(expectedResource.Firstname);
            actualResource.Lastname.Should().Be(expectedResource.Lastname);
            
            _urlHelperMock.Verify();

        }

        [Fact]
        public async Task Get_Should_Returns_BadRequest()
        {
            //Arrange
           

            //Act
            IActionResult actionResult = await _controller.Get(Guid.Empty);

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<BadRequestResult>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public async Task GetLastTemperaturesMesures_Returns_NotFound(int count)
        {
            // Arrange
            
            
            // Act
            IActionResult actionResult = await _controller.MostRecentTemperatures(Guid.NewGuid(), count, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                 .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task GetLastTemperaturesMesures_Returns_OkResult()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();

            Patient p = new Patient
            {
                Lastname = "Constantine",
                UUID = patientId
            };
            IEnumerable<Temperature> measures = new[]
            {
                new Temperature { Patient = p,  Value = 37.2f, DateOfMeasure = 23.August(2007).AddHours(12).AddMinutes(30) },
                new Temperature { Patient = p,  Value = 37.2f, DateOfMeasure = 23.July(2007) }
            };


            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<Temperature>().Create(measures);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }

            // Act
            IActionResult actionResult = await _controller.MostRecentTemperatures(p.UUID, 15, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            IEnumerable<BrowsableResource<TemperatureInfo>> resources = actionResult.Should()
                 .BeAssignableTo<OkObjectResult>().Which
                 .Value.Should()
                    .BeAssignableTo<IEnumerable<BrowsableResource<TemperatureInfo>>>().Which;

            resources.Should()
                .NotContainNulls().And
                .NotContain(x => x.Links == null).And
                .NotContain(x => !x.Links.Any(), $"Each resource in the result must have one {nameof(Link)}").And
                .NotContain(x => x.Links.Any(link => string.IsNullOrWhiteSpace(link.Href))).And
                .NotContain(x => x.Links.Any(link => string.IsNullOrWhiteSpace(link.Method)), $"Each {nameof(Link)} of {nameof(BrowsableResource<TemperatureInfo>.Links)} must have its '{nameof(Link.Method)}' property set").And
                .OnlyContain(x => x.Links.Any(link => link.Relation == LinkRelation.Self), "Each resource must provide a link to itself").And
                .OnlyContain(x => x.Links.Any(link => link.Relation == "patient"), "Each resource must provide a link to the patient that owns the measure").And
                .BeInDescendingOrder(x => x.Resource.DateOfMeasure)
                ;

            resources.ForEach(resource =>
            {
                Link self = resource.Links.Single(x => x.Relation == LinkRelation.Self);
                self.Method.Should().Be("GET");

                Link linkToPatient = resource.Links.Single(x => x.Relation == "patient");
                self.Method.Should().Be("GET");

            });
        }


        public static IEnumerable<object[]> GetLastTemperatures_BadRequest_Cases
        {
            get
            {
                yield return new object[] { Guid.Empty, 10 };
                yield return new object[] { Guid.NewGuid(), int.MinValue };
                yield return new object[] { Guid.NewGuid(), -1 };
                yield return new object[] { Guid.NewGuid(), 0 };
                yield return new object[] { Guid.Empty, 0 };
            }
        }

        [Theory]
        [MemberData(nameof(GetLastTemperatures_BadRequest_Cases))]
        public async Task GetLastTemperaturesMesures_Returns_Bad_Request(Guid patientId, int count)
        {
            // Act
            IActionResult actionResult = await _controller.MostRecentTemperatures(patientId, count, CancellationToken.None);

            // Assert
            actionResult.Should()
                 .BeAssignableTo<BadRequestResult>();
        }

        


    }
}