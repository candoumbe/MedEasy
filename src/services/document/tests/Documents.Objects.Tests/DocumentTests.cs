using FluentAssertions;
using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using Xunit.Categories;

namespace Documents.Objects.Tests
{
    [UnitTest]
    [Feature("Documents")]
    public class DocumentTests
    {
        [Fact]
        public void CtorBuildsValidInstance()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            string name = $"Document {Guid.NewGuid()}";
            byte[] content = Encoding.UTF8.GetBytes($"{Guid.NewGuid()}");
            string mimeType = $"application/octet-stream+{Guid.NewGuid()}";

            // Act
            Document document = new Document(id, name, mimeType)
                .SetFile(content);

            // Assert
            document.Id.Should()
                .Be(id);
            document.Name.Should()
                .Be(name);
            document.MimeType.Should()
                .Be(mimeType);
            document.Hash.Should()
                .Be(Encoding.UTF8.GetString(SHA256.Create().ComputeHash(content)));

        }

        [Fact]
        public void TwoDocuments_With_Same_Content_Has_Same_Hash()
        {
            // Arrange
            byte[] content =new byte[] { 1, 2, 3 };

            Document first = new Document(Guid.NewGuid(), "My super file", "application/text")
                .SetFile(content);
            Document second = new Document(Guid.NewGuid(), "My regular file", "application/text")
                .SetFile(content);

            // Act
            string firstHash = first.Hash;
            string secondHash = second.Hash;

            // Assert
            firstHash.Should()
                .Be(secondHash, "Both file holds the same content");
        }

        [Fact]
        public void DocumentHashOnlyDependsOnFileContent()
        {
            // Arrange
            byte[] content = Encoding.UTF8.GetBytes($"{Guid.NewGuid()}");
            string expected = Encoding.UTF8.GetString(SHA256.Create().ComputeHash(content));

            // Act
            Document document = new Document(Guid.NewGuid(), $"Document_{Guid.NewGuid()}", "mimeType")
                .SetFile(content);

            // Assert
            document.Hash.Should()
                .Be(expected, $"{nameof(document.Hash)} should rely solely on the file content");
        }
    }
}
