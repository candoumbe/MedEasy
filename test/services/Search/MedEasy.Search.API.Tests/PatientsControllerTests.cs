using MedEasy.DTO;
using MedEasy.DTO.Search;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.RestObjects;
using MedEasy.Search.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using static System.StringSplitOptions;
using FluentAssertions;
using System.Threading.Tasks;
using MedEasy.Objects;
using MedEasy.Queries.Search;
using System.Threading;
using MedEasy.Data;
using static Newtonsoft.Json.JsonConvert;
using MedEasy.DAL.Repositories;
using Microsoft.AspNetCore.Mvc.Routing;

namespace MedEasy.Search.API.Tests
{
    public class PatientsControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private PatientsController _controller;
        private Mock<IOptionsSnapshot<ApiOptions>> _apiOptionsMock;
        private Mock<IHandleSearchQuery> _iHandleSearchQueryMock;
        private Mock<IUrlHelper> _urlHelperMock;

        public PatientsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Action(It.IsNotNull<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}{(urlContext.Values == null ? string.Empty : $"?{urlContext.Values?.ToQueryString()}")}");

            _iHandleSearchQueryMock = new Mock<IHandleSearchQuery>(Strict);

            _apiOptionsMock = new Mock<IOptionsSnapshot<ApiOptions>>(Strict);


            _controller = new PatientsController(_urlHelperMock.Object, _iHandleSearchQueryMock.Object, _apiOptionsMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;

            _urlHelperMock = null;
            _iHandleSearchQueryMock = null;
            _apiOptionsMock = null;

            _controller = null;
        }

        public static IEnumerable<object> SearchCases
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
                        Enumerable.Empty<PatientInfo>(),
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation == "first" &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.PageSize)}={searchInfo.PageSize}")  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Sort)}={searchInfo.Sort}" )

                           )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

                    };

                }
                {
                    SearchPatientInfo searchInfo = new SearchPatientInfo
                    {
                        Firstname = "!bruce",
                        Page = 1,
                        PageSize = 30,
                        Sort = "-birthdate"
                    };
                    yield return new object[]
                    {
                        new [] {
                            new PatientInfo { Firstname = "Bruce", Lastname = "Wayne" }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation == "first" &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Firstname)}={Uri.EscapeDataString(searchInfo.Firstname)}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.PageSize)}={searchInfo.PageSize}")  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Sort)}={searchInfo.Sort}" )

                           )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

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
                            new PatientInfo { Firstname = "bruce" }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation == "first" &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 3 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.PageSize)}={searchInfo.PageSize}")

                            )),
                        ((Expression<Func<Link, bool>>)(previous => previous == null)),
                        ((Expression<Func<Link, bool>>)(next => next == null)),
                        ((Expression<Func<Link, bool>>)(last => last == null))

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
                            new PatientInfo { Firstname = "bruce", BirthDate = 31.July(2010) }
                        },
                        searchInfo,
                        ((Expression<Func<Link, bool>>)(first =>
                            first != null &&
                            first.Relation == "first" &&
                            first.Href != null &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries).Length == 2 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Length == 4 &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Firstname)}={searchInfo.Firstname}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.BirthDate)}={searchInfo.BirthDate.Value.ToString("s")}" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.Page)}=1" )  &&
                            first.Href.Split(new [] {"?" }, RemoveEmptyEntries)[1].Split(new [] {"&"}, RemoveEmptyEntries).Once(x => x == $"{nameof(SearchPatientInfo.PageSize)}={searchInfo.PageSize}")

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
        public async Task Search(IEnumerable<PatientInfo> entries, SearchPatientInfo searchRequest,
        Expression<Func<Link, bool>> firstPageLinkExpectation, Expression<Func<Link, bool>> previousPageLinkExpectation, Expression<Func<Link, bool>> nextPageLinkExpectation, Expression<Func<Link, bool>> lastPageLinkExpectation)
        {
            _outputHelper.WriteLine($"Entries : {SerializeObject(entries)}");
            _outputHelper.WriteLine($"Request : {SerializeObject(searchRequest)}");


            // Arrange
            ApiOptions apiOptions = new ApiOptions { DefaultPageSize = 30, MaxPageSize = 50 };
            _apiOptionsMock.Setup(mock => mock.Value).Returns(apiOptions);
            _iHandleSearchQueryMock.Setup(mock => mock.Search<Patient, PatientInfo>(It.IsNotNull<SearchQuery<PatientInfo>>(), It.IsAny<CancellationToken>()))
                    .Returns((SearchQuery<PatientInfo> query, CancellationToken cancellationToken) => Task.Run(() =>
                    {
                        SearchQueryInfo<PatientInfo> data = query.Data;
                        Expression<Func<PatientInfo, bool>> filter = data.Filter.ToExpression<PatientInfo>();
                        int page = query.Data.Page;
                        int pageSize = query.Data.PageSize;
                        Func<PatientInfo, bool> fnFilter = filter.Compile();

                        IEnumerable<PatientInfo> result = entries.Where(fnFilter)
                            .Skip(page * pageSize)
                            .Take(pageSize);

                        IPagedResult<PatientInfo> pageOfResult = new PagedResult<PatientInfo>(result, entries.Count(fnFilter), pageSize);
                        return pageOfResult;
                    })
                    );


            // Act
            IActionResult actionResult = await _controller.Search(searchRequest);

            // Assert
            IGenericPagedGetResponse<PatientInfo> content = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .NotBeNull().And
                        .BeAssignableTo<IGenericPagedGetResponse<PatientInfo>>().Which;


            content.Items.Should()
                .NotBeNull();

            content.Links.Should().NotBeNull();
            PagedRestResponseLink links = content.Links;

            links.First.Should().Match(firstPageLinkExpectation);
            links.Previous.Should().Match(previousPageLinkExpectation);
            links.Next.Should().Match(nextPageLinkExpectation);
            links.Last.Should().Match(nextPageLinkExpectation);
        }
    }
}
