using eQuantic.Core.Outcomes.Results;

namespace eQuantic.Core.Outcomes.Builder {
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