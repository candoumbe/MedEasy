﻿using FluentAssertions;
using MedEasy.Core.Filters;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using static System.StringComparison;
using static Newtonsoft.Json.JsonConvert;
using static Newtonsoft.Json.NullValueHandling;
using static Newtonsoft.Json.Formatting;
using Newtonsoft.Json;
using Bogus;

namespace MedEasy.Core.UnitTests.Filters
{
    [UnitTest]
    [Feature("Filters")]
    public class AddTotalCountHeaderFilterAttributeTests : IDisposable
    {
        public class Minion
        {
            public string Name { get; set; }
        }

        private AddCountHeadersFilterAttribute _sut;
        private readonly ITestOutputHelper _outputHelper;

        public AddTotalCountHeaderFilterAttributeTests(ITestOutputHelper outputHelper)
        {
            _sut = new AddCountHeadersFilterAttribute();
            _outputHelper = outputHelper;
        }

        public void Dispose() => _sut = null;


        public static IEnumerable<object[]> AddHeadersDependingOnValueCases
        {
            get
            {
                yield return new object[]
                {
                    new { page = 1},
                    ((Expression<Func<IHeaderDictionary, bool>>)(headers => headers.None(header => AddCountHeadersFilterAttribute.CountHeaderName.Equals(header.Key, OrdinalIgnoreCase))
                        && headers.None(header => AddCountHeadersFilterAttribute.TotalCountHeaderName.Equals(header.Key, OrdinalIgnoreCase))

                    )),
                    "value is an anonymous object"
                };

                yield return new object[]
                {
                    new GenericPagedGetResponse<BrowsableResource<Minion>>(Enumerable.Empty<BrowsableResource<Minion>>(), count: 20),
                    ((Expression<Func<IHeaderDictionary, bool>>)(headers => headers.Once(header => AddCountHeadersFilterAttribute.CountHeaderName.Equals(header.Key, OrdinalIgnoreCase))
                        && headers.Once(header => AddCountHeadersFilterAttribute.TotalCountHeaderName.Equals(header.Key, OrdinalIgnoreCase))

                    )),
                    $"value is a {nameof(GenericPagedGetResponse<object>)}"
                };

                yield return new object[]
                {
                    Enumerable.Empty<Minion>(),
                    ((Expression<Func<IHeaderDictionary, bool>>)(headers => headers.None(header => AddCountHeadersFilterAttribute.CountHeaderName.Equals(header.Key, OrdinalIgnoreCase))
                        && headers.None(header => AddCountHeadersFilterAttribute.TotalCountHeaderName.Equals(header.Key, OrdinalIgnoreCase))

                    )),
                    $"value is a collection"
                };

                yield return new object[]
                {
                    new List<Minion>(),
                    ((Expression<Func<IHeaderDictionary, bool>>)(headers => headers.None(header => AddCountHeadersFilterAttribute.CountHeaderName.Equals(header.Key, OrdinalIgnoreCase))
                        && headers.None(header => AddCountHeadersFilterAttribute.TotalCountHeaderName.Equals(header.Key, OrdinalIgnoreCase))

                    )),
                    $"value is a list"
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddHeadersDependingOnValueCases))]
        public void GivenOkObjectResult_Filter_AddCountHeaders_DependingValue(object value, Expression<Func<IHeaderDictionary, bool>> headersExpectation, string reason)
        {
            // Arrange
            DefaultHttpContext httpContext = new DefaultHttpContext();
            ActionContext actionContext = new ActionContext(
               httpContext,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            ResultExecutingContext resultExecutingContext = new ResultExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new OkObjectResult(value),
                new Mock<Controller>());

            // Act
            _sut.OnResultExecuting(resultExecutingContext);

            // Assert
            IHeaderDictionary headers = resultExecutingContext.HttpContext.Response.Headers;
            headers.Should()
                .Match(headersExpectation, reason);
        }


        public static IEnumerable<object[]> ActionResultWithCollectionOfElementCases
        {
            get
            {
                yield return new object[]
                {
                    new GenericPagedGetResponse<BrowsableResource<Minion>>(Enumerable.Empty<BrowsableResource<Minion>>(), count: 20),
                    (expectedTotalCount : 20, expectedCount : 0)
                };
                {
                    IEnumerable<Minion> minions = new Faker<Minion>()
                        .RuleFor(minion => minion.Name, faker => faker.Name.FullName())
                        .Generate(10);

                    IEnumerable<BrowsableResource<Minion>> resources = minions
                        .Select(minion => new BrowsableResource<Minion>
                        {
                            Resource = minion,
                            Links = new[]
                            {
                                new Link { Relation = LinkRelation.Self, Method = "GET", Href = new Faker().Internet.Url() }
                            }
                        });

                    int resourcesCount = resources.Count();
                    yield return new object[]
                    {
                        new GenericPagedGetResponse<BrowsableResource<Minion>>(resources, count: resourcesCount),
                        (expectedTotal : resourcesCount, expectedCount : resourcesCount)
                    };

                }


            }
        }

        [Theory]
        [MemberData(nameof(ActionResultWithCollectionOfElementCases))]
        public void GivenOkObjectResultWithData_FilterAddHeaders(object okResultValue, (int expectedTotalCount, int expectedCount) headersCountExpectation)
        {
            // Arrange
            DefaultHttpContext httpContext = new DefaultHttpContext();
            ActionContext actionContext = new ActionContext(
               httpContext,
               new Mock<RouteData>().Object,
               new Mock<ActionDescriptor>().Object,
               new ModelStateDictionary());

            ResultExecutingContext resultExecutingContext = new ResultExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new OkObjectResult(okResultValue),
                new Mock<Controller>());

            // Act
            _sut.OnResultExecuting(resultExecutingContext);

            // Assert
            IHeaderDictionary headers = resultExecutingContext.HttpContext.Response.Headers;

            _outputHelper.WriteLine($"Headers : {SerializeObject(headers, new JsonSerializerSettings { NullValueHandling = Ignore, Formatting = Indented })}");

            headers.Should()
                .ContainKey(AddCountHeadersFilterAttribute.TotalCountHeaderName).WhichValue.Should()
                .HaveCount(1).And
                .ContainSingle(value => headersCountExpectation.expectedTotalCount.ToString().Equals(value));
            headers.Should()
                .ContainKey(AddCountHeadersFilterAttribute.CountHeaderName).WhichValue.Should()
                .HaveCount(1).And
                .ContainSingle(value => headersCountExpectation.expectedCount.ToString().Equals(value));

        }
    }
}