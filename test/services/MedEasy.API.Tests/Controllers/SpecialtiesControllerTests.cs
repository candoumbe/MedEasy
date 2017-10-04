﻿using AutoMapper;
using FluentAssertions;
using FluentValidation.Results;
using GenFu;
using MedEasy.API;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using MedEasy.Commands;
using MedEasy.Commands.Specialty;
using MedEasy.CQRS.Core;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using static System.StringComparison;
using static FluentValidation.Severity;

namespace MedEasy.WebApi.Tests
{
    public class SpecialtiesControllerTests : IDisposable
    {
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<ILogger<SpecialtiesController>> _loggerMock;
        private SpecialtiesController _controller;
        private ITestOutputHelper _outputHelper;
        private Mock<IHandleGetSpecialtyInfoByIdQuery> _iHandleGetOneSpecialtyInfoByIdQueryMock;
        private Mock<IHandleGetPageOfSpecialtyInfosQuery> _iHandlerGetManySpecialtyInfoQueryMock;
        private EFUnitOfWorkFactory _factory;
        private IMapper _mapper;
        private Mock<IRunCreateSpecialtyCommand> _iRunCreateSpecialtyInfoCommandMock;
        private Mock<IRunDeleteSpecialtyByIdCommand> _iRunDeleteSpecialtyInfoByIdCommandMock;
        private Mock<IHandleFindDoctorsBySpecialtyIdQuery> _iHandleFindDoctorsBySpecialtyIdQueryMock;
        private Mock<IOptionsSnapshot<MedEasyApiOptions>> _apiOptionsMock;

        public SpecialtiesControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<SpecialtiesController>>(Strict);
            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}")
                .Verifiable();

            DbContextOptionsBuilder<MedEasyContext> dbOptions = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptions.UseInMemoryDatabase($"InMemoryMedEasyDb_{Guid.NewGuid()}");
            _factory = new EFUnitOfWorkFactory(dbOptions.Options);
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _iHandleGetOneSpecialtyInfoByIdQueryMock = new Mock<IHandleGetSpecialtyInfoByIdQuery>(Strict);
            _iHandlerGetManySpecialtyInfoQueryMock = new Mock<IHandleGetPageOfSpecialtyInfosQuery>(Strict);
            _iHandleFindDoctorsBySpecialtyIdQueryMock = new Mock<IHandleFindDoctorsBySpecialtyIdQuery>(Strict);
            _iRunCreateSpecialtyInfoCommandMock = new Mock<IRunCreateSpecialtyCommand>(Strict);
            _iRunDeleteSpecialtyInfoByIdCommandMock = new Mock<IRunDeleteSpecialtyByIdCommand>(Strict);
            _apiOptionsMock = new Mock<IOptionsSnapshot<MedEasyApiOptions>>(Strict);

