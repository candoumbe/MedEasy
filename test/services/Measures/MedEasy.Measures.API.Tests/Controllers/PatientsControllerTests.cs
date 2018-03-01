using AutoMapper.QueryableExtensions;
using FluentAssertions;
using FluentAssertions.Extensions;
using GenFu;
using Measures.API.Controllers;
using Measures.API.Routing;
using Measures.Context;
using Measures.CQRS.Commands.Patients;
using Measures.CQRS.Queries.BloodPressures;
using Measures.CQRS.Queries.Patients;
using Measures.DTO;
using Measures.Mapping;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.RestObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Optional;
using System;
using System.Collections;
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

namespace Measures.API.Tests
{
    [Feature("Patients")]
    public class PatientsControllerTests : IDisposable
    {
        private Mock<IUrlHelper> _urlHelperMock;
        private PatientsController _controller;
        private ITestOutputHelper _outputHelper;
        private Mock<IOptionsSnapshot<MeasuresApiOptions>> _apiOptionsMock;
        private Mock<IMediator> _mediatorMock;
        private const string _baseUrl = "http://host/api";
        private IUnitOfWorkFactory _uowFactory;

        public PatientsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routename, object routeValues) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            DbContextOptionsBuilder<MeasuresContext> dbOptions = new DbContextOptionsBuilder<MeasuresContext>();
            string dbName = $"InMemoryMedEasyDb_{Guid.NewGuid()}";
            dbOptions.UseInMemoryDatabase(dbName);
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbOptions.Options, (options) => new MeasuresContext(options));

            _apiOptionsMock = new Mock<IOptionsSnapshot<MeasuresApiOptions>>(Strict);

            _mediatorMock = new Mock<IMediator>(Strict);

            _controller = new PatientsController(
                _urlHelperMock.Object,
                _apiOptionsMock.Object,
                _mediatorMock.Object);

        }

        public void Dispose()
        {
            _urlHelperMock = null;
            _controller = null;
            _outputHelper = null;
            _apiOptionsMock = null;
            _mediatorMock = null;
            _uowFactory = null;
        }


        public static IEnumerable<object> GetLastBloodPressuresMesuresCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<BloodPressure>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new BloodPressure { PatientId = 2, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },

                    ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => !x.Any()))
                };

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new []
                        {
                            new BloodPressure { PatientId = 1, CreatedDate = DateTimeOffset.UtcNow, Patient = new Patient { UUID = patientId } }
                        },
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        ((Expression<Func<IEnumerable<BloodPressureInfo>, bool>>) (x => x.All(measure => measure.PatientId == patientId) && x.Count() == 1))
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetMostRecentTemperaturesMeasuresCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Temperature>(),
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any()))
                };

                yield return new object[]
                {
                    new []
                    {
                        new Temperature { PatientId = 2, CreatedDate = DateTimeOffset.UtcNow }
                    },
                    new GetMostRecentPhysiologicalMeasuresInfo { PatientId = Guid.NewGuid(), Count = 10 },
                    ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => !x.Any()))
                };
                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        new []
                        {
                            new Temperature { PatientId = 1, CreatedDate = DateTimeOffset.UtcNow, Patient = new Patient { UUID = patientId } }
                        },
                        new GetMostRecentPhysiologicalMeasuresInfo { PatientId = patientId, Count = 10 },
                        ((Expression<Func<IEnumerable<TemperatureInfo>, bool>>) (x => x.All(measure => measure.PatientId == patientId) && x.Count() == 1))
                    };
                }
            }
        }

        
        public static IEnumerable<object[]> GetAllTestCases
        {
            get
            {
                int[] pageSizes = { 1, 10, 20};
                int[] pages = { 1, 5, 10 };


                foreach (int pageSize in pageSizes)
                {
                    foreach (int page in pages)
                    {
                        yield return new object[]
                        {
                            Enumerable.Empty<Patient>(), // Current store state
                            (pageSize, page), // request,
                            (defaultPageSize : 30, maxPageSize : 200),
                            0,    //expected total
                            (
                                first : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase)), // expected link to first page
                                previous : (Expression<Func<Link, bool>>) (x => x == null), // expected link to previous page
                                next :(Expression<Func<Link, bool>>) (x => x == null), // expected link to next page
                                last : (Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last &&
                                    ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}").Equals(x.Href, OrdinalIgnoreCase))
                            )  // expected link to last page
                        };
                    }
                }

                {
                    IEnumerable<Patient> items = A.ListOf<Patient>(400);
                    items.ForEach(item => item.Id = default);
                    yield return new object[]
                    {
                        items,
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)))
                        )  // expected link to last page
                    };
                }
                {
                    IEnumerable<Patient> items = A.ListOf<Patient>(400);
                    items.ForEach(item => item.Id = default);

                    yield return new object[]
                    {
                        items,
                        (pageSize : 10, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        400,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                        )
                    };
                }

                yield return new object[]
                    {
                        new [] {
                            new Patient { Id = 1, Firstname = "Bruce",  Lastname = "Wayne" }
                        },
                        (pageSize : PaginationConfiguration.DefaultPageSize, page : 1), // request
                        (defaultPageSize : 30, maxPageSize : 200),
                        1,    //expected total
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize={PaginationConfiguration.DefaultPageSize}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous :((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?Controller={PatientsController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)))
                        ), // expected link to last page
                    };
            }
        }
        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<Patient> items, (int pageSize, int page) request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(PatientsController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {request.pageSize}");
            _outputHelper.WriteLine($"Page : {request.page}");
            _outputHelper.WriteLine($"specialties store count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(items);
                await uow.SaveChangesAsync();
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfPatientInfoQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetPageOfPatientInfoQuery query, CancellationToken cancellationToken) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();
                        Page<PatientInfo> result = await uow.Repository<Patient>()
                            .ReadPageAsync(
                                selector,
                                query.Data.PageSize,
                                query.Data.Page,
                                new[] { OrderClause<PatientInfo>.Create(x => x.UpdatedDate) },
                                cancellationToken)
                            .ConfigureAwait(false);

                        return result;
                    }
                });
            
            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { Page = request.page, PageSize = request.pageSize });

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(PatientsController)}.{nameof(PatientsController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MeasuresApiOptions.MaxPageSize)} value");
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfPatientInfoQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<IGenericPagedGetResponse<BrowsableResource<PatientInfo>>>();

            IGenericPagedGetResponse<BrowsableResource<PatientInfo>> response = (IGenericPagedGetResponse<BrowsableResource<PatientInfo>>)value;

            _outputHelper.WriteLine($"response : {response}");

            response.Items.Should()
                .NotBeNull();

            if (response.Items.Any())
            {
                response.Items.Should()
                    .NotContainNulls().And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self));
            }

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<PatientInfo>)}.{nameof(IGenericPagedGetResponse<PatientInfo>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);

        }


        public static IEnumerable<object[ ] > SearchCases
        {
            get
            {
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };

                    yield return new object[]
                    {
                        Enumerable.Empty<Patient>(),
                        searchInfo,
                        ((
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&firstname={searchInfo.Firstname}"+
                                $"&page=1" +
                                $"&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))))
                        )

                    };

                }
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Lastname = "!wayne",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };
                    yield return new object[]
                    {
                        new [] {
                            new Patient { Firstname = "Bruce", Lastname = "Wayne" }
                        },
                        searchInfo,
                        (
                           ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"Controller={PatientsController.EndpointName}" +
                                $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                $"&page=1&pageSize=30" +
                                $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase))),
                            ((Expression<Func<Link, bool>>)(previous => previous == null)),
                            ((Expression<Func<Link, bool>>)(next => next == null)),
                            ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&lastname={Uri.EscapeDataString(searchInfo.Lastname)}"+
                                    $"&page=1&pageSize=30" +
                                    $"&sort={searchInfo.Sort}").Equals(x.Href, OrdinalIgnoreCase)))
                        )
                    };

                }
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                    };
                    yield return new object[]
                    {
                        new[] {
                            new Patient{ Firstname = "bruce" }
                        },
                        searchInfo,
                        (
                            ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                    $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            ((Expression<Func<Link, bool>>)(previous => previous == null)),
                            ((Expression<Func<Link, bool>>)(next => next == null)),
                            ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                    $"Controller={PatientsController.EndpointName}" +
                                    $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                    $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase)))
                        )

                    };
                }

                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Firstname = "bruce",
                        Page = 1,
                        PageSize = 30,
                        BirthDate = 31.July(2010)
                    };
                    yield return new object[]
                    {
                        new[] {
                            new Patient { Firstname = "bruce", BirthDate = 31.July(2010) }
                        },
                        searchInfo,
                        ( ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.First
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value.ToString("s")}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>) (x => x != null
                            && x.Relation == LinkRelation.Last
                            && ($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?" +
                                $"birthdate={searchInfo.BirthDate.Value.ToString("s")}" +
                                $"&Controller={PatientsController.EndpointName}" +
                                $"&firstname={Uri.EscapeDataString(searchInfo.Firstname)}"+
                                $"&page=1&pageSize=30").Equals(x.Href, OrdinalIgnoreCase))))

                    };
                }
            }
        }


        [Theory]
        [MemberData(nameof(SearchCases))]
        public async Task Search(IEnumerable<Patient> entries, SearchPatientInfo searchRequest,
        (Expression<Func<Link, bool>> firstPageLink, Expression<Func<Link, bool>> previousPageLink, Expression<Func<Link, bool>> nextPageLink, Expression<Func<Link, bool>> lastPageLink) linksExpectation)
        {
            _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
            _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");


            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(entries);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()))
                .Returns(async (SearchQuery<PatientInfo> query, CancellationToken cancellationToken) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression<Patient, PatientInfo>();

                        Expression<Func<Patient, bool>> filter = query.Data.Filter?.ToExpression<Patient>() ?? (x => true);
                            
                        Page<PatientInfo> resources = await uow.Repository<Patient>()
                            .WhereAsync(
                                selector,
                                filter,
                                query.Data.Sorts.Select(sort => OrderClause<PatientInfo>.Create(sort.Expression, sort.Direction == MedEasy.Data.SortDirection.Ascending
                                    ? MedEasy.DAL.Repositories.SortDirection.Ascending
                                    : MedEasy.DAL.Repositories.SortDirection.Descending)),
                                query.Data.PageSize,
                                query.Data.Page,
                                cancellationToken)
                            .ConfigureAwait(false);

                        return resources;
                    }
                });

            // Act
            IActionResult actionResult = await _controller.Search(searchRequest, default)
                    .ConfigureAwait(false);

            // Assert
            GenericPagedGetResponse<BrowsableResource<PatientInfo>> content = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<GenericPagedGetResponse<BrowsableResource<PatientInfo>>>().Which;


            content.Items.Should()
                .NotBeNull($"{nameof(GenericPagedGetResponse<object>.Items)} must not be null").And
                .NotContainNulls($"{nameof(GenericPagedGetResponse<object>.Items)} must not contains null").And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            content.Links.Should()
                .NotBeNull();
            PagedRestResponseLink links = content.Links;

            links.First.Should().Match(linksExpectation.firstPageLink);
            links.Previous.Should().Match(linksExpectation.previousPageLink);
            links.Next.Should().Match(linksExpectation.nextPageLink);
            links.Last.Should().Match(linksExpectation.lastPageLink);
        }
        

        [Fact]
        public async Task GivenMediatorReturnsEmptyPage_Search_Returns_NotFound_When_Requesting_PageTwoOfResult()
        {
            // Arrange
            SearchPatientInfo searchRequest = new SearchPatientInfo
            {
                Page = 2,
                PageSize = 10,
                Lastname = "*e*"
            };
            
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Page<PatientInfo>.Default);

            // Act
            IActionResult actionResult = await _controller.Search(searchRequest, default)
                    .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .NotBeNull().And
                .BeAssignableTo<NotFoundResult>();

        }


        public static IEnumerable<object> PatchCases
        {
            get
            {
                {
                    JsonPatchDocument<PatientInfo> patchDocument = new JsonPatchDocument<PatientInfo>();
                    patchDocument.Replace(x => x.Firstname, "Bruce");
                    yield return new object[]
                    {
                        new Patient { Id = 1, },
                        patchDocument,
                        ((Expression<Func<Patient, bool>>)(x => x.Id == 1 && x.Firstname == "Bruce"))
                    };
                }
            }
        }
        
        [Fact]
        public async Task GetWithUnknownIdShouldReturnNotFound()
        {
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetPatientInfoByIdQuery query, CancellationToken ct) =>
                {
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                            .GetMapExpression<Patient, PatientInfo>();

                        return await uow.Repository<Patient>().SingleOrDefaultAsync(selector, x => x.UUID == query.Data, ct)
                            .ConfigureAwait(false);
                    }
                });


            //Act
            IActionResult actionResult = await _controller.Get(Guid.NewGuid());

            //Assert
            actionResult.Should()
                .NotBeNull().And
                .BeOfType<NotFoundResult>().Which
                    .StatusCode.Should().Be(404);

            _mediatorMock.Verify();

        }

        [Fact]
        public async Task Get()
        {
            //Arrange
            Guid patientId = Guid.NewGuid();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(new Patient
                {
                    UUID = patientId,
                    Firstname = "Bruce",
                    Lastname = "Wayne"
                });
                await uow.SaveChangesAsync();
            }
            PatientInfo expectedResource = new PatientInfo
            {
                Id = patientId,
                Firstname = "Bruce",
                Lastname = "Wayne"
            };

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPatientInfoByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetPatientInfoByIdQuery query, CancellationToken ct) =>
               {
                   using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                   {
                       Expression<Func<Patient, PatientInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                           .GetMapExpression<Patient, PatientInfo>();

                       return await uow.Repository<Patient>().SingleOrDefaultAsync(selector, x => x.UUID == query.Data, ct)
                           .ConfigureAwait(false);
                   }
               });


            //Act
            IActionResult actionResult = await _controller.Get(patientId);

            //Assert
            
            BrowsableResource<PatientInfo> result = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<BrowsableResource<PatientInfo>>().Which;

            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href), $"{nameof(BrowsableResource<PatientInfo>)}{nameof(BrowsableResource<PatientInfo>.Links)} cannot contain any element " +
                    $"with null/empty/whitespace {nameof(Link.Href)}s" ).And
                .ContainSingle(x => x.Relation == LinkRelation.Self).And
                .ContainSingle(x => x.Relation == "delete").And
                .ContainSingle(x => x.Relation == BloodPressuresController.EndpointName.ToLowerKebabCase());

            Link self = links.Single(x => x.Relation == LinkRelation.Self);
            self.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={patientId}");
            self.Relation.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo(LinkRelation.Self);
            self.Method.Should()
                .Be("GET");


            Link linkDelete = links.Single(x => x.Relation == "delete");
            linkDelete.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?Controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={expectedResource.Id}");
            linkDelete.Method.Should().Be("DELETE");


            Link bloodPressuresLink = links.Single(x => x.Relation == BloodPressuresController.EndpointName.ToLowerKebabCase());
            bloodPressuresLink.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?Controller={BloodPressuresController.EndpointName}&{nameof(BloodPressureInfo.PatientId)}={expectedResource.Id}");
            bloodPressuresLink.Method.Should().Be("GET");
            

            PatientInfo actualResource = result.Resource;
            actualResource.Should().NotBeNull();
            actualResource.Id.Should().Be(expectedResource.Id);
            actualResource.Firstname.Should().Be(expectedResource.Firstname);
            actualResource.Lastname.Should().Be(expectedResource.Lastname);
            
            _urlHelperMock.Verify();
            _mediatorMock.Verify();

        }

        [Fact]
        public async Task WhenMediatorReturnsNotFound_Delete_Returns_NotFound()
        {
            // Arrange
            Guid idToDelete = Guid.NewGuid();

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Failed_NotFound);

            // Act
            IActionResult actionResult = await _controller.Delete(id : idToDelete, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeletePatientInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();

        }

        [Fact]
        public async Task WhenMediatorReturnsSuccess_Delete_Returns_NoContent()
        {
            // Arrange
            Guid idToDelete = Guid.NewGuid();

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteCommandResult.Done);

            // Act
            IActionResult actionResult = await _controller.Delete(id: idToDelete, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<DeletePatientInfoByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<DeletePatientInfoByIdCommand>(cmd => cmd.Data == idToDelete), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NoContentResult>();

        }

        [Fact]
        public async Task GivenMediatorReturnsNone_GetBloodPressures_ReturnsNotFound()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            PaginationConfiguration pagination = new PaginationConfiguration { Page = 1, PageSize = 50 };

            _apiOptionsMock.Setup(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = 10, MaxPageSize = 100 });
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoByPatientIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.None<Page<BloodPressureInfo>>());

            // Act
            IActionResult actionResult = await _controller.GetBloodPressures(id: patientId, pagination : pagination, ct: default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoByPatientIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfBloodPressureInfoByPatientIdQuery>(query => query.Data.patientId == patientId && query.Data.pagination == pagination), It.IsAny<CancellationToken>()), Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();

        }

        public static IEnumerable<object[]> GetBloodPressuresWhenPatientExistsCases
        {
            get
            {
                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        patientId,
                        Enumerable.Empty<BloodPressure>(),
                        (page :1, pageSize:10),
                        (defaultPageSize : 30, maxPageSize : 200),
                        0,
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize=10").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous :((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize=10").Equals(x.Href, OrdinalIgnoreCase)))
                        )

                    };
                }

                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        patientId,
                        new[]
                        {
                            new BloodPressure { SystolicPressure = 120, DiastolicPressure = 80 }
                        },
                        (page :1, pageSize:10),
                        (defaultPageSize : 30, maxPageSize : 200),
                        1,
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize=10").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous :((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize=10").Equals(x.Href, OrdinalIgnoreCase)))
                        )

                    };
                }
                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        patientId,
                        new[]
                        {
                            new BloodPressure { SystolicPressure = 120, DiastolicPressure = 80 },
                            new BloodPressure { SystolicPressure = 140, DiastolicPressure = 80 },
                            new BloodPressure { SystolicPressure = 170, DiastolicPressure = 100 }
                        },
                        (page :1, pageSize:1),
                        (defaultPageSize : 30, maxPageSize : 200),
                        3,
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize=1").Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previous :((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            next : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Next
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=2" +
                                    $"&pageSize=1").Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            last : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=3" +
                                    $"&pageSize=1").Equals(x.Href, OrdinalIgnoreCase)))
                        )

                    };
                }
                {
                    Guid patientId = Guid.NewGuid();
                    yield return new object[]
                    {
                        patientId,
                        new[]
                        {
                            new BloodPressure { SystolicPressure = 120, DiastolicPressure = 80 },
                            new BloodPressure { SystolicPressure = 140, DiastolicPressure = 80 },
                            new BloodPressure { SystolicPressure = 170, DiastolicPressure = 100 }
                        },
                        (page :2, pageSize:1),
                        (defaultPageSize : 30, maxPageSize : 200),
                        3,
                        (
                            first : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize=1").Equals(x.Href, OrdinalIgnoreCase))), 
                            previous : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Previous
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=1" +
                                    $"&pageSize=1").Equals(x.Href, OrdinalIgnoreCase))), 
                            next :((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Next
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=3" +
                                    $"&pageSize=1").Equals(x.Href, OrdinalIgnoreCase))), 
                            last : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.Last
                                && ($"{_baseUrl}/{RouteNames.DefaultGetAllSubResourcesByResourceIdApi}/?" +
                                    $"Action=GetBloodPressures&Controller={PatientsController.EndpointName}" +
                                    $"&page=3" +
                                    $"&pageSize=1").Equals(x.Href, OrdinalIgnoreCase)))
                        )

                    };
                }

            }
        }

        [Theory]
        [MemberData(nameof(GetBloodPressuresWhenPatientExistsCases))]
        public async Task GivenMediatorReturnsSome_GetBloodPressures_ReturnsOkObjectResult(Guid patientId, IEnumerable<BloodPressure> resources, (int page, int pageSize) request,
            (int defaultPageSize, int maxPageSize) pagingOptions,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) linksExpectation)
        {
            // Arrange
            PaginationConfiguration pagination = new PaginationConfiguration { Page = request.page, PageSize = request.pageSize };
            _outputHelper.WriteLine($"pagination : {pagination}");
            _outputHelper.WriteLine($"resources : {resources}");

            
            Patient patient = new Patient { UUID = patientId };
            resources.ForEach(x => x.Patient = patient);
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Patient>().Create(patient);
                uow.Repository<BloodPressure>().Create(resources);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }


            _apiOptionsMock.Setup(mock => mock.Value)
                .Returns(new MeasuresApiOptions { DefaultPageSize = pagingOptions.defaultPageSize, MaxPageSize = pagingOptions.maxPageSize });

            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoByPatientIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(async (GetPageOfBloodPressureInfoByPatientIdQuery query, CancellationToken ct) =>
                {
                    var (id, paging) = query.Data;
                    using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
                    {
                        Option<Page<BloodPressureInfo>> result;
                        if (await uow.Repository<Patient>().AnyAsync(x => x.UUID == id).ConfigureAwait(false))
                        {
                            Expression<Func<BloodPressure, BloodPressureInfo>> selector = AutoMapperConfig.Build().ExpressionBuilder
                                .GetMapExpression<BloodPressure, BloodPressureInfo>();
                            result = Option.Some(await uow.Repository<BloodPressure>()
                                        .WhereAsync(
                                            selector,
                                            (BloodPressureInfo x) => x.PatientId == id,
                                            new[] { OrderClause<BloodPressureInfo>.Create(x => x.UpdatedDate) },
                                            paging.PageSize,
                                            paging.Page,
                                            ct)
                                        .ConfigureAwait(false));
                        }
                        else
                        {
                            result = Option.None<Page<BloodPressureInfo>>();
                        }

                        return result;

                    }
                });


            // Act
            IActionResult actionResult = await _controller.GetBloodPressures(id: patientId, pagination: pagination, ct: default)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoByPatientIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfBloodPressureInfoByPatientIdQuery>(query => query.Data.patientId == patientId && query.Data.pagination == pagination), It.IsAny<CancellationToken>()), Times.Once);

            GenericPagedGetResponse<BrowsableResource<BloodPressureInfo>> page = actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                   .NotBeNull().And
                   .BeAssignableTo<GenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>>().Which;


            page.Count.Should()
                .Be(expectedCount);

            page.Items.Should()
                .NotBeNull();

            if (page.Items.Any())
            {
                page.Items.Should()
                    .NotContainNulls().And
                    .NotContain(x => x.Resource == null, $"{nameof(BrowsableResource<object>.Resource)} must be provided").And
                    .NotContain(x => x.Links == null, $"{nameof(BrowsableResource<object>.Links)} must not be null").And
                    .NotContain(x => !x.Links.Any(), $"at least one link must be provided").And
                    .OnlyContain(x => x.Links.Once()).And
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self), $"self link must be provided");
            }

            PagedRestResponseLink links = page.Links;
            links.First.Should().Match(linksExpectation.firstPageUrlExpectation);
            links.Previous.Should().Match(linksExpectation.previousPageUrlExpectation);
            links.Next.Should().Match(linksExpectation.nextPageUrlExpectation);
            links.Last.Should().Match(linksExpectation.lastPageUrlExpectation);

        }


        [Fact]    
        public async Task GivenPageIndexOutOfPageCountBound_GetBloodPressures_Returns_NotFound()
        {

            // Arrange
            Guid patientId = Guid.NewGuid();

            _apiOptionsMock.Setup(mock => mock.Value)
                .Returns(new MeasuresApiOptions { DefaultPageSize = 10, MaxPageSize = 200 });
            _mediatorMock.Setup(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoByPatientIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Option.Some(new Page<BloodPressureInfo>(new[] { new BloodPressureInfo { SystolicPressure = 120, DiastolicPressure = 80 } }, 1, 10)));

            // Act
            IActionResult actionResult = await _controller.GetBloodPressures(patientId, new PaginationConfiguration { Page = 2, PageSize = 10 }, default)
                .ConfigureAwait(false);

            // Assert
            _mediatorMock.Verify(mock => mock.Send(It.IsAny<GetPageOfBloodPressureInfoByPatientIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(mock => mock.Send(It.Is<GetPageOfBloodPressureInfoByPatientIdQuery>(q => q.Data.patientId == patientId), It.IsAny<CancellationToken>()), Times.Once);
            _apiOptionsMock.Verify(mock => mock.Value, Times.Once);

            actionResult.Should()
                .BeAssignableTo<NotFoundResult>("Requested page index is out of page count bound");

        }
    }
}