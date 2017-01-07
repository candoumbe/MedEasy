using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System;

namespace MedEasy.Tools.Tests
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_concrete_open_generic_type()
        {
            typeof(Foo<>).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_generic_interface_type()
        {
            typeof(IFoo<int>).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_itself()
        {
            typeof(IFoo<>).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_concrete_generic_type()
        {
            typeof(Foo<int>).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }


        [Fact]
        public void Should_be_assignable_from_open_generic_type_to_nongeneric_concrete_type()
        {
           typeof(Bar).IsAssignableToGenericType(typeof(IFoo<>)).Should().BeTrue();
        }

        public interface IFoo<T> { }
        public class Foo<T> : IFoo<T> { }
        public class Bar : IFoo<int> { }


        public static IEnumerable<object> IsAnonymousCases
        {
            get
            {
                yield return new object[]
                {
                    new {}.GetType(),
                    true
                };

                yield return new object[]
                {
                    typeof(Bar),
                    false
                };

                yield return new object[]
                {
                    typeof(Foo<>),
                    false
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsAnonymousCases))]
        public void IsAnonymous(Type t, bool expectedResult)
            => t.IsAnonymousType().Should().Be(expectedResult);
    }
}
