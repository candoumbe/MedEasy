﻿using MedEasy.DTO;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Handlers.Patient.Queries;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace MedEasy.Services.Tests
{
    public class PhysiologicalMeasureServiceTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo>> _iRunAddNewBloodPressureCommandMock;
        private Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo>> _iRunAddNewTemperatureCommandMock;
        private Mock<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>> _iHandleGetOnePatientTemperatureMock;
        private Mock<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>> _iHandleGetOnePatientBloodPressureMock;
        private Mock<IHandleGetMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo>> _iHandleGetLastBloodPressuresMock;
        private Mock<IHandleGetMostRecentPhysiologicalMeasuresQuery<TemperatureInfo>> _iHandleGetLastTemperaturesMock;
        private PhysiologicalMeasureService _physiologicalMeasureService;

        public PhysiologicalMeasureServiceTests(ITestOutputHelper outputHelper)
        {

            _outputHelper = outputHelper;
            
            _iRunAddNewTemperatureCommandMock = new Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo>>(Strict);
            _iRunAddNewBloodPressureCommandMock = new Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo>>(Strict);
            _iHandleGetOnePatientTemperatureMock = new Mock<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>>(Strict);
            _iHandleGetOnePatientBloodPressureMock = new Mock<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>>(Strict);
            _iHandleGetLastBloodPressuresMock = new Mock<IHandleGetMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo>>(Strict);
            _iHandleGetLastTemperaturesMock = new Mock<IHandleGetMostRecentPhysiologicalMeasuresQuery<TemperatureInfo>>(Strict);

            _physiologicalMeasureService = new PhysiologicalMeasureService(
                _iRunAddNewTemperatureCommandMock.Object,
                _iRunAddNewBloodPressureCommandMock.Object,
                _iHandleGetOnePatientTemperatureMock.Object,
                _iHandleGetOnePatientBloodPressureMock.Object,
                _iHandleGetLastBloodPressuresMock.Object,
                _iHandleGetLastTemperaturesMock.Object
                );
        }


        // TODO Write all tests that certifies that this service  is only a facade !!!
        

        public void Dispose()
        {
            _iHandleGetLastBloodPressuresMock = null;
            _iHandleGetLastTemperaturesMock = null;
            _iHandleGetOnePatientBloodPressureMock = null;
            _iHandleGetOnePatientTemperatureMock = null;
            _iRunAddNewBloodPressureCommandMock = null;
            _iRunAddNewTemperatureCommandMock = null;

            _physiologicalMeasureService = null;
        }
    }
}
