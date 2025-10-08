using System;
using System.Collections.Generic;
using eQuantic.Core.Outcomes.Results;

namespace eQuantic.Core.Outcomes.Builder {
    public abstract class ResultBuilder<TBuilder, TResult>
        where TBuilder : ResultBuilder<TBuilder, TResult>
        where TResult : BasicResult
    {
        protected readonly TResult result;
		protected ResultBuilder (TResult result) {
			this.result = result;

		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TBuilder WithSuccess()
        {
            this.result.Success = true;
            return (TBuilder)this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TBuilder WithError()
        {
            this.result.Success = false;
            return (TBuilder)this;
        }

        /// <summary>
        /// Add a formatted string to the end of the outcome's message collection.
        /// </summary>
        /// <param name="message">String with format pattern to add. The format patterns will be used in string.Format.</param>
        /// <param name="paramList">Shorthand for String.Format</param>
        /// <returns></returns>
        public TBuilder WithMessageFormat(string message, params object[] paramList)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message = string.Format(message, paramList);
                this.result.Messages.Add(message);
            }
            return (TBuilder)this;
        }

        /// <summary>
		/// Add a string to the end of the outcome's message collection.
		/// </summary>
		/// <param name="message">String to add.</param>
		/// <returns></returns>
		public TBuilder WithMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
                this.result.Messages.Add(message);
            return (TBuilder)this;
        }

        /// <summary>
        /// Append a list of strings to the end of the outcome's message collection.
        /// </summary>
        /// <param name="messages">Enum of strings to add.</param>
        /// <returns></returns>
        public TBuilder WithMessage(IEnumerable<string> messages)
        {
            if (messages == null)
                return (TBuilder)this;

            this.result.Messages.AddRange(messages);
            return (TBuilder)this;
        }

        /// <summary>
        /// Adds messages from the specified outcome, if any.
        /// </summary>
        /// <param name="outcome">Source outcome that messages are pulled from.</param>
        public TBuilder WithMessagesFrom(IResult result)
        {
            WithMessage(result.Messages);
            return (TBuilder)this;
        }

        /// <summary>
        /// Alternate syntax for WithMessage. Adds a collection of messages to the end of the outcome's message list. 
        /// </summary>
        public TBuilder WithMessagesFrom(IEnumerable<string> messages)
        {
            WithMessage(messages);
            return (TBuilder)this;
        }

        /// <summary>
        /// Append a list of exception messages to the end of the outcome's message collection.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">String to add.</param>
        /// <param name="errorCode">Error code number is optional</param>
        /// <returns></returns>
        public TBuilder WithException(Exception ex, string message = null, int? errorCode = null)
        {
            this.result.Success = false;
            this.result.Status = ResultStatus.Error;
            this.result.ErrorCode = errorCode;

            WithMessage(message);
            DeepException(ex);

            return (TBuilder)this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public TBuilder WithStatus(ResultStatus status)
        {
            this.result.Status = status;
            return (TBuilder)this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual TBuilder MergeWith(TResult result)
        {
            this.result.Status = result.Status;
            this.result.Success = result.Success;
            this.result.Messages.AddRange(result.Messages);

            return (TBuilder)this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TResult Result () {
			return this.result;
		}

        protected void DeepException(Exception ex)
        {
            this.result.Messages.Add(ex.Message);
            if(ex.InnerException != null)
                DeepException(ex.InnerException);
        }
    }
}