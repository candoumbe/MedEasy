using System;
using Xunit.Abstractions;

namespace MedEasy.Queries.Tests.Patient
{
    public class GetPatientCivilStatusByIdQueryTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public GetPatientCivilStatusByIdQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}
