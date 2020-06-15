
using FluentAssertions;

using Forms;

using System;
using System.Linq;

using Xunit;
using Xunit.Categories;

namespace Measures.Objects.UnitTests
{
    [UnitTest]
    [Feature(nameof(Measures))]
    public class MeasureFormTests
    {
        [Fact]
        public void Ctor_throws_ArgumentNullException_when_name_is_null()
        {
            // Act
            Action ctor = () => new MeasureForm(Guid.NewGuid(), null);

            // Assert
            ctor.Should()
                .ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void AddFloatField_throws_ArgumentNullException_when_fieldname_is_null()
        {
            // Arrange
            MeasureForm mf = new MeasureForm(Guid.NewGuid(), "heart-beat");

            // Act
            Action addField = () => mf.AddFloatField(name: null, min: 0);

            // Assert
            addField.Should()
                    .ThrowExactly<ArgumentNullException>("name of the field cannot be null");
        }

        [Fact]
        public void AddFloatField_throws_InvalidOperationException_when_field_with_the_same_name_already_exists()
        {
            // Arrange
            MeasureForm mf = new MeasureForm(Guid.NewGuid(), "heart-beat");
            mf.AddFloatField(name: "value", min : 0);

            // Act
            Action addField = () => mf.AddFloatField(name: "value", min: 10);

            // Assert
            addField.Should()
                    .ThrowExactly<InvalidOperationException>("a field with the same name already exists");
        }

        [Fact]
        public void AddFloatField_adds_field_to_the_form()
        {
            // Arrange
            MeasureForm mf = new MeasureForm(Guid.NewGuid(), "a-measure");

            // Act
            mf.AddFloatField("a property", "A physiological measure", min: 0, max: 300);

            // Assert
            mf.Fields.Should()
                     .HaveCount(1);

            FormField field = mf.Fields.Single();
            field.Name.Should()
                      .Be("a-property", "The name of the for");
            field.Type.Should()
                      .Be(FormFieldType.Decimal);
            field.Min.Should()
                     .Be(0);
            field.Max.Should()
                     .Be(300);
            field.Description.Should()
                             .Be("A physiological measure");
        }

        [Fact]
        public void AddTextField_throws_ArgumentNullException_when_fieldname_is_null()
        {
            // Arrange
            MeasureForm mf = new MeasureForm(Guid.NewGuid(), "heart-beat");

            // Act
            Action addField = () => mf.AddTextField(name: null);

            // Assert
            addField.Should()
                    .ThrowExactly<ArgumentNullException>("name of the field cannot be null");
        }


        [Fact]
        public void AddTextField_adds_field_to_the_form()
        {
            // Arrange
            MeasureForm mf = new MeasureForm(Guid.NewGuid(), "a-measure");

            // Act
            mf.AddTextField("description", "Additional comments");

            // Assert
            mf.Fields.Should()
                     .HaveCount(1);

            FormField field = mf.Fields.Single();
            field.Name.Should()
                      .Be("description", "The name of the for");
            field.Type.Should()
                      .Be(FormFieldType.String);
            field.Min.Should()
                     .BeNull();
            field.Max.Should()
                     .BeNull();
            field.Description.Should()
                             .Be("Additional comments");
        }

        [Fact]
        public void AddTextField_throws_InvalidOperationException_when_field_with_the_same_name_already_exists()
        {
            // Arrange
            MeasureForm mf = new MeasureForm(Guid.NewGuid(), "heart-beat");
            mf.AddTextField(name: "value");

            // Act
            Action addField = () => mf.AddTextField(name: "value");

            // Assert
            addField.Should()
                    .ThrowExactly<InvalidOperationException>("a field with the same name already exists");
        }

        [Fact]
        public void RemoveField_throws_ArgumentNullException_when_name_is_null()
        {
            // Arrange
            MeasureForm mf = new MeasureForm(Guid.NewGuid(), "a-measure");

            // Act
            Action removeField = () => mf.RemoveField(name: null);

            // Assert
            removeField.Should()
                       .ThrowExactly<ArgumentNullException>("The name of the field to delete cannot be null");
        }

        [Fact]
        public void RemoveField_removes_the_specified_field()
        {
            // Arrange
            MeasureForm mf = new MeasureForm(Guid.NewGuid(), "heartbeat");
            mf.AddFloatField("value", min: 0);

            // Act
            mf.RemoveField("value");

            // Assert
            mf.Fields.Should()
                     .NotContain(field => field.Name == "value");
        }
    }
}
