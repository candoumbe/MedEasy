﻿using AutoMapper;
using FluentAssertions;
using GenFu;
using MedEasy.API;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using MedEasy.Commands;
using MedEasy.Commands.Appointment;
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
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Handlers.Core.Appointment.Queries;
using MedEasy.DTO.Search;
using MedEasy.DAL.Interfaces;
using System.Threading;
using System.Net;

namespace MedEasy.WebApi.Tests
{
    /// <summary>
    /// Unit tests for <see cref="AppointmentsController"/>
    /// </summary>
    public class AppointmentsControllerTests : IDisposable
    {
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<ILogger<AppointmentsController>> _loggerMock;
        private AppointmentsController _controller;
        private ITestOutputHelper _outputHelper;
        private IActionContextAccessor _actionContextAccessor;
        private Mock<IHandleGetAppointmentInfoByIdQuery> _handleGetOneAppointmentInfoByIdQueryMock;
        private Mock<IHandleGetManyAppointmentInfosQuery> _handlerGetManyAppointmentInfoQueryMock;
        private EFUnitOfWorkFactory _factory;
        private IMapper _mapper;
        private Mock<IRunCreateAppointmentCommand> _iRunCreateAppointmentInfoCommandMock;
        private Mock<IRunDeleteAppointmentInfoByIdCommand> _iRunDeleteAppointmentInfoByIdCommandMock;
        private Mock<IOptionsSnapshot<MedEasyApiOptions>> _apiOptionsMock;
        private Mock<IRunPatchAppointmentCommand> _iRunPatchAppointmentCommandMock;
        private Mock<IHandleSearchQuery> _iHandleSearchQueryMock;

        private const string GuidRegexPattern = @"^[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";

        /// <summary>
        /// Builds new <see cref="AppointmentsControllerTests"/> instance
        /// </summary>
        /// <param name="outputHelper"></param>
        public AppointmentsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<AppointmentsController>>(Strict);
            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
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

            _handleGetOneAppointmentInfoByIdQueryMock = new Mock<IHandleGetAppointmentInfoByIdQuery>(Strict);
            _handlerGetManyAppointmentInfoQueryMock = new Mock<IHandleGetManyAppointmentInfosQuery>(Strict);
            _iRunCreateAppointmentInfoCommandMock = new Mock<IRunCreateAppointmentCommand>(Strict);
            _iRunDeleteAppointmentInfoByIdCommandMock = new Mock<IRunDeleteAppointmentInfoByIdCommand>(Strict);
            _apiOptionsMock = new Mock<IOptionsSnapshot<MedEasyApiOptions>>(Strict);
            _iRunPatchAppointmentCommandMock = new Mock<IRunPatchAppointmentCommand>(Strict);
            _iHandleSearchQueryMock = new Mock<IHandleSearchQuery>(Strict);

            _mapper = AutoMapperConfig.Build().CreateMapper();

            _controller = new AppointmentsController(_loggerMock.Object, _urlHelperMock.Object, _apiOptionsMock.Object, _mapper, _handleGetOneAppointmentInfoByIdQueryMock.Object,
                _handlerGetManyAppointmentInfoQueryMock.Object,
                _iRunCreateAppointmentInfoCommandMock.Object,
                _iRunDeleteAppointmentInfoByIdCommandMock.Object,
                _iRunPatchAppointmentCommandMock.Object,
                _iHandleSearchQueryMock.Object);
        }


