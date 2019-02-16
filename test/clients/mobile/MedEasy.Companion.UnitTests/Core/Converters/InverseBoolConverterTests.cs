using FluentAssertions;
using MedEasy.Mobile.Core.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xunit;
using Xunit.Categories;

namespace MedEasy.Mobile.UnitTests.Core.Converters
{
    [UnitTest]
    [Feature(nameof(Mobile.Core.Converters))]
    public class InverseBoolConverterTests
    {
        private InverseBoolConverter _sut;

        public InverseBoolConverterTests()
        {
            _sut = new InverseBoolConverter();
        }

        [Fact]
        public void IsValueConverter() => typeof(InverseBoolConverter).Should()
            .Implement<IValueConverter>();


        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Convert(bool input, bool expectedOutput)
        {
            // Act
            bool actualOutput = (bool)_sut.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);

            // Assert
            actualOutput.Should()
                .Be(expectedOutput);
        }

        [Fact]
        public void ConvertBackThrowArgumentOutOfRange_If_TargetType_IsNotBool()
        {
            Action action = () => _sut.ConvertBack(false, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("Target type must be bool");
        }

        [Fact]
        public void Convert_ThrowsArgumentOutOfRange_If_TargetType_IsNotBool()
        {
            Action action = () => _sut.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            action.Should()
                .ThrowExactly<ArgumentOutOfRangeException>("Target type must be bool");
        }
    }
}
