using Moq;
using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using MedEasy.Commands.Appointment;

namespace MedEasy.BLL.Tests.Commands.Appointment
{
    public class DeleteAppointmentCommandTests : IDisposable
    {

        private Mock<ILogger<DeleteAppointmentByIdCommand>> _loggerMock;

        private ITestOutputHelper _outputHelper;
        

        public DeleteAppointmentCommandTests(ITestOutputHelper outputHelper)
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
