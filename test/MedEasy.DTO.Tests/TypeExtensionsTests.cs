using FluentAssertions;
using MedEasy.RestObjects;
using System;
using System.Linq;
using Xunit;

namespace MedEasy.DTO.Extensions.Tests
{
    public class TypeExtensionsTests
    {

        public TypeExtensionsTests()
        {

        }


        [Fact]
        public void CreatePatientInfo_ToForm()
        {
            // Act
            Form f = typeof(CreatePatientInfo).ToForm(new Link { Method = "POST", Href = "url/of/the/form", Relation="create-form" });

            // Assert
            
            f.Items.Should()
                .Contain(x => x.Name == nameof(CreatePatientInfo.Firstname)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.Lastname)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.BirthPlace)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.MainDoctorId)).And
                .Contain(x => x.Name == nameof(CreatePatientInfo.BirthDate));


            FormField firstnameField = f.Items.Single(x => x.Name == nameof(CreatePatientInfo.Firstname));
            firstnameField.Name.Should().Be(firstnameField.Name);
            firstnameField.Label.Should().Be(firstnameField.Name);
            firstnameField.Type.Should().Be(FormFieldType.String);
            firstnameField.Placeholder.Should().BeNull();
            firstnameField.Secret.Should().BeNull();
            firstnameField.MaxLength.Should().Be(255);
            firstnameField.Pattern.Should().BeNull();
            firstnameField.Required.Should().BeNull();

            FormField lastnameField = f.Items.Single(x => x.Name == nameof(CreatePatientInfo.Lastname));
            lastnameField.Name.Should().Be(lastnameField.Name);
            lastnameField.Label.Should().Be(lastnameField.Name);
            lastnameField.Type.Should().Be(FormFieldType.String);
            lastnameField.Placeholder.Should().BeNull();
            lastnameField.Secret.Should().BeNull();
            lastnameField.MaxLength.Should().Be(255);
            lastnameField.Pattern.Should().BeNull();
            lastnameField.Required.Should().BeTrue();

            FormField birthPlaceField = f.Items.Single(x => x.Name == nameof(CreatePatientInfo.BirthPlace));
            birthPlaceField.Name.Should().Be(birthPlaceField.Name);
            birthPlaceField.Label.Should().Be(birthPlaceField.Name);
            birthPlaceField.Type.Should().Be(FormFieldType.String);
            birthPlaceField.Placeholder.Should().BeNull();
            birthPlaceField.Secret.Should().BeNull();
            birthPlaceField.MaxLength.Should().Be(255);
            birthPlaceField.Pattern.Should().BeNull();
            birthPlaceField.Required.Should().BeNull();

            FormField birthDateField = f.Items.Single(x => x.Name == nameof(CreatePatientInfo.BirthDate));
            birthDateField.Name.Should().Be(birthDateField.Name);
            birthDateField.Label.Should().Be(birthDateField.Name);
            birthDateField.Type.Should().Be(FormFieldType.Date);
            birthDateField.Placeholder.Should().BeNull();
            birthDateField.Secret.Should().BeNull();
            birthDateField.MaxLength.Should().BeNull();
            birthDateField.Pattern.Should().BeNull();
            birthDateField.Required.Should().BeNull();

            FormField mainDoctorIdField = f.Items.Single(x => x.Name == nameof(CreatePatientInfo.MainDoctorId));
            mainDoctorIdField.Name.Should().Be(mainDoctorIdField.Name);
            mainDoctorIdField.Label.Should().Be(mainDoctorIdField.Name);
            mainDoctorIdField.Type.Should().Be(FormFieldType.String);
            mainDoctorIdField.Placeholder.Should().BeNull();
            mainDoctorIdField.Secret.Should().BeNull();
            mainDoctorIdField.MaxLength.Should().BeNull();
            mainDoctorIdField.Pattern.Should().BeNull();
            mainDoctorIdField.Required.Should().BeNull();


        }


        
    }


}
