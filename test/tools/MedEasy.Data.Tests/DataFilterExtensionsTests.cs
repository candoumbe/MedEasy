using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using static MedEasy.Data.DataFilterLogic;
using static MedEasy.Data.DataFilterOperator;

namespace MedEasy.Data.Tests
{
    public class DataFilterExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public class Person
        {
            public string Firstname { get; set; }

            public string Lastname { get; set; }

            public DateTime BirthDate { get; set; }

        }

        public class SuperHero : Person
        {
            public string Nickname { get; set; }

            public int Height { get; set; }

            public Henchman Henchman { get; set; }
        }

        public class Henchman : SuperHero
        {

        }

        public DataFilterExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }


        public static IEnumerable<object> EqualToTestCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<SuperHero>(),
                    new DataFilter(field : nameof(SuperHero.Firstname), @operator : EqualTo, value : "Bruce"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == "Bruce"))
                };


                yield return new object[]
                {
                    new[] {new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" }},
                    new DataFilter(field : nameof(SuperHero.Firstname), @operator : EqualTo, value : null),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == null))
                };

                yield return new object[]
                {
                    new[] {new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }},
                    new DataFilter(field : nameof(SuperHero.Firstname), @operator : EqualTo, value : "Bruce"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == "Bruce"))
                };


                yield return new object[]
                {
                    new[] {new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190 }},
                    new DataFilter(field : nameof(SuperHero.Height), @operator : EqualTo, value : 190),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Height == 190 ))
                };



                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                    },
                    new DataCompositeFilter {
                        Logic = Or,
                        Filters = new [] {
                            new DataFilter(field : nameof(SuperHero.Nickname), @operator : EqualTo, value : "Batman"),
                            new DataFilter(field : nameof(SuperHero.Nickname), @operator : EqualTo, value : "Superman")
                        }
                    },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Nickname == "Batman" || item.Nickname == "Superman"))
                };


                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }

                    },
                    new DataCompositeFilter {
                        Logic = And,
                        Filters = new IDataFilter [] {
                            new DataFilter(field : nameof(SuperHero.Firstname), @operator : Contains, value : "a"),
                            new DataCompositeFilter
                            {
                                Logic = Or,
                                Filters = new [] {
                                    new DataFilter(field : nameof(SuperHero.Nickname), @operator : EqualTo, value : "Batman"),
                                    new DataFilter(field : nameof(SuperHero.Nickname), @operator : EqualTo, value : "Superman")
                                }
                            }
                        }
                    },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a") && (item.Nickname == "Batman" || item.Nickname == "Superman")))
                };


            }

        }
        [Theory]
        [MemberData(nameof(EqualToTestCases))]
        public void BuildEqual(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

        public static IEnumerable<object> IsNullTestCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<SuperHero>(),
                    new DataFilter(field : nameof(SuperHero.Firstname), @operator : IsNull),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == null))
                };

                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = null, Lastname = "", Height = 178, Nickname = "Sinestro" }

                    },
                    new DataFilter(field : nameof(SuperHero.Firstname), @operator : IsNull),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == null)),
                };
            }
        }
        [Theory]
        [MemberData(nameof(IsNullTestCases))]
        public void BuildIsNull(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

        public static IEnumerable<object> IsEmptyTestCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<SuperHero>(),
                    new DataFilter(field : nameof(SuperHero.Lastname), @operator : IsEmpty),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname == string.Empty))
                };


                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "", Lastname = "", Height = 178, Nickname = "Sinestro" }

                    },
                    new DataFilter(field : nameof(SuperHero.Lastname), @operator : IsEmpty),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname == string.Empty)),

                };

                yield return new object[]
                {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                            Henchman = new Henchman
                            {
                                Firstname = "Dick", Lastname = "Grayson", Nickname = "Robin"
                            }
                        },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman",
                           Henchman = new Henchman { Nickname = "Krypto" } }
                    },
                    new DataFilter(field : $"{nameof(SuperHero.Henchman)}.{nameof(Henchman.Firstname)}", @operator : IsEmpty),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Henchman.Lastname == string.Empty))
                };


                yield return new object[]
                {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce",
                            Lastname = "Wayne",
                            Height = 190,
                            Nickname = "Batman",
                            Henchman = new Henchman
                            {
                                Firstname = "Dick",
                                Lastname = "Grayson"
                            }
                        }
                    },
                    new DataFilter(field : $"{nameof(SuperHero.Henchman)}.{nameof(Henchman.Firstname)}", @operator : NotEqualTo, value: "Dick"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Henchman.Firstname != "Dick"))
                };

            }
        }

        [Theory]
        [MemberData(nameof(IsEmptyTestCases))]
        public void BuildIsEmpty(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);


        public static IEnumerable<object> IsNotEmptyTestCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<SuperHero>(),
                    new DataFilter(field : nameof(SuperHero.Lastname), @operator : IsNotEmpty),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname != string.Empty))
                };


                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "", Lastname = "", Height = 178, Nickname = "Sinestro" }

                    },
                    new DataFilter(field : nameof(SuperHero.Lastname), @operator : IsNotEmpty),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname != string.Empty)),

                };

                yield return new object[]
                {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman",
                            Henchman = new Henchman
                            {
                                Firstname = "Dick", Lastname = "Grayson", Nickname = "Robin"
                            }
                        },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman",
                           Henchman = new Henchman { Nickname = "Krypto" } }
                    },
                    new DataFilter(field : $"{nameof(SuperHero.Henchman)}.{nameof(Henchman.Firstname)}", @operator : IsNotEmpty),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Henchman.Lastname != string.Empty))
                };

            }
        }

        [Theory]
        [MemberData(nameof(IsNotEmptyTestCases))]
        public void BuildIsNotEmpty(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

                public static IEnumerable<object> StartsWithCases
        {
            get
            {
                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }

                    },
                    new DataFilter(field : nameof(SuperHero.Nickname), @operator : StartsWith, value: "B"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Nickname.StartsWith("B")))
                };
            }

        }


        [Theory]
        [MemberData(nameof(StartsWithCases))]
        public void BuildStartsWith(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

        public static IEnumerable<object> EndsWithCases
        {
            get
            {
                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }

                    },
                    new DataFilter(field: nameof(SuperHero.Nickname), @operator: EndsWith, value:"n"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Nickname.EndsWith("n")))
                };
            }

        }

        [Theory]
        [MemberData(nameof(EndsWithCases))]
        public void BuildEndsWith(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

        public static IEnumerable<object> ContainsCases
        {
            get
            {
                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }

                    },
                    new DataFilter(field:nameof(SuperHero.Nickname), @operator: Contains, value: "an"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Nickname.Contains("n")))
                };
            }

        }

        [Theory]
        [MemberData(nameof(ContainsCases))]
        public void BuildContains(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);


        public static IEnumerable<object> LessThanTestCases
        {
            get
            {
                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = null, Lastname = "", Height = 178, Nickname = "Sinestro" }

                    },
                    new DataFilter(field : nameof(SuperHero.Height), @operator : LessThan, value: 150),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Height < 150)),
                };


            }
        }

        [Theory]
        [MemberData(nameof(LessThanTestCases))]
        public void BuildLessThan(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

        public static IEnumerable<object> NotEqualToTestCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<SuperHero>(),
                    new DataFilter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname != "Kent"))
                };
                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                    },
                    new DataFilter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname != "Kent"))
                };

                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                    },
                    new DataFilter(field : nameof(SuperHero.Lastname), @operator : NotEqualTo, value : "Kent"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname != "Kent"))
                };


                yield return new object[]
                {
                    new[] {
                        new SuperHero {
                            Firstname = "Bruce",
                            Lastname = "Wayne",
                            Height = 190,
                            Nickname = "Batman",
                            Henchman = new Henchman
                            {
                                Firstname = "Dick",
                                Lastname = "Grayson"
                            }
                        }
                    },
                    new DataFilter(field : $"{nameof(SuperHero.Henchman)}.{nameof(Henchman.Firstname)}", @operator : NotEqualTo, value : "Dick"),
                    ((Expression<Func<SuperHero, bool>>)(item => item.Henchman.Firstname != "Dick"))
                };

            }
        }

        [Theory]
        [MemberData(nameof(NotEqualToTestCases))]
        public void BuildNotEqual(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);


        /// <summary>
        /// Tests various filters
        /// </summary>
        /// <param name="superheroes">Collections of </param>
        /// <param name="filter">filter under test</param>
        /// <param name="expression">Expression the filter should match once built</param>
        private void Build(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
        {
            _output.WriteLine($"Filtering {JsonConvert.SerializeObject(superheroes)}");
            _output.WriteLine($"Filter under test : {filter}");
            _output.WriteLine($"Reference expression : {expression.Body.ToString()}");

            // Act
            Expression<Func<SuperHero, bool>> buildResult = filter.ToExpression<SuperHero>();
            IEnumerable<SuperHero> filteredResult = superheroes
                .Where(buildResult.Compile())
                .ToList();
            _output.WriteLine($"Current expression : {buildResult.Body.ToString()}");

            // Assert
            filteredResult.Should()
                .NotBeNull()
                .And.BeEquivalentTo(superheroes?.Where(expression.Compile()));

        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void ShouldReturnAlwaysTrueExpression()
        {
            // Act
            IEnumerable<SuperHero> superHeroes = new[]
            {
                new SuperHero { Firstname = "Clark", Lastname = "Kent",  Nickname = "Superman" },
                new SuperHero { Firstname = "Bruce", Lastname = "Wayne",  Nickname = "Batman" },
                new SuperHero { Firstname = "Dick", Lastname = "Grayson",  Nickname = "Nightwing" },
            };





        }


        public static IEnumerable<object> QueryStringToFilterCases
        {
            get
            {

                yield return new object[]
               {
                    string.Empty,
                    ((Expression<Func<IDataFilter, bool>>)(x => x is DataFilter &&
                        ((DataFilter)x).Field ==  null &&
                        ((DataFilter)x).Value == null))
               };

                yield return new object[]
                {
                    "Firstname=Bruce",
                    ((Expression<Func<IDataFilter, bool>>)(x => x is DataFilter &&
                        ((DataFilter)x).Field == "Firstname" &&
                        ((DataFilter)x).Operator == EqualTo &&
                         Equals(((DataFilter)x).Value, "Bruce")
                        ))
                };

                yield return new object[]
                {
                    "Firstname=!Bruce",
                    ((Expression<Func<IDataFilter, bool>>)(x => x is DataFilter &&
                        ((DataFilter)x).Field == "Firstname" &&
                        ((DataFilter)x).Operator == NotEqualTo &&
                            Equals(((DataFilter)x).Value, "Bruce")
                        ))
                };

                yield return new object[]
                {
                    $"Firstname={Uri.EscapeDataString("!!Bruce")}",
                    ((Expression<Func<IDataFilter, bool>>)(x => x is DataFilter &&
                        ((DataFilter)x).Field == "Firstname" &&
                        ((DataFilter)x).Operator == NotEqualTo &&
                            Equals(((DataFilter)x).Value, "Bruce")
                        ))
                };

                yield return new object[]
                {
                    $"Firstname={Uri.EscapeDataString("Bruce Dick")}",
                    ((Expression<Func<IDataFilter, bool>>)(x => x is DataCompositeFilter &&
                        ((DataCompositeFilter)x).Logic == Or &&

                        ((DataCompositeFilter)x).Filters != null &&
                        ((DataCompositeFilter)x).Filters.Count() == 2 &&

                        ((DataCompositeFilter)x).Filters.Once(f =>
                            f is DataFilter &&
                            ((DataFilter)f).Field == "Firstname" &&
                            ((DataFilter)f).Operator == EqualTo &&
                            Equals(((DataFilter)f).Value, "Bruce")) &&

                        ((DataCompositeFilter)x).Filters.Once(f =>
                            f is DataFilter &&
                            ((DataFilter)f).Field == "Firstname" &&
                            ((DataFilter)f).Operator == EqualTo &&
                            Equals(((DataFilter)f).Value, "Dick"))
                        ))
                };

                yield return new object[]
                {
                    "Firstname=Bru*",
                    ((Expression<Func<IDataFilter, bool>>)(x => x is DataFilter &&
                        ((DataFilter)x).Field == "Firstname" &&
                        ((DataFilter)x).Operator == StartsWith &&
                            Equals(((DataFilter)x).Value, "Bru")
                        ))
                };

                yield return new object[]
                {
                    "Firstname=*Bru",
                    ((Expression<Func<IDataFilter, bool>>)(x => x is DataFilter &&
                        ((DataFilter)x).Field == "Firstname" &&
                        ((DataFilter)x).Operator == EndsWith &&
                            Equals(((DataFilter)x).Value, "Bru")
                        ))
                };
                
            }
        }

        [Fact]
        public void ToFilterThrowsArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => DataFilterExtensions.ToFilter<SuperHero>(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void ToExpressionThrowsArgumentNullExceptionWhenParameterIsNull()
        {
            // Act
            Action action = () => DataFilterExtensions.ToExpression<SuperHero>(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        /// <summary>
        /// Tests for the <see cref="DataFilterExtensions.ToFilter{T}(string)"/>
        /// </summary>
        /// <param name="superHeroes">Collection on which the <paramref name="queryString"/> filter would apply.</param>
        /// <param name="queryString">The queryString under test</param>
        /// <param name="resultExpression">The <see cref="Expression{TDelegate}"/> the current <paramref name="queryString"/> should be an equivalent of</param>
        [Theory]
        [MemberData(nameof(QueryStringToFilterCases))]
        public void ToFilter(string queryString, Expression<Func<IDataFilter, bool>> resultExpression)
        {
            _output.WriteLine($"Input : {queryString}");
            _output.WriteLine($"Reference expression : {resultExpression}");

            // Act
            IDataFilter filter = queryString.ToFilter<SuperHero>();
            _output.WriteLine($"Filter : {filter}");
            // Assert
            filter.Should()
                .Match(resultExpression);


        }


        [Theory]
        [InlineData(Contains, " ")]
        [InlineData(EndsWith, "")]
        [InlineData(EqualTo, "")]
        [InlineData(GreaterThan, "")]
        [InlineData(GreaterThanOrEqual, "")]
        [InlineData(IsEmpty, "")]
        [InlineData(IsNotEmpty, "")]
        [InlineData(IsNull, "")]
        [InlineData(IsNotNull, "")]
        [InlineData(LessThan, " ")]
        [InlineData(LessThanOrEqualTo, " ")]
        [InlineData(NotEqualTo, " ")]
        [InlineData(StartsWith, " ")]
        public void FilterIsConvertedToAlwaysTrueExpressionWhenFieldIsNull(DataFilterOperator @operator, object value)
        {

            // Arrange
            IEnumerable<SuperHero> superHeroes = new []
            {
                new SuperHero { Firstname = "Bruce", Lastname = "Wayne" },
                new SuperHero { Firstname = "Dick", Lastname = "Grayson" },
                new SuperHero { Firstname = "Diana", Lastname = "Price" },
            };
            IDataFilter filter = new DataFilter(field: null, @operator: @operator, value: value);

            // Act
            Expression<Func<SuperHero, bool>> actualExpression = filter.ToExpression<SuperHero>();
            IEnumerable<SuperHero> superHeroesFiltered = superHeroes.Where(actualExpression.Compile());

            // Assert
            superHeroesFiltered.Should()
                .HaveSameCount(superHeroes).And
                .OnlyContain(x => superHeroes.Once(sh => x.Firstname == sh.Firstname && x.Lastname == sh.Lastname));
        }
    }

}
