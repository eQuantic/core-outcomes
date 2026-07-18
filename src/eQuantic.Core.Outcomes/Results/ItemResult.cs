using System.Text.Json.Serialization;

namespace eQuantic.Core.Outcomes.Results
{
    public class ItemResult<TItem> : BasicResult
    {
        public TItem Item { get; set; }

        [JsonPropertyName("__list")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Metadata ListMetadata { get; set; }
    }
}