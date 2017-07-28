using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;
using Xunit.Abstractions;

using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Tools.Tests
{
    public class EnumerableExtensionsTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EnumerableExtensionsTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        /// <summary>
        /// <see cref="Once(IEnumerable{int}, Expression{Func{int, bool}}, bool)"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> OnceCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    ((Expression<Func<int, bool>>) (x => x == 1)),
                    false
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    ((Expression<Func<int, bool>>) (x => x == 1)),
                    true
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    ((Expression<Func<int, bool>>) (x => x == 5)),
                    false
                };

            }
        }

        /// <summary>
        /// Unit tests for <see cref="EnumerableExtensions.Once{T}(IEnumerable{T})"/>
        /// </summary>
        /// <param name="source">collection to apply <see cref="EnumerableExtensions.Once{T}(IEnumerable{T})"/> onto.</param>
        /// <param name="predicate">predicate</param>
        /// <param name="expectedResult">expected result</param>
        [Theory]
        [MemberData(nameof(OnceCases))]
        public void Once(IEnumerable<int> source, Expression<Func<int, bool>> predicate, bool expectedResult)
        {
            _outputHelper.WriteLine($"{nameof(source)} : {SerializeObject(source)}");
            _outputHelper.WriteLine($"{nameof(predicate)} : {predicate}");

            // Act and assert
            source.Once(predicate).Should().Be(expectedResult);
        }


        /// <summary>
        /// <see cref="AtLeastOnce(IEnumerable{int}, Expression{Func{int, bool}}, bool)"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> AtLeastOnceCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    ((Expression<Func<int, bool>>) (x => x == 1)),
                    false
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    ((Expression<Func<int, bool>>) (x => x == 1)),
                    true
                };

                yield return new object[]
                {
                    new []{ 1, 3 },
                    ((Expression<Func<int, bool>>) (x => x == 5)),
                    false
                };

                yield return new object[]
                {
                    new []{ 1, 3, 3 },
                    ((Expression<Func<int, bool>>) (x => x ==3)),
                    true
                };

            }
        }

        /// <summary>
        /// Unit tests for <see cref="EnumerableExtensions.AtLeastOnce{T}(IEnumerable{T})"/>
        /// </summary>
        /// <param name="source">collection to apply <see cref="EnumerableExtensions.AtLeastOnce{T}(IEnumerable{T})"/> onto.</param>
        /// <param name="predicate">predicate</param>
        /// <param name="expectedResult">expected result</param>
        [Theory]
        [MemberData(nameof(AtLeastOnceCases))]
        public void AtLeastOnce(IEnumerable<int> source, Expression<Func<int, bool>> predicate, bool expectedResult)
        {
            _outputHelper.WriteLine($"{nameof(source)} : {SerializeObject(source)}");
            _outputHelper.WriteLine($"{nameof(predicate)} : {predicate}");

            // Act and assert
            source.AtLeastOnce(predicate).Should().Be(expectedResult);
        }

        /// <summary>
        /// <see cref="CrossJoin(IEnumerable{int}, IEnumerable{int})"/> tests cases
        /// </summary>
        public static IEnumerable<object[]> CrossJoinCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    Enumerable.Empty<int>(),
                    ((Expression<Func<IEnumerable<(int, int)>, bool>>)(items => !items.Any()))
                };

                yield return new object[]
                {
                    new [] { 1, 3 },
                    Enumerable.Empty<int>(),
                    ((Expression<Func<IEnumerable<(int, int)>, bool>>)(items => !items.Any()))
                };

                yield return new object[]
                {
                    Enumerable.Empty<int>(),
                    new [] { 1, 3 },
                    ((Expression<Func<IEnumerable<(int, int)>, bool>>)(items => !items.Any()))
                };

                yield return new object[]
                {
                    new [] { 1, 3 },
                    new [] { 2 },
                    ((Expression<Func<IEnumerable<(int X, int Y)>, bool>>)(items => 
                        items.Count() == 2 &&
                        items.Once(tuple => tuple.X == 1 && tuple.Y == 2) &&
                        items.Once(tuple => tuple.X == 3 && tuple.Y == 2)
                    ))
                };


                yield return new object[]
                {
                    new [] { 2 },
                    new [] { 1, 3 },
                    ((Expression<Func<IEnumerable<(int X, int Y)>, bool>>)(items =>
                        items.Count() == 2 &&
                        items.Once(tuple => tuple.X == 2 && tuple.Y == 1) &&
                        items.Once(tuple => tuple.X == 2 && tuple.Y == 3)
                    ))
                };

            }
        }

        /// <summary>
        /// Unit tests <see cref="EnumerableExtensions.CrossJoin{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})"/>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="crossJoinResultExpectation"></param>
        [Theory]
        [MemberData(nameof(CrossJoinCases))]
        public void CrossJoin(IEnumerable<int> first, IEnumerable<int> second, Expression<Func<IEnumerable<(int, int)>, bool>> crossJoinResultExpectation)
        {
            _outputHelper.WriteLine($"{nameof(first)} : {SerializeObject(first)}");
            _outputHelper.WriteLine($"{nameof(second)} : {SerializeObject(second)}");
            
            // Act
            IEnumerable<(int, int)> result = first.CrossJoin(second);
            
            // Assert
            result.Should()
                .Match(crossJoinResultExpectation);
        }


    }
}
