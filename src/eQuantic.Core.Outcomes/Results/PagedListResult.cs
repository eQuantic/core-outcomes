using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using eQuantic.Core.Collections;
using System.Text.Json.Serialization;

namespace eQuantic.Core.Outcomes.Results
{
    public class PagedListResult<T> : ListResult<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }

        [JsonPropertyName("__next")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Metadata Next { get; set; }

        [JsonPropertyName("__previous")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Metadata Previous { get; set; }

        public bool HaveNext => PageIndex * PageSize < TotalCount;
        public bool HavePrevious => (PageIndex * PageSize) - PageSize > 0;


        public PagedListResult() : base()
        {
        }

        public PagedListResult(IEnumerable<T> items, int pageIndex, int pageSize, long totalCount) : this()
        {
            AddPagedItems(items, pageIndex, pageSize, totalCount);
        }

        public PagedListResult(IPagedEnumerable<T> items) : this(items, items.PageIndex, items.PageSize, items.TotalCount)
        {
        }

        public void AddPagedItems(IEnumerable<T> items, int pageIndex, int pageSize, long totalCount)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            Items = items.ToList();
        }

        public void AddPagedItems(IPagedEnumerable<T> items)
        {
            AddPagedItems(items, items.PageIndex, items.PageSize, items.TotalCount);
        }

        public PagedList<T> ToPagedList()
        {
            return new PagedList<T>(Items, TotalCount) {PageIndex = PageIndex, PageSize = PageSize};
        }
    }
}