using FluentAssertions;
using TripCraft.Application.Quotations;

namespace TripCraft.Tests.Quotations;

public class QuotationDecisionValidatorsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_replan_needs_a_note_and_a_decline_needs_a_reason(string text)
    {
        new ReplanRequestValidator().Validate(new ReplanRequest(text)).IsValid.Should().BeFalse();
        new DeclineQuotationRequestValidator().Validate(new DeclineQuotationRequest(text)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Texts_are_limited_to_1000_characters()
    {
        var tooLong = new string('x', 1001);
        new ReplanRequestValidator().Validate(new ReplanRequest(tooLong)).IsValid.Should().BeFalse();
        new QuotationDecisionRequestValidator().Validate(new QuotationDecisionRequest(tooLong)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void The_send_comment_is_optional()
    {
        new QuotationDecisionRequestValidator().Validate(new QuotationDecisionRequest(null)).IsValid.Should().BeTrue();
        new ReplanRequestValidator().Validate(new ReplanRequest("Cheaper hotels")).IsValid.Should().BeTrue();
    }
}
