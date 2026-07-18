using eQuantic.Core.Outcomes.Results;

namespace eQuantic.Core.Outcomes.Builder {
    [Obsolete("The v1 builder API is deprecated and will be removed in v4. Use Result / Result<T> factory methods (Success/Failure) with the eQuantic.Core.Outcomes.Extensions pipeline instead.")]
    public class BasicResultBuilder : ResultBuilder<BasicResultBuilder, BasicResult>
    {
        public BasicResultBuilder() : base(new BasicResult()) {
		}
    }
}