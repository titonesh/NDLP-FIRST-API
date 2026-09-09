using System;

namespace NDLP_FIRST_API.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public object Details { get; set; }
        public DateTime Timestamp { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Code = "SUCCESS",
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ErrorResponse(string code, string message, object details = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Code = code,
                Message = message,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
