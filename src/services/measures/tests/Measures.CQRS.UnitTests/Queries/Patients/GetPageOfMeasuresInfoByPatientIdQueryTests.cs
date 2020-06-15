using FluentAssertions;

using Measures.CQRS.Queries.Patients;
using Measures.DTO;

using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;

using Optional;

using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace Measures.CQRS.UnitTests.Queries.Patients
{
    public class GetPageOfMeasuresInfoByPatientIdQueryTests
    {

        [Fact]
        public void Query_all_measures()
        {
            // Act
            Type queryType = typeof(GetPageOfMeasuresInfoByPatientIdQuery);

            // Assert
            queryType.Should()
                     .BeDerivedFrom<QueryBase<Guid, (Guid, string, PaginationConfiguration), Option<Page<GenericMeasureInfo>>>>("it's a query");

        }

    }
}
