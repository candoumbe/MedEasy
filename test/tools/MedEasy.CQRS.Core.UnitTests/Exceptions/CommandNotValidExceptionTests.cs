using FluentAssertions;
using MedEasy.CQRS.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MedEasy.CQRS.Core.UnitTests
{
    public class CommandNotValidExceptionTests
    {
        public static IEnumerable<object[]> CtorThatThrowsArgumentExceptionCases
        {
            get
            {
                yield return new object[]
                {
                    ((Action)(() => new CommandNotValidException<string>(null, Enumerable.Empty<ErrorInfo>())))
                };

                yield return new object[]
                {
                    ((Action)(() => new CommandNotValidException<int>(0, Enumerable.Empty<ErrorInfo>())))
                };


                yield return new object[]
                {
                    ((Action)(() => new CommandNotValidException<Guid>(Guid.Empty, Enumerable.Empty<ErrorInfo>())))
                };
            }
        }

        [Theory]
        [MemberData(nameof(CtorThatThrowsArgumentExceptionCases))]
        public void CtorShouldThrowArgumentOutOfRangeExceptionWhenCommandIdIsDefaultValue(Action ctorAction)
        {
            ctorAction.Should().Throw<ArgumentOutOfRangeException>().Which
                 .ParamName.Should()
                 .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void InheritValidationException() => typeof(ValidationException).IsAssignableFrom(typeof(CommandNotValidException<>))
            .Should().BeTrue();

    }
}
