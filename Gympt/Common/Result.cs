using System.Text.Json.Serialization;

namespace Gympt.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }

        protected Result(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(string error) => new(false, error);
    }

    public sealed class Result<T> : Result
    {
        public T? Value { get; }

        [JsonConstructor] // Esto permite que System.Text.Json deserialice
        public Result(bool isSuccess, string? error, T? value) : base(isSuccess, error) { Value = value; }

        public static Result<T> Success(T value) => new(true, null, value);
        public static new Result<T> Failure(string error) => new(false, error, default);
    }
}
