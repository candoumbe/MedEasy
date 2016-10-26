using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace MedEasy.Services.Tests
{
    public class PrescriptionServicesTests : IDisposable
    {
        private Mock<IUnitOfWorkFactory> _factoryMock;
        private Mock<ILogger<PrescriptionService>> _loggerMock;
        private ITestOutputHelper _outputHelper;
        private PrescriptionService _service;

        public PrescriptionServicesTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            _loggerMock = new Mock<ILogger<PrescriptionService>>(Strict);

            _factoryMock = new Mock<IUnitOfWorkFactory>(Strict);
            _factoryMock.Setup(mock => mock.New().Dispose());

            _service = new PrescriptionService();
        }

        public void Dispose()
        {
            _outputHelper = null;

            _loggerMock = null;

            _factoryMock = null;

            _service = null;
        }
    }
}
