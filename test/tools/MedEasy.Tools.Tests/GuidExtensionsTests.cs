using FluentAssertions;
using MedEasy.Tools.Extensions;
using System;
using Xunit;

namespace MedEasy.Tools.Tests
{
    public class GuidExtensionsTests
    {
        [Fact]
        public void Encode()
        {
            Guid guid = Guid.NewGuid();
            string encodedString = guid.Encode();

            encodedString.Should().HaveLength(22);
            encodedString.Decode().Should().Be(guid);
        }
    }
}
