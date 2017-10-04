using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Moq;
using static Moq.MockBehavior;
using MedEasy.Validators;
using AutoMapper;
using Xunit;
using FluentAssertions;
using MedEasy.Commands.Specialty;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MedEasy.Handlers.Specialty.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using System.Linq;
using MedEasy.Mapping;
using System.Linq.Expressions;
using MedEasy.Handlers.Core.Specialty.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;
using Optional;

namespace MedEasy.Handlers.Tests.Handlers.Commands.Specialty
{
    public class HandleCreateSpecialtyCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private RunCreateSpecialtyCommand _runner;

        private IMapper _mapper;

        public HandleCreateSpecialtyCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _mapper = AutoMapperConfig.Build().CreateMapper();
            DbContextOptionsBuilder<MedEasyContext> dbOptionsBuiler = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptionsBuiler.UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(dbOptionsBuiler.Options);

            _runner = new RunCreateSpecialtyCommand(_unitOfWorkFactory, _mapper.ConfigurationProvider.ExpressionBuilder);
        }

        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {
                yield return new object[]
                {
                    null,
                    null
                };

                yield return new object[]
                {
                    Mock.Of<IUnitOfWorkFactory>(),
                    null
                };

                yield return new object[]
                {
                    null,
                    Mock.Of<IExpressionBuilder>()
                };

            }
        }

        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IUnitOfWorkFactory factory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"Unit of work factory  == null : {factory == null}");
            _outputHelper.WriteLine($"expression builder == null : {expressionBuilder == null}");

            // Act
            Action action = () => new RunCreateSpecialtyCommand(factory, expressionBuilder);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        
        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            // Act
            Func<Task> action = async () => await _runner.RunAsync(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ShouldCreateTheResource()
        {
            // Arrange
            CreateSpecialtyInfo info = new CreateSpecialtyInfo
            {
                Name = "Médecine générale"
            };

            // Act
            CreateSpecialtyCommand cmd = new CreateSpecialtyCommand(info);
            Option<SpecialtyInfo, CommandException> result = await _runner.RunAsync(cmd)
                .ConfigureAwait(false);

            // Assert
            result.HasValue.Should().BeTrue();
            result.MatchSome(createdResource =>
            {
                createdResource.Name.Should().Be(info.Name);
            });
            

        }

        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _runner = null;
            _mapper = null;
        }
    }
}
