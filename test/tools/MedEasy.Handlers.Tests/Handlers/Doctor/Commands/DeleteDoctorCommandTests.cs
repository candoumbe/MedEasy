using Moq;
using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using MedEasy.Commands.Doctor;

namespace MedEasy.BLL.Tests.Commands.Doctor
{
    public class DeleteDoctorCommandTests : IDisposable
    {

        private Mock<ILogger<DeleteDoctorByIdCommand>> _loggerMock;

        private ITestOutputHelper _outputHelper;
        

        public DeleteDoctorCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

        }


        




       
        public void Dispose()
        {
            _outputHelper = null;
            _loggerMock = null;
        }
    }
}
