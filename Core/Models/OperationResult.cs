namespace ForzaRadioModTool.Core.Models
{
    /// <summary>
    /// Operation result wrapper to provide better error handling
    /// </summary>
    public class OperationResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public Exception? Exception { get; init; }

        private OperationResult() { }

        public static OperationResult<T> Success(T data)
        {
            return new OperationResult<T> { IsSuccess = true, Data = data };
        }

        public static OperationResult<T> Failure(string errorMessage, Exception? exception = null)
        {
            return new OperationResult<T> 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage, 
                Exception = exception 
            };
        }
    }

    /// <summary>
    /// Operation result for void operations
    /// </summary>
    public class OperationResult
    {
        public bool IsSuccess { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public Exception? Exception { get; init; }

        private OperationResult() { }

        public static OperationResult Success()
        {
            return new OperationResult { IsSuccess = true };
        }

        public static OperationResult Failure(string errorMessage, Exception? exception = null)
        {
            return new OperationResult 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage, 
                Exception = exception 
            };
        }
    }
}