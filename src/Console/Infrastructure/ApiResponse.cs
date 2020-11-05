using System.Collections.Generic;
using System.Net;

namespace Omnia.CLI.Infrastructure
{
    public class ApiResponse
    {
        public bool Success { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public ApiError ErrorDetails { get; set; }

        public Dictionary<string, object> ResponseValues { get; set; }

        public ApiResponse(bool success)
        {
            this.Success = success;
        }

        public ApiResponse(bool success, HttpStatusCode statusCode) : this(success)
        {
            this.StatusCode = statusCode;
        }

        public ApiResponse(bool success, HttpStatusCode statusCode, Dictionary<string, object> responseValues) : this(success, statusCode)
        {
            this.ResponseValues = responseValues;
        }

        public ApiResponse(bool success, HttpStatusCode statusCode, ApiError errorDetails) : this(success, statusCode)
        {
            this.ErrorDetails = errorDetails;
        }
    }
}
