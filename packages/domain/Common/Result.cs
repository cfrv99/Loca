namespace Loca.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ResultError? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(ResultError error)
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string code, string message) => new(new ResultError(code, message));

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ResultError, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);

    public async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> onSuccess, Func<ResultError, Task<TResult>> onFailure)
        => IsSuccess ? await onSuccess(Value!) : await onFailure(Error!);
}

public record ResultError(string Code, string Message);
