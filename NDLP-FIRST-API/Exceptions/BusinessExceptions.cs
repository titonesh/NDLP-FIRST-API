using System;
using System.Net;

namespace NDLP_FIRST_API.Exceptions
{
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }
        public HttpStatusCode StatusCode { get; }

        public BusinessException(string errorCode, string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }

    public class EntityNotFoundException : BusinessException
    {
        public EntityNotFoundException(string entityName, string key)
            : base($"{entityName.ToUpper()}_NOT_FOUND", $"{entityName} with identifier '{key}' was not found.", HttpStatusCode.NotFound)
        {
        }
    }

    public class UnauthorizedAccessDomainException : BusinessException
    {
        public UnauthorizedAccessDomainException(string message = "You are not authorized to perform this operation or access this resource.")
            : base("UNAUTHORIZED_ACCESS", message, HttpStatusCode.Forbidden)
        {
        }
    }

    public class DeviceLimitExceededException : BusinessException
    {
        public DeviceLimitExceededException(int limit)
            : base("DEVICE_LIMIT_EXCEEDED", $"Maximum registered active device limit ({limit}) has been reached for this customer.", HttpStatusCode.BadRequest)
        {
        }
    }

    public class DuplicateDeviceException : BusinessException
    {
        public DuplicateDeviceException(string deviceIdentifier)
            : base("DUPLICATE_DEVICE", $"Device with identifier '{deviceIdentifier}' is already registered for this customer.", HttpStatusCode.Conflict)
        {
        }
    }

    public class KycRestrictionException : BusinessException
    {
        public KycRestrictionException(string message)
            : base("KYC_RESTRICTION", message, HttpStatusCode.BadRequest)
        {
        }
    }
}
