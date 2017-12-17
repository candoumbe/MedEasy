using AutoMapper.QueryableExtensions;
using FluentAssertions;
using GenFu;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Optional;
using Patients.API.Context;
using Patients.API.Controllers;
using Patients.API.Routing;
using Patients.CQRS.Commands;
using Patients.DTO;
using Patients.Mapping;
using Patients.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.DAL.Repositories.SortDirection;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using static System.StringComparison;

namespace Patients.API.Tests.Controllers
{

    [Collection("Patient")]
    public class PatientsControllerTests : IDisposable
    {
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<ILogger<PatientsController>> _loggerMock;
        private PatientsController _controller;
        private ITestOutputHelper _outputHelper;
        private IActionContextAccessor _actionContextAccessor;
        private EFUnitOfWorkFactory<PatientsContext> _factory;
        private IExpressionBuilder _expressionBuilder;
        private Mock<IOptionsSnapshot<PatientsApiOptions>> _apiOptionsMock;
        private const string _baseUrl = "http://host/api";
        private Mock<IRunPatchPatientCommand> _runPatchPatientCommand;
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

            DbContextOptionsBuilder<PatientsContext> dbOptions = new DbContextOptionsBuilder<PatientsContext>();
            dbOptions.UseInMemoryDatabase($"InMemoryMedEasyDb_{Guid.NewGuid()}");
            _factory = new EFUnitOfWorkFactory<PatientsContext>(dbOptions.Options, (options) => new PatientsContext(options));


            _apiOptionsMock = new Mock<IOptionsSnapshot<PatientsApiOptions>>(Strict);
            _expressionBuilder = AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder;
            _runPatchPatientCommand = new Mock<IRunPatchPatientCommand>(Strict);

            _controller = new PatientsController(
                _loggerMock.Object,
                _urlHelperMock.Object,
                _apiOptionsMock.Object,
                _expressionBuilder,
                    _factory);

        }

