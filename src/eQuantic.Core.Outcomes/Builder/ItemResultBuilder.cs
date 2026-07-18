using eQuantic.Core.Outcomes.Results;

namespace eQuantic.Core.Outcomes.Builder {
    [Obsolete("The v1 builder API is deprecated and will be removed in v4. Use Result / Result<T> factory methods (Success/Failure) with the eQuantic.Core.Outcomes.Extensions pipeline instead.")]
    public class ItemResultBuilder<TItem> : ResultBuilder<ItemResultBuilder<TItem>, ItemResult<TItem>>
    {
        public ItemResultBuilder() : base(new ItemResult<TItem>()) {
		}

		public override ItemResultBuilder<TItem> MergeWith(ItemResult<TItem> result)
        {
            this.result.Item = result.Item;
            return base.MergeWith(result);
        }

        public ItemResultBuilder<TItem> WithItem(TItem item)
        {
            this.result.Item = item;
            return this;
        }
    }
}