namespace Documents.API.UnitTests.Features.v1
{
    using AutoMapper.QueryableExtensions;

    using Bogus;

    using Documents.API.Features.v1;
    using Documents.CQRS.Commands;
    using Documents.CQRS.Queries;
    using Documents.DataStore;
    using Documents.DTO;
    using Documents.DTO.v1;
    using Documents.Ids;
    using Documents.Mapping;
    using Documents.Objects;

    using FluentAssertions;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.CQRS.Core.Handlers;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
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
    using NodaTime.Testing;

    using Optional;

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

    [UnitTest]
    [Feature("Documents")]
    public class DocumentsControllerTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<DocumentsStore>>
    {
        private ITestOutputHelper _outputHelper;

        private readonly IUnitOfWorkFactory _uowFactory;
        private static readonly DocumentsApiOptions ApiOptions = new() { DefaultPageSize = 30, MaxPageSize = 200 };
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<LinkGenerator> _urlHelperMock;
        private readonly Mock<IOptionsSnapshot<DocumentsApiOptions>> _apiOptionsMock;
        private readonly DocumentsController _sut;
        private const string BaseUrl = "http://host/api";

        private readonly Mock<ILogger<DocumentsController>> _loggerMock;
        private static readonly ApiVersion ApiVersion = new(1, 0);

