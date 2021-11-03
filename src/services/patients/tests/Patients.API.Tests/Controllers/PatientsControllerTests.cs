namespace Patients.API.UnitTests.Controllers
{
    using AutoMapper.QueryableExtensions;

    using Bogus;

    using FluentAssertions;
    using FluentAssertions.Extensions;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;

    using MediatR;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Moq;

    using NodaTime;
    using NodaTime.Extensions;
    using NodaTime.Testing;

    using Patients.API.Controllers;
    using Patients.API.Routing;
    using Patients.Context;
    using Patients.CQRS.Commands;
    using Patients.DTO;
    using Patients.Ids;
    using Patients.Mapping;
    using Patients.Objects;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    using static Moq.MockBehavior;
    using static Newtonsoft.Json.JsonConvert;
    using static System.StringComparison;

    [UnitTest]
    [Feature("Patients")]
    public class PatientsControllerTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<PatientsContext>>
    {
        private readonly Mock<LinkGenerator> _urlHelperMock;
        private readonly Mock<ILogger<PatientsController>> _loggerMock;
        private readonly PatientsController _sut;
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _factory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly Mock<IOptionsSnapshot<PatientsApiOptions>> _apiOptionsMock;
        private const string _baseUrl = "http://host/api";
        private readonly Mock<IMediator> _mediatorMock;


        public PatientsControllerTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<PatientsContext> database)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<PatientsController>>(Strict);
            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString __, LinkOptions ___)
                => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _factory = new EFUnitOfWorkFactory<PatientsContext>(database.OptionsBuilder.Options,
                                                                (options) =>
                                                                {
                                                                    PatientsContext context = new PatientsContext(options, new FakeClock(new Instant()));
                                                                    context.Database.EnsureCreated();
                                                                    return context;
                                                                });

            _apiOptionsMock = new Mock<IOptionsSnapshot<PatientsApiOptions>>(Strict);
            _expressionBuilder = AutoMapperConfig.Build().CreateMapper().ConfigurationProvider.ExpressionBuilder;
            _mediatorMock = new(Strict);

            _sut = new PatientsController(
                _loggerMock.Object,
                _urlHelperMock.Object,
                _apiOptionsMock.Object,
                _expressionBuilder,
                _factory,
                _mediatorMock.Object);
        }

        ///<inheritdoc/>
        public async Task InitializeAsync() => await DisposeAsync().ConfigureAwait(false);

        ///<inheritdoc/>
        public async Task DisposeAsync()
        {
            using IUnitOfWork uow = _factory.NewUnitOfWork();
            uow.Repository<Patient>().Clear();

            await uow.SaveChangesAsync().ConfigureAwait(false);
        }


        public static IEnumerable<object[]> GetAllTestCases
        {
            get
            {
                int[] pageSizes = { 1, int.MaxValue };
                int[] pages = { 10, 1, int.MaxValue };

                foreach (int pageSize in pageSizes)
                {
                    foreach (int page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<Patient>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First
                                &&
                                ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                "&page=1" +
                                $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                            (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last
                                &&
                                ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&page=1" +
                                $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        };
                    }
                }

                Faker<Patient> patientFaker = new Faker<Patient>()
                    .CustomInstantiator(faker => new Patient(PatientId.New(), faker.Person.FirstName, faker.Person.LastName));

                {
                    IEnumerable<Patient> items = patientFaker.Generate(400);

                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                        (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                        (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)),  // expected link to last page
                    };
                }
                {
                    IEnumerable<Patient> items = patientFaker.Generate(400);

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                        (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                        (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)),  // expected link to last page
                    };
                }

                yield return new object[]
                    {
                        new [] {
                            new Patient(PatientId.New(), firstname: "Bruce",  lastname: "Wayne")
                        },
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        1,    //expected total
                        (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&page=1" +
                                $"&pageSize={PaginationConfiguration.DefaultPageSize}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                        (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                        (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to last page
                    };
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Patient> items,
                                 int pageSize,
                                 int page,
                                 int expectedCount,
                                 Expression<Func<Link, bool>> firstPageUrlExpectation,
                                 Expression<Func<Link, bool>> previousPageUrlExpectation,
                                 Expression<Func<Link, bool>> nextPageUrlExpectation,
                                 Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _factory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new PatientsApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });

            // Act
            IActionResult actionResult = await _sut.Get(new PaginationConfiguration { Page = page, PageSize = pageSize })
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(PatientsController)}.{nameof(PatientsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(PatientsApiOptions.MaxPageSize)} value");

            actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<Browsable<PatientInfo>>>();

            GenericPagedGetResponse<Browsable<PatientInfo>> response = (GenericPagedGetResponse<Browsable<PatientInfo>>)value;

            response.Items.Should()
                .NotBeNull();

            if (response.Items.Any())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self));
            }

            response.Total.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPagedGetResponse<PatientInfo>)}.{nameof(GenericPagedGetResponse<PatientInfo>.Total)}"" property indicates the number of elements");

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
                    SearchPatientInfo searchInfo = new()
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
                        (
                            firstPageLinkExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={searchInfo.Firstname}"+
                                    $"&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}" +
                                    $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageLinkExpectation : (Expression<Func<Link, bool>>)(previous => previous == null),
                            nextPageLinkExpectation : (Expression<Func<Link, bool>>)(next => next == null),
                            lastPageLinkExpectation :(Expression<Func<Link, bool>>)(x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={searchInfo.Firstname}"+
                                    $"&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}" +
                                    $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))
                        )
                    };
                }

                {
                    SearchPatientInfo searchInfo = new()
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
                           (Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page,
                            (Expression<Func<Link, bool>>)(previous => previous == null),
                            (Expression<Func<Link, bool>>)(next => next == null),
                            (Expression<Func<Link, bool>>)(last => last != null
                                && last.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                    $"&page=1&pageSize=30" +
                                    $"&sort={searchInfo.Sort}").Equals(last.Href, OrdinalIgnoreCase))
                        )
                    };
                }
                {
                    SearchPatientInfo searchInfo = new()
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
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        (Expression<Func<Link, bool>>)(previous => previous == null),
                        (Expression<Func<Link, bool>>)(next => next == null),
                        (Expression<Func<Link, bool>>)(last => last != null
                            && last.Relation == LinkRelation.Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30").Equals(last.Href, OrdinalIgnoreCase)
                        ))
                    };
                }

                {
                    SearchPatientInfo searchInfo = new()
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                        BirthDate = 31.July(2010)
                    };
                    yield return new object[]
                    {
                        new[] {
                            new PatientInfo { Firstname = "bruce", BirthDate = 31.July(2010).ToLocalDateTime().Date }
                        },
                        searchInfo,
                         ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value:s}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                        (Expression<Func<Link, bool>>)(previous => previous == null),
                        (Expression<Func<Link, bool>>)(next => next == null),
                        (Expression<Func<Link, bool>>)(last => last != null
                            && last.Relation == LinkRelation.Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value:s}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30").Equals(last.Href, OrdinalIgnoreCase))
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<PatientInfo> entries,
                                 SearchPatientInfo searchRequest,
                                 (Expression<Func<Link, bool>> firstPageLink, Expression<Func<Link, bool>> previousPageLink, Expression<Func<Link, bool>> nextPageLink, Expression<Func<Link, bool>> lastPageLink) linksExpectation)
        {
            _outputHelper.WriteLine($"Entries : {entries.Jsonify()}");
            _outputHelper.WriteLine($"Request : {searchRequest.Jsonify()}");

            // Arrange
            PatientsApiOptions apiOptions = new() { DefaultPageSize = 30, MaxPageSize = 50 };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);

            // Act
            IActionResult actionResult = await _sut.Search(searchRequest)
                .ConfigureAwait(false);

            // Assert
            GenericPagedGetResponse<Browsable<PatientInfo>> content = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<GenericPagedGetResponse<Browsable<PatientInfo>>>().Which;

            content.Items.Should()
                .NotBeNull($"{nameof(GenericPagedGetResponse<object>.Items)} must not be null").And
                .NotContainNulls($"{nameof(GenericPagedGetResponse<object>.Items)} must not contains null").And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            content.Links.Should().NotBeNull();
            PageLinks links = content.Links;

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
                    JsonPatchDocument<PatientInfo> patchDocument = new();
                    patchDocument.Add(x => x.Lastname, "Grayson");

                    PatientId patientId = PatientId.New();
                    yield return new object[]
                    {
                        new Patient(patientId, firstname: null,  lastname: "Wayne")
                            .WasBornOn(14.June(1960).ToLocalDateTime().Date),
                        patchDocument,
                        (Expression<Func<Patient, bool>>)(x => x.Id == patientId && x.Lastname == "Grayson")
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(PatchCases))]
        public async Task Patch(Patient source,
                                JsonPatchDocument<PatientInfo> patchDocument,
                                Expression<Func<Patient, bool>> patchResultExpectation)
        {
            _outputHelper.WriteLine($"Patient : {SerializeObject(source)}");
            _outputHelper.WriteLine($"Patch : {SerializeObject(patchDocument)}");

            // Arrange
            using (IUnitOfWork uow = _factory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(source);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            // Act
            IActionResult actionResult = await _sut.Patch(source.Id, patchDocument)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<NoContentResult>();

            using (IUnitOfWork uow = _factory.NewUnitOfWork())
            {
                Patient sourceAfterPatch = await uow.Repository<Patient>().SingleAsync(x => x.Id == source.Id)
                                                                          .ConfigureAwait(false);
                sourceAfterPatch.Should().Match(patchResultExpectation);
            }
        }

        [Fact]
        public async Task PatchUnknownIdReturnsNotFound()
        {
            // Arrange
            JsonPatchDocument<PatientInfo> patchDocument = new();
            patchDocument.Replace(x => x.Firstname, "John");

            // Act
            IActionResult actionResult = await _sut.Patch(PatientId.New(), patchDocument)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            //Act
            IActionResult actionResult = await _sut.Get(PatientId.New())
                .ConfigureAwait(false);

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
            PatientId patientId = PatientId.New();
            PatientInfo expectedResource = new()
            {
                Id = patientId,
                Firstname = "Bruce",
                Lastname = "Wayne"
            };

            using (IUnitOfWork uow = _factory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(new Patient(patientId, firstname: "Bruce", lastname: "Wayne"));
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            //Act
            IActionResult actionResult = await _sut.Get(patientId)
                .ConfigureAwait(false);

            //Assert

            Browsable<PatientInfo> result = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<Browsable<PatientInfo>>().Which;

            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation), $"Each {nameof(Browsable<PatientInfo>.Links)}'s must have a not null/empty {nameof(Link.Relation)}").And
                .Contain(x => x.Relation == LinkRelation.Self, "Direct link to the resource must be provided").And
                .Contain(x => x.Relation == "delete", "Link to delete the resource must be provided");

            Link self = links.Single(x => x.Relation == LinkRelation.Self);
            self.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={patientId.Value}");
            self.Relation.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo(LinkRelation.Self);
            self.Method.Should()
                .Be("GET");

            Link linkDelete = links.Single(x => x.Relation == "delete");
            linkDelete.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={expectedResource.Id.Value}");

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
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<CreatePatientInfoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreatePatientInfoCommand command, CancellationToken ct) => new PatientInfo {
                    Id = command.Data.Id ?? PatientId.New(),
                    Firstname = command.Data.Firstname,
                    Lastname = command.Data.Lastname
                });

            //Act
            CreatePatientInfo info = new()
            {
                Firstname = "Bruce",
                Lastname = "Wayne"
            };

            IActionResult actionResult = await _sut.Post(info)
                .ConfigureAwait(false);

            //Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<CreatePatientInfoCommand>(), It.IsAny<CancellationToken>()));

            CreatedAtRouteResult createdActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtRouteResult>().Which;

            createdActionResult.RouteName.Should().Be(RouteNames.DefaultGetOneByIdApi);
            createdActionResult.RouteValues.Should()
                .HaveCount(2).And
                .ContainKey("id").And
                .ContainKey("controller");

            createdActionResult.RouteValues["id"].Should()
                .BeOfType<PatientId>();
            createdActionResult.RouteValues["controller"].Should()
                .BeOfType<string>().Which.Should()
                .Be(PatientsController.EndpointName);

            Browsable<PatientInfo> browsableResource = createdActionResult.Value.Should()
                                                                                .NotBeNull().And
                                                                                .BeAssignableTo<Browsable<PatientInfo>>().Which;

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
            IActionResult actionResult = await _sut.Delete(PatientId.Empty)
                .ConfigureAwait(false);

            //Assert
            actionResult.Should().BeAssignableTo<BadRequestResult>();
        }
    }
}
