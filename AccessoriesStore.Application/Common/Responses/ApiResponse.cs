namespace AccessoriesStore.Application.Common.Responses
{

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }

        private ApiResponse(bool success,T? data = default,
            string? message = null)
        {
            Success = success;
            Data = data;
            Message = message;
        }

        public static ApiResponse<T> SuccessResponse(
            T? data,
            string? message = null)
        {
            return new ApiResponse<T>(
                true,
                data,
                message);
        }

        public static ApiResponse<T> Failure(
            string message)
        {
            return new ApiResponse<T>(
                false,
                default,
                message);
        }
    }
}