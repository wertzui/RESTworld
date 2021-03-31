using System;
using System.Net;

namespace RESTworld.Business
{
    public record ServiceResponse<T>
    {
        internal ServiceResponse(HttpStatusCode status, T responseObject)
        {
            Status = status;
            ResponseObject = responseObject;
        }

        internal ServiceResponse(HttpStatusCode status, string problemDetails)
        {
            Status = status;
            ProblemDetails = problemDetails;
        }

        public T ResponseObject { get; }

        public bool Succeeded => (int)Status >= 200 && (int)Status < 300;

        public string ProblemDetails { get; }

        public HttpStatusCode Status { get; }
    }

    public static class ServiceResponse
    {
        public static ServiceResponse<T> FromException<T>(HttpStatusCode status, Exception exception)
            => FromProblem<T>(status, exception.ToString());

        public static ServiceResponse<T> FromException<T>(Exception exception)
            => FromException<T>(HttpStatusCode.InternalServerError, exception);

        public static ServiceResponse<T> FromProblem<T>(HttpStatusCode status, string problemDetails)
            => new(status, problemDetails);

        public static ServiceResponse<T> FromProblem<T>(string problemDetails)
            => FromProblem<T>(HttpStatusCode.InternalServerError, problemDetails);

        public static ServiceResponse<T> FromResult<T>(T responseObject)
            => new(HttpStatusCode.OK, responseObject);

        public static ServiceResponse<T> FromResult<T>(HttpStatusCode status, T responseObject)
            => new(status, responseObject);

        public static ServiceResponse<T> FromStatus<T>(HttpStatusCode status)
            => FromProblem<T>(status, (int)status >= 200 && (int)status < 300 ? null : status.ToString());
    }
}