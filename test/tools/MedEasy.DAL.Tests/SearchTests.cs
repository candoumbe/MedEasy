﻿namespace MedEasy.DAL.Tests
{
    using FluentAssertions;

    using MedEasy.DAL.Repositories;

    using System;

    using Xunit;

    public class SearchTests
    {
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

        /// <summary>
        /// Constructor builds a valid instance
        /// </summary>
        [Fact]
        public void CtorBuildValidInstance()
        {
            // Act
            Search<Henchman> search = new();

            // Assert
            search.Filter.Should()
                .BeNull();
            search.Sort.Should()
                .BeNull();
        }
    }
}