        public void Dispose()
        {
            _loggerMock = null;
            _urlHelperMock = null;
            _controller = null;
            _outputHelper = null;
            _actionContextAccessor = null;
            _apiOptionsMock = null;

            _factory = null;
            _expressionBuilder = null;
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
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First &&
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
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
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
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
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
                            && x.Relation == LinkRelation.First
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
            using (IUnitOfWork uow = _factory.New())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync();
            }
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new PatientsApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });

            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { Page = page, PageSize = pageSize });

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(PatientsController)}.{nameof(PatientsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(PatientsApiOptions.MaxPageSize)} value");

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


        public static IEnumerable<object[]> SearchCases
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
                        ((
                            firstPageLinkExpectation : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={searchInfo.Firstname}"+
                                    $"&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}" +
                                    $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previousPageLinkExpectation : ((Expression<Func<Link, bool>>)(previous => previous == null)),
                            nextPageLinkExpectation : ((Expression<Func<Link, bool>>)(next => next == null)),
                            lastPageLinkExpectation :((Expression<Func<Link, bool>>)(x => x != null
                                && x.Relation == "last"
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={searchInfo.Firstname}"+
                                    $"&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}" +
                                    $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase)))
                        ))

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
                            new PatientInfo { Firstname = "Bruce", Lastname = "Wayne" }
                        },
                        searchInfo,
                        (
                           ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page,
                            ((Expression<Func<Link, bool>>)(previous => previous == null)),
                            ((Expression<Func<Link, bool>>)(next => next == null)),
                            ((Expression<Func<Link, bool>>)(last => last != null
                                && last.Relation == "last"
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                    $"&page=1&pageSize=30" +
                                    $"&sort={searchInfo.Sort}").Equals(last.Href, OrdinalIgnoreCase)))
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
                            new PatientInfo { Firstname = "bruce" }
                        },
                        searchInfo,
                        (
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last != null
                            && last.Relation == LinkRelation.Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30").Equals(last.Href, OrdinalIgnoreCase)
                        )))

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
                        ( ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value.ToString("s")}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last != null
                            && last.Relation == LinkRelation.Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value.ToString("s")}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30").Equals(last.Href, OrdinalIgnoreCase)
                        )))

                    };
                }

            }
        }


        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<PatientInfo> entries, SearchPatientInfo searchRequest,
        (Expression<Func<Link, bool>> firstPageLink, Expression<Func<Link, bool>> previousPageLink, Expression<Func<Link, bool>> nextPageLink, Expression<Func<Link, bool>> lastPageLink) linksExpectation)
        {
            _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
            _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");


            // Arrange
            PatientsApiOptions apiOptions = new PatientsApiOptions { DefaultPageSize = 30, MaxPageSize = 50 };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);

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

            content.Links.Should().NotBeNull();
            PagedRestResponseLink links = content.Links;

            links.First.Should().Match(linksExpectation.firstPageLink);
            links.Previous.Should().Match(linksExpectation.previousPageLink);
            links.Next.Should().Match(linksExpectation.nextPageLink);
            links.Last.Should().Match(linksExpectation.lastPageLink);
        }

        public static IEnumerable<object[]> PatchCases
        {
            get
            {
                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Firstname, "Bruce");
                    yield return new object[]
                    {
                        new Patient { Id = 1, UUID = Guid.NewGuid(), Lastname = "Wayne"  },
                        patchDocument,
                        ((Expression<Func<Patient, bool>>)(x => x.Id == 1 && x.Firstname == "Bruce" && x.Lastname == "Wayne"))
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(PatchCases))]
        public async Task Patch(Patient source, JsonPatchDocument<PatientInfo> patchDocument, Expression<Func<Patient, bool>> patchResultExpectation)
        {

            // Arrange
            Guid patientId = source.UUID;


            // Act
            IActionResult actionResult = await _controller.Patch(source.UUID, patchDocument);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<NoContentResult>();
            
            source.Should().Match(patchResultExpectation);

        }


        [Fact]
        public async Task PatchUnknownIdReturnsNotFound()
        {

            // Arrange
            JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
            patchDocument.Replace(x => x.Firstname, "John");


            // Act
            IActionResult actionResult = await _controller.Patch(Guid.NewGuid(), patchDocument);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<NotFoundResult>();
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
            PatientInfo expectedResource = new PatientInfo
            {
                Id = patientId,
                Firstname = "Bruce",
                Lastname = "Wayne"
            };

            using (IUnitOfWork uow = _factory.New())
            {
                uow.Repository<Patient>().Create(new Patient { UUID = patientId, Firstname = "Bruce", Lastname = "Wayne" });
                await uow.SaveChangesAsync();
            }

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
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation), $"Each {nameof(BrowsableResource<PatientInfo>.Links)}'s must have a not null/empty {nameof(Link.Relation)}").And
                .Contain(x => x.Relation == LinkRelation.Self, "Direct link to the resource must be provided").And
                .Contain(x => x.Relation == "delete", "Link to delete the resource must be provided");

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

            PatientInfo actualResource = result.Resource;
            actualResource.Should().NotBeNull();
            actualResource.Id.Should().Be(expectedResource.Id);
            actualResource.Firstname.Should().Be(expectedResource.Firstname);
            actualResource.Lastname.Should().Be(expectedResource.Lastname);

            _urlHelperMock.Verify();

        }

        [Fact]
        public async Task Post()
        {
            //Arrange
            Guid patientId = Guid.NewGuid();
            
            //Act
            CreatePatientInfo info = new CreatePatientInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne"
            };

            IActionResult actionResult = await _controller.Post(info);

            //Assert

            using (IUnitOfWork uow = _factory.New())
            {
                (await uow.Repository<Patient>().AnyAsync(x => x.Firstname == "Bruce" && x.Lastname == "Wayne")).Should().BeTrue();
            }

            CreatedAtRouteResult createdActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtRouteResult>().Which;


            createdActionResult.RouteName.Should().Be(RouteNames.DefaultGetOneByIdApi);
            createdActionResult.RouteValues.Should()
                .HaveCount(2).And
                .ContainKey("id").And
                .ContainKey("controller");

            createdActionResult.RouteValues["id"].Should()
                .BeOfType<Guid>().Which.Should()
                .NotBeEmpty();
            createdActionResult.RouteValues["controller"].Should()
                .BeOfType<string>().Which.Should()
                .Be(PatientsController.EndpointName);


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

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeNullOrEmpty().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .Contain(x => x.Relation == "delete");


        }


        [Fact]
        public async Task DeletePatientByEmptyIdReturnsBadRequest()
        {
            //Arrange

            //Act
            IActionResult actionResult = await _controller.Delete(Guid.Empty);

            //Assert
            actionResult.Should().BeAssignableTo<BadRequestResult>();
        }
    }
}