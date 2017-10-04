using FluentAssertions;
using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MedEasy.Queries.Tests
{
    public class GenericGetPageOfResourcesQueryTests
    {
        [Fact]
        public void DefaultCtor()
        {
            // Act
            GenericGetPageOfResourcesQuery<int> instance = new GenericGetPageOfResourcesQuery<int>(new PaginationConfiguration());


            // Assert
            instance.Id.Should().NotBeEmpty("query id must not be empty");
        }
    }
}
