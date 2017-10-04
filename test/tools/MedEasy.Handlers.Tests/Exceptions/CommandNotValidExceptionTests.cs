using FluentAssertions;
using FluentValidation.Results;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Handlers.Tests.Exceptions
{
    public class CommandNotValidExceptionTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public CommandNotValidExceptionTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose() => _outputHelper = null;

        public static IEnumerable<object> CtorThatThrowsArgumentExceptionCases
        {
            get
            {
                yield return new object[]
                {
                    ((Action)(() => new CommandNotValidException<string>(null, Enumerable.Empty<ValidationFailure>())))
                };

                yield return new object[]
                {
                    ((Action)(() => new CommandNotValidException<int>(0, Enumerable.Empty<ValidationFailure>())))
                };


                yield return new object[]
                {
                    ((Action)(() => new CommandNotValidException<Guid>(Guid.Empty, Enumerable.Empty<ValidationFailure>())))
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
        public void InheritCommandException() => typeof(CommandException).IsAssignableFrom(typeof(CommandNotValidException<>))
            .Should().BeTrue();

    }
}