            _controller = new SpecialtiesController(
                _loggerMock.Object,
                _urlHelperMock.Object,
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

            _iHandlerGetManySpecialtyInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantPageOfResources<Guid, SpecialtyInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(async (IWantPageOfResources<Guid, SpecialtyInfo> getQuery, CancellationToken cancellationToken) => 
                {
                    using (IUnitOfWork uow = _factory.New())
                    {
                        PaginationConfiguration queryConfig = getQuery.Data ?? new PaginationConfiguration();

                        IPagedResult<SpecialtyInfo> results = await uow.Repository<Specialty>()
                            .ReadPageAsync(x => _mapper.Map<SpecialtyInfo>(x), getQuery.Data.PageSize, getQuery.Data.Page);

                        return results;
                    }
                });
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

            IGenericPagedGetResponse<BrowsableResource<SpecialtyInfo>> response = okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<IGenericPagedGetResponse<BrowsableResource<SpecialtyInfo>>>().Which;

            response.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);
        
            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<BrowsableResource<SpecialtyInfo>>)}.{nameof(IGenericPagedGetResponse<BrowsableResource<SpecialtyInfo>>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);

        }


        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            //Arrange
            _iHandleGetOneSpecialtyInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, SpecialtyInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Option<SpecialtyInfo>>(Option.None<SpecialtyInfo>()));

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
            _urlHelperMock.Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}");

            Guid resourceId = Guid.NewGuid();
            _iHandleGetOneSpecialtyInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, SpecialtyInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Option<SpecialtyInfo>>(new SpecialtyInfo { Id = resourceId, Name = "Specialty" }.Some()))
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
            _urlHelperMock.Verify();

        }

        [Fact]
        public async Task Post()
        {
            //Arrange
            _iRunCreateSpecialtyInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreateSpecialtyCommand>(), It.IsAny<CancellationToken>()))
                .Returns((ICreateSpecialtyCommand cmd, CancellationToken cancellationToken) => new ValueTask<Option<SpecialtyInfo, CommandException>>(
                    new SpecialtyInfo
                    {
                        Id = Guid.NewGuid(),
                        Name = cmd.Data.Name,
                        UpdatedDate = new DateTimeOffset(2012, 2, 1, 0, 0, 0, TimeSpan.Zero)
                    }
                .Some<SpecialtyInfo, CommandException>())
                .AsTask());

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

            IBrowsableResource<SpecialtyInfo> createdResource = createdAtActionResult.Value.Should()
                .BeAssignableTo<IBrowsableResource<SpecialtyInfo>>().Which;
            
            createdResource.Should()
                .NotBeNull();
            createdResource.Resource.Should()
                .NotBeNull();

            createdResource.Links.Should()
                .NotBeNull().And
                .Contain(x => x.Relation == nameof(SpecialtiesController.Doctors) );

            SpecialtyInfo resource = createdResource.Resource;
            resource.Name.Should()
                .Be(info.Name);

            resource.UpdatedDate.Should().HaveDay(1);
            resource.UpdatedDate.Should().HaveMonth(2);
            resource.UpdatedDate.Should().HaveYear(2012);

            _iRunCreateSpecialtyInfoCommandMock.Verify(mock => mock.RunAsync(It.IsAny<ICreateSpecialtyCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task FindDoctorsBySpecialtyIdShouldReturnEmptyResultWhenSpecialtyNotFound()
        {
            // Arrange
            _iHandleFindDoctorsBySpecialtyIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IFindDoctorsBySpecialtyIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Option<IPagedResult<DoctorInfo>>>(PagedResult<DoctorInfo>.Default.None<IPagedResult<DoctorInfo>>()))
                .Verifiable();
            _apiOptionsMock.Setup(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });

            // Act
            Guid specialtyId = Guid.NewGuid();
            IActionResult actionResult = await _controller.Doctors(specialtyId, new PaginationConfiguration());

            // Assert 
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<NotFoundResult>();

            _iHandleFindDoctorsBySpecialtyIdQueryMock.Verify();
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            //_urlHelperFactoryMock.VerifyAll();

        }


        [Fact]
        public void PostShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new CommandNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ValidationFailure("PropName", "A description") { Severity = Error }
                });

            //Arrange

            _iRunCreateSpecialtyInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreateSpecialtyCommand>(), It.IsAny<CancellationToken>()))
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
                    new ValidationFailure("PropName", "A description") { Severity = Error }
                });

            //Arrange
            _iHandleGetOneSpecialtyInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, SpecialtyInfo>>(), It.IsAny<CancellationToken>()))
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
                    new ValidationFailure("PropName", "A description") { Severity = Error }
                });

            //Arrange
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaultPageSize = 20, MaxPageSize = 200 });

            _iHandlerGetManySpecialtyInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantPageOfResources<Guid, SpecialtyInfo>>(), It.IsAny<CancellationToken>()))
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
                    new ValidationFailure("PropName", "A description") { Severity = Error }
                });

            //Arrange
            _iRunDeleteSpecialtyInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeleteSpecialtyByIdCommand>(), It.IsAny<CancellationToken>()))
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
            _iRunDeleteSpecialtyInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeleteSpecialtyByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Nothing.Value.Some<Nothing, CommandException>())
                .Verifiable();


            //Act
            IActionResult actionResult = await _controller.Delete(Guid.NewGuid());

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<NoContentResult>();

            _iRunDeleteSpecialtyInfoByIdCommandMock.Verify();
        }


        public void Dispose()
        {
            _loggerMock = null;
            _urlHelperMock = null;
            _controller = null;
            _outputHelper = null;
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

