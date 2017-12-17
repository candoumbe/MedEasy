using FluentAssertions;
using GenFu;
using Measures.API;
using Measures.API.Context;
using Measures.API.Controllers;
using Measures.API.Routing;
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

namespace MedEasy.WebApi.Tests
{
    [Collection("Patient")]
    public class PatientsControllerTests : IDisposable
    {
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<ILogger<PatientsController>> _loggerMock;
        private PatientsController _controller;
        private ITestOutputHelper _outputHelper;
        private IActionContextAccessor _actionContextAccessor;
        private Mock<IOptionsSnapshot<MeasuresApiOptions>> _apiOptionsMock;
        private const string _baseUrl = "http://host/api";
        private EFUnitOfWorkFactory<MeasuresContext> _uowFactory;

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
                int[] pageSizes = { 0, int.MinValue, int.MaxValue };
                int[] pages = { 0, int.MinValue, int.MaxValue };


                foreach (int pageSize in pageSizes)
                {
                    foreach (int page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<Patient>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" &&
                                ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&page=1" +
                                $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            ((Expression<Func<Link, bool>>) (x => x == null))  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<Patient> items = A.ListOf<Patient>(400);
                    items.ForEach(item => item.Id = default);
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }
                {
                    IEnumerable<Patient> items = A.ListOf<Patient>(400);
                    items.ForEach(item => item.Id = default);

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
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
                            && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&page=1" +
                                $"&pageSize={PaginationConfiguration.DefaultPageSize}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
                    };
            }
        }
        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Patient> items, int pageSize, int page,
            int expectedCount,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync();
            }

            
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });

            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { Page = page, PageSize = pageSize });

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

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);

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
                            && x.Relation == "first"
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == "last"
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
                            && x.Relation == "first"
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))),
                            ((Expression<Func<Link, bool>>)(previous => previous == null)),
                            ((Expression<Func<Link, bool>>)(next => next == null)),
                            ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == "last"
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
                                && x.Relation == "first"
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                    $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>)(previous => previous == null)),
                            ((Expression<Func<Link, bool>>)(next => next == null)),
                            ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == "last"
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
                            && x.Relation == "first"
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value.ToString("s")}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == "last"
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
                await uow.SaveChangesAsync();
            }

            // Act
            IActionResult actionResult = await _controller.Search(searchRequest);

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
        [MemberData(nameof(GetMostRecentTemperaturesMeasuresCases))]
        public async Task GetLastTemperaturesMesures(IEnumerable<Temperature> measuresInStore, GetMostRecentPhysiologicalMeasuresInfo query, Expression<Func<IEnumerable<TemperatureInfo>, bool>> resultExpectation)
        {
            _outputHelper.WriteLine($"Current store state : {measuresInStore}");
            _outputHelper.WriteLine($"Query : {query}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<Temperature>().Create(measuresInStore);
                await uow.SaveChangesAsync();
            }

            
            // Act
            IActionResult actionResult = await _controller.MostRecentTemperatures(query.PatientId, query.Count, CancellationToken.None);

            // Assert
            actionResult.Should()
                 .BeAssignableTo<OkObjectResult>().Which
                 .Value.Should()
                     .BeAssignableTo<IEnumerable<BrowsableResource<TemperatureInfo>>>().Which.Should()
                        .NotContainNulls().And
                        .NotContain(x => x.Resource == null).And
                        .NotContain(x => x.Links == null).And.Subject
                        .Select(x => x.Resource).Should()
                            .Match(resultExpectation);
        }

        

        

        

    }
}