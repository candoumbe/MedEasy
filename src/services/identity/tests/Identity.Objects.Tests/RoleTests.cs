using Bogus;

using FluentAssertions;

using System;

using Xunit;
using Xunit.Abstractions;

namespace Identity.Objects.Tests
{
    public class RoleTests
    {
        private readonly Faker<Role> _roleFaker;
        private readonly ITestOutputHelper _outputHelper;

        public RoleTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _roleFaker = new Faker<Role>()
                .CustomInstantiator(faker => new Role(Guid.NewGuid(), faker.Hacker.Abbreviation()));
        }

        [Fact]
        public void Ctor_throws_ArgumentOutOfRangeException_when_id_is_empty()
        {
            Action invalidCtor = () => new Role(Guid.Empty, "a_role");

            // Assert
            invalidCtor.Should()
                       .ThrowExactly<ArgumentOutOfRangeException>("id cannot be empty");
        }

        [Fact]
        public void Ctor_throws_ArgumentNullException_when_code_is_null()
        {
            Action invalidCtor = () => new Role(Guid.NewGuid(), null);

            // Assert
            invalidCtor.Should()
                       .ThrowExactly<ArgumentNullException>("code cannot be null");
        }


        [Theory]
        [InlineData("documents", null)]
        [InlineData(null, "create")]
        public void AddOrUpdateClaim_throws_ArgumentNullException_when_either_type_or_value_is_null(string type, string value)
        {
            // Arrange
            Role role = _roleFaker;

            // Act
            Action addOrUpdateInvalid = () => role.AddOrUpdateClaim(type, value);

            // Assert
            addOrUpdateInvalid.Should()
                              .ThrowExactly<ArgumentNullException>("neither type nor value can be null");
        }


        [Fact]
        public void AddOrUpdateClaim_add_claim_when_it_does_not_exist_yet()
        {
            // Arrange
            Role role = _roleFaker;

            // Act
            role.AddOrUpdateClaim("documents", "create");

            // Assert
            role.Claims.Should()
                .ContainSingle(rc => rc.Claim.Type == "documents" && rc.Claim.Value == "create", "the claim is added when it does not exists yet");
        }

        [Fact]
        public void AddOrUpdateClaim_update_claim_when_it_does_not_exist_yet()
        {
            // Arrange
            Role role = _roleFaker;
            role.AddOrUpdateClaim("documents", "create");

            // Act
            role.AddOrUpdateClaim("documents", "create:update");

            // Assert
            role.Claims.Should()
                .NotContain(rc => rc.Claim.Type == "documents" && rc.Claim.Value == "create", "No claim with the old value exists").And
                .ContainSingle(rc => rc.Claim.Type == "documents" && rc.Claim.Value == "create:update", "the claim is updated with the new value");
        }

        [Fact]
        public void Remove_claim_remove_all_claims_when_no_value_specified()
        {
            // Arrange
            Role role = _roleFaker;
            role.AddOrUpdateClaim("documents", "create");
            role.AddOrUpdateClaim("documents", "update");

            // Act
            role.RemoveClaim("documents");

            // Assert
            role.Claims.Should()
                .NotContain(rc => rc.Claim.Type == "documents", "The claim no longer exists after being removed from the role");
        }

        [Fact]
        public void Remove_claim_remove_only_claim_with_specific_value_when_value_specified()
        {
            // Arrange
            Role role = _roleFaker;
            role.AddOrUpdateClaim("documents", "create");
            role.AddOrUpdateClaim("documents", "read");
            role.AddOrUpdateClaim("documents", "update");

            // Act
            role.RemoveClaim("documents", "update");

            // Assert
            role.Claims.Should()
                .NotContain(rc => rc.Claim.Type == "documents", "The claim no longer exists after being removed from the role");
        }
    }
}
