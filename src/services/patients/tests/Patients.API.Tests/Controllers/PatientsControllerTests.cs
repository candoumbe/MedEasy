namespace Patients.API.UnitTests.Controllers
{

    using Bogus;

    using FluentAssertions;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.Ids;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;

    using MediatR;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Moq;

    using NodaTime;
    using NodaTime.Testing;

    using Optional;

    using Patients.API.Controllers;
    using Patients.API.Routing;
    using Patients.Context;
    using Patients.CQRS.Commands;
    using Patients.CQRS.Queries;
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

    using static MedEasy.RestObjects.LinkRelation;
    using static Moq.MockBehavior;
    using static System.StringComparison;
    using static System.Uri;
    using static Microsoft.AspNetCore.Http.StatusCodes;
    using MedEasy.CQRS.Core.Commands;
    using Microsoft.AspNetCore.JsonPatch;
    using DataFilters;
    using MedEasy.DTO.Search;

    [UnitTest]
    [Feature("Patients")]
    public class PatientsControllerTests : IClassFixture<SqliteEfCoreDatabaseFixture<PatientsDataStore>>
    {
        private readonly Mock<LinkGenerator> _urlHelperMock;
        private static readonly PatientsApiOptions _apiOptions = new() { DefaultPageSize = 30, MaxPageSize = 200 };
        private readonly Mock<ILogger<PatientsController>> _loggerMock;
        private readonly PatientsController _sut;
        private readonly ITestOutputHelper _outputHelper;
        private readonly Mock<IOptionsSnapshot<PatientsApiOptions>> _apiOptionsMock;
        private const string _baseUrl = "http://host/api";
        private readonly Mock<IMediator> _mediatorMock;
        private readonly IUnitOfWorkFactory _uowFactory;

        public PatientsControllerTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<PatientsDataStore> database)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<PatientsController>>(Strict);
            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString __, LinkOptions ___)
                => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _apiOptionsMock = new Mock<IOptionsSnapshot<PatientsApiOptions>>(Strict);
            _mediatorMock = new(Strict);
            _uowFactory = new EFUnitOfWorkFactory<PatientsDataStore>(database.OptionsBuilder.Options, (options) =>
            {
                PatientsDataStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _sut = new PatientsController(_loggerMock.Object,
                                          _urlHelperMock.Object,
                                          _apiOptionsMock.Object,
                                          _mediatorMock.Object);
        }

        ///<inheritdoc/>
        public Task InitializeAsync() => Task.CompletedTask;

        ///<inheritdoc/>
        public async Task DisposeAsync()
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Patient>().Clear();
            await uow.SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public static IEnumerable<object[]> GetAllTestCases
        {
            get
            {
                int[] pageSizes = { 1, 10, 500 };
                int[] pages = { 1, 10, 500 };

                foreach (int pageSize in pageSizes)
                {
                    foreach (int page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<Patient>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            (
                                firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First  && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&page=1&pageSize={Math.Min(pageSize, _apiOptions.MaxPageSize) }".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&page=1&pageSize={Math.Min(pageSize, _apiOptions.MaxPageSize)}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        };
                    }
                }

                Faker<Patient> accountFaker = new Faker<Patient>().CustomInstantiator(faker => new Patient(PatientId.New(),
                                                                                                           faker.Person.FirstName,
                                                                                                           faker.Person.LastName));
                {
                    IEnumerable<Patient> items = accountFaker.Generate(400);
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }
                {
                    IEnumerable<Patient> items = accountFaker.Generate(400);

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == First  && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={PatientsController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Patient> items, int pageSize, int page,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) pageLinksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(_apiOptions);

            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfPatientsQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetPageOfPatientsQuery query, CancellationToken _) =>
                {
                    PaginationConfiguration pagination = query.Data;
                    Expression<Func<Patient, PatientInfo>> expression = x => new PatientInfo { Id = x.Id, Firstname = x.Firstname, Lastname = x.Lastname };
                    Func<Patient, PatientInfo> selector = expression.Compile();
                    _outputHelper.WriteLine($"Selector : {selector}");

                    IEnumerable<PatientInfo> results = items.Select(selector)
                        .ToArray();

                    results = results.Skip(pagination.PageSize * (pagination.Page == 1 ? 0 : pagination.Page - 1))
                         .Take(pagination.PageSize)
                         .ToArray();

                    return Task.FromResult(new Page<PatientInfo>(results, items.Count(), pagination.PageSize));
                });

            // Act
            IActionResult actionResult = await _sut.Get(new PaginationConfiguration { PageSize = pageSize, Page = page })
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfPatientsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfPatientsQuery>(cmd => cmd.Data.Page == page && cmd.Data.PageSize == Math.Min(pageSize, _apiOptions.MaxPageSize)), It.IsAny<CancellationToken>()), Times.Once,
                "Controller must cap pageSize of the query before sending it to the mediator");

            GenericPagedGetResponse<Browsable<PatientInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                        .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<GenericPagedGetResponse<Browsable<PatientInfo>>>().Which;

            response.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            response.Total.Should()
                    .Be(expectedCount, $@"the ""{nameof(GenericPagedGetResponse<Browsable<PatientInfo>>)}.{nameof(GenericPagedGetResponse<Browsable<PatientInfo>>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageLinksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageLinksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageLinksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageLinksExpectation.lastPageUrlExpectation);
        }

        [Fact]
        public async Task Delete_Returns_NoContent()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            PatientId idToDelete = PatientId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NoContentResult>();

            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeletePatientInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Get_Returns_The_Element()
        {
            // Arrange
            PatientId patientId = PatientId.New();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(new Patient
                (
                    id: patientId,
                    "bruce",
                    "wayne"

                ));

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOnePatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                         .Returns(async (GetOnePatientInfoByIdQuery query, CancellationToken ct) =>
                         {
                             using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                             return await uow.Repository<Patient>()
                                             .SingleOrDefaultAsync(x => new PatientInfo { Id = x.Id, Firstname = x.Firstname, Lastname = x.Lastname },
                                                                   (Patient x) => x.Id == query.Data,
                                                                   ct)
                                             .ConfigureAwait(false);
                         });

            // Act
            IActionResult actionResult = await _sut.Get(patientId, cancellationToken: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOnePatientInfoByIdQuery>(q => q.Data == patientId), It.IsAny<CancellationToken>()), Times.Once);

            Browsable<PatientInfo> browsableResource = actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                .BeAssignableTo<Browsable<PatientInfo>>().Which;

            browsableResource.Links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                .ContainSingle(x => x.Relation == Self).And
                .ContainSingle(x => x.Relation == "delete");

            Link self = browsableResource.Links.Single(x => x.Relation == Self);
            self.Method.Should()
                .Be("GET");

            PatientInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            resource.Id.Should().Be(patientId);
            resource.Firstname.Should().Be("bruce");
            resource.Lastname.Should().Be("wayne");
        }

        [Fact]
        public async Task Get_Returns_The_Element_With_Tenant()
        {
            // Arrange
            PatientId patientId = PatientId.New();
            TenantId tenantId = TenantId.New();
            Patient newPatient = new Patient(patientId, "dick", "grayson").OwnedBy(tenantId);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(new[] { newPatient });

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOnePatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetOnePatientInfoByIdQuery query, CancellationToken ct) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    return await uow.Repository<Patient>()
                                    .SingleOrDefaultAsync(
                                        x => new PatientInfo { Id = x.Id, Firstname = x.Firstname, TenantId = x.TenantId, Lastname = x.Lastname },
                                        (Patient x) => x.Id == query.Data,
                                        ct)
                                    .ConfigureAwait(false);
                });

            // Act
            IActionResult actionResult = await _sut.Get(patientId, default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOnePatientInfoByIdQuery>(q => q.Data == patientId), It.IsAny<CancellationToken>()), Times.Once);

            Browsable<PatientInfo> browsableResource = actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                .BeAssignableTo<Browsable<PatientInfo>>().Which;

            browsableResource.Links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                .ContainSingle(x => x.Relation == Self).And
                .ContainSingle(x => x.Relation == "delete");

            Link self = browsableResource.Links.Single(x => x.Relation == Self);
            self.Method.Should()
                .Be("GET");

            PatientInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={resource.Id}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            resource.Id.Should().Be(patientId);
            resource.Firstname.Should().Be(newPatient.Firstname);
            resource.Lastname.Should().Be(newPatient.Lastname);
            resource.TenantId.Should().Be(newPatient.TenantId);
        }

        [Fact]
        public async Task Get_UnknonwnId_Returns_NotFound()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOnePatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<PatientInfo>());

            // Act
            IActionResult actionResult = await _sut.Get(PatientId.New(), default)
                                                   .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteResource()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            PatientId idToDelete = PatientId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeletePatientInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async Task Delete_Unknown_Resource_Returns_Not_Found()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Failed_NotFound);

            // Act
            PatientId idToDelete = PatientId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeletePatientInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_UnknownEntity_Returns_NotFound()
        {
            JsonPatchDocument<PatientInfo> changes = new();
            changes.Replace(x => x.Lastname, "wayne");

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<PatientId, PatientInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _sut.Patch(PatientId.New(), changes, default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            JsonPatchDocument<PatientInfo> changes = new();
            changes.Replace(x => x.Firstname, "bruce");

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<PatientId, PatientInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Done);

            // Act
            IActionResult actionResult = await _sut.Patch(PatientId.New(), changes)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<PatientId, PatientInfo>>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async Task Given_mediator_returns_Conflict_Post_should_returns_ConflictedResult()
        {
            // Arrange
            CreatePatientInfo newPatient = new()
            {
                Firstname = "bruce",
                Lastname = "wayne"
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreatePatientInfoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<PatientInfo, CreateCommandFailure>(CreateCommandFailure.Conflict));

            // Act
            IActionResult actionResult = await _sut.Post(newPatient, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<CreatePatientInfoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<CreatePatientInfoCommand>(cmd => cmd.Data == newPatient), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<StatusCodeResult>().Which
                .StatusCode.Should()
                    .Be(Status409Conflict);
        }

        [Fact]
        public async Task GivenMediatorReturnPatientCreated_PostReturns_OkObjectResult()
        {
            // Arrange
            CreatePatientInfo newPatient = new()
            {
                Firstname = "bruce",
                Lastname = "wayne"
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreatePatientInfoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((CreatePatientInfoCommand cmd, CancellationToken _) => Option.Some<PatientInfo, CreateCommandFailure>(new PatientInfo { Firstname = cmd.Data.Firstname, Id = PatientId.New(), Lastname = cmd.Data.Lastname }));

            // Act
            IActionResult actionResult = await _sut.Post(newPatient, ct: default)
                                                   .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<CreatePatientInfoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<CreatePatientInfoCommand>(cmd => cmd.Data == newPatient), It.IsAny<CancellationToken>()), Times.Once);

            CreatedAtRouteResult createdAtRouteResult = actionResult.Should()
                                                                    .BeAssignableTo<CreatedAtRouteResult>().Which;

            Browsable<PatientInfo> browsableResource = createdAtRouteResult.Value.Should()
                                                                                 .BeAssignableTo<Browsable<PatientInfo>>().Which;

            PatientInfo createdResource = browsableResource.Resource;

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                 .NotBeNullOrEmpty().And
                 .NotContainNulls().And
                 .NotContain(link => string.IsNullOrWhiteSpace(link.Href)).And
                 .NotContain(link => string.IsNullOrWhiteSpace(link.Method)).And
                 .NotContain(link => string.IsNullOrWhiteSpace(link.Relation)).And
                 .Contain(link => link.Relation == Self);

            Link linkSelf = links.Single(link => link.Relation == Self);
            linkSelf.Method.Should()
                           .Be("GET");
            linkSelf.Href.Should()
                         .Be($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={createdResource.Id.Value}");

            createdResource.Id.Should()
                              .NotBeNull().And
                              .NotBe(PatientId.Empty);
            createdResource.Firstname.Should()
                                     .Be(newPatient.Firstname);
            createdResource.Lastname.Should()
                                    .Be(newPatient.Lastname);

            createdAtRouteResult.RouteName.Should()
                                          .Be(RouteNames.DefaultGetOneByIdApi);
            RouteValueDictionary routeValues = createdAtRouteResult.RouteValues;
            routeValues.Should()
                .ContainKey("controller").WhichValue.Should().Be(PatientsController.EndpointName);
            routeValues.Should()
                       .ContainKey("id").WhichValue.Should()
                            .BeOfType<PatientId>().Which.Should()
                            .NotBe(PatientId.Empty);
        }

        public static IEnumerable<object[]> SearchTestCases
        {
            get
            {
                Faker<Patient> accountFaker = new Faker<Patient>()
                    .CustomInstantiator(faker => new Patient(
                        id: PatientId.New(),
                        firstname: $"{faker.PickRandom("Bruce", "Clark", "Oliver", "Martha")}",
                        lastname: "Wayne"))
                    ;
                {
                    IEnumerable<Patient> items = accountFaker.Generate(40);

                    yield return new object[]
                    {
                        items,
                        new SearchPatientInfo
                        {
                            Lastname = "*Wayne",
                            Page = 1, PageSize = 10,
                            Sort = nameof(PatientInfo.Lastname)
                        },
                        (maxPageSize : 200, defaultPageSize : 30),
                        (
                            count : 40,
                            items : (Expression<Func<IEnumerable<Browsable<PatientInfo>>, bool>>)(resources => resources.All(x => x.Resource.Lastname.Like("*Wayne"))),
                            links :
                            (
                                firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                    && x.Relation == First
                                    && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={PatientsController.EndpointName}&lastname={EscapeDataString("*Wayne")}&page=1&pageSize=10&sort=lastname".Equals(x.Href, CurrentCultureIgnoreCase)), // expected link to first page
                                previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Next && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={PatientsController.EndpointName}&lastname={EscapeDataString("*Wayne")}&page=2&pageSize=10&Sort=lastname".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                                lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={PatientsController.EndpointName}&lastname={EscapeDataString("*Wayne")}&page=4&pageSize=10&Sort=lastname".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchTestCases))]
        [Feature("Search")]
        public async Task Search(IEnumerable<Patient> items, SearchPatientInfo searchQuery,
                                (int maxPageSize, int defaultPageSize) apiOptions,
                                (
                                    int count,
                                    Expression<Func<IEnumerable<Browsable<PatientInfo>>, bool>> items,
                                    (
                                        Expression<Func<Link, bool>> firstPageUrlExpectation,
                                        Expression<Func<Link, bool>> previousPageUrlExpectation,
                                        Expression<Func<Link, bool>> nextPageUrlExpectation,
                                        Expression<Func<Link, bool>> lastPageUrlExpectation
                                    ) links
                                ) pageExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.Search)}({nameof(SearchPatientInfo)})");
            _outputHelper.WriteLine($"Search : {searchQuery.Jsonify()}");
            _outputHelper.WriteLine($"store items: {items.Jsonify()}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new PatientsApiOptions { DefaultPageSize = apiOptions.defaultPageSize, MaxPageSize = apiOptions.maxPageSize });

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchPatientInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns((SearchPatientInfoQuery request, CancellationToken ct) =>
                {
                    SearchPatientInfo search = request.Data;
                    IList<IFilter> filters = new List<IFilter>();
                    if (!string.IsNullOrEmpty(search.Firstname))
                    {
                        filters.Add($"{nameof(search.Firstname)}={search.Firstname}".ToFilter<PatientInfo>());
                    }

                    if (!string.IsNullOrEmpty(search.Lastname))
                    {
                        filters.Add($"{nameof(search.Lastname)}={search.Lastname}".ToFilter<PatientInfo>());
                    }

                    SearchQuery<PatientInfo> searchQuery = new(new SearchQueryInfo<PatientInfo>()
                    {
                        Filter = filters.Exactly(1)
                                                ? filters.Single()
                                                : new MultiFilter { Logic = FilterLogic.And, Filters = filters },
                        Page = search.Page,
                        PageSize = search.PageSize,
                        Sort = search.Sort?.ToSort<PatientInfo>() ?? new Sort<PatientInfo>(nameof(PatientInfo.Lastname), SortDirection.Descending)
                    });

                    return new HandleSearchQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder)
                        .Search<Patient, PatientInfo>(searchQuery, ct);
                });

            // Act
            IActionResult actionResult = await _sut.Search(searchQuery)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<SearchPatientInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<SearchPatientInfoQuery>(query => query.Data.Page == searchQuery.Page && query.Data.PageSize == Math.Min(searchQuery.PageSize, apiOptions.maxPageSize)), It.IsAny<CancellationToken>()), Times.Once);
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.AtLeastOnce, $"because {nameof(PatientsController)}.{nameof(PatientsController.Search)} must always check that " +
                $"{nameof(SearchPatientInfo.PageSize)} don't exceed {nameof(PatientsApiOptions.MaxPageSize)} value");

            GenericPagedGetResponse<Browsable<PatientInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<Browsable<PatientInfo>>>().Which;

            response.Items.Should()
                          .NotBeNull().And
                          .NotContainNulls().And
                          .NotContain(x => x.Resource == null).And
                          .NotContain(x => x.Links == null).And
                          .NotContain(x => !x.Links.Any()).And
                          .Match(pageExpectation.items);

            if (response.Items.Any())
            {
                response.Items.Should()
                    .OnlyContain(x => x.Links.Once(link => link.Relation == Self));
            }

            response.Total.Should()
                    .Be(pageExpectation.count, $@"the ""{nameof(GenericPagedGetResponse<Browsable<PatientInfo>>)}.{nameof(GenericPagedGetResponse<Browsable<PatientInfo>>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageExpectation.links.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageExpectation.links.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageExpectation.links.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageExpectation.links.lastPageUrlExpectation);
        }
    }
}