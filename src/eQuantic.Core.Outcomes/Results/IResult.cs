using System;
using System.Collections.Generic;
using System.Linq;

namespace eQuantic.Core.Outcomes.Results
{
    public interface IResult
    {
        bool Success { get; }
        ResultStatus Status { get; }
        int? ErrorCode { get; }
        List<string> Messages { get; }
    }
}