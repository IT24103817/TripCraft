using System.Text.Json;
using System.Text.Json.Serialization;

namespace TripCraft.Application.Workflows;

/// <summary>Reads and writes the jsonb columns of agent_workflows and agent_steps.</summary>
public static class WorkflowJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string? json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, Options);

    /// <summary>A jsonb column as a JsonElement for a response DTO. Null stays null.</summary>
    public static JsonElement? ToElement(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    /// <summary>JSON sent by the agent service, stored as-is (it is a summary). Missing becomes "{}".</summary>
    public static string Raw(JsonElement? json) =>
        json is null || json.Value.ValueKind == JsonValueKind.Undefined ? "{}" : json.Value.GetRawText();
}
