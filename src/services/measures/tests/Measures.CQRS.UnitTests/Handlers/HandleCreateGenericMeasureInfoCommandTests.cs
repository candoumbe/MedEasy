using AutoMapper.QueryableExtensions;

using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

using FluentAssertions;
using FluentAssertions.Extensions;

using Measures.Context;
using Measures.CQRS.Commands;
using Measures.CQRS.Handlers;
using Measures.DTO;
using Measures.Mapping;
using Measures.Objects;

using MedEasy.Abstractions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using Optional;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace Measures.CQRS.UnitTests.Handlers
{
    public class HandleCreateGenericMeasureInfoCommandTests : IAsyncLifetime
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly Mock<IExpressionBuilder> _expressionBuilderMock;
        private readonly Mock<ILogger<HandleCreateGenericMeasureInfoCommand>> _loggerMock;
        private HandleCreateGenericMeasureInfoCommand _sut;

        public HandleCreateGenericMeasureInfoCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MeasuresContext>()
                    .UseInMemoryDatabase($"{Guid.NewGuid()}")
                    .EnableDetailedErrors();

            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbContextOptionsBuilder.Options, options => new MeasuresContext(options));

            _expressionBuilderMock = new Mock<IExpressionBuilder>(Strict);
            _loggerMock = new Mock<ILogger<HandleCreateGenericMeasureInfoCommand>>(Strict);
            _loggerMock.Setup(mock => mock.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));

            _sut = new HandleCreateGenericMeasureInfoCommand(logger: _loggerMock.Object, uowFactory: _uowFactory, expressionBuilder: _expressionBuilderMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Patient>().Clear();
            uow.Repository<MeasureForm>().Clear();

            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);
        }

        [Fact]
        public void Is_valid_handler()
        {
            // Act
            Type handlerType = typeof(HandleCreateGenericMeasureInfoCommand);

            // Assert
            handlerType.Should()
                       .Implement<IRequestHandler<CreateGenericMeasureInfoCommand, Option<GenericMeasureInfo, CreateCommandResult>>>().And
                       .NotHaveDefaultConstructor().And
                       .HaveConstructor(new[] { typeof(ILogger<HandleCreateGenericMeasureInfoCommand>), typeof(IUnitOfWorkFactory), typeof(IExpressionBuilder) });
        }

        [Fact]
        public async Task Handle_creates_the_measure()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            Patient patient = new Patient(patientId, "John Doe");

            Guid formId = Guid.NewGuid();
            CreateGenericMeasureInfo createGenericMeasureInfo = new CreateGenericMeasureInfo
            {
                PatientId = patient.Id,
                DateOfMeasure = 12.May(2017).Add(13.Hours()),
                FormId = formId,
                Data = new Dictionary<string, object>
                {
                    ["systolic"] = 10f,
                    ["diastolic"] = 30f
                }
            };

            MeasureForm form = new MeasureForm(formId, "blood pressure");
            form.AddFloatField("systolic", min: 0, max: 30);
            form.AddFloatField("diastolic", min: 0, max: 30);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<MeasureForm>().Create(form);
                uow.Repository<Patient>().Create(patient);

                await uow.SaveChangesAsync()
                         .ConfigureAwait(false);
            }

            CreateGenericMeasureInfoCommand cmd = new CreateGenericMeasureInfoCommand(createGenericMeasureInfo);

            _expressionBuilderMock.Setup(mock => mock.GetMapExpression(It.IsAny<Type>(), It.IsAny<Type>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<MemberInfo[]>()))
                                  .Returns((Type sourceType, Type destinationType, IDictionary<string, object> parameters, MemberInfo[] membersToExpand) => AutoMapperConfig.Build().ExpressionBuilder.GetMapExpression(sourceType, destinationType, parameters, membersToExpand));

            // Act
            Option<GenericMeasureInfo, CreateCommandResult> maybeMeasure = await _sut.Handle(cmd, default)
                                                                                     .ConfigureAwait(false);

            // Assert
            maybeMeasure.HasValue.Should()
                                 .BeTrue("The measure was successfully created in the underlying store");

            maybeMeasure.MatchSome(
                async measure =>
                {
                    measure.Id.Should()
                              .NotBeEmpty();
                    measure.PatientId.Should()
                                     .Be(patientId);
                    measure.DateOfMeasure.Should()
                                         .Be(createGenericMeasureInfo.DateOfMeasure);
                    measure.FormId.Should()
                                  .Be(formId);
                    JsonElement root = measure.Data.RootElement;
                    root.GetProperty("systolic").GetDecimal().Should()
                                                             .Be(10);
                    root.GetProperty("diastolic").GetDecimal().Should()
                                                              .Be(20);

                    patient.Measures.Should()
                           .Contain(m => m.Id == measure.Id);

                    using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
                    bool measureExists = await uow.Repository<GenericMeasure>()
                                                  .AnyAsync(x => x.Id == measure.Id)
                                                  .ConfigureAwait(false);

                    measureExists.Should()
                                 .BeTrue();
                });
        }
    }
}