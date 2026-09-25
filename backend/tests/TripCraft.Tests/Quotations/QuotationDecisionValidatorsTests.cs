using FluentAssertions;
using TripCraft.Application.Quotations;

namespace TripCraft.Tests.Quotations;

public class QuotationDecisionValidatorsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_revision_needs_a_comment(string comment) =>
        new RequestRevisionRequestValidator().Validate(new RequestRevisionRequest(comment)).IsValid.Should().BeFalse();

    [Fact]
    public void Comments_are_limited_to_1000_characters()
    {
        var tooLong = new string('x', 1001);
        new RequestRevisionRequestValidator().Validate(new RequestRevisionRequest(tooLong)).IsValid.Should().BeFalse();
        new QuotationDecisionRequestValidator().Validate(new QuotationDecisionRequest(tooLong)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Approve_and_reject_comments_are_optional()
    {
        new QuotationDecisionRequestValidator().Validate(new QuotationDecisionRequest(null)).IsValid.Should().BeTrue();
        new RequestRevisionRequestValidator().Validate(new RequestRevisionRequest("Cheaper hotels")).IsValid.Should().BeTrue();
    }
}
