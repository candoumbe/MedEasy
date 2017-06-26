using FluentAssertions;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Validators;
using MedEasy.Validators.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace MedEasy.Handlers.Tests.Exceptions
{
    public class NotValidExceptionTests
    {
        public static IEnumerable<object> CtorThatThrowsArgumentExceptionCases
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
            ctorAction.ShouldThrow<ArgumentOutOfRangeException>().Which
                 .ParamName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void InheritVQueryException() => typeof(QueryException).IsAssignableFrom(typeof(QueryNotValidException<>))
            .Should().BeTrue();

    }
}
