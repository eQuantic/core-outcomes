using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace eQuantic.Core.Outcomes.Results
{
    public class ListResult<T> : BasicResult
    {
        public virtual List<T> Items { get; set; }

        [JsonPropertyName("__metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Metadata Metadata { get; set; }

        public ListResult()
        {
            Items = new List<T>();
        }
        public ListResult(IEnumerable<T> items)
        {
            Items = items.ToList();
        }
    }
}
