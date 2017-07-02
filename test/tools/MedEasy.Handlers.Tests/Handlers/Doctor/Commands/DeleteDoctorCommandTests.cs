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

        /// <summary>
        /// Tests that building an instance of <see cref="DeleteDoctorByIdCommand"/> with <see cref="Guid.Empty"/>
        /// is not valid
        /// </summary>
        [Fact]
        public void Ctor_With_Empty_Guid_Throws_Exception()
        {
            Action action = () => new DeleteDoctorByIdCommand(Guid.Empty);


            action.ShouldThrow<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        




       
        public void Dispose()
        {
            _outputHelper = null;
        }
    }
}
