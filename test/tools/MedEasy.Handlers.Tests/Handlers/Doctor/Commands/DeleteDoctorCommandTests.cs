using Moq;
using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using MedEasy.Commands.Doctor;
using Xunit;
using System.Collections.Generic;
using FluentAssertions;

namespace MedEasy.BLL.Tests.Commands.Doctor
{
    public class DeleteDoctorCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;


        public DeleteDoctorCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

        }

        public static IEnumerable<object[]> CtorInvalidCases {
            get
            {
                yield return new object[] { null };
            }
        }
        
        
        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}
