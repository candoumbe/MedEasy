using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace MedEasy.Commands.Tests.Patient
{
    public class AddNewTemperatureMeasureCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;

        public AddNewTemperatureMeasureCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }






        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}
