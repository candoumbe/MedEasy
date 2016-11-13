using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static MedEasy.DTO.ChangeInfoType;

namespace MedEasy.DTO.Tests
{
    public class ChangeInfoTests
    {


        public static IEnumerable<object> EquatableCases
        {
            get
            {
                yield return new object[]
                {
                    new ChangeInfo(),
                    new ChangeInfo(),
                    true
                };

                yield return new object[]
                {
                    new ChangeInfo(),
                    null,
                    false
                };


                yield return new object[]
                {
                    new ChangeInfo { Op = Update, Path = $"/{nameof(PatientInfo.MainDoctorId)}", Value = "" },
                    new ChangeInfo { Op = Update, Path = $"/{nameof(PatientInfo.MainDoctorId)}", Value = "" },
                    true
                };
            }
        }

        [Theory]
        [MemberData(nameof(EquatableCases))]
        public void EqualsIOfIEquatable(ChangeInfo first, ChangeInfo second, bool expectedResult)
        {
            (first.Equals(second)).Should().Be(expectedResult);
        }


        public static IEnumerable<object> ObjectEqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new ChangeInfo(),
                    new ChangeInfo(),
                    true
                };

                yield return new object[]
                {
                    new ChangeInfo(),
                    null,
                    false
                };

                yield return new object[]
                {
                    new ChangeInfo { Op = Update, Path = $"/{nameof(PatientInfo.MainDoctorId)}", Value = "" },
                    new ChangeInfo { Op = Update, Path = $"/{nameof(PatientInfo.MainDoctorId)}", Value = "" },
                    true
                };
            }
        }

        [Theory]
        [MemberData(nameof(ObjectEqualsCases))]
        public void ObjectEquals(ChangeInfo first, ChangeInfo second, bool expectedResult)
        {
            Equals(first, second).Should().Be(expectedResult);
        }
    }
}
