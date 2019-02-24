using eQuantic.Core.Outcomes.Results;

namespace eQuantic.Core.Outcomes.Builder {
    public class BasicResultBuilder : ResultBuilder<BasicResultBuilder, BasicResult>
    {
        public BasicResultBuilder() : base(new BasicResult()) {
		}
    }
}