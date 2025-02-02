using Newtonsoft.Json;

namespace HangfireDemo.API;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public record JobContext(
    [property: JsonProperty] int UserId,
    [property: JsonProperty] string CorrelationId,
    [property: JsonProperty]
    IDictionary<string, string> Headers
)
{
    [JsonConstructor]
    public JobContext(int userId, string correlationId) 
        : this(userId, correlationId, new Dictionary<string, string>())
    {
    }
}