        public void Dispose()
        {
            _loggerMock = null;
            _urlHelperMock = null;
            _controller = null;
            _outputHelper = null;
            _actionContextAccessor = null;
            _handleGetOneAppointmentInfoByIdQueryMock = null;
            _handlerGetManyAppointmentInfoQueryMock = null;
            _iRunCreateAppointmentInfoCommandMock = null;
            _iRunDeleteAppointmentInfoByIdCommandMock = null;
            _factory = null;
            _mapper = null;
            _apiOptionsMock = null;
            _iRunPatchAppointmentCommandMock = null;
            _iHandleSearchQueryMock = null;
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
                            Enumerable.Empty<Appointment>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, PaginationConfiguration.MaxPageSize))}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            ((Expression<Func<Link, bool>>) (x => x == null))  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<Appointment> items = A.ListOf<Appointment>(400);
                    items.ForEach(item => item.Id = default(int));
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("next") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=14".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }
                {
                    IEnumerable<Appointment> items = A.ListOf<Appointment>(400);
                    items.ForEach(item => item.Id = default(int));

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize=10&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("next") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize=10&page=2".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize=10&page=40".Equals(x.Href, OrdinalIgnoreCase))),  // expected link to last page
                    };
                }

                yield return new object[]
                    {
                        A.ListOf<Appointment>(1),
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        1,    //expected total
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("first") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains("last") && $"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?pageSize={PaginationConfiguration.DefaultPageSize}&page=1".Equals(x.Href, OrdinalIgnoreCase))), // expected link to last page
                    };
            }
        }


        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Appointment> items, int pageSize, int page,
            int expectedCount,
            Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(AppointmentsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _factory.New())
            {
                uow.Repository<Appointment>().Create(items);
                await uow.SaveChangesAsync();
            }

            _handlerGetManyAppointmentInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, AppointmentInfo>>(), It.IsAny<CancellationToken>()))
                .Returns((IWantManyResources<Guid, AppointmentInfo> getQuery, CancellationToken cancellationToken) => Task.Run(async () =>
                {


                    using (IUnitOfWork uow = _factory.New())
                    {
                        PaginationConfiguration queryConfig = getQuery.Data ?? new PaginationConfiguration();

                        IPagedResult<AppointmentInfo> results = await uow.Repository<Appointment>()
                            .ReadPageAsync(x => _mapper.Map<AppointmentInfo>(x), getQuery.Data.PageSize, getQuery.Data.Page);

                        return results;
                    }
                }));

            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaulLimit = 30, MaxPageSize = 200 });
            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { PageSize = pageSize, Page = page });

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(AppointmentsController)}.{nameof(AppointmentsController.GetAll)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MedEasyApiOptions.MaxPageSize)} value");


            actionResult.Should()
                    .NotBeNull()
                    .And.BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<IGenericPagedGetResponse<AppointmentInfo>>();

            IGenericPagedGetResponse<AppointmentInfo> response = (IGenericPagedGetResponse<AppointmentInfo>)value;

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<AppointmentInfo>)}.{nameof(IGenericPagedGetResponse<BrowsableResource<AppointmentInfo>>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(firstPageUrlExpectation);
            response.Links.Previous.Should().Match(previousPageUrlExpectation);
            response.Links.Next.Should().Match(nextPageUrlExpectation);
            response.Links.Last.Should().Match(lastPageUrlExpectation);


        }


        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            //Arrange
            _handleGetOneAppointmentInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, AppointmentInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentInfo)null);

            //Act
            IActionResult actionResult = await _controller.Get(Guid.NewGuid(), default(CancellationToken));

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
            AppointmentInfo expectedAppointementInfo = new AppointmentInfo
            {
                Id = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                StartDate = 1.February(2015),
                Duration = 1.Hours().TotalSeconds
            };

            _urlHelperMock.Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}");


            _handleGetOneAppointmentInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, AppointmentInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedAppointementInfo)
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Get(expectedAppointementInfo.Id);

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<BrowsableResource<AppointmentInfo>>().Which
                    .Links.Should()
                        .NotBeNull();

            IBrowsableResource<AppointmentInfo> result = (IBrowsableResource<AppointmentInfo>)((OkObjectResult)actionResult).Value;
            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .Contain(x => x.Relation.Contains("self"));

            Link location = links.Single(x => x.Relation.Contains("self"));

            location.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{AppointmentsController.EndpointName}/{nameof(AppointmentsController.Get)}?{nameof(AppointmentInfo.Id)}={expectedAppointementInfo.Id}");
            location.Relation?.Should()
                .BeEquivalentTo("self");

            AppointmentInfo resource = result.Resource;
            resource.Should().NotBeNull();
            resource.Id.Should().Be(expectedAppointementInfo.Id);
            resource.DoctorId.Should().Be(expectedAppointementInfo.DoctorId);
            resource.PatientId.Should().Be(expectedAppointementInfo.PatientId);
            resource.StartDate.Should().Be(expectedAppointementInfo.StartDate);


            _handleGetOneAppointmentInfoByIdQueryMock.Verify();
            _urlHelperMock.Verify();

        }

        /// <summary>
        /// Unit test post
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Post()
        {
            //Arrange
            Guid appointmentId = Guid.NewGuid();
            _iRunCreateAppointmentInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreateAppointmentCommand>(), It.IsAny<CancellationToken>()))
                .Returns((ICreateAppointmentCommand cmd, CancellationToken cancellationToken) => Task.Run(()
                => new AppointmentInfo
                {
                    Id = appointmentId,
                    DoctorId = cmd.Data.DoctorId,
                    PatientId = cmd.Data.PatientId,
                    StartDate = cmd.Data.StartDate,
                    Duration = cmd.Data.Duration,
                    UpdatedDate = new DateTimeOffset(2012, 2, 1, 0, 0, 0, TimeSpan.Zero)
                }));

            //Act
            CreateAppointmentInfo info = new CreateAppointmentInfo
            {
                PatientId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                StartDate = 23.July(2015),
                Duration = 3.Hours().TotalSeconds
            };

            IActionResult actionResult = await _controller.Post(info, CancellationToken.None);

            //Assert
            CreatedAtActionResult createdActionResult = actionResult.Should()
                .NotBeNull().And
                .BeOfType<CreatedAtActionResult>().Which;

            createdActionResult.ActionName.Should().Be(nameof(AppointmentsController.Get));
            createdActionResult.ControllerName.Should().Be(AppointmentsController.EndpointName);
            createdActionResult.RouteValues.Should()
                .HaveCount(1).And
                .ContainKey("id");

            createdActionResult.RouteValues["id"].Should().Be(appointmentId);


            createdActionResult.Value.Should()
                .NotBeNull().And
                .BeAssignableTo<IBrowsableResource<AppointmentInfo>>();

            IBrowsableResource<AppointmentInfo> browsableResource = (IBrowsableResource<AppointmentInfo>)((CreatedAtActionResult)actionResult).Value;
            browsableResource.Links.Should()
                .NotContainNulls().And
                .Contain(x => x.Relation == nameof(Appointment.Doctor)).And
                .Contain(x => x.Relation == nameof(Appointment.Patient));


            AppointmentInfo createdResource = browsableResource.Resource;

            createdResource.Should()
                .NotBeNull();
            createdResource.Id.Should().NotBeEmpty();
            createdResource.DoctorId.Should()
                .Be(info.DoctorId);
            createdResource.PatientId.Should()
                .Be(info.PatientId);

            createdResource.UpdatedDate.Should().HaveDay(1);
            createdResource.UpdatedDate.Should().HaveMonth(2);
            createdResource.UpdatedDate.Should().HaveYear(2012);

            _iRunCreateAppointmentInfoCommandMock.Verify(mock => mock.RunAsync(It.IsAny<ICreateAppointmentCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public void PostShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new CommandNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrRequiredField", $"{nameof(CreateAppointmentInfo.StartDate)}", ErrorLevel.Error)
                });

            //Arrange

            _iRunCreateAppointmentInfoCommandMock.Setup(mock => mock.RunAsync(It.IsAny<ICreateAppointmentCommand>(), It.IsAny<CancellationToken>()))
                .Throws(exceptionFromTheHandler);

            //Act
            CreateAppointmentInfo info = new CreateAppointmentInfo
            {
                PatientId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                Duration = 3.Hours().TotalSeconds
            };


            Func<Task> action = async () => await _controller.Post(info, CancellationToken.None);

            //Assert
            action.ShouldThrow<CommandNotValidException<Guid>>().Which.Should()
                .Be(exceptionFromTheHandler);
            _iRunCreateAppointmentInfoCommandMock.Verify();

        }


        [Fact]
        public async Task Patch()
        {

            // Arrange
            _iRunPatchAppointmentCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IPatchCommand<Guid, Appointment>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Nothing.Value);


            // Act
            JsonPatchDocument<AppointmentInfo> patchDocument = new JsonPatchDocument<AppointmentInfo>();
            IActionResult actionResult = await _controller.Patch(Guid.NewGuid(), patchDocument, CancellationToken.None);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<NoContentResult>();

            _iRunPatchAppointmentCommandMock.Verify();
        }



        public static IEnumerable<object> SearchCases
        {
            get
            {
                {
                    SearchAppointmentInfo searchInfo = new SearchAppointmentInfo
                    {
                        To = 31.December(2010),
                        Page = 1,
                        PageSize = 30,
                        Sort = "-StartDate"
                    };
                    yield return new object[]
                    {
                        Enumerable.Empty<AppointmentInfo>(),
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation.Contains("first") &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.To)}={searchInfo.To.Value.ToString("s")}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.PageSize)}={searchInfo.PageSize}")  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.Sort)}={searchInfo.Sort}" )

                           )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };

                }
                {
                    SearchAppointmentInfo searchInfo = new SearchAppointmentInfo
                    {
                        From = 1.February(2015),
                        To = 1.February(2016),
                        Page = 1,
                        PageSize = 30,
                        Sort = "-StartDate"
                    };
                    yield return new object[]
                    {
                        new [] {
                            new AppointmentInfo { StartDate = 1.February(2014), Duration = 1.Hours().TotalSeconds }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation.Contains("first") &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 5 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.From)}={searchInfo.From.Value.ToString("s")}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.To)}={searchInfo.To.Value.ToString("s")}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.PageSize)}={searchInfo.PageSize}")  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.Sort)}={searchInfo.Sort}" )

                           )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };

                }
                {
                    SearchAppointmentInfo searchInfo = new SearchAppointmentInfo
                    {
                        From = 1.January(2016),
                        Page = 1,
                        PageSize = 30,
                    };
                    yield return new object[]
                    {
                        new[] {
                            new AppointmentInfo { StartDate = 1.January(2016) }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation.Contains("first") &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 3 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.From)}={searchInfo.From.Value.ToString("s")}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchAppointmentInfo.PageSize)}={searchInfo.PageSize}")

                            )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };
                }

            }
        }

        /// <summary>
        /// Unit tests for <see cref="AppointmentsController.Search(SearchAppointmentInfo, CancellationToken)"/>
        /// </summary>
        /// <param name="entries"><see cref="Appointment"/>s in the repository.</param>
        /// <param name="searchRequest">Search request.</param>
        /// <param name="firstPageLinkExpectation"></param>
        /// <param name="previousPageLinkExpectation"></param>
        /// <param name="nextPageLinkExpectation"></param>
        /// <param name="lastPageLinkExpectation"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<AppointmentInfo> entries, SearchAppointmentInfo searchRequest,
        Expression<Func<Link, bool>> firstPageLinkExpectation, Expression<Func<Link, bool>> previousPageLinkExpectation, Expression<Func<Link, bool>> nextPageLinkExpectation, Expression<Func<Link, bool>> lastPageLinkExpectation)
        {
            _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
            _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");


            // Arrange
            MedEasyApiOptions apiOptions = new MedEasyApiOptions { DefaulLimit = 30, MaxPageSize = 50 };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);
            _iHandleSearchQueryMock.Setup(mock => mock.Search<Appointment, AppointmentInfo>(It.IsAny<SearchQuery<AppointmentInfo>>(), It.IsAny<CancellationToken>()))
                    .Returns((SearchQuery<AppointmentInfo> query, CancellationToken cancellationToken) => Task.Run(() =>
                    {
                        SearchQueryInfo<AppointmentInfo> data = query.Data;
                        Expression<Func<AppointmentInfo, bool>> filter = data.Filter.ToExpression<AppointmentInfo>();
                        int page = query.Data.Page;
                        int pageSize = query.Data.PageSize;
                        Func<AppointmentInfo, bool> fnFilter = filter.Compile();

                        IEnumerable<AppointmentInfo> result = entries.Where(fnFilter)
                            .Skip(page * pageSize)
                            .Take(pageSize);

                        IPagedResult<AppointmentInfo> pageOfResult = new PagedResult<AppointmentInfo>(result, entries.Count(fnFilter), pageSize);
                        return pageOfResult;
                    })
                    );


            // Act
            IActionResult actionResult = await _controller.Search(searchRequest);

            // Assert
            IGenericPagedGetResponse<AppointmentInfo> content = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IGenericPagedGetResponse<AppointmentInfo>>().Which;


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
            _handleGetOneAppointmentInfoByIdQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantOneResource<Guid, Guid, AppointmentInfo>>(), It.IsAny<CancellationToken>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.Get(Guid.NewGuid());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _handleGetOneAppointmentInfoByIdQueryMock.Verify();

        }



        [Fact]
        public void GetAllShouldNotSwallowQueryNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MedEasyApiOptions { DefaulLimit = 20, MaxPageSize = 200 });

            _handlerGetManyAppointmentInfoQueryMock.Setup(mock => mock.HandleAsync(It.IsAny<IWantManyResources<Guid, AppointmentInfo>>(), It.IsAny<CancellationToken>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.GetAll(new PaginationConfiguration());

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _handlerGetManyAppointmentInfoQueryMock.Verify();

        }


        [Fact]
        public void DeleteShouldNotSwallowCommandNotValidExceptions()
        {
            Exception exceptionFromTheHandler = new QueryNotValidException<Guid>(Guid.NewGuid(), new[]
                {
                    new ErrorInfo ("ErrCode", "A description", ErrorLevel.Error)
                });

            //Arrange
            _iRunDeleteAppointmentInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsAny<IDeleteAppointmentByIdCommand>(), It.IsAny<CancellationToken>()))
                .Throws(exceptionFromTheHandler)
                .Verifiable();


            //Act
            Func<Task> action = async () => await _controller.Delete(Guid.NewGuid(), CancellationToken.None);

            //Assert
            action.ShouldThrow<QueryNotValidException<Guid>>().Which.Should().Be(exceptionFromTheHandler);
            _handlerGetManyAppointmentInfoQueryMock.Verify();
        }

        [Fact]
        public async Task DeleteMustRelyOnDeleteCommandHandler()
        {
            //Arrange
            _iRunDeleteAppointmentInfoByIdCommandMock.Setup(mock => mock.RunAsync(It.IsNotNull<IDeleteAppointmentByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Nothing.Value)
                .Verifiable();

            //Act
            await _controller.Delete(Guid.NewGuid());

            //Assert
            _iRunDeleteAppointmentInfoByIdCommandMock.Verify();
        }

        /// <summary>
        /// Tests <see cref="AppointmentsController.Post(CreateAppointmentInfo, CancellationToken)"/>
        /// </summary>
        /// <remarks>
        ///  The response should be <c>HTTP/1.1 409 Conflicted</c> when the new <see cref="Appointment"/> overlaps an existing one 
        ///  and <see cref="MedEasyApiOptions.AllowOverlaping"/> is <c>false</c>.
        /// </remarks>
        /// <returns></returns>
        public void Post_Appointment_That_Overlaps_An_Existing_Appointment_Returns_Conflicted_Result()
        {
            // Arrange
            Guid doctorId = Guid.NewGuid();

            CommandConflictException<Guid> exceptionThrown = new CommandConflictException<Guid>(Guid.NewGuid());
            _iRunCreateAppointmentInfoCommandMock.Setup(mock => mock.RunAsync(It.IsNotNull<ICreateAppointmentCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exceptionThrown);


            // Act
            CreateAppointmentInfo newAppointment = new CreateAppointmentInfo
            {
                DoctorId = doctorId,
                StartDate = 2.February(2017).AddHours(14).AddMinutes(30),
                Duration = 30
            };


            Func<Task> action = async () => await _controller.Post(newAppointment);

            // Assert
            action.ShouldThrowExactly<CommandConflictException<Guid>>().Which.Should()
                .BeSameAs(exceptionThrown);

        }
    }
}

