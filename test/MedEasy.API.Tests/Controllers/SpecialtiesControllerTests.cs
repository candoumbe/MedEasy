using AutoMapper;
using FluentAssertions;
using GenFu;
using MedEasy.API;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using MedEasy.Commands;
using MedEasy.Commands.Specialty;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Handlers.Core.Specialty.Commands;
using MedEasy.Handlers.Core.Specialty.Queries;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.Queries;
using MedEasy.Queries.Specialty;
using MedEasy.RestObjects;
using MedEasy.Validators;
using Microsoft.AspNetCore.Http;
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
using static System.StringComparison;

namespace MedEasy.WebApi.Tests
{
    public class SpecialtiesControllerTests : IDisposable
    {
        private Mock<IUrlHelperFactory> _urlHelperFactoryMock;
        private Mock<ILogger<SpecialtiesController>> _loggerMock;
        private SpecialtiesController _controller;
        private ITestOutputHelper _outputHelper;
        private IActionContextAccessor _actionContextAccessor;
        private Mock<IHandleGetSpecialtyInfoByIdQuery> _iHandleGetOneSpecialtyInfoByIdQueryMock;
        private Mock<IHandleGetManySpecialtyInfosQuery> _iHandlerGetManySpecialtyInfoQueryMock;
        private EFUnitOfWorkFactory _factory;
        private IMapper _mapper;
        private Mock<IRunCreateSpecialtyCommand> _iRunCreateSpecialtyInfoCommandMock;
        private Mock<IRunDeleteSpecialtyByIdCommand> _iRunDeleteSpecialtyInfoByIdCommandMock;
        private Mock<IHandleFindDoctorsBySpecialtyIdQuery> _iHandleFindDoctorsBySpecialtyIdQueryMock;
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;

