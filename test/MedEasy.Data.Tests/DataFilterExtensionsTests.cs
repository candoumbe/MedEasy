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
                    new DataFilter { Field = nameof(SuperHero.Firstname), Operator = EqualTo, Value  = "Bruce"  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == "Bruce"))
                };


                yield return new object[]
                {
                    new[] {new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" }},
                    new DataFilter { Field = nameof(SuperHero.Firstname), Operator = EqualTo, Value  = "Bruce"  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == "Bruce"))
                };

                yield return new object[]
                {
                    new[] {new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }},
                    new DataFilter { Field = nameof(SuperHero.Firstname), Operator = EqualTo, Value  = "Bruce"  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == "Bruce"))
                };


                yield return new object[]
                {
                    new[] {new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190 }},
                    new DataFilter { Field = nameof(SuperHero.Height), Operator = EqualTo, Value  = 190  },
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
                            new DataFilter { Field = nameof(SuperHero.Nickname), Operator = EqualTo, Value  = "Batman" },
                            new DataFilter { Field = nameof(SuperHero.Nickname), Operator = EqualTo, Value  = "Superman" }
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
                            new DataFilter { Field = nameof(SuperHero.Firstname), Operator = Contains, Value  = "a" },
                            new DataCompositeFilter
                            {
                                Logic = Or,
                                Filters = new [] {
                                    new DataFilter { Field = nameof(SuperHero.Nickname), Operator = EqualTo, Value  = "Batman" },
                                    new DataFilter { Field = nameof(SuperHero.Nickname), Operator = EqualTo, Value  = "Superman" }
                                }
                            }
                        }
                    },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname.Contains("a") && (item.Nickname == "Batman" || item.Nickname == "Superman")))
                };

                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "Barry", Lastname = "Allen", Height = 190, Nickname = "Flash" }

                    },
                    new DataFilter (),
                    ((Expression<Func<SuperHero, bool>>)(item => true))
                };

            }

        }

        public static IEnumerable<object> NotEqualToTestCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<SuperHero>(),
                    new DataFilter { Field = nameof(SuperHero.Lastname), Operator = NotEqualTo, Value  = "Kent"  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname != "Kent"))
                };
                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                    },
                    new DataFilter { Field = nameof(SuperHero.Lastname), Operator = NotEqualTo, Value  = "Kent"  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname != "Kent"))
                };

                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Bruce", Lastname = "Wayne", Height = 190, Nickname = "Batman" },
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" }
                    },
                    new DataFilter { Field = nameof(SuperHero.Lastname), Operator = NotEqualTo, Value  = "Kent"  },
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
                    new DataFilter { Field = $"{nameof(SuperHero.Henchman)}.{nameof(Henchman.Firstname)}", Operator = NotEqualTo, Value  = "Dick"  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Henchman.Firstname != "Dick"))
                };

            }
        }

        public static IEnumerable<object> IsEmptyTestCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<SuperHero>(),
                    new DataFilter { Field = nameof(SuperHero.Lastname), Operator = IsEmpty  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Lastname == string.Empty))
                };


                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = "", Lastname = "", Height = 178, Nickname = "Sinestro" }

                    },
                    new DataFilter { Field = nameof(SuperHero.Lastname), Operator = IsEmpty  },
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
                    new DataFilter { Field = $"{nameof(SuperHero.Henchman)}.{nameof(Henchman.Firstname)}", Operator = IsEmpty   },
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
                    new DataFilter { Field = $"{nameof(SuperHero.Henchman)}.{nameof(Henchman.Firstname)}", Operator = NotEqualTo, Value  = "Dick"  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Henchman.Firstname != "Dick"))
                };

            }
        }

        public static IEnumerable<object> IsNullTestCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<SuperHero>(),
                    new DataFilter { Field = nameof(SuperHero.Firstname), Operator = IsNull  },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == null))
                };

                yield return new object[]
                {
                    new[] {
                        new SuperHero { Firstname = "Clark", Lastname = "Kent", Height = 190, Nickname = "Superman" },
                        new SuperHero { Firstname = null, Lastname = "", Height = 178, Nickname = "Sinestro" }

                    },
                    new DataFilter { Field = nameof(SuperHero.Firstname), Operator = IsNull },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Firstname == null)),
                };
            }
        }

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
                    new DataFilter { Field = nameof(SuperHero.Height), Operator = LessThan,  Value = 150 },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Height <  150)),
                };


            }
        }

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
                    new DataFilter { Field = nameof(SuperHero.Nickname), Operator = StartsWith, Value  = "B" },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Nickname.StartsWith("B")))
                };
            }

        }

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
                    new DataFilter { Field = nameof(SuperHero.Nickname), Operator = EndsWith, Value  = "n" },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Nickname.EndsWith("n")))
                };
            }

        }

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
                    new DataFilter { Field = nameof(SuperHero.Nickname), Operator = Contains, Value  = "an" },
                    ((Expression<Func<SuperHero, bool>>)(item => item.Nickname.Contains("n")))
                };
            }

        }

        [Theory]
        [MemberData(nameof(EqualToTestCases))]
        public void BuildEqual(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

        [Theory]
        [MemberData(nameof(IsNullTestCases))]
        public void BuildIsNull(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);


        [Theory]
        [MemberData(nameof(IsEmptyTestCases))]
        public void BuildIsEmpty(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);


        [Theory]
        [MemberData(nameof(StartsWithCases))]
        public void BuildStartsWith(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

        [Theory]
        [MemberData(nameof(EndsWithCases))]
        public void BuildEndsWith(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);

        [Theory]
        [MemberData(nameof(ContainsCases))]
        public void BuildContains(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);



        [Theory]
        [MemberData(nameof(LessThanTestCases))]
        public void BuildLessThan(IEnumerable<SuperHero> superheroes, IDataFilter filter, Expression<Func<SuperHero, bool>> expression)
            => Build(superheroes, filter, expression);


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
                    "Firstname=!!Bruce",
                    ((Expression<Func<IDataFilter, bool>>)(x => x is DataFilter &&
                        ((DataFilter)x).Field == "Firstname" &&
                        ((DataFilter)x).Operator == EqualTo &&
                            Equals(((DataFilter)x).Value, "Bruce")
                        ))
                };

                yield return new object[]
                {
                    "Firstname=Bruce,Dick",
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
            _output.WriteLine($"Reference expression : {resultExpression.Body.ToString()}");

            // Act
            IDataFilter filter = queryString.ToFilter<SuperHero>();

            // Assert
            filter.Should()
                .Match(resultExpression);


        }

    }

}
