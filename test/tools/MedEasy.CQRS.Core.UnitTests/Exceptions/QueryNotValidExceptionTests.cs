using MedEasy.CQRS.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace MedEasy.CQRS.Core.UnitTests.Exceptions
{
    public class QueryNotValidExceptionTests
    {
        public static IEnumerable<object[]> CtorThatThrowsArgumentExceptionCases
        {
            get
            {
                yield return new object[]
                {
                    ((Action)(() => new QueryNotValidException<string>(null, Enumerable.Empty<ErrorInfo>())))
                };

                yield return new object[]
                {
                    ((Action)(() => new QueryNotValidException<int>(0, Enumerable.Empty<ErrorInfo>())))
                };


                yield return new object[]
                {
                    ((Action)(() => new QueryNotValidException<Guid>(Guid.Empty, Enumerable.Empty<ErrorInfo>())))
                };
            }
        }

        [Theory]
        [MemberData(nameof(CtorThatThrowsArgumentExceptionCases))]
        public void CtorShouldThrowArgumentOutOfRangeExceptionWhenCommandIdIsDefaultValue(Action ctorAction)
        {
            ctorAction.Should().Throw<ArgumentOutOfRangeException>().Which
                 .ParamName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void InheritValidationException() => typeof(ValidationException).IsAssignableFrom(typeof(QueryNotValidException<>))
            .Should().BeTrue();

    }
}
