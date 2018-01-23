using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace MedEasy.RestObjects.Tests
{
    public class TypeExtensionsTests
    {

        private abstract class Person
        {

            public string Firstname { get; set; }

            public string Lastname { get; set; }
        }
        private class CreateHenchman : Person
        {
            public string BirthPlace { get; set; }

            public int? MinionId { get; set; }

            public DateTime? BirthDate { get; set; }
        }

        [Fact]
        public void CreateHenchman_ToForm()
        {
            // Act
            Form f = typeof(CreateHenchman).ToForm(new Link { Method = "POST", Href = "url/of/the/form", Relation = "create-form" });

            // Assert

            f.Items.Should()
                .Contain(x => x.Name == nameof(CreateHenchman.Firstname)).And
                .Contain(x => x.Name == nameof(CreateHenchman.Lastname)).And
                .Contain(x => x.Name == nameof(CreateHenchman.BirthPlace)).And
                .Contain(x => x.Name == nameof(CreateHenchman.MinionId)).And
                .Contain(x => x.Name == nameof(CreateHenchman.BirthDate));


            FormField firstnameField = f.Items.Single(x => x.Name == nameof(CreateHenchman.Firstname));
            firstnameField.Name.Should().Be(firstnameField.Name);
            firstnameField.Label.Should().Be(firstnameField.Name);
            firstnameField.Type.Should().Be(FormFieldType.String);
            firstnameField.Placeholder.Should().BeNull();
            firstnameField.Secret.Should().BeNull();
            firstnameField.MaxLength.Should().BeNull();
            firstnameField.Pattern.Should().BeNull();
            firstnameField.Required.Should().BeNull();

            FormField lastnameField = f.Items.Single(x => x.Name == nameof(CreateHenchman.Lastname));
            lastnameField.Name.Should().Be(lastnameField.Name);
            lastnameField.Label.Should().Be(lastnameField.Name);
            lastnameField.Type.Should().Be(FormFieldType.String);
            lastnameField.Placeholder.Should().BeNull();
            lastnameField.Secret.Should().BeNull();
            lastnameField.MaxLength.Should().BeNull();
            lastnameField.Pattern.Should().BeNull();
            lastnameField.Required.Should().BeNull();

            FormField birthPlaceField = f.Items.Single(x => x.Name == nameof(CreateHenchman.BirthPlace));
            birthPlaceField.Name.Should().Be(birthPlaceField.Name);
            birthPlaceField.Label.Should().Be(birthPlaceField.Name);
            birthPlaceField.Type.Should().Be(FormFieldType.String);
            birthPlaceField.Placeholder.Should().BeNull();
            birthPlaceField.Secret.Should().BeNull();
            birthPlaceField.MaxLength.Should().BeNull();
            birthPlaceField.Pattern.Should().BeNull();
            birthPlaceField.Required.Should().BeNull();

            FormField birthDateField = f.Items.Single(x => x.Name == nameof(CreateHenchman.BirthDate));
            birthDateField.Name.Should().Be(birthDateField.Name);
            birthDateField.Label.Should().Be(birthDateField.Name);
            birthDateField.Type.Should().Be(FormFieldType.Date);
            birthDateField.Placeholder.Should().BeNull();
            birthDateField.Secret.Should().BeNull();
            birthDateField.MaxLength.Should().BeNull();
            birthDateField.Pattern.Should().BeNull();
            birthDateField.Required.Should().BeNull();

            FormField minionIdField = f.Items.Single(x => x.Name == nameof(CreateHenchman.MinionId));
            minionIdField.Name.Should().Be(minionIdField.Name);
            minionIdField.Label.Should().Be(minionIdField.Name);
            minionIdField.Type.Should().Be(FormFieldType.Integer);
            minionIdField.Placeholder.Should().BeNull();
            minionIdField.Secret.Should().BeNull();
            minionIdField.MaxLength.Should().BeNull();
            minionIdField.Pattern.Should().BeNull();
            minionIdField.Required.Should().BeNull();


        }
    }
}
