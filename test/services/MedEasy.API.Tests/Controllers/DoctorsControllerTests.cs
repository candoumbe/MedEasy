using AutoMapper;
using FluentAssertions;
using GenFu;
using MedEasy.API;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using MedEasy.Commands;
using MedEasy.Commands.Doctor;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.Queries;
using MedEasy.Queries.Search;
using MedEasy.RestObjects;
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
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using static System.StringComparison;
using static System.StringSplitOptions;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.Handlers.Core.Doctor.Commands;
using MedEasy.Handlers.Core.Doctor.Queries;
using MedEasy.DTO.Search;
using MedEasy.DAL.Interfaces;

namespace MedEasy.WebApi.Tests
{
    public class DoctorsControllerTests : IDisposable
    {
        private Mock<IUrlHelperFactory> _urlHelperFactoryMock;
        private Mock<ILogger<DoctorsController>> _loggerMock;
        private DoctorsController _controller;
        private ITestOutputHelper _outputHelper;
        private IActionContextAccessor _actionContextAccessor;
        private Mock<IHandleGetDoctorInfoByIdQuery> _handleGetOneDoctorInfoByIdQueryMock;
        private Mock<IHandleGetManyDoctorInfosQuery> _handlerGetManyDoctorInfoQueryMock;
        private EFUnitOfWorkFactory _factory;
        private IMapper _mapper;
        private Mock<IRunCreateDoctorCommand> _iRunCreateDoctorInfoCommandMock;
        private Mock<IRunDeleteDoctorInfoByIdCommand> _iRunDeleteDoctorInfoByIdCommandMock;
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;
        private Mock<IRunPatchDoctorCommand> _iRunPatchDoctorCommandMock;
        private Mock<IHandleSearchQuery> _iHandleSearchQueryMock;

        public DoctorsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<DoctorsController>>(Strict);
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
           
            _handleGetOneDoctorInfoByIdQueryMock = new Mock<IHandleGetDoctorInfoByIdQuery>(Strict);
            _handlerGetManyDoctorInfoQueryMock = new Mock<IHandleGetManyDoctorInfosQuery>(Strict);
            _iRunCreateDoctorInfoCommandMock = new Mock<IRunCreateDoctorCommand>(Strict);
            _iRunDeleteDoctorInfoByIdCommandMock = new Mock<IRunDeleteDoctorInfoByIdCommand>(Strict);
            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);
            _iRunPatchDoctorCommandMock = new Mock<IRunPatchDoctorCommand>(Strict);
            _iHandleSearchQueryMock = new Mock<IHandleSearchQuery>(Strict);

            _mapper = AutoMapperConfig.Build().CreateMapper();

            _controller = new DoctorsController(_loggerMock.Object, _urlHelperFactoryMock.Object, _actionContextAccessor, _apiOptionsMock.Object, _mapper,
                _handleGetOneDoctorInfoByIdQueryMock.Object,
                _handlerGetManyDoctorInfoQueryMock.Object,
                _iRunCreateDoctorInfoCommandMock.Object,
                _iRunDeleteDoctorInfoByIdCommandMock.Object,
                _iRunPatchDoctorCommandMock.Object,
                _iHandleSearchQueryMock.Object);
        }


        public void Dispose()
        {
            _loggerMock = null;
            _urlHelperFactoryMock = null;
            _controller = null;
            _outputHelper = null;
            _actionContextAccessor = null;
            _handleGetOneDoctorInfoByIdQueryMock = null;
            _handlerGetManyDoctorInfoQueryMock = null;
            _iRunCreateDoctorInfoCommandMock = null;
            _iRunDeleteDoctorInfoByIdCommandMock = null;
            _factory = null;
            _mapper = null;
            _apiOptionsMock = null;
            _iRunPatchDoctorCommandMock = null;
            _iHandleSearchQueryMock = null;
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
                            Enumerable.Empty<Doctor>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, PaginationConfiguration.MaxPageSize))}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            ((Expression<Func<Link, bool>>) (x => x == null))  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<Doctor> items = A.ListOf<Doctor>(400);
                    items.ForEach(item => item.Id = default(int));
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("next") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=14".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }
                {
                    IEnumerable<Doctor> items = A.ListOf<Doctor>(400);
                    items.ForEach(item => item.Id = default(int));

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize=10&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("next") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize=10&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize=10&page=40".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }

                yield return new object[]
                    {
                        A.ListOf<Doctor>(1),
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        1,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
                    };
            }
        }

        
        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Doctor> items, int pageSize, int page,
            int expectedCount,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(DoctorsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _factory.New())
            {
                uow.Repository<Doctor>().Create(items);
                await uow.SaveChangesAsync();
            }

            _handlerGetManyDoctorInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, DoctorInfo>>()))
                .Returns((IWantManyResources<Guid, DoctorInfo> getQuery) => Task.Run(async () =>
                {


                    using (IUnitOfWork uow = _factory.New())
                    {
                        PaginationConfiguration queryConfig = getQuery.Data ?? new PaginationConfiguration();

                        IPagedResult<DoctorInfo> results = await uow.Repository<Doctor>()
                            .ReadPageAsync(x => _mapper.Map<DoctorInfo>(x), getQuery.Data.PageSize, getQuery.Data.Page);

                        return results;
                    }
                }));

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });
            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { PageSize = pageSize, Page = page });

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(DoctorsController)}.{nameof(DoctorsController.GetAll)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");


            actionResult.Should()
                    .NotBeNull()
                    .And.BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<IGenericPagedGetResponse<DoctorInfo>>();

            IGenericPagedGetResponse<DoctorInfo> response = (IGenericPagedGetResponse<DoctorInfo>)value;

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<DoctorInfo>)}.{nameof(IGenericPagedGetResponse<BrowsableResource<DoctorInfo>>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);
           

        }


        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            //Arrange
            _handleGetOneDoctorInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, DoctorInfo>>()))
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

            Guid doctorId = Guid.NewGuid();
            _handleGetOneDoctorInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, DoctorInfo>>()))
                .ReturnsAsync(new DoctorInfo {Id = doctorId, Firstname = "Bruce", Lastname = "Wayne" })
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Get(doctorId);

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeOfType<BrowsableResource<DoctorInfo>>().Which
                    .Links.Should()
                        .NotBeNull();

            BrowsableResource<DoctorInfo> result = (BrowsableResource<DoctorInfo>)((OkObjectResult)actionResult).Value;
            IEnumerable<Link> links = result.Links;

            DoctorInfo resource = result.Resource;
            resource.Should().NotBeNull();
            resource.Id.Should().Be(doctorId);
            resource.Firstname.Should().Be("Bruce");
            resource.Lastname.Should().Be("Wayne");

            links.Should()
                .NotBeNull().And
                .Contain(x => x.Relation.Contains("self"));

            Link location = links.Single(x => x.Relation.Contains("self"));

            location.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?{nameof(DoctorInfo.Id)}={resource.Id}");
            location.Relation?.Should()
                .BeEquivalentTo("self");

            _handleGetOneDoctorInfoByIdQueryMock.Verify();
            _urlHelperFactoryMock.Verify();

        }

        [Fact]
        public async Task Post()
        {
            //Arrange
            _iRunCreateDoctorInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreateDoctorCommand>()))
                .Returns((ICreateDoctorCommand cmd) => Task.Run(() 
                => new DoctorInfo {
                    Id = Guid.NewGuid(),
                    Firstname = cmd.Data.Firstname,
                    Lastname = cmd.Data.Lastname,
                    UpdatedDate = new DateTimeOffset(2012, 2, 1, 0, 0, 0, TimeSpan.Zero)
                }));

            //Act
            CreateDoctorInfo info = new CreateDoctorInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne"
            };

           IActionResult actionResult = await _controller.Post(info);

            //Assert
            CreatedAtActionResult createdActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;

            createdActionResult.ActionName.Should().Be(nameof(DoctorsController.Get));
            createdActionResult.ControllerName.Should().Be(DoctorsController.EndpointName);
            createdActionResult.RouteValues.Should()
                .HaveCount(1).And
                .ContainKey("id").WhichValue.Should()
                    .BeOfType<Guid>().And
                    .NotBe(Guid.Empty);
           


            DoctorInfo createdResource = (DoctorInfo)((CreatedAtActionResult)actionResult).Value;

            createdResource.Should()
                .NotBeNull();
            createdResource.Firstname.Should()
                .Be(info.Firstname);
            createdResource.Lastname.Should()
                .Be(info.Lastname);
            createdResource.UpdatedDate.Should().HaveDay(1);
            createdResource.UpdatedDate.Should().HaveMonth(2);
            createdResource.UpdatedDate.Should().HaveYear(2012);

            _iRunCreateDoctorInfoCommandMock.Verify(mock => mock.RunAsync(It.IsAny<ICreateDoctorCommand>()), Times.Once);

        }

        [Fact]
        public void PostShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new CommandNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrRequiredField", $"{nameof(CreateDoctorInfo.Lastname)}", ErrorLevel.Error)
                });

            //Arrange

            _iRunCreateDoctorInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreateDoctorCommand>()))
                .Throws(exceptionFromTheHandler);

            //Act
            CreateDoctorInfo info = new CreateDoctorInfo
            {
                Firstname = "Bruce",
                //Lastname = "Wayne"
            };

            Func<Task> action = async () => await _controller.Post(info);

            //Assert
            action.ShouldThrow<CommandNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iRunCreateDoctorInfoCommandMock.Verify();

        }



        public static IEnumerable<object> PatchCases
        {
            get
            {
                {
                    JsonPatchDocument<DoctorInfo> patchDocument = new JsonPatchDocument<DoctorInfo>();
                    patchDocument.Replace(x => x.Firstname, "Bruce");
                    yield return new object[]
                    {
                        new Doctor { Id = 1, },
                        patchDocument.Operations,
                        ((Expression<Func<Doctor, bool>>)(x => x.Id == 1 && x.Firstname == "Bruce"))
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(PatchCases))]
        public async Task Patch(Doctor source, IEnumerable<Operation<DoctorInfo>> operations, Expression<Func<Doctor, bool>> patchResultExpectation)
        {

            // Arrange
            _iRunPatchDoctorCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IPatchCommand<Guid, Doctor>>()))
                .Returns((IPatchCommand<Guid, Doctor> command) => Task.Run(() => 
                {
                    command.Data.PatchDocument.ApplyTo(source);

                    return Nothing.Value;
                }));


            // Act
            JsonPatchDocument<DoctorInfo> patchDocument = new JsonPatchDocument<DoctorInfo>();
            patchDocument.Operations.AddRange(operations);
            IActionResult actionResult = await _controller.Patch(Guid.NewGuid(), patchDocument);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<OkResult>();

            _iRunPatchDoctorCommandMock.Verify();

            source.Should().Match(patchResultExpectation);

        }



        public static IEnumerable<object> SearchCases
        {
            get
            {
                {
                    SearchDoctorInfo searchInfo = new SearchDoctorInfo
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };
                    yield return new object[]
                    {
                        Enumerable.Empty<DoctorInfo>(),
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation.Contains("first") &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.PageSize)}={searchInfo.PageSize}")  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.Sort)}={searchInfo.Sort}" )

                           )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };

                }
                {
                    SearchDoctorInfo searchInfo = new SearchDoctorInfo
                    {
                        Firstname = "!bruce",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };
                    yield return new object[]
                    {
                        new [] {
                            new DoctorInfo { Firstname = "Bruce", Lastname = "Wayne" }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation.Contains("first") &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.PageSize)}={searchInfo.PageSize}")  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.Sort)}={searchInfo.Sort}" )

                           )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };

                }
                {
                    SearchDoctorInfo searchInfo = new SearchDoctorInfo
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                    };
                    yield return new object[]
                    {
                        new[] {
                            new DoctorInfo { Firstname = "bruce" }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation.Contains("first") &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 3 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchDoctorInfo.PageSize)}={searchInfo.PageSize}")

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
        public async Task Search(IEnumerable<DoctorInfo> entries, SearchDoctorInfo searchRequest,
        Expression<Func<Link, bool>> firstPageLinkExpectation, Expression<Func<Link, bool>> previousPageLinkExpectation, Expression<Func<Link, bool>> nextPageLinkExpectation, Expression<Func<Link, bool>> lastPageLinkExpectation)
        {
            _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
            _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");


            // Arrange
            MedEasyApiOptions apiOptions = new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 50 };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);
            _iHandleSearchQueryMock.Setup(mock => mock.Search<Doctor, DoctorInfo>(It.IsAny<SearchQuery<DoctorInfo>>()))
                    .Returns((SearchQuery<DoctorInfo> query) => Task.Run(() =>
                    {
                        SearchQueryInfo<DoctorInfo> data = query.Data;
                        Expression<Func<DoctorInfo, bool>> filter = data.Filter.ToExpression<DoctorInfo>();
                        int page = query.Data.Page;
                        int pageSize = query.Data.PageSize;
                        Func<DoctorInfo, bool> fnFilter = filter.Compile();

                        IEnumerable<DoctorInfo> result = entries.Where(fnFilter)
                            .Skip(page * pageSize)
                            .Take(pageSize);

                        IPagedResult<DoctorInfo> pageOfResult = new PagedResult<DoctorInfo>(result, entries.Count(fnFilter), pageSize);
                        return pageOfResult;
                    })
                    );


            // Act
            IActionResult actionResult = await _controller.Search(searchRequest);

            // Assert
            IGenericPagedGetResponse<DoctorInfo> content = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IGenericPagedGetResponse<DoctorInfo>>().Which;


            content.Items.Should()
                .NotBeNull();

            content.Links.Should().NotBeNull();
            PagedRestResponseLink links = content.Links;

            links.First.Should().Match(firstPageLinkExpectation);
            links.Previous.Should().Match(previousPageLinkExpectation);
            links.Next.Should().Match(nextPageLinkExpectation);
            links.Last.Should().Match(nextPageLinkExpectation);



        }


        [Fact]
        public void GetShouldNotSwallowQueryNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _handleGetOneDoctorInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, DoctorInfo>>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();
            

            //Act
            Func<Task> action = async () => await _controller.Get(Guid.NewGuid());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _handleGetOneDoctorInfoByIdQueryMock.Verify();

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

            _handlerGetManyDoctorInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, DoctorInfo>>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.GetAll(new PaginationConfiguration());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _handlerGetManyDoctorInfoQueryMock.Verify();

        }


        [Fact]
        public void DeleteShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _iRunDeleteDoctorInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeleteDoctorByIdCommand>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.Delete(Guid.NewGuid());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _handlerGetManyDoctorInfoQueryMock.Verify();
        }

        [Fact]
        public async Task DeleteMustRelyOnDeleteCommandHandler()
        {

            //Arrange
            _iRunDeleteDoctorInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeleteDoctorByIdCommand>()))
                .Returns(Nothing.Task)
                .Verifiable();


            //Act
            await _controller.Delete(Guid.NewGuid());

            //Assert
            _iRunDeleteDoctorInfoByIdCommandMock.Verify();
        }




    }
}

