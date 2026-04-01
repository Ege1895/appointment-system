using Shared.Responses;

namespace AppointmentSystem.Api.Shared.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; private set; }

        public T? Data { get; private set; }

        public string? Error { get; private set; }

        public Dictionary<string, string[]>? ValidationErrors { get; private set; }

        public PaginationMeta? Meta { get; private set; }

        private ApiResponse(
            bool success,
            T? data,
            string? error,
            Dictionary<string, string[]>? validationErrors,
            PaginationMeta? meta)
        {
            Success = success;
            Data = data;
            Error = error;
            ValidationErrors = validationErrors;
            Meta = meta;
        }

        public static ApiResponse<T> SuccessResponse(T data)
        {
            return new ApiResponse<T>(true, data, null, null, null);
        }

        public static ApiResponse<T> SuccessResponse(T data, PaginationMeta meta)
        {
            return new ApiResponse<T>(true, data, null, null, meta);
        }

        public static ApiResponse<T> FailureResponse(string error)
        {
            return new ApiResponse<T>(false, default, error, null, null);
        }

        public static ApiResponse<T> ValidationFailure(Dictionary<string, string[]> errors)
        {
            return new ApiResponse<T>(false, default, "Validation failed", errors, null);
        }
    }
}