        public SpecialtiesControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<SpecialtiesController>>(Strict);
            _urlHelperFactoryMock = new Mock<IUrlHelperFactory>(Strict);
            _urlHelperFactoryMock.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>()).Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}")
                .Verifiable();

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

            _iHandleGetOneSpecialtyInfoByIdQueryMock = new Mock<IHandleGetSpecialtyInfoByIdQuery>(Strict);
            _iHandlerGetManySpecialtyInfoQueryMock = new Mock<IHandleGetManySpecialtyInfosQuery>(Strict);
            _iHandleFindDoctorsBySpecialtyIdQueryMock = new Mock<IHandleFindDoctorsBySpecialtyIdQuery>(Strict);
            _iRunCreateSpecialtyInfoCommandMock = new Mock<IRunCreateSpecialtyCommand>(Strict);
            _iRunDeleteSpecialtyInfoByIdCommandMock = new Mock<IRunDeleteSpecialtyByIdCommand>(Strict);
            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);

            _controller = new SpecialtiesController(
                _loggerMock.Object, 
                _urlHelperFactoryMock.Object, 
                _actionContextAccessor,
                _apiOptionsMock.Object,
                _iHandleGetOneSpecialtyInfoByIdQueryMock.Object,
                _iHandlerGetManySpecialtyInfoQueryMock.Object,
                _iRunCreateSpecialtyInfoCommandMock.Object,
                _iRunDeleteSpecialtyInfoByIdCommandMock.Object,
                _iHandleFindDoctorsBySpecialtyIdQueryMock.Object);

        }


        public static IEnumerable<object> GetAllTestCases
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
                            Enumerable.Empty<Specialty>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first")  && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            ((Expression<Func<Link, bool>>) (x => x == null))  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<Specialty> items = A.ListOf<Specialty>(400);
                    items.ForEach(item => item.Id = default(int));
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first")  && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("next") && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=14".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }
                {
                    IEnumerable<Specialty> items = A.ListOf<Specialty>(400);
                    items.ForEach(item => item.Id = default(int));

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first")  && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize=10&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("next") && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize=10&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize=10&page=40".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }

                yield return new object[]
                    {
                        new [] {
                            new Specialty { Id = 1, Name = "Médecine générale" }
                        },
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        1,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
                    };
            }
        }


        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Specialty> items, int pageSize, int page,
            int expectedCount,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(SpecialtiesController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _factory.New())
            {
                uow.Repository<Specialty>().Create(items);
                await uow.SaveChangesAsync();
            }

            _iHandlerGetManySpecialtyInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, SpecialtyInfo>>()))
                .Returns((IWantManyResources<Guid, SpecialtyInfo> getQuery) => Task.Run(async () =>
                {


                    using (IUnitOfWork uow = _factory.New())
                    {
                        PaginationConfiguration queryConfig = getQuery.Data ?? new PaginationConfiguration();

                        IPagedResult<SpecialtyInfo> results = await uow.Repository<Specialty>()
                            .ReadPageAsync(x => _mapper.Map<SpecialtyInfo>(x), getQuery.Data.PageSize, getQuery.Data.Page);

                        return results;
                    }
                }));
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });
            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { PageSize = pageSize, Page = page });

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(SpecialtiesController)}.{nameof(SpecialtiesController.GetAll)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");

            actionResult.Should()
                    .NotBeNull()
                    .And.BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<IGenericPagedGetResponse<SpecialtyInfo>>();

            IGenericPagedGetResponse<SpecialtyInfo> response = (IGenericPagedGetResponse<SpecialtyInfo>)value;

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<SpecialtyInfo>)}.{nameof(IGenericPagedGetResponse<SpecialtyInfo>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);
            
        }


        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            //Arrange
            _iHandleGetOneSpecialtyInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, SpecialtyInfo>>()))
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

            Guid resourceId = Guid.NewGuid();
            _iHandleGetOneSpecialtyInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, SpecialtyInfo>>()))
                .ReturnsAsync(new SpecialtyInfo { Id =resourceId, Name = "Specialty" })
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Get(resourceId);

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<IBrowsableResource<SpecialtyInfo>>().Which
                    .Links.Should()
                        .NotBeNull();

            IBrowsableResource<SpecialtyInfo> result = (IBrowsableResource<SpecialtyInfo>)((OkObjectResult)actionResult).Value;
            IEnumerable<Link> links = result.Links;
            links.Should()
                .NotBeNull().And
                .Contain(x => x.Relation.Contains("self"));

            Link location = result.Links.Single(x => x.Relation.Contains("self"));
            location.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Get)}?{nameof(SpecialtyInfo.Id)}={resourceId}");
            location.Relation.Should()
                .NotBeNullOrEmpty().And
                .Contain("self");

            SpecialtyInfo resource = result.Resource;
            resource.Should().NotBeNull();
            resource.Id.Should().Be(resourceId);
            resource.Name.Should().Be("Specialty");

            _iHandleGetOneSpecialtyInfoByIdQueryMock.Verify();
            _urlHelperFactoryMock.Verify();

        }

        [Fact]
        public async Task Post()
        {
            //Arrange
            _iRunCreateSpecialtyInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreateSpecialtyCommand>()))
                .Returns((ICreateSpecialtyCommand cmd) => Task.Run(()
                => new SpecialtyInfo { Id = Guid.NewGuid(), Name = cmd.Data.Name, UpdatedDate = new DateTimeOffset(2012, 2, 1, 0, 0, 0, TimeSpan.Zero) }));

            //Act
            CreateSpecialtyInfo info = new CreateSpecialtyInfo
            {
                Name = "médecine générale"
            };

            IActionResult actionResult = await _controller.Post(info);

            //Assert


            CreatedAtActionResult createdAtActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;

            createdAtActionResult.ActionName.Should().Be(nameof(SpecialtiesController.Get));
            createdAtActionResult.ControllerName.Should().Be(SpecialtiesController.EndpointName);
            createdAtActionResult.RouteValues.Should().NotBeNull();
            //createdAtActionResult.RouteValues.ToQueryString().Should().MatchRegex(@"[iI]d=[1-9]\d*");


            SpecialtyInfo createdResource = (SpecialtyInfo)createdAtActionResult.Value;
            createdResource.Should()
                .NotBeNull();

            createdResource.Name.Should()
                .Be(info.Name);

            createdResource.UpdatedDate.Should().HaveDay(1);
            createdResource.UpdatedDate.Should().HaveMonth(2);
            createdResource.UpdatedDate.Should().HaveYear(2012);

            _iRunCreateSpecialtyInfoCommandMock.Verify(mock => mock.RunAsync(It.IsAny<ICreateSpecialtyCommand>()), Times.Once);

        }

        [Fact]
        public async Task FindDoctorsBySpecialtyIdShouldReturnEmptyResultWhenNoDoctorFound()
        {
            // Arrange
            _iHandleFindDoctorsBySpecialtyIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IFindDoctorsBySpecialtyIdQuery>()))
                .ReturnsAsync(PagedResult<DoctorInfo>.Default)
                .Verifiable();
            _apiOptionsMock.Setup(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });

            // Act
            Guid doctorId = Guid.NewGuid();
            IActionResult actionResult = await _controller.Doctors(doctorId, new PaginationConfiguration());

            // Assert 
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>();

            OkObjectResult objectResult = (OkObjectResult)actionResult;
            objectResult.Value.Should()
                .BeAssignableTo<IGenericPagedGetResponse<DoctorInfo>>();

            IGenericPagedGetResponse<DoctorInfo> pagedResponse = (IGenericPagedGetResponse<DoctorInfo>)objectResult.Value;
            pagedResponse.Links.Should().NotBeNull();

            Link firstPageLink = pagedResponse.Links.First;
            firstPageLink.Should().NotBeNull();
            firstPageLink.Relation.Should()
                .BeEquivalentTo("first");
            firstPageLink.Href.Should()
                .BeEquivalentTo($"api/{SpecialtiesController.EndpointName}/{nameof(SpecialtiesController.Doctors)}?id={doctorId}&pageSize=30&page=1");

            pagedResponse.Links.Previous.Should().BeNull();
            pagedResponse.Links.Next.Should().BeNull();
            pagedResponse.Links.Last.Should().BeNull();

            _iHandleFindDoctorsBySpecialtyIdQueryMock.Verify();
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            //_urlHelperFactoryMock.VerifyAll();

        }


        [Fact]
        public void PostShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new CommandNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrDuplicateName", "A specialty with the same name already exists", ErrorLevel.Error)
                });

            //Arrange

            _iRunCreateSpecialtyInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreateSpecialtyCommand>()))
                .Throws(exceptionFromTheHandler);

            //Act
            CreateSpecialtyInfo info = new CreateSpecialtyInfo
            {
                Name = "médecine générale"
            };

            Func<Task> action = async () => await _controller.Post(info);

            //Assert
            action.ShouldThrow<CommandNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iRunCreateSpecialtyInfoCommandMock.Verify();

        }

        [Fact]
        public void GetShouldNotSwallowQueryNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _iHandleGetOneSpecialtyInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, SpecialtyInfo>>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.Get(Guid.NewGuid());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iHandleGetOneSpecialtyInfoByIdQueryMock.Verify();

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

            _iHandlerGetManySpecialtyInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, SpecialtyInfo>>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();

            //Act
            Func<Task> action = async () => await _controller.GetAll(new PaginationConfiguration());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iHandlerGetManySpecialtyInfoQueryMock.Verify();

        }

        [Fact]
        public void DeleteShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _iRunDeleteSpecialtyInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeleteSpecialtyByIdCommand>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.Delete(Guid.NewGuid());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _iHandlerGetManySpecialtyInfoQueryMock.Verify();
        }

        [Fact]
        public async Task DeleteMustRelyOnDeleteCommandHandler()
        {

            //Arrange
            _iRunDeleteSpecialtyInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeleteSpecialtyByIdCommand>()))
                .ReturnsAsync(Nothing.Value)
                .Verifiable();


            //Act
            IActionResult actionResult = await _controller.Delete(Guid.NewGuid());

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<OkResult>();

            _iRunDeleteSpecialtyInfoByIdCommandMock.Verify();
        }


        public void Dispose()
        {
            _loggerMock = null;
            _urlHelperFactoryMock = null;
            _controller = null;
            _outputHelper = null;
            _actionContextAccessor = null;
            _apiOptionsMock = null;

            _iHandleGetOneSpecialtyInfoByIdQueryMock = null;
            _iHandlerGetManySpecialtyInfoQueryMock = null;
            _iHandleFindDoctorsBySpecialtyIdQueryMock = null;

            _iRunCreateSpecialtyInfoCommandMock = null;
            _iRunDeleteSpecialtyInfoByIdCommandMock = null;

            _factory = null;
            _mapper = null;
        }
    }
}

