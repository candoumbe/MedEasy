using FluentAssertions;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MedEasy.Tests.RestObjects
{
    public class GenericGetQueryTests
    {


        /// <summary>
        /// Tests cases when setting <see cref="GenericGetQuery.PageSize"/>
        /// </summary>
        public static IEnumerable<object[]> PageSizeTestCases
        {
            get
            {
                                
                IEnumerable<int> pageSizes = new[] { 0, int.MinValue, int.MaxValue };
                foreach (var pageSize in pageSizes)
                {
                    yield return new object[]
                    {
                        pageSize,
                        pageSize < 1 ? 1 : pageSize
                    };
                }
                
            }
        }

        /// <summary>
        /// Tests cases when setting <see cref="GenericGetQuery.Page"/>
        /// </summary>
        public static IEnumerable<object[]> PageTestCases
        {
            get
            {
                
                IEnumerable<int> pages = new[] { 0, int.MinValue, int.MaxValue };
                foreach (var page in pages)
                {
                    yield return new object[]
                    {
                        page,
                        page < 1 ? 1 : page
                    };
                
                }
            }
        }

        [Fact]
        public void DefaultCtor()
        {
            GenericGetQuery getQuery = new GenericGetQuery();

            getQuery.PageSize.Should().Be(GenericGetQuery.DefaultPageSize);
            getQuery.Page.Should().Be(1);
        }


        [Theory]
        [MemberData(nameof(PageSizeTestCases))]
        public void PageSize(int input, int expectedValue)
        {
            GenericGetQuery query = new GenericGetQuery { PageSize = input };
            query.PageSize.Should()
                .Be(expectedValue, $"because getting the value of {nameof(GenericGetQuery.PageSize)} after setting its value with {input} should output {expectedValue} ");

        }


        [Theory]
        [MemberData(nameof(PageTestCases))]
        public void Page( int input, int expectedValue)
        {
            GenericGetQuery query =  new GenericGetQuery { Page = input };
            query.Page.Should()
                .Be(expectedValue, $"because getting the value of {nameof(GenericGetQuery.Page)} after setting its value with {input} should output {expectedValue} ");
        }
    }
}
