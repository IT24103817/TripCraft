namespace TripCraft.Tests.Common;

/// <summary>
/// The in-memory API with the real Resource Management and Quotation components behind the workflow ports
/// (seeded guides, van and hotels; quotations in their own tables). Agents and third parties are still fakes.
/// </summary>
public class RealComponentsFactory : TestWebApplicationFactory
{
    protected override bool UseRealResourceManagement => true;
    protected override bool UseRealQuotations => true;
}
