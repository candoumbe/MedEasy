using FluentAssertions;
using GenFu;
using Measures.API.Controllers;
using Measures.API.Routing;
using Measures.Context;
using Measures.DTO;
using Measures.Mapping;
using Measures.Objects;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
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
using Xunit.Categories;
using FluentAssertions.Extensions;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using static System.StringComparison;

namespace Measures.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="BloodPressuresController"/>
    /// </summary>
    [UnitTest]
    [Feature("Blood pressures")]
    [Feature("Measures")]
    public class BloodPressureControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        private IUnitOfWorkFactory _unitOfWorkFactory;
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<ILogger<BloodPressuresController>> _loggerMock;
        private Mock<IOptionsSnapshot<MeasuresApiOptions>> _apiOptionsMock;
        private BloodPressuresController _controller;
        private const string _baseUrl = "http://host/api";


        public BloodPressureControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;


            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((string routename, object routeValues) => $"{_baseUrl}/{routename}/?{routeValues?.ToQueryString()}");

            _loggerMock = new Mock<ILogger<BloodPressuresController>>(Strict);
            _apiOptionsMock = new Mock<IOptionsSnapshot<MeasuresApiOptions>>(Strict);

            DbContextOptionsBuilder<MeasuresContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MeasuresContext>();
            string dbName = $"InMemoryDb_{Guid.NewGuid()}";
            dbContextOptionsBuilder.UseInMemoryDatabase(dbName);
            
            _unitOfWorkFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbContextOptionsBuilder.Options, (options) => new MeasuresContext(options));


            _controller = new BloodPressuresController(_loggerMock.Object, _urlHelperMock.Object, _apiOptionsMock.Object, AutoMapperConfig.Build().ExpressionBuilder, _unitOfWorkFactory);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _urlHelperMock = null;
            _loggerMock = null;
            _apiOptionsMock = null;


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
                            Enumerable.Empty<BloodPressure>(), // Current store state
                            pageSize, page, // request
                            0,    //expected total
                            (
                                firstPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First  && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                                previousPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                                nextPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                                lastPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={(pageSize < 1 ? 1 : Math.Min(pageSize, 200))}".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                            )
                        };
                    }
                }

                {
                    IEnumerable<BloodPressure> items = A.ListOf<BloodPressure>(400);
                    items.ForEach(item => item.Id = default);
                    yield return new object[]
                    {
                        items,
                        PaginationConfiguration.DefaultPageSize, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null
                                && x.Relation == LinkRelation.First
                                && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previousPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            nextPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=2&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            lastPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=14&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                        )
                    };
                }
                {
                    IEnumerable<BloodPressure> items = A.ListOf<BloodPressure>(400);
                    items.ForEach(item => item.Id = default);

                    yield return new object[]
                    {
                        items,
                        10, 1, // request
                        400,    //expected total
                        (
                            firstPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.First  && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previousPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            nextPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == "next" && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=2&pageSize=10".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                            lastPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=40&pageSize=10".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                        )
                    };
                }

                yield return new object[]
                {
                    new [] {
                        new BloodPressure { Id = 1, SystolicPressure = 120, DiastolicPressure = 80 }
                    },
                    PaginationConfiguration.DefaultPageSize, 1, // request
                    1,    //expected total
                    (
                        firstPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains(LinkRelation.First) && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                        previousPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                        nextPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                        lastPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains(LinkRelation.Last) && $"{_baseUrl}/{RouteNames.DefaultGetAllApi}/?controller={BloodPressuresController.EndpointName}&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))) // expected link to last page
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetAllTestCases))]
        public async Task GetAll(IEnumerable<BloodPressure> items, int pageSize, int page,
            int expectedCount,
            (Expression<Func<Link, bool>> firstPageUrlExpectation, Expression<Func<Link, bool>> previousPageUrlExpectation, Expression<Func<Link, bool>> nextPageUrlExpectation, Expression<Func<Link, bool>> lastPageUrlExpectation) pageLinksExpectation)
        {
            _outputHelper.WriteLine($"Testing {nameof(BloodPressuresController.Get)}({nameof(PaginationConfiguration)})");
            _outputHelper.WriteLine($"Page size : {pageSize}");
            _outputHelper.WriteLine($"Page : {page}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(items);
                await uow.SaveChangesAsync();
            }


            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = 30, MaxPageSize = 200 });
            // Act
            IActionResult actionResult = await _controller.Get(new PaginationConfiguration { PageSize = pageSize, Page = page });

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.Once, $"because {nameof(BloodPressuresController)}.{nameof(BloodPressuresController.Get)} must always check that {nameof(PaginationConfiguration.PageSize)} don't exceed {nameof(MeasuresApiOptions.MaxPageSize)} value");

            actionResult.Should()
                    .NotBeNull()
                    .And.BeOfType<OkObjectResult>();
            ObjectResult okObjectResult = (OkObjectResult)actionResult;

            object value = okObjectResult.Value;

            IGenericPagedGetResponse<BrowsableResource<BloodPressureInfo>> response = okObjectResult.Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<IGenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>>().Which;

            response.Items.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => x.Resource == null).And
                .NotContain(x => x.Links == null);

            response.Count.Should()
                    .Be(expectedCount, $@"because the ""{nameof(IGenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>)}.{nameof(IGenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageLinksExpectation.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageLinksExpectation.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageLinksExpectation.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageLinksExpectation.lastPageUrlExpectation);
        }

        public static IEnumerable<object[]> SearchTestCases
        {
            get
            {
                {
                    IEnumerable<BloodPressure> items = A.ListOf<BloodPressure>(400);
                    items.ForEach(async (x) => await Task.Factory.StartNew(() =>
                    {
                        x.DateOfMeasure = 26.January(2001);
                        x.Id = default;
                    }));


                    yield return new object[]
                    {
                        items,
                        new SearchBloodPressureInfo
                        {
                            From = 1.January(2001),
                            To = 31.January(2001),
                            Page = 1, PageSize = 30
                        }, 
                        (maxPageSize : 200, defaultPageSize : 30),
                        (
                            count : 400,
                            items :
                            ((Expression<Func<IEnumerable<BrowsableResource<BloodPressureInfo>>, bool>>)(resources => 
                                resources.All(x =>1.January(2001) <= x.Resource.DateOfMeasure && x.Resource.DateOfMeasure <= 31.January(2001) ))
                            ),
                            links :
                            (
                                firstPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null
                                    && x.Relation == LinkRelation.First
                                    && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00&page=1&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                                previousPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                                nextPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Next && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00&page=2&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00".Equals(x.Href, OrdinalIgnoreCase))), // expected link to next page
                                lastPageUrlExpecation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation == LinkRelation.Last && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2001-01-01T00:00:00&page=14&pageSize={PaginationConfiguration.DefaultPageSize}&to=2001-01-31T00:00:00".Equals(x.Href, OrdinalIgnoreCase)))  // expected link to last page
                            )
                        )
                    };
                }

                yield return new object[]
                {
                    new [] {
                        new BloodPressure
                        {
                            Id = 1,
                            UUID = Guid.NewGuid(),
                            SystolicPressure = 120,
                            DiastolicPressure = 80,
                            DateOfMeasure = 23.June(2012)
                                .Add(new TimeSpan(hours : 10, minutes : 30, seconds : 0))
                        }
                    },
                    new SearchBloodPressureInfo { From = 23.June(2012), Page = 1, PageSize = 30 }, // request
                    (maxPageSize : 200, pageSize : 30),
                    (
                        count : 1,    
                        items :
                          ((Expression<Func<IEnumerable<BrowsableResource<BloodPressureInfo>>, bool>>)(resources =>
                            resources.All(x => 23.June(2012) <= x.Resource.DateOfMeasure ))),
                        links : (
                            firstPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains(LinkRelation.First) && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2012-06-23T00:00:00&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))), // expected link to first page
                            previousPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to previous page
                            nextPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x == null)), // expected link to next page
                            lastPageUrlExpectation : ((Expression<Func<Link, bool>>) (x => x != null && x.Relation.Contains(LinkRelation.Last) && $"{_baseUrl}/{RouteNames.DefaultSearchResourcesApi}/?controller={BloodPressuresController.EndpointName}&from=2012-06-23T00:00:00&page=1&pageSize={PaginationConfiguration.DefaultPageSize}".Equals(x.Href, OrdinalIgnoreCase))) // expected link to last page
                        )
                    )
                };
            }
        }

        [Theory]
        [MemberData(nameof(SearchTestCases))]
        [Feature("Search")]
        [Trait("Resource", "BloodPressures")]
        public async Task Search(IEnumerable<BloodPressure> items, SearchBloodPressureInfo searchQuery,
            (int maxPageSize, int defaultPageSize) apiOptions,
            (
                int count, 
                Expression<Func<IEnumerable<BrowsableResource<BloodPressureInfo>>, bool>> items,
                (
                    Expression<Func<Link, bool>> firstPageUrlExpectation,
                    Expression<Func<Link, bool>> previousPageUrlExpectation, 
                    Expression<Func<Link, bool>> nextPageUrlExpectation, 
                    Expression<Func<Link, bool>> lastPageUrlExpectation
                ) links
            ) pageExpectation)
        {
             _outputHelper.WriteLine($"Testing {nameof(BloodPressuresController.Search)}({nameof(SearchBloodPressureInfo)})");
            _outputHelper.WriteLine($"Search : {SerializeObject(searchQuery)}");
            _outputHelper.WriteLine($"store items count: {items.Count()}");

            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(items);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }


            _apiOptionsMock.SetupGet(mock => mock.Value).Returns(new MeasuresApiOptions { DefaultPageSize = apiOptions.defaultPageSize, MaxPageSize = apiOptions.maxPageSize });
            
            // Act
            IActionResult actionResult = await _controller.Search(searchQuery)
                .ConfigureAwait(false);

            // Assert
            _apiOptionsMock.VerifyGet(mock => mock.Value, Times.AtLeastOnce, $"because {nameof(BloodPressuresController)}.{nameof(BloodPressuresController.Search)} must always check that {nameof(SearchBloodPressureInfo.PageSize)} don't exceed {nameof(MeasuresApiOptions.MaxPageSize)} value");

            GenericPagedGetResponse<BrowsableResource<BloodPressureInfo>> response = actionResult.Should()
                    .NotBeNull().And
                    .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .NotBeNull().And
                    .BeAssignableTo<GenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>>().Which;

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
                    .OnlyContain(x => x.Links.Once(link => link.Relation == LinkRelation.Self));
            }

            response.Count.Should()
                    .Be(pageExpectation.count, $@"the ""{nameof(IGenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>)}.{nameof(IGenericPagedGetResponse<BrowsableResource<BloodPressureInfo>>.Count)}"" property indicates the number of elements");

            response.Links.First.Should().Match(pageExpectation.links.firstPageUrlExpectation);
            response.Links.Previous.Should().Match(pageExpectation.links.previousPageUrlExpectation);
            response.Links.Next.Should().Match(pageExpectation.links.nextPageUrlExpectation);
            response.Links.Last.Should().Match(pageExpectation.links.lastPageUrlExpectation);
        }


        public static IEnumerable<object[]> OutOfBoundSearchCases
        {
            get
            {
                yield return new object[]
                {
                    new SearchBloodPressureInfo { Page = 2, PageSize = 30, From = 31.July(2013) },
                    (maxPageSize : 30, defaultPageSize : 30),
                    Enumerable.Empty<BloodPressure>(),
                    "page index is not 1 and there's no result for the search query"
                };

                {
                    yield return new object[]
                    {
                        new SearchBloodPressureInfo { Page = 2, PageSize = 30 },
                        (maxPageSize : 30, defaultPageSize : 30),
                        new [] {
                            new BloodPressure
                            {
                                UUID = Guid.NewGuid(),
                                DiastolicPressure = 80,
                                SystolicPressure = 120
                            }
                        },
                        "page index is not 1 and there's no result for the search query"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(OutOfBoundSearchCases))]
        [UnitTest]
        [Trait("Resource", "BloodPressures")]
        [Trait("Resource", "Search")]
        public async Task Search_With_OutOfBound_PagingConfiguration_Returns_NotFound(
            SearchBloodPressureInfo query, 
            (int maxPageSize, int defaultPageSize) apiOptions,
            IEnumerable<BloodPressure> measures, string reason)
        {
            // Arrange
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(measures);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _apiOptionsMock.Setup(mock => mock.Value)
                .Returns(new MeasuresApiOptions { MaxPageSize = apiOptions.maxPageSize, DefaultPageSize = apiOptions.defaultPageSize });
            // Act
            IActionResult actionResult = await _controller.Search(query)
                .ConfigureAwait(false);


            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>(reason);
        }


        

        [Fact]
        public async Task Delete_Returns_NoContent()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(new BloodPressure
                {
                    SystolicPressure = 150,
                    DiastolicPressure = 90,
                    UUID = id,
                    Patient = new Patient
                    {
                        Firstname = "Bruce",
                        Lastname = "Wayne"
                    }
                });

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            // Act
            IActionResult actionResult = await _controller.Delete(id)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NoContentResult>();


            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                bool deleted = !await uow.Repository<BloodPressure>()
                    .AnyAsync(x => x.UUID == id)
                    .ConfigureAwait(false);

                deleted.Should().BeTrue();
            }
        }
        

        [Fact]
        public async Task Get_Returns_The_Element()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(new BloodPressure
                {
                    SystolicPressure = 150,
                    DiastolicPressure = 90,
                    UUID = id,
                    Patient = new Patient
                    {
                        Firstname = "Bruce",
                        Lastname = "Wayne"
                    }
                });

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            // Act
            IActionResult actionResult = await _controller.Get(id)
                .ConfigureAwait(false);

            // Assert
            BrowsableResource<BloodPressureInfo> browsableResource = actionResult.Should()
                .BeAssignableTo<OkObjectResult>().Which
                .Value.Should()
                .BeAssignableTo<BrowsableResource<BloodPressureInfo>>().Which;

            browsableResource.Links.Should()
                .NotBeNull().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation)).And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href)).And
                .ContainSingle(x => x.Relation == LinkRelation.Self).And
                .ContainSingle(x => x.Relation == "delete").And
                .ContainSingle(x => x.Relation == "patient");


            Link self = browsableResource.Links.Single(x => x.Relation == LinkRelation.Self);
            self.Method.Should()
                .Be("GET");

            BloodPressureInfo resource = browsableResource.Resource;
            self.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            Link delete = browsableResource.Links.Single(x => x.Relation == "delete");
            delete.Method.Should()
                .Be("DELETE");
            delete.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={BloodPressuresController.EndpointName}&{nameof(resource.Id)}={resource.Id}");

            Link linkToPatient = browsableResource.Links.Single(x => x.Relation == "patient");
            linkToPatient.Method.Should()
                .Be("GET");
            linkToPatient.Href.Should()
                .BeEquivalentTo($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?controller={PatientsController.EndpointName}&{nameof(PatientInfo.Id)}={resource.PatientId}");


            resource.Id.Should().Be(id);
            resource.SystolicPressure.Should().Be(150);
            resource.DiastolicPressure.Should().Be(90);
            resource.PatientId.Should()
                .NotBeEmpty();
        }

        [Fact]
        public async Task Get_UnknonwnId_Returns_NotFound()
        {
            // Act
            IActionResult actionResult = await _controller.Get(Guid.NewGuid());

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        
        [Fact]
        public async Task Post_CreateTheResource_With_Patient()
        {
            // Arrange
            CreateBloodPressureInfo newResource = new CreateBloodPressureInfo
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 30.September(2010).AddHours(14).AddMinutes(53),
                Patient = new PatientInfo
                {
                    Firstname = "victor",
                    Lastname = "zsasz"
                }
            };

            // Act
            IActionResult actionResult = await _controller.Post(newResource);

            // Assert
            BrowsableResource<BloodPressureInfo> browsableResource = actionResult.Should()
                .BeOfType<CreatedAtRouteResult>().Which
                .Value.Should()
                    .BeAssignableTo<BrowsableResource<BloodPressureInfo>>().Which;


            BloodPressureInfo createdResource = browsableResource.Resource;
            createdResource.Should()
                .NotBeNull();
            createdResource.Id.Should().NotBeEmpty();
            createdResource.PatientId.Should().NotBeEmpty();
            createdResource.DateOfMeasure.Should().Be(newResource.DateOfMeasure);
            createdResource.SystolicPressure.Should().Be(newResource.SystolicPressure);
            createdResource.DiastolicPressure.Should().Be(newResource.DiastolicPressure);

            IEnumerable<Link> links = browsableResource.Links;
            links.Should()
                .NotBeEmpty().And
                .NotContainNulls().And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Href), $"{nameof(Link.Href)} must be provided for each link of the resource").And
                .NotContain(x => string.IsNullOrWhiteSpace(x.Relation), $"{nameof(Link.Relation)} must be provided for each link of the resource").And
                .Contain(x => x.Relation == "delete").And
                .Contain(x => x.Relation == "patient");

            Link linkToPatient = links.Single(x => x.Relation == "patient");
            linkToPatient.Href.Should().Be($"{_baseUrl}/{RouteNames.DefaultGetOneByIdApi}/?id={createdResource.PatientId}");

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                (await uow.Repository<Patient>().AnyAsync(x => x.UUID == createdResource.PatientId)
                    .ConfigureAwait(false)).Should().BeTrue("Creating a blood pressure with patient data should create the patient");
            }
        }

        [Fact]
        public async Task DeleteResource()
        {
            // Arrange
            BloodPressure measure = new BloodPressure
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 30.June(2004),
                Patient = new Patient
                {
                    Firstname = "Victor",
                    Lastname = "Zsaasz",
                    UUID = Guid.NewGuid(),
                },
                UUID = Guid.NewGuid(),
            };
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(measure);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }


            // Act
            IActionResult actionResult = await _controller.Delete(measure.UUID)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NoContentResult>();

            
        }

        [Fact]
        public async Task Delete_Unknown_Resource_Returns_Not_Found()
        {
            
            // Act
            IActionResult actionResult = await _controller.Delete(Guid.NewGuid())
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();


        }

        

        [Fact]
        public async Task Patch_UnknownEntity_Returns_NotFound()
        {
            JsonPatchDocument<BloodPressureInfo> changes = new JsonPatchDocument<BloodPressureInfo>();
            changes.Replace(x => x.SystolicPressure, 120);


            // Act
            IActionResult actionResult = await _controller.Patch(Guid.NewGuid(), changes)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task Patch_Valid_Resource_Returns_NoContentResult()
        {
            // Arrange
            BloodPressure measure = new BloodPressure
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 31.October(2003),
                Patient = new Patient
                {
                    Firstname = "Solomon",
                    Lastname = "Grundy",
                    UUID = Guid.NewGuid()
                },

                UUID = Guid.NewGuid()
            };

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(measure);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            JsonPatchDocument<BloodPressureInfo> changes = new JsonPatchDocument<BloodPressureInfo>();
            changes.Replace(x => x.DiastolicPressure, 90);

            // Act
            IActionResult actionResult = await _controller.Patch(measure.UUID, changes)
                .ConfigureAwait(false);

            // Assert
            actionResult.Should()
                .BeAssignableTo<NoContentResult>();

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                BloodPressure actualMeasure = await uow.Repository<BloodPressure>()
                    .SingleAsync(x => x.UUID == measure.UUID)
                    .ConfigureAwait(false);

                actualMeasure.DiastolicPressure.Should().Be(90);
                actualMeasure.SystolicPressure.Should().Be(measure.SystolicPressure);
            }
           

        }

    }
}
