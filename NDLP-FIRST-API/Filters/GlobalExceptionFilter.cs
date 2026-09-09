using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using NDLP_FIRST_API.Exceptions;
using NDLP_FIRST_API.Models.DTOs;

namespace NDLP_FIRST_API.Filters
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var exception = context.Exception;

            if (exception is BusinessException busEx)
            {
                var response = ApiResponse<object>.ErrorResponse(busEx.ErrorCode, busEx.Message);
                context.Response = context.Request.CreateResponse(busEx.StatusCode, response);
                return;
            }

            if (exception is UnauthorizedAccessException unauthEx)
            {
                var response = ApiResponse<object>.ErrorResponse("UNAUTHORIZED_ACCESS", unauthEx.Message);
                context.Response = context.Request.CreateResponse(HttpStatusCode.Forbidden, response);
                return;
            }

            // Log unexpected exceptions safely
            System.Diagnostics.Trace.TraceError($"Unhandled exception occurred: {exception.Message} - {exception.StackTrace}");

            var genericResponse = ApiResponse<object>.ErrorResponse("INTERNAL_SERVER_ERROR", "An unexpected system error occurred. Please try again later.");
            context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, genericResponse);
        }
    }
}
