﻿using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Moq;
using Microsoft.Extensions.Logging;
using static Moq.MockBehavior;
using System.Threading.Tasks;
using Xunit;
using static Newtonsoft.Json.JsonConvert;
using System.Linq.Expressions;
using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Specialty.Queries;
using MedEasy.Queries.Specialty;
using System.Threading;

namespace MedEasy.Handlers.Tests.Specialty.Queries
{
    public class HandleIsNameAvailableForNewSpecialtyQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private Mock<ILogger<HandleIsNameAvailableForNewSpecialtyQuery>> _loggerMock;
        private HandleIsNameAvailableForNewSpecialtyQuery _handler;

        public HandleIsNameAvailableForNewSpecialtyQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _unitOfWorkFactoryMock.Setup(mock => mock.New().Dispose());

            _loggerMock = new Mock<ILogger<HandleIsNameAvailableForNewSpecialtyQuery>>(Strict);

            _handler = new HandleIsNameAvailableForNewSpecialtyQuery(_unitOfWorkFactoryMock.Object, _loggerMock.Object);
        }



        public static IEnumerable<object[]> IsNameAvailaibleCases

        {
            get
            {
                yield return new object[]
                {
                   Enumerable.Empty<Objects.Specialty>(),
                   "SPEC-01",
                   true
                };

                yield return new object[]
               {
                   Enumerable.Empty<Objects.Specialty>(),
                   "SPEC-01 ",
                   true
               };

                yield return new object[]
                {
                   Enumerable.Empty<Objects.Specialty>(),
                   "  SPEC-01",
                   true
                };


                yield return new object[]
                {
                   Enumerable.Empty<Objects.Specialty>(),
                   " SPEC-01 ",
                   true
                };

                yield return new object[]
                {
                   new [] {
                       new Objects.Specialty { Name = "SPEC-01" }
                   },
                   "SPEC-01",
                   false
                };

                yield return new object[]
                {
                   new [] {
                       new Objects.Specialty { Name = "SPEC-01" }
                   },
                   "SPEC-01",
                   false
                };

                yield return new object[]
                {
                   new [] {
                       new Objects.Specialty { Name = "SPEC-01" }
                   },
                   " SPEC-01",
                   false
                };

                yield return new object[]
                {
                   new [] {
                       new Objects.Specialty { Name = "SPEC-01" }
                   },
                   "SPEC-01 ",
                   false
                };

                yield return new object[]
                {
                   new [] {
                       new Objects.Specialty { Name = "SPEC-01" }
                   },
                   " SPEC-01 ",
                   false
                };
                yield return new object[]
                {
                   new [] {
                       new Objects.Specialty { Name = "SPEC-01" }
                   },
                   " spec-01 ",
                   false
                };
            }
        }

        public static IEnumerable<object[]> CtorCases {
            get
            {
                yield return new object[] { null, null };
                yield return new object[] { Mock.Of<IUnitOfWorkFactory>(), null };
                yield return new object[] { null, Mock.Of<ILogger<HandleIsNameAvailableForNewSpecialtyQuery>>() };
            }
        }

        [Theory]
        [MemberData(nameof(CtorCases))]
        public void CtorShouldThrowArgumentNullExcption(IUnitOfWorkFactory factory, ILogger<HandleIsNameAvailableForNewSpecialtyQuery> logger)
        {
            Action action = () => new HandleIsNameAvailableForNewSpecialtyQuery(factory, logger);
            action.ShouldThrow<ArgumentNullException>()
                .Where(x => x.ParamName != null, "paramName must always be set in order to debug easily");
        }

        [Theory]
        [MemberData(nameof(IsNameAvailaibleCases))]
        public async Task HandleAsync(IEnumerable<Objects.Specialty> specialtiesInStore, string codeToSearch, bool expectedAvailabilty)
        {
            _outputHelper.WriteLine($"Testing {nameof(HandleIsNameAvailableForNewSpecialtyQuery)}.{nameof(HandleIsNameAvailableForNewSpecialtyQuery.HandleAsync)} {Environment.NewLine}");
            _outputHelper.WriteLine($"Code to test : '{codeToSearch}'");
            _outputHelper.WriteLine($"Specialty store content : '{SerializeObject(specialtiesInStore)}");

            // Arrange

            _unitOfWorkFactoryMock.Setup(mock => mock.New().Repository<Objects.Specialty>().AnyAsync(It.IsAny<Expression<Func<Objects.Specialty, bool>>>(), It.IsAny<CancellationToken>()))
                .Returns((Expression<Func<Objects.Specialty, bool>> filter, CancellationToken cancellationToken) => new ValueTask<bool>(specialtiesInStore.Any(filter.Compile())));

            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()));
            
            // Act
            bool currentResult = await _handler.HandleAsync(new IsNameAvailableForNewSpecialtyQuery(codeToSearch));

            // Assert

            currentResult.Should().Be(expectedAvailabilty);

            _unitOfWorkFactoryMock.Verify();
            _loggerMock.Verify(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()), Times.AtLeast(2));
        }


        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactoryMock = null;
            _loggerMock = null;
        }
    }
}
