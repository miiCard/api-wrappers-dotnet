using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using miiCard.Consumers.Service.v1.Claims;

namespace miiCard.Consumers
{
    /// <summary>
    /// Contains data associated with a failure to perform an authorised API request.
    /// </summary>
    public class MiiCardAuthorisationFailureEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the MiiApiErrorCode that was raised when making the request.
        /// </summary>
        public MiiApiErrorCode ErrorCode { get; private set; }
        /// <summary>
        /// Gets the MiiApiCallStatus that was returned with the request.
        /// </summary>
        public MiiApiCallStatus Status { get; private set; }
        /// <summary>
        /// Gets any error message that was returned by the API when making the request.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the exception that was thrown at the client-side when making the request,
        /// if one was raised. For errors related to the API (for example, when a user has
        /// revoked access to your application) this is likely to be null.
        /// </summary>
        public Exception Exception { get; private set; }

        public MiiCardAuthorisationFailureEventArgs(Exception exception)
        {
            this.Exception = exception;
            this.Status = MiiApiCallStatus.Failure;
            this.ErrorCode = MiiApiErrorCode.Exception;
            this.ErrorMessage = exception.Message;
        }

        public MiiCardAuthorisationFailureEventArgs(MiiApiErrorCode errorCode, string message)
        {
            this.ErrorCode = errorCode;
            this.ErrorMessage = message;
            this.Status = MiiApiCallStatus.Failure;
        }
    }
}
