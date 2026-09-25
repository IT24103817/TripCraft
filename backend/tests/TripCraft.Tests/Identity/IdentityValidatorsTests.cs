using FluentAssertions;
using TripCraft.Application.Identity;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Application.Identity.Validators;

namespace TripCraft.Tests.Identity;

public class IdentityValidatorsTests
{
    [Fact]
    public void Login_needs_a_valid_email_and_a_password()
    {
        new LoginRequestValidator().Validate(new LoginRequest("manager1@tripcraft.test", "x")).IsValid.Should().BeTrue();
        var result = new LoginRequestValidator().Validate(new LoginRequest("not-an-email", ""));
        result.Errors.Select(e => e.PropertyName).Should().BeEquivalentTo(["Email", "Password"]);
    }

    [Theory]
    [InlineData("short1A", "at least 8")]
    [InlineData("alllowercase1", "upper-case")]
    [InlineData("ALLUPPERCASE1", "lower-case")]
    [InlineData("NoDigitsHere", "digit")]
    public void Register_requires_a_strong_password(string password, string expected)
    {
        var result = new RegisterRequestValidator().Validate(new RegisterRequest("a@b.lk", password, "Name"));

        result.Errors.Should().ContainSingle(e => e.PropertyName == "Password")
            .Which.ErrorMessage.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Create_user_rejects_an_unknown_role_and_an_empty_name()
    {
        var result = new CreateUserRequestValidator().Validate(new CreateUserRequest("a@b.lk", "Str0ngPass", "", (UserRole)99));

        result.Errors.Select(e => e.PropertyName).Should().BeEquivalentTo(["FullName", "Role"]);
    }
}
