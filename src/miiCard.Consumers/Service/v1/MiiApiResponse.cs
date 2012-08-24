using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using miiCard.Consumers.Service.v1.Claims;

namespace miiCard.Consumers.Service.v1
{
    /// <summary>
    /// Wraps the result of an API call in a standardised response envelope.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MiiApiResponse<T>
    {
        /// <summary>
        /// Gets or sets the overall status of the API call.
        /// </summary>
        public MiiApiCallStatus Status { get; set; }
        /// <summary>
        /// Gets or sets the specific error code associated with the API call.
        /// </summary>
        public MiiApiErrorCode ErrorCode { get; set; }
        /// <summary>
        /// Gets or sets any additional error information associated with an error state. Intended for
        /// use only with the MiiCardApiErrorCode.Exception ErrorCode value, though can be used for any
        /// additional error information.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The payload of the response, representing the raw result of the API call being wrapped.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Initialises a new MiiApiResponse.
        /// </summary>
        public MiiApiResponse()
        {
            this.Status = MiiApiCallStatus.Success;
            this.ErrorCode = MiiApiErrorCode.Success;
        }

        /// <summary>
        /// Initialises a new MiiApiResponse.
        /// </summary>
        /// <param name="status">The status of the API call.</param>
        /// <param name="errorCode">The error code of any error raised by making the API call.</param>
        /// <param name="errorMessage">The error message associated with any error raised by making the API call.</param>
        /// <param name="data">The payload of the call, if any.</param>
        public MiiApiResponse(MiiApiCallStatus status, MiiApiErrorCode errorCode, string errorMessage, T data)
        {
            this.Status = status;
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;

            this.Data = data;
        }

        public static implicit operator T(MiiApiResponse<T> response)
        {
            if (response == null)
            {
                return default(T);
            }
            else
            {
                return response.Data;
            }
        }
    }
}
