using System.Linq;
using eQuantic.Core.Collections;
using eQuantic.Core.Outcomes.Results;

namespace eQuantic.Core.Outcomes.Builder {
    public class PagedListResultBuilder<TItem> : ResultBuilder<PagedListResultBuilder<TItem>, PagedListResult<TItem>>
    {
        public PagedListResultBuilder() : base(new PagedListResult<TItem>()) {
		}

		public override PagedListResultBuilder<TItem> MergeWith(PagedListResult<TItem> result)
        {
            this.result.Items = result.Items;
            this.result.PageIndex = result.PageIndex;
            this.result.PageSize = result.PageSize;
            this.result.TotalCount = result.TotalCount;
            return base.MergeWith(result);
        }

        public PagedListResultBuilder<TItem> WithPagedItems(IPagedEnumerable<TItem> items)
        {
            this.result.Items = items.ToList();
            this.result.PageIndex = items.PageIndex;
            this.result.PageSize = items.PageSize;
            this.result.TotalCount = items.TotalCount;

            return this;
        }
    }
}