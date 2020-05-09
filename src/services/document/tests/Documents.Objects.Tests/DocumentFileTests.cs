using FluentAssertions;
using System;
using Xunit;

namespace Documents.Objects.Tests
{
    public class DocumentFileTests
    {

        [Fact]
        public void Ctor_throws_ArgumentOutOfRangeException_when_DocumentId_is_empty()
        {
            // Act
            Action action = () => new DocumentPart(Guid.Empty, 0, new byte[] { 1 });

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>("DocumentId cannot be empty");
        }

        [Fact]
        public void Ctor_throws_ArgumentOutOfRangeException_when_position_is_lt_0()
        {
            // Act
            Action action = () => new DocumentPart(Guid.NewGuid(), -1, new byte[] { 1 });

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>("position cannot be less than 0");
        }

        [Fact]
        public void Ctor_throws_ArgumentNullException_When_content_is_null()
        {
            // Act
            Action action = () => new DocumentPart(Guid.NewGuid(), 10, null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>("content cannot be null");
        }
    }
}
