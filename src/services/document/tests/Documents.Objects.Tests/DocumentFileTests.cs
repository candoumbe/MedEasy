using FluentAssertions;
using System;
using Xunit;

namespace Documents.Objects.Tests
{
    public class DocumentFileTests
    {
        [Fact]
        public void CtorThrows_ArgumentNullException_When_Content_IsNull()
        {
            // Act
            Action action = () => new DocumentFile(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>("content of document file cannot be null");
        }

        [Fact]
        public void CtorThrows_ArgumentOutOfRangeException_When_Content_IsEmpty()
        {
            // Act
            Action action = () => new DocumentFile(Array.Empty<byte>());

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>("Content of document file cannot be empty");
        }
    }
}
