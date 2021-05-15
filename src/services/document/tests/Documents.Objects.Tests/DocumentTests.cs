namespace Documents.Objects.Tests
{
    using Documents.Ids;

    using FluentAssertions;

    using System;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Documents")]
    public class DocumentTests
    {
        [Fact]
        public void Ctor_builds_a_valid_instance()
        {
            // Arrange
            DocumentId id = DocumentId.New();
            string name = $"Document {Guid.NewGuid()}";
            string mimeType = $"application/octet-stream+{Guid.NewGuid()}";

            // Act
            Document document = new(id, name, mimeType);

            // Assert
            document.Id.Should()
                .Be(id);
            document.Name.Should()
                .Be(name);
            document.MimeType.Should()
                .Be(mimeType);
            document.Hash.Should()
                .BeNull();
            document.Status.Should()
                           .Be(Status.Ongoing);
        }

        [Fact]
        public void Changing_size_for_a_document_with_status_Done_throws_InvalidOperationException()
        {
            // Arrange
            DocumentId id = DocumentId.New();
            string name = $"Document {Guid.NewGuid()}";
            string mimeType = $"application/octet-stream+{Guid.NewGuid()}";
            Document document = new(id, name, mimeType);
            document.Lock();

            // Act
            Action changeSize = () => document.UpdateSize(0);

            // Assert
            changeSize.Should()
                      .ThrowExactly<InvalidOperationException>("the size of a locked document cannot be changed");
        }

        [Fact]
        public void Changing_hash_for_a_document_with_status_Done_throws_InvalidOperationException()
        {
            // Arrange
            DocumentId id = DocumentId.New();
            string name = $"Document {Guid.NewGuid()}";
            string mimeType = $"application/octet-stream+{Guid.NewGuid()}";
            Document document = new(id, name, mimeType);

            document.Lock();

            // Act
            Action changingHash = () => document.UpdateHash($"{Guid.NewGuid()}");

            // Assert
            changingHash.Should()
                        .ThrowExactly<InvalidOperationException>($"{nameof(Document)}.{nameof(Document.Hash)} cannot be changed when its {nameof(Document.Status)} is {Status.Done}");
        }

        [Fact]
        public void Changing_hash_for_a_document_to_null_throws_ArgumentNullException()
        {
            // Arrange
            DocumentId id = DocumentId.New();
            string name = $"Document {Guid.NewGuid()}";
            string mimeType = $"application/octet-stream+{Guid.NewGuid()}";
            Document document = new(id, name, mimeType);

            // Act
            Action changingHashToNull = () => document.UpdateHash(null);

            // Assert
            changingHashToNull.Should()
                              .ThrowExactly<ArgumentNullException>($"{nameof(Document)}.{nameof(Document.Hash)} cannot be changed to null");
        }

        [Fact]
        public void Changing_size_to_negative_value_throws_ArgumentOutOfRangeException()
        {
            // Arrange
            DocumentId id = DocumentId.New();
            string name = $"Document {Guid.NewGuid()}";
            string mimeType = $"application/octet-stream+{Guid.NewGuid()}";
            Document document = new(id, name, mimeType);

            // Act
            Action changingSizeToNegativeValue = () => document.UpdateSize(-1);

            // Assert
            changingSizeToNegativeValue.Should()
                                       .ThrowExactly<ArgumentOutOfRangeException>($"{nameof(Document)}.{nameof(Document.Size)} cannot be negative");
        }

        [Fact]
        public void Changing_name_null_throws_ArgumentNullRangeException()
        {
            // Arrange
            DocumentId id = DocumentId.New();
            string name = $"Document {Guid.NewGuid()}";
            string mimeType = $"application/octet-stream+{Guid.NewGuid()}";
            Document document = new(id, name, mimeType);

            // Act
            Action changingNameToNull = () => document.ChangeNameTo(null);

            // Assert
            changingNameToNull.Should()
                                       .ThrowExactly<ArgumentNullException>($"{nameof(Document)}.{nameof(Document.Name)} cannot be null");
        }

        [Fact]
        public void Lock_change_status_to_done()
        {
            // Arrange
            DocumentId id = DocumentId.New();
            string name = $"Document {Guid.NewGuid()}";
            string mimeType = $"application/octet-stream+{Guid.NewGuid()}";
            Document document = new(id, name, mimeType);

            // Act
            document.Lock();

            // Assert
            document.Status.Should()
                           .Be(Status.Done);
        }
    }
}
