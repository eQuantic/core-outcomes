using Newtonsoft.Json;

namespace eQuantic.Core.Outcomes.Results
{
    public class ItemResult<TItem> : BasicResult
    {
        public TItem Item { get; set; }

        [JsonProperty("__list", NullValueHandling = NullValueHandling.Ignore)]
        public Metadata ListMetadata { get; set; }
    }
}