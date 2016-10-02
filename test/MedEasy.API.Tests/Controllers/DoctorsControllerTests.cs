using MedEasy.Objects;
using System.Collections.Generic;
using FluentAssertions;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.MockBehavior;
using System;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit.Abstractions;
using static Newtonsoft.Json.JsonConvert;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using GenFu;
using static System.StringComparison;
using MedEasy.Mapping;
using MedEasy.DAL.Repositories;
using MedEasy.Commands.Doctor;
using MedEasy.Handlers.Doctor.Commands;
using MedEasy.Queries.Doctor;
using MedEasy.Queries;
using MedEasy.Handlers.Doctor.Queries;
using MedEasy.Handlers.Exceptions;
using MedEasy.Validators;
using MedEasy.API;
using Microsoft.Extensions.Options;

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
            _controller = new DoctorsController(_loggerMock.Object, _urlHelperFactoryMock.Object, _actionContextAccessor, _apiOptionsMock.Object,
                _handleGetOneDoctorInfoByIdQueryMock.Object,
                _handlerGetManyDoctorInfoQueryMock.Object,
                _iRunCreateDoctorInfoCommandMock.Object,
                _iRunDeleteDoctorInfoByIdCommandMock.Object);

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
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, GenericGetQuery.MaxPageSize))}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
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
                        GenericGetQuery.DefaultPageSize, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "next" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=14".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
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
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize=10&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "next" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize=10&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize=10&page=40".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }

                yield return new object[]
                    {
                        A.ListOf<Doctor>(1),
                        GenericGetQuery.DefaultPageSize, 1, // request
                        1,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "first" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Rel == "last" && $"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?pageSize={GenericGetQuery.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
                    };
            }
        }


        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Doctor> items, int pageSize, int page,
            int expectedCount,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(DoctorsController.Get)}({nameof(GenericGetQuery)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (var uow = _factory.New())
            {
                uow.Repository<Doctor>().Create(items);
                await uow.SaveChangesAsync();
            }

            _handlerGetManyDoctorInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, DoctorInfo>>()))
                .Returns((IWantManyResources<Guid, DoctorInfo> getQuery) => Task.Run(async () =>
                {


                    using (var uow = _factory.New())
                    {
                        GenericGetQuery queryConfig = getQuery.Data ?? new GenericGetQuery();

                        IPagedResult<DoctorInfo> results = await uow.Repository<Doctor>()
                            .ReadPageAsync(x => _mapper.Map<DoctorInfo>(x), getQuery.Data.PageSize, getQuery.Data.Page);

                        return results;
                    }
                }));

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });
            // Act
            IActionResult actionResult = await _controller.Get(new GenericGetQuery { PageSize = pageSize, Page = page });

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(DoctorsController)}.{nameof(DoctorsController.GetAll)} must always check that {nameof(GenericGetQuery.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");


            actionResult.Should()
                    .NotBeNull()
                    .And.BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<GenericPagedGetResponse<BrowsableResource<DoctorInfo>>>();

            GenericPagedGetResponse<BrowsableResource<DoctorInfo>> response = (GenericPagedGetResponse<BrowsableResource<DoctorInfo>>)value;

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(GenericPagedGetResponse<BrowsableResource<DoctorInfo>>)}.{nameof(GenericPagedGetResponse<BrowsableResource<DoctorInfo>>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);
           

        }


        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            //Arrange
            _handleGetOneDoctorInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, int, DoctorInfo>>()))
                .ReturnsAsync(null);

            //Act
            IActionResult actionResult = await _controller.Get(1);

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

            _handleGetOneDoctorInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, int, DoctorInfo>>()))
                .ReturnsAsync(new DoctorInfo {Id = 1, Firstname = "Bruce", Lastname = "Wayne" })
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Get(1);

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeOfType<BrowsableResource<DoctorInfo>>().Which
                    .Location.Should()
                        .NotBeNull();

            BrowsableResource<DoctorInfo> result = (BrowsableResource<DoctorInfo>)((OkObjectResult)actionResult).Value;
            Link location = result.Location;
            location.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?{nameof(DoctorInfo.Id)}=1");
            location.Rel.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo("self");

            DoctorInfo resource = result.Resource;
            resource.Should().NotBeNull();
            resource.Id.Should().Be(1);
            resource.Firstname.Should().Be("Bruce");
            resource.Lastname.Should().Be("Wayne");

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
                    Id = 1,
                    Firstname = cmd.Data.Firstname,
                    Lastname = cmd.Data.Lastname,
                    UpdatedDate = new DateTime(2012, 2, 1)
                }));

            //Act
            CreateDoctorInfo info = new CreateDoctorInfo
            {
                Firstname = "Bruce",
                Lastname = "Wayne"
            };

           IActionResult actionResult = await _controller.Post(info);

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<BrowsableResource<DoctorInfo>>();


            BrowsableResource<DoctorInfo> createdResource = (BrowsableResource<DoctorInfo>)((OkObjectResult)actionResult).Value;

            createdResource.Should()
                .NotBeNull();

            createdResource.Location.Should()
                .NotBeNull();

            createdResource.Location.Href.Should()
                .NotBeNull().And
                .MatchEquivalentOf($"api/{DoctorsController.EndpointName}/{nameof(DoctorsController.Get)}?{nameof(DoctorInfo.Id)}=*");
            
            createdResource.Resource.Firstname.Should()
                .Be(info.Firstname);
            createdResource.Resource.Lastname.Should()
                .Be(info.Lastname);
            createdResource.Resource.UpdatedDate.Should().Be(1.February(2012));

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

        [Fact]
        public void GetShouldNotSwallowQueryNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _handleGetOneDoctorInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, int, DoctorInfo>>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();
            

            //Act
            Func<Task> action = async () => await _controller.Get(1);

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
            Func<Task> action = async () => await _controller.GetAll(new GenericGetQuery());

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
            Func<Task> action = async () => await _controller.Delete(1);

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _handlerGetManyDoctorInfoQueryMock.Verify();
        }

        [Fact]
        public async Task DeleteMustRelyOnDeleteCommandHandler()
        {

            //Arrange
            _iRunDeleteDoctorInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeleteDoctorByIdCommand>()))
                .Returns(Task.CompletedTask)
                .Verifiable();


            //Act
            await _controller.Delete(1);

            //Assert
            _iRunDeleteDoctorInfoByIdCommandMock.Verify();
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
        }
    }
}

