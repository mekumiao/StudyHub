using System.Diagnostics.CodeAnalysis;

namespace StudyHub.Service.Base;

public record ServiceResult {
    [MemberNotNullWhen(false, nameof(Message))]
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public static ServiceResult Ok() => new() { IsSuccess = true };

    public static ServiceResult NotFound(string message = "未找到资源") => new() { Message = message };

    public static ServiceResult Error(string message = "发生错误") => new() { Message = message };

    public static ServiceResult<T> Ok<T>(T result) where T : notnull {
        return new ServiceResult<T> { IsSuccess = true, Result = result };
    }

    public static ServiceResult<T> NotFound<T>(string message = "未找到资源") where T : notnull {
        return new ServiceResult<T> { Message = message };
    }

    public static ServiceResult<T> Error<T>(string message = "发生错误") where T : notnull {
        return new ServiceResult<T> { Message = message };
    }
}

public record ServiceResult<T> where T : notnull {
    [MemberNotNullWhen(true, nameof(Result))]
    [MemberNotNullWhen(false, nameof(Message))]
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }
}
