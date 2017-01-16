using FluentAssertions;
using MedEasy.API.Controllers;
using MedEasy.Handlers.Core.Document.Queries;
using Moq;
using System;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace MedEasy.API.Tests.Controllers
{
    public class DocumentsControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IHandleGetManyDocumentsQuery> _iHandleGetManyDocumentsQueryMock;
        private DocumentsController _controller;

        public DocumentsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _iHandleGetManyDocumentsQueryMock = new Mock<IHandleGetManyDocumentsQuery>(Strict);

            _controller = new DocumentsController();
           
        }

        public void Dispose()
        {
            _outputHelper = null;
            _iHandleGetManyDocumentsQueryMock = null;
            _controller = null;
        }

        [Fact]
        public void CheckEndpointName() => DocumentsController.EndpointName
            .Should().BeEquivalentTo(nameof(DocumentsController).Replace("Controller", string.Empty));


        //public static IEnumerable<object> GetAllTestCases
        //{
        //    get
        //    {
        //        int[] pageSizes = { 0, int.MinValue, int.MaxValue };
        //        int[] pages = { 0, int.MinValue, int.MaxValue };


        //        foreach (var pageSize in pageSizes)
        //        {
        //            foreach (var page in pages)
        //            {
        //                yield return new object[]
        //                {
        //                    Enumerable.Empty<DocumentMetadata>(), // Current store state
        //                    pageSize, page, // request
        //                    0,    //expected total
        //                    ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
        //                    ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
        //                    ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
        //                    ((Expression<Func<Link, bool>>) (x => x == null))  // expected link to last page
        //                };
        //            }
        //        }

        //        {
        //            IEnumerable<Document> items = A.ListOf<Document>(400);
        //            items.ForEach(item => item.Id = default(int));
        //            yield return new object[]
        //            {
        //                items,
        //                PaginationConfiguration.DefaultPageSize, 1, // request
        //                400,    //expected total
        //                ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
        //                ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
        //                ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "next" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
        //                ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=14".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
        //            };
        //        }
        //        {
        //            IEnumerable<Document> items = A.ListOf<Document>(400);
        //            items.ForEach(item => item.Id = default(int));

        //            yield return new object[]
        //            {
        //                items,
        //                10, 1, // request
        //                400,    //expected total
        //                ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize=10&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
        //                ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
        //                ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "next" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize=10&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
        //                ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize=10&page=40".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
        //            };
        //        }

        //        yield return new object[]
        //            {
        //                new [] {
        //                    new Document { Id = 1, Firstname = "Bruce",  Lastname = "Wayne" }
        //                },
        //                PaginationConfiguration.DefaultPageSize, 1, // request
        //                1,    //expected total
        //                ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
        //                ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
        //                ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
        //                ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{DocumentsController.EndpointName}/{nameof(DocumentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
        //            };
        //    }
        //}


        //[Theory]
        //[MemberData(nameof(GetAllTestCases))]
        //public async Task GetAll(IEnumerable<Document> items, int pageSize, int page,
        //    int expectedCount,
        //    Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        //{
        //    _outputHelper.WriteLine($"Testing {nameof(DocumentsController.GetAll)}({nameof(PaginationConfiguration)})");
        //    _outputHelper.WriteLine($"Page size : {pageSize}");
        //    _outputHelper.WriteLine($"Page : {page}");
        //    _outputHelper.WriteLine($"specialties store count: {items.Count()}");

        //    // Arrange
        //    using (var uow = _factory.New())
        //    {
        //        uow.Repository<DocumentMetadata>().Create(items);
        //        await uow.SaveChangesAsync();
        //    }

        //    _iHandleGetManyDocumentInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, DocumentInfo>>()))
        //        .Returns((IWantManyResources<Guid, DocumentMetadataInfo> getQuery) => Task.Run(async () =>
        //        {


        //            using (var uow = _factory.New())
        //            {
        //                PaginationConfiguration queryConfig = getQuery.Data ?? new PaginationConfiguration();

        //                IPagedResult<DocumentInfo> results = await uow.Repository<Document>()
        //                    .ReadPageAsync(x => _mapper.Map<DocumentInfo>(x), getQuery.Data.PageSize, getQuery.Data.Page);

        //                return results;
        //            }
        //        }));
        //    _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });

        //    // Act
        //    IActionResult actionResult = await _controller.Get(page, pageSize);

        //    // Assert
        //    _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(DocumentsController)}.{nameof(DocumentsController.GetAll)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");

        //    actionResult.Should()
        //            .NotBeNull().And
        //            .BeOfType<OkObjectResult>();
        //    ObjectResult okObjectResult = (OkObjectResult)actionResult;

        //    object value = okObjectResult.Value;

        //    okObjectResult.Value.Should()
        //            .NotBeNull().And
        //            .BeAssignableTo<IGenericPagedGetResponse<DocumentInfo>>();

        //    IGenericPagedGetResponse<DocumentInfo> response = (IGenericPagedGetResponse<DocumentInfo>)value;

        //    response.Count.Should()
        //            .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<DocumentInfo>)}.{nameof(IGenericPagedGetResponse<DocumentInfo>.Count)}"" property indicates the number of elements");

        //    response.Links.First.Should().Match(firstPageUrlExpectation);
        //    response.Links.Previous.Should().Match(previousPageUrlExpectation);
        //    response.Links.Next.Should().Match(nextPageUrlExpectation);
        //    response.Links.Last.Should().Match(lastPageUrlExpectation);

        //}


    }
}
