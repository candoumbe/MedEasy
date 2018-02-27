using FluentAssertions;
using FluentValidation.Results;
using MedEasy.RestObjects;
using System;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.Validators.Tests
{
    /// <summary>
    /// Unit tests for <see cref="PaginationConfiguration"/>
    /// </summary>
    public class PaginationConfigurationValidatorTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private PaginationConfigurationValidator _validator;

        public PaginationConfigurationValidatorTests(ITestOutputHelper outputHelper)
        {
            _validator = new PaginationConfigurationValidator();
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            _validator = null;
            _outputHelper = null;
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(int.MinValue, 10)]
        [InlineData(10, int.MinValue)]
        public void ValidateTest(int page, int pageSize)
        {
            
            // Arrange
            PaginationConfiguration pagination = new PaginationConfiguration
            {
                Page = page,
                PageSize = pageSize
            };

            _outputHelper.WriteLine($"Validation of {{{nameof(PaginationConfiguration.Page)} : {pagination.Page}, {nameof(PaginationConfiguration.PageSize)} : {pagination.PageSize}}}");

            // Act
            ValidationResult vr = _validator.Validate(pagination);
            
            // Assert
            vr.IsValid.Should().BeFalse();

        }
    }
}
