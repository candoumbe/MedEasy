using Moq;
using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using MedEasy.Commands.Appointment;

namespace MedEasy.Handlers.Tests.Commands.Appointment
{
    public class DeleteAppointmentCommandTests : IDisposable
    {

        private ITestOutputHelper _outputHelper;
        

        public DeleteAppointmentCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

        }


        




       
        public void Dispose()
        {
            _outputHelper = null;
            
        }
    }
}
