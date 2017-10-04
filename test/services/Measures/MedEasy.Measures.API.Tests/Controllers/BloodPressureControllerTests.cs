using System;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Measures.API.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="BloodPressuresController"/>
    /// </summary>
    public class BloodPressureControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public BloodPressureControllerTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

        public void Dispose() => _outputHelper = null;



        
    }
}
