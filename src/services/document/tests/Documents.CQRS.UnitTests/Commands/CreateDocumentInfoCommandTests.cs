namespace Documents.CQRS.UnitTests.Commands
{
    using Documents.CQRS.Commands;
    using Documents.DTO;

    using FluentAssertions;

    using System;
    using System.Collections.Generic;

    using Xunit;
    using Xunit.Categories;

    [Feature(nameof(Documents))]
    [UnitTest]
    public class CreateDocumentInfoCommandTests
    {
        [Fact]
        public void Ctor_Is_Valid()
        {
            CreateDocumentInfoCommand instance = new(new NewDocumentInfo()
            {
            });

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
        }

        [Fact]
        public void Ctor_Throws_ArgumentNullException()
        {
            // Act
            Action action = () => new CreateDocumentInfoCommand(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                {
                    NewDocumentInfo data = new() { Name = "Wayne tower schema", MimeType = "application/octect-stream", Content = new byte[] { 123 } };
                    yield return new object[]
                            {
                    new CreateDocumentInfoCommand(data),
                    new CreateDocumentInfoCommand(data),
                    true,
                    $"two {nameof(CreateDocumentInfoCommand)} instances with same {nameof(CreateDocumentInfoCommand.Data)}"
                            };
                }

                yield return new object[]
                {
                    new CreateDocumentInfoCommand(new NewDocumentInfo { Name = "Wayne tower schema", MimeType = "application/jpg", Content= new byte[]{ 123 }  }),
                    null,
                    false,
                    "comparing an instance to null"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(CreateDocumentInfoCommand first, object second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }
    }
}
