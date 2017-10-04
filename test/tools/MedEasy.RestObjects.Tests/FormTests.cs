using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MedEasy.RestObjects.Tests
{
    public class FormTests
    {

        [Fact]
        public void DefaultConstructorSetItemsToEmpty() => new Form().Items
            .Should().BeEmpty();
    }
}