        public DocumentsControllerTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<DocumentsStore> database)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<LinkGenerator>(Strict);
            _urlHelperMock.Setup(mock => mock.GetPathByAddress(It.IsAny<string>(), It.IsAny<RouteValueDictionary>(), It.IsAny<PathString>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
                .Returns((string routename, RouteValueDictionary routeValues, PathString _, FragmentString _, LinkOptions _)
                => $"{BaseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _apiOptionsMock = new Mock<IOptionsSnapshot<DocumentsApiOptions>>(Strict);

            _uowFactory = new EFUnitOfWorkFactory<DocumentsStore>(database.OptionsBuilder.Options, (options) =>
            {
                DocumentsStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _mediatorMock = new Mock<IMediator>(Strict);
            _loggerMock = new Mock<ILogger<DocumentsController>>();

            _sut = new DocumentsController(urlHelper: _urlHelperMock.Object,
                                           apiOptions: _apiOptionsMock.Object,
                                           mediator: _mediatorMock.Object,
                                           _loggerMock.Object,
                                           ApiVersion);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            _outputHelper = null;
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            uow.Repository<Document>().Clear();
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
                            Enumerable.Empty<Document>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            (
                                firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                                                                               && x.Relation == First
                                                                                               && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={DocumentsController.EndpointName}&page=1&pageSize={Math.Min(pageSize, ApiOptions.MaxPageSize) }&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                                                                              && x.Relation == Last
                                                                                              && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={DocumentsController.EndpointName}&page=1&pageSize={Math.Min(pageSize, ApiOptions.MaxPageSize)}&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        };
                    }
                }

                Faker<Document> accountFaker = new Faker<Document>()
                    .CustomInstantiator(faker => new Document(id: DocumentId.New(),
                        name: faker.System.CommonFileName(),
                        mimeType: faker.System.MimeType())
                    );
                {
                    IEnumerable<Document> items = accountFaker.Generate(400);
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == First
                                && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={DocumentsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                                                                         && x.Relation == "next"
                                                                                         && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={DocumentsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                                                                         && x.Relation == Last
                                                                                         && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={DocumentsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }
                {
                    IEnumerable<Document> items = accountFaker.Generate(400);

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                                                                           && x.Relation == First
                                                                                           && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={DocumentsController.EndpointName}&page=1&pageSize=10&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                            previousPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                            nextPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                                                                          && x.Relation == "next"
                                                                                          && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={DocumentsController.EndpointName}&page=2&pageSize=10&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                            lastPageUrlExpectation : (Expression<Func<Link, bool>>) (x => x != null
                                                                                          && x.Relation == Last
                                                                                          && $"{BaseUrl}/{RouteNames.DefaultGetAllApi}/?controller={DocumentsController.EndpointName}&page=40&pageSize=10&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Document> items, int pageSize, int page,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) pageLinksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(DocumentsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(ApiOptions);

            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfDocumentInfoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetPageOfDocumentInfoQuery query, CancellationToken _) =>
                {
                    PaginationConfiguration pagination = query.Data;
                    Expression<Func<Document, DocumentInfo>> expression = x => new DocumentInfo { Id = x.Id, Name = x.Name, MimeType = x.MimeType, Hash = x.Hash };
                    Func<Document, DocumentInfo> selector = expression.Compile();
                    _outputHelper.WriteLine($"Selector : {selector}");

                    IEnumerable<DocumentInfo> results = items.Select(selector)
                        .ToArray();

                    results = results.Skip(pagination.PageSize * (pagination.Page == 1 ? 0 : pagination.Page - 1))
                         .Take(pagination.PageSize)
                         .ToArray();

                    return new Page<DocumentInfo>(results, items.Count(), pagination.PageSize);
                });

            // Act
            IActionResult actionResult = await _sut.Get(new PaginationConfiguration { PageSize = pageSize, Page = page })
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfDocumentInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfDocumentInfoQuery>(cmd => cmd.Data.Page == page && cmd.Data.PageSize == Math.Min(pageSize, ApiOptions.MaxPageSize)), It.IsAny<CancellationToken>()), Times.Once,
                "Controller must cap pageSize of the query before sending it to the mediator");

            GenericPagedGetResponse<Browsable<DocumentInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                        .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<GenericPagedGetResponse<Browsable<DocumentInfo>>>().Which;

            response.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            if (response.Items.AtLeastOnce())
            {
                response.Items.Should()
                    .OnlyContain(x => x.Links.Exactly(link => link.Relation == Self, 1), "All resources must provided one direct link");
            }
            response.Total.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPagedGetResponse<Browsable<DocumentInfo>>)}.{nameof(GenericPagedGetResponse<Browsable<DocumentInfo>>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageLinksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageLinksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageLinksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageLinksExpectation.lastPageUrlExpectation);
        }

        [Fact]
        public async Task Delete_Returns_NoContent()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsNotNull<DeleteDocumentInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            DocumentId idToDelete = DocumentId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, ct: default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NoContentResult>();

            _mediatorMock.Verify(mock => mock.Send(It.IsNotNull<DeleteDocumentInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteDocumentInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Get_Returns_The_Element()
        {
            // Arrange
            DocumentId documentId = DocumentId.New();
            Document entry = new(
                id: documentId,
                name: "the batman in action",
                mimeType: "image/mpeg4"
            );
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Create(entry);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneDocumentInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns((GetOneDocumentInfoByIdQuery query, CancellationToken ct) =>
                {
                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    return uow.Repository<Document>()
                                    .SingleOrDefaultAsync(
                                        x => new DocumentInfo { Id = x.Id, Name = x.Name, Hash = x.Hash, MimeType = x.MimeType },
                                        (Document x) => x.Id == query.Data,
                                        ct)
                                    .AsTask();
                });

            // Act
            ActionResult<Browsable<DocumentInfo>> actionResult = await _sut.Get(documentId, ct: default)
                                                                           .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneDocumentInfoByIdQuery>(q => q.Data == documentId), It.IsAny<CancellationToken>()), Times.Once);

            Browsable<DocumentInfo> browsableResource = actionResult.Value;

            browsableResource.Links.Should()
                                   .NotBeNull().And
                                   .NotContainNulls().And
                                   .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                                   .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                                   .ContainSingle(x => x.Relation == Self).And
                                   .ContainSingle(x => x.Relation == "file").And
                                   .ContainSingle(x => x.Relation == "delete");

            Link self = browsableResource.Links.Single(x => x.Relation == Self);
            self.Method.Should()
                .Be("GET");

            DocumentInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={DocumentsController.EndpointName}&{nameof(resource.Id)}={resource.Id.Value}&version={ApiVersion}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={DocumentsController.EndpointName}&{nameof(resource.Id)}={resource.Id.Value}&version={ApiVersion}");

            Link file = browsableResource.Links.Single(x => x.Relation == "file");
            file.Method.Should()
                .Be("GET");
            file.Href.Should()
                .BeEquivalentTo($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?action={nameof(DocumentsController.File)}&controller={DocumentsController.EndpointName}&{nameof(resource.Id)}={resource.Id.Value}&version={ApiVersion}");

            resource.Id.Should().Be(documentId);
            resource.Name.Should().Be(entry.Name);
            resource.MimeType.Should().Be(entry.MimeType);
            resource.Hash.Should().Be(entry.Hash);
        }

        [Fact]
        public async Task Get_UnknonwnId_Returns_NotFound()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneDocumentInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<DocumentInfo>());

            // Act
            ActionResult<Browsable<DocumentInfo>> actionResult = await _sut.Get(id: DocumentId.New(), ct: default)
                                                                           .ConfigureAwait(false);

            // Assert
            actionResult.Result.Should()
                        .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task GetFile_Returns_The_Element()
        {
            // Arrange
            Faker faker = new();
            DocumentId documentId = DocumentId.New();

            DocumentPart[] parts = new[]
            {
                new DocumentPart(documentId, 0, faker.Random.Bytes(10)),
                new DocumentPart(documentId, 1, faker.Random.Bytes(10)),
                new DocumentPart(documentId, 2, faker.Random.Bytes(10))
            };

            Document entry = new(id: documentId, name: "the batman in action", mimeType: "image/mpeg4");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Create(entry);
                uow.Repository<DocumentPart>().Create(parts);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneDocumentFileInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetOneDocumentFileInfoByIdQuery query, CancellationToken ct) =>
                {
                    IUnitOfWork uow = _uowFactory.NewUnitOfWork();

                    Expression<Func<DocumentPart, DocumentPartInfo>> selector = AutoMapperConfig.Build()
                                                                                                .ExpressionBuilder
                                                                                                .GetMapExpression<DocumentPart, DocumentPartInfo>();

                    return uow.Repository<DocumentPart>()
                              .Stream(selector,
                                      predicate: (DocumentPart x) => x.DocumentId == query.Data,
                                      ct);
                });

            // Act
            IAsyncEnumerable<DocumentPartInfo> chunks = _sut.File(documentId, ct: default);
            IList<DocumentPartInfo> items = new List<DocumentPartInfo>();

            await foreach (DocumentPartInfo item in chunks)
            {
                items.Add(item);
            }

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetOneDocumentFileInfoByIdQuery>(q => q.Data == documentId), It.IsAny<CancellationToken>()), Times.Once);
            items.Should()
                 .HaveCount(parts.Count());
        }

        [Fact]
        public async Task GetFile_UnknonwnId_Returns_Empty()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetOneDocumentFileInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(AsyncEnumerable.Empty<DocumentPartInfo>());

            // Act
            var chunks = _sut.File(id: DocumentId.New(), ct: default);
            bool hasValue = await chunks.GetAsyncEnumerator().MoveNextAsync()
                                        .ConfigureAwait(false);

            // Assert
            hasValue.Should()
                    .BeFalse();
        }

        [Fact]
        public async Task DeleteResource_Returns_NoContent_when_command_returns_Done()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteDocumentInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            DocumentId idToDelete = DocumentId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeleteDocumentInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteDocumentInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async Task Delete_Unknown_Resource_returns_NotFound()
        {
            // Arrange
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeleteDocumentInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Failed_NotFound);

            // Act
            DocumentId idToDelete = DocumentId.New();
            IActionResult actionResult = await _sut.Delete(idToDelete, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeleteDocumentInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeleteDocumentInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_UnknownEntity_Returns_NotFound()
        {
            JsonPatchDocument<DocumentInfo> changes = new();
            changes.Replace(x => x.Name, "batman fake");

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, DocumentInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _sut.Patch(id: Guid.NewGuid(), changes, ct: default)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            JsonPatchDocument<DocumentInfo> changes = new();
            changes.Replace(x => x.Name, "bruce.wayne@gorham.com");

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<PatchCommand<Guid, DocumentInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ModifyCommandResult.Done);

            // Act
            IActionResult actionResult = await _sut.Patch(Guid.NewGuid(), changes)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<PatchCommand<Guid, DocumentInfo>>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();
        }

        [Fact]
        public async Task GivenMediatorReturnDocumentCreated_PostReturns_OkObjectResult()
        {
            // Arrange
            NewDocumentInfo newDocument = new()
            {
                Name = $"file-{Guid.NewGuid()}",
                MimeType = "image/jpeg",
                Content = new byte[] { 123, 8 }
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<CreateDocumentInfoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateDocumentInfoCommand cmd, CancellationToken _) =>
                    Option.Some<DocumentInfo, CreateCommandResult>(new DocumentInfo { Name = cmd.Data.Name, Id = DocumentId.New() }));

            // Act
            IActionResult actionResult = await _sut.Post(newDocument, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<CreateDocumentInfoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<CreateDocumentInfoCommand>(cmd => cmd.Data == newDocument), It.IsAny<CancellationToken>()), Times.Once);

            CreatedAtRouteResult createdAtRouteResult = actionResult.Should()
                .BeAssignableTo<CreatedAtRouteResult>().Which;

            Browsable<DocumentInfo> browsableResource = createdAtRouteResult.Value.Should()
                .BeAssignableTo<Browsable<DocumentInfo>>().Which;

            DocumentInfo createdResource = browsableResource.Resource;

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
            linkSelf.Href.ToLowerInvariant().Should()
                    .Be($"{BaseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={DocumentsController.EndpointName}&{nameof(DocumentInfo.Id)}={createdResource.Id.Value}&version={ApiVersion}".ToLowerInvariant());

            createdResource.Name.Should()
                .Be(newDocument.Name);

            createdAtRouteResult.RouteName.Should()
                .Be(RouteNames.DefaultGetOneByIdApi);
            RouteValueDictionary routeValues = createdAtRouteResult.RouteValues;
            routeValues.Should()
                .ContainKey("controller").WhichValue.Should().Be(DocumentsController.EndpointName);
            routeValues.Should()
                .ContainKey("id").WhichValue.Should()
                    .BeOfType<DocumentId>().Which.Should()
                    .NotBe(DocumentId.Empty);
        }

        public static IEnumerable<object[]> SearchTestCases
        {
            get
            {
                Faker<Document> accountFaker = new Faker<Document>()
                    .CustomInstantiator(faker => new Document(
                        id: DocumentId.New(),
                        name: $"{faker.PickRandom("Bruce", "Clark", "Oliver", "Martha")} Wayne"));
                {
                    IEnumerable<Document> items = accountFaker.Generate(40);

                    yield return new object[]
                    {
                        items,
                        new SearchDocumentInfo
                        {
                            Name = "*Wayne",
                            Page = 1, PageSize = 10
                        },
                        (maxPageSize : 200, defaultPageSize : 30),
                        (
                            count : 40,
                            items :
                            (Expression<Func<IEnumerable<Browsable<DocumentInfo>>, bool>>)(resources => resources.All(x => x.Resource.Name.Like("*Wayne")))
                            ,
                            links :
                            (
                                firstPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null
                                    && x.Relation == First
                                    && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={DocumentsController.EndpointName}&name={EscapeDataString("*Wayne")}&page=1&pageSize=10&version={ApiVersion}".Equals(x.Href, CurrentCultureIgnoreCase)), // expected link to first page
                                previousPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                nextPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Next && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={DocumentsController.EndpointName}&name={EscapeDataString("*Wayne")}&page=2&pageSize=10&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase)), // expected link to next page
                                lastPageUrlExpecation : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == Last && $"{BaseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={DocumentsController.EndpointName}&name={EscapeDataString("*Wayne")}&page=4&pageSize=10&version={ApiVersion}".Equals(x.Href, OrdinalIgnoreCase))  // expected link to last page
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(SearchTestCases))]
        [Feature("Search")]
        public async Task Search(IEnumerable<Document> items, SearchDocumentInfo searchQuery,
            (int maxPageSize, int defaultPageSize) apiOptions,
            (
                int count,
                Expression<Func<IEnumerable<Browsable<DocumentInfo>>, bool>> items,
                (
                    Expression<Func<Link, bool>> firstPageUrlExpectation,
                    Expression<Func<Link, bool>> previousPageUrlExpectation,
                    Expression<Func<Link, bool>> nextPageUrlExpectation,
                    Expression<Func<Link, bool>> lastPageUrlExpectation
                ) links
            ) pageExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(DocumentsController.Search)}({nameof(SearchDocumentInfo)})");
            _outputHelper.WriteLine($"Search : {searchQuery.Jsonify()}");
            _outputHelper.WriteLine($"store items: {items.Jsonify()}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new DocumentsApiOptions { DefaultPageSize = apiOptions.defaultPageSize, MaxPageSize = apiOptions.maxPageSize });

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<DocumentInfo>>(), It.IsAny<CancellationToken>()))
                .Returns((SearchQuery<DocumentInfo> query, CancellationToken ct) =>
                {
                    return new HandleSearchQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder)
                        .Search<Document, DocumentInfo>(query, ct);
                });

            // Act
            IActionResult actionResult = await _sut.Search(searchQuery)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<SearchQuery<DocumentInfo>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<SearchQuery<DocumentInfo>>(query => query.Data.Page == searchQuery.Page && query.Data.PageSize == Math.Min(searchQuery.PageSize, apiOptions.maxPageSize)), It.IsAny<CancellationToken>()), Times.Once);
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.AtLeastOnce, $"because {nameof(DocumentsController)}.{nameof(DocumentsController.Search)} must always check that " +
                $"{nameof(SearchDocumentInfo.PageSize)} don't exceed {nameof(DocumentsApiOptions.MaxPageSize)} value");

            GenericPagedGetResponse<Browsable<DocumentInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<Browsable<DocumentInfo>>>().Which;

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
                    .Be(pageExpectation.count, $@"the ""{nameof(GenericPagedGetResponse<Browsable<DocumentInfo>>)}.{nameof(GenericPagedGetResponse<Browsable<DocumentInfo>>.Total)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageExpectation.links.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageExpectation.links.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageExpectation.links.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageExpectation.links.lastPageUrlExpectation);
        }
    }
}
