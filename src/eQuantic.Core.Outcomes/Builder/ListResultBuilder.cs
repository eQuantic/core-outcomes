using System.Collections.Generic;
using eQuantic.Core.Outcomes.Results;

namespace eQuantic.Core.Outcomes.Builder {
    [Obsolete("The v1 builder API is deprecated and will be removed in v4. Use Result / Result<T> factory methods (Success/Failure) with the eQuantic.Core.Outcomes.Extensions pipeline instead.")]
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