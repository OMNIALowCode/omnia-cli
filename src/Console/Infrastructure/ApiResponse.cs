using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

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

        public ApiResponse(bool success, HttpStatusCode statusCode, ApiError errorDetails, Dictionary<string, object> responseValues) : this(success, statusCode, errorDetails)
        {
            this.ResponseValues = responseValues;
        }

        public void AddResponseValues(String key, Object value)
        {
            if (this.ResponseValues == null)
                this.ResponseValues = new Dictionary<string, object>();

            this.ResponseValues.Add(key, value);
        }
    }
}
