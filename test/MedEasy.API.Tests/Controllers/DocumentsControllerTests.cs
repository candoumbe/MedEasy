using FluentAssertions;
using MedEasy.API.Controllers;
using MedEasy.Handlers.Core.Document.Queries;
using Moq;
using System;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using System.Threading.Tasks;
using System.Collections.Generic;
using MedEasy.DTO;
using System.Linq.Expressions;
using MedEasy.RestObjects;
using System.Linq;
using MedEasy.Queries;
using Microsoft.Extensions.Options;
using MedEasy.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using GenFu;
using static System.StringComparison;
using Microsoft.Extensions.Logging;
using MedEasy.Handlers.Core.Queries;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace MedEasy.API.Tests.Controllers
{
    public class DocumentsControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private DocumentsController _controller;
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;
        private Mock<ILogger<DocumentsController>> _logger;
        private Mock<IHandleGetOneDocumentMetadataInfoByIdQuery> _iHandleGetOneDocumentMetadataInfoByIdQueryMock;
        private Mock<IHandleGetManyDocumentsQuery> _iHandleGetManyDocumentMetadataInfoQueryMock;
        private Mock<IHandleGetOneDocumentInfoByIdQuery> _iHandleGetOneDocumentInfoByIdQueryMock;
        private Mock<IUrlHelperFactory> _urlHelperFactoryMock;
        private ActionContextAccessor _actionContextAccessor;

        public DocumentsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            
            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);
            _logger = new Mock<ILogger<DocumentsController>>(Strict);

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


            _iHandleGetOneDocumentMetadataInfoByIdQueryMock = new Mock<IHandleGetOneDocumentMetadataInfoByIdQuery>(Strict);
            _iHandleGetManyDocumentMetadataInfoQueryMock = new Mock<IHandleGetManyDocumentsQuery>(Strict);
            _iHandleGetOneDocumentInfoByIdQueryMock = new Mock<IHandleGetOneDocumentInfoByIdQuery>(Strict);


            _controller = new DocumentsController(
                _logger.Object, 
                _apiOptionsMock.Object, 
                _iHandleGetOneDocumentMetadataInfoByIdQueryMock.Object, 
                _iHandleGetManyDocumentMetadataInfoQueryMock.Object,
                _urlHelperFactoryMock.Object,
                _actionContextAccessor,
                _iHandleGetOneDocumentInfoByIdQueryMock.Object);
           
        }

        public void Dispose()
        {
            _outputHelper = null;
            
            _controller = null;
            _apiOptionsMock = null;
            _logger = null;
            _urlHelperFactoryMock = null;

            _iHandleGetManyDocumentMetadataInfoQueryMock = null;
            _iHandleGetOneDocumentMetadataInfoByIdQueryMock = null;
            _iHandleGetOneDocumentInfoByIdQueryMock = null;
        }

        [Fact]
        public void CheckEndpointName() => DocumentsController.EndpointName
            .Should().BeEquivalentTo(nameof(DocumentsController).Replace("Controller", string.Empty));


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
                            Enumerable.Empty<DocumentMetadataInfo>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            ((Expression<Func<Link, bool>>) (x => x == null))  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<DocumentMetadataInfo> items = A.ListOf<DocumentMetadataInfo>(400);
                    items.ForEach(item => item.Id = default(int));
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=14".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }
                {
                    IEnumerable<DocumentMetadataInfo> items = A.ListOf<DocumentMetadataInfo>(400);
                    items.ForEach(item => item.Id = default(int));

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize=10&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize=10&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize=10&page=40".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }

                yield return new object[]
                    {
                        new [] {
                            new DocumentMetadataInfo { Id = 1, Title = "Doc 1",  MimeType = "application/pdf" }
                        },
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        1,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "first" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "last" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
                    };
            }
        }


        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<DocumentMetadataInfo> items, int pageSize, int page,
            int expectedCount,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(DocumentsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            _iHandleGetManyDocumentMetadataInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, DocumentMetadataInfo>>()))
                .Returns((IWantManyResources<Guid, DocumentMetadataInfo> getQuery) => Task.Run(() =>
                {
                    IEnumerable<DocumentMetadataInfo> results = items
                        .OrderByDescending(x => x.UpdatedDate)
                        .Skip(getQuery.Data.PageSize * getQuery.Data.Page)
                        .Take(getQuery.Data.PageSize);
                    return (IPagedResult<DocumentMetadataInfo>) new PagedResult<DocumentMetadataInfo>(results, items.Count(), getQuery.Data.PageSize);
                }));
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });

            // Act
            IActionResult actionResult = await _controller.Get(page, pageSize);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(DocumentsController)}.{nameof(DocumentsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");

            actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<IGenericPagedGetResponse<DocumentMetadataInfo>>();

            IGenericPagedGetResponse<DocumentMetadataInfo> response = (IGenericPagedGetResponse<DocumentMetadataInfo>)value;

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<DocumentMetadataInfo>)}.{nameof(IGenericPagedGetResponse<DocumentMetadataInfo>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);

        }



        [Fact]
        public async Task GetById_ShouldReturns_The_BrowsableResource()
        {
            // Arrange
            DocumentMetadataInfo expectedResource = new DocumentMetadataInfo
            {
                Id = 1,
                DocumentId = 3,
                MimeType = "application/pdf",
                PatientId = 5,
                Size = 542,
                Title = "Document 1"
            };

            _iHandleGetOneDocumentMetadataInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsNotNull<IWantOneResource<Guid, int, DocumentMetadataInfo>>()))
                .ReturnsAsync(expectedResource);

            // Act
            IActionResult actionResult = await _controller.Get(1);

            // Assert
            IBrowsableResource<DocumentMetadataInfo> browsableResource = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IBrowsableResource<DocumentMetadataInfo>>().Which;


            DocumentMetadataInfo actualResource = browsableResource.Resource;
            
            actualResource.Should().NotBeNull();
            actualResource.Id.Should().Be(expectedResource.Id);
            actualResource.Title.Should().Be(expectedResource.Title);
            actualResource.MimeType.Should().Be(expectedResource.MimeType);
            actualResource.Size.Should().Be(expectedResource.Size);
            actualResource.DocumentId.Should().Be(expectedResource.DocumentId);

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeNullOrEmpty().And
                .HaveCount(2).And
                .Contain(x => x.Relation == "file").And
                .Contain(x => x.Relation == "self");

            Link self = links.Single(x => x.Relation == "self");
            self.Href.Should()
                .BeEquivalentTo($"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?{nameof(DocumentMetadataInfo.Id)}={expectedResource.Id}");

            Link file = links.Single(x => x.Relation == "file");
            file.Href.Should()
                .BeEquivalentTo($"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.File)}?{nameof(DocumentMetadataInfo.Id)}={expectedResource.Id}");

        }

        [Fact]
        public async Task File_ShouldReturns_The_BrowsableResource()
        {
            // Arrange
            DocumentInfo expectedResource = new DocumentInfo
            {
                Id = 1,
                Content = new byte[] { 1, 2, 3, 4, 5}
            };

            _iHandleGetOneDocumentInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsNotNull<IWantOneResource<Guid, int, DocumentInfo>>()))
                .ReturnsAsync(expectedResource);

            // Act
            IActionResult actionResult = await _controller.File(1);

            // Assert
            IBrowsableResource<DocumentInfo> browsableResource = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IBrowsableResource<DocumentInfo>>().Which;


            DocumentInfo actualResource = browsableResource.Resource;

            actualResource.Should().NotBeNull();
            actualResource.Id.Should().Be(expectedResource.Id);
            actualResource.Content.Should().BeEquivalentTo(expectedResource.Content);
          

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeNullOrEmpty().And
                .HaveCount(2).And
                .Contain(x => x.Relation == "metadata").And
                .Contain(x => x.Relation == "self");


            Link self = links.Single(x => x.Relation == "self");
            self.Href.Should()
                .BeEquivalentTo($"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.File)}?{nameof(DocumentInfo.Id)}={expectedResource.Id}");


            Link metadata = links.Single(x => x.Relation == "metadata");
            metadata.Href.Should()
                .BeEquivalentTo($"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?{nameof(DocumentMetadataInfo.Id)}={expectedResource.Id}");
        }
    }
}
