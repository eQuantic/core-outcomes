using System.Collections.Generic;
using eQuantic.Core.Outcomes.Results;

namespace eQuantic.Core.Outcomes.Builder {
    public class ListResultBuilder<TItem> : ResultBuilder<ListResultBuilder<TItem>, ListResult<TItem>>
    {
        public ListResultBuilder() : base(new ListResult<TItem>()) {
		}

		public override ListResultBuilder<TItem> MergeWith(ListResult<TItem> result)
        {
            this.result.Items = result.Items;
            return base.MergeWith(result);
        }

        public ListResultBuilder<TItem> WithItems(List<TItem> items)
        {
            this.result.Items = items;
            return this;
        }
    }
}