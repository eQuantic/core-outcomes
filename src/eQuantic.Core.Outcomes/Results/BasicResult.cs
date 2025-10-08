using System;
using System.Collections.Generic;
using System.Linq;

namespace eQuantic.Core.Outcomes.Results
{
    public class BasicResult : IResult
    {
        private bool _success;

        public bool Success
        {
            get { return _success; }
            set
            {
                if(value) Status = ResultStatus.Success;
                _success = value;
            }
        }

        public int? ErrorCode { get; set; }

        public List<string> Messages { get; set; } = new List<string>();

        public ResultStatus Status { get; set; } = ResultStatus.NotModified;

        public BasicResult()
        {
            Success = false;
        }
    }
}