using FluentAssertions;
using Measures.CQRS.Queries.BloodPressures;
using Measures.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using Optional;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Measures.CQRS.UnitTests.Queries.BloodPressures
{
    public class GetPageOfBloodPressuresByPatientIdQueryTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public GetPageOfBloodPressuresByPatientIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public static IEnumerable<object[]> CtorWithIndividualParameterCases
        {
            get
            {
                yield return new object[]
                {
                    Guid.Empty, null,
                    $"Guid.Empty is not valid for patientId and null is not valid for pagination arguments"
                };

                yield return new object[]
                {
                    Guid.NewGuid(), null,
                    $"null not a valid argument for patientId"
                };

                yield return new object[]
                {
                    Guid.Empty, new PaginationConfiguration(),
                    $"Guid.Empty is not valid for patientId"
                };
            }
        }

        [Theory]
        [MemberData(nameof(CtorWithIndividualParameterCases))]
        public void GivenDefaultParameters_Ctor_ThrowsArgumentOutOfRangeException(Guid patientId, PaginationConfiguration pagination, string reason)
        {
            // Act
            Action action = () => new GetPageOfBloodPressureInfoByPatientIdQuery(patientId, pagination);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>(reason).Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> CtorWithTupleCases
        {
            get
            {
                yield return new object[]
                {
                    default((Guid, PaginationConfiguration)),
                    $"{default((Guid, PaginationConfiguration))} is not a valid argument"
                };

                yield return new object[]
                {
                    new ValueTuple<Guid, PaginationConfiguration>(Guid.Empty, null),
                    $"<default((Guid, PaginationConfiguration))> is not a valid argument"
                };
            }
        }

        [Theory]
        [MemberData(nameof(CtorWithTupleCases))]
        public void GivenDefaultParameters_CtorWithTuple_ThrowsArgumentOutOfRangeException((Guid, PaginationConfiguration) data, string reason)
        {
            _outputHelper.WriteLine($"{nameof(data)} : <{data}>");

            // Act
            Action action = () => new GetPageOfBloodPressureInfoByPatientIdQuery(data);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>(reason).Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GivenSameData_TwoInstances_Have_DifferentIds()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            PaginationConfiguration pagination = new PaginationConfiguration();

            (Guid, PaginationConfiguration) data = (patientId, pagination);

            // Act
            GetPageOfBloodPressureInfoByPatientIdQuery first = new GetPageOfBloodPressureInfoByPatientIdQuery(data);
            GetPageOfBloodPressureInfoByPatientIdQuery second = new GetPageOfBloodPressureInfoByPatientIdQuery(data);

            first.Id.Should()
                .NotBeEmpty();
            second.Id.Should()
                .NotBeEmpty();

            first.Should()
                .NotBeSameAs(second);
            

        }
    }
}
