using OperationResults;

namespace OperationResultsTests;

public class ResultTests
{
    [Fact]
    public void GenericResult_SuccessWithContent_IsSuccessful()
    {
        Result<int> result = 42;

        Assert.True(result.Success);
        Assert.True(result);
        Assert.True(result.HasValue);
        Assert.Equal(42, result.Content);
        Assert.True(result is int value && value == 42);
        Assert.False(result is Error);
    }

    [Fact]
    public void GenericResult_SuccessWithDefaultValue_IsSuccessful()
    {
        Result<int> result = 0;

        Assert.True(result.Success);
        Assert.True(result);
        Assert.Equal(0, result.Content);
        Assert.True(result is 0);
    }

    [Fact]
    public void GenericResult_SuccessWithNullContent_IsSuccessfulAndMatchesNullPattern()
    {
        var result = new Result<string?>((string?)null);

        Assert.True(result.Success);
        Assert.True(result);
        Assert.False(result.HasValue);
        Assert.Null(result.Content);
        Assert.True(result is null);
        Assert.False(result.TryGetValue(out string? _));
    }

    [Fact]
    public void GenericResult_Failure_IsNotSuccessfulAndExposesError()
    {
        var validationErrors = new[] { new ValidationError("Field", "Error") };

        var result = Result<int>.Fail(FailureReasons.ClientError, "message", "detail", validationErrors);

        Assert.False(result.Success);
        Assert.False(result);
        Assert.True(result.HasValue);
        Assert.Throws<InvalidOperationException>(() => result.Content);
        var error = Assert.IsType<Error>(result.Value);
        Assert.True(result.TryGetValue(out Error? matched));
        Assert.Same(error, matched);
        Assert.Equal(FailureReasons.ClientError, error.FailureReason);
        Assert.Equal("message", error.ErrorMessage);
        Assert.Equal("detail", error.ErrorDetail);
        Assert.Same(validationErrors, error.ValidationErrors);
        Assert.False(result.TryGetValue(out int _));
    }

    [Fact]
    public void GenericResult_FailureWithException_UsesExceptionMessages()
    {
        var exception = new InvalidOperationException("message", new Exception("detail"));

        var result = Result<int>.Fail(FailureReasons.DatabaseError, exception);

        var error = Assert.IsType<Error>(result.Value);
        Assert.True(error.HasException);
        Assert.Same(exception, error.Exception);
        Assert.Equal("message", error.ErrorMessage);
        Assert.Equal("detail", error.ErrorDetail);
    }

    [Fact]
    public void GenericResult_Default_IsNotSuccessfulAndReturnsDefaultContent()
    {
        var result = default(Result<int>);

        Assert.False(result.Success);
        Assert.False(result);
        Assert.False(result.HasValue);
        Assert.Null(result.Value);
        Assert.Equal(0, result.Content);
        Assert.True(result is null);
        Assert.False(result is Error);
    }

    [Fact]
    public void GenericResult_ConvertedFromError_IsFailure()
    {
        var error = Assert.IsType<Error>(Result<int>.Fail(FailureReasons.ItemNotFound).Value);

        Result<string> result = error;

        Assert.False(result.Success);
        Assert.Same(error, result.Value);
    }

    [Fact]
    public void GenericResult_NullError_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Result<int>((Error)null!));
    }

    [Fact]
    public void Result_Ok_IsSuccessful()
    {
        var result = Result.Ok();

        Assert.True(result.Success);
        Assert.True(result);
        Assert.True(result.HasValue);
        Assert.True(result is true);
        Assert.False(result is Error);
    }

    [Fact]
    public void Result_FalseValue_IsNotSuccessfulWithoutError()
    {
        Result result = false;

        Assert.False(result.Success);
        Assert.False(result);
        Assert.True(result.HasValue);
        Assert.True(result is false);
        Assert.False(result.TryGetValue(out Error? _));
    }

    [Fact]
    public void Result_Failure_IsNotSuccessfulAndExposesError()
    {
        var result = Result.Fail(FailureReasons.Forbidden, "message");

        Assert.False(result.Success);
        Assert.False(result);
        var error = Assert.IsType<Error>(result.Value);
        Assert.Equal(FailureReasons.Forbidden, error.FailureReason);
        Assert.Equal("message", error.ErrorMessage);
        Assert.False(result.TryGetValue(out bool _));
    }

    [Fact]
    public void Result_Default_IsFalse()
    {
        var result = default(Result);

        Assert.False(result.Success);
        Assert.False(result);
        Assert.True(result.HasValue);
        Assert.Equal(false, result.Value);
        Assert.True(result is false);
        Assert.False(result is null);
        Assert.False(result.TryGetValue(out Error? _));
    }

    [Fact]
    public void Result_Value_ReturnsBooleanOrError()
    {
        var failure = Result.Fail(FailureReasons.Conflict);

        Assert.Equal(true, Result.Ok().Value);
        Assert.Equal(false, new Result(false).Value);
        Assert.IsType<Error>(failure.Value);
        Assert.True(failure.HasValue);
    }

    [Fact]
    public void Result_NullError_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Result((Error)null!));
    }

    [Fact]
    public void Result_TryGetValue_ReturnsBooleanCase()
    {
        Assert.True(Result.Ok().TryGetValue(out bool success));
        Assert.True(success);
        Assert.True(new Result(false).TryGetValue(out bool failure));
        Assert.False(failure);
        Assert.False(Result.Fail(FailureReasons.GenericError).TryGetValue(out bool _));
        Assert.True(default(Result).TryGetValue(out bool defaultValue));
        Assert.False(defaultValue);
    }

    [Fact]
    public void Result_FailOverloads_PopulateError()
    {
        var validationError = new ValidationError("Field", "Error");
        IEnumerable<ValidationError> validationErrors = [validationError];
        var exception = new InvalidOperationException("exception", new Exception("inner"));

        AssertError(Result.Fail(FailureReasons.ClientError, validationError).Value, FailureReasons.ClientError, null, null, null, [validationError]);
        AssertError(Result.Fail(FailureReasons.ClientError, "message", validationError).Value, FailureReasons.ClientError, "message", null, null, [validationError]);
        AssertError(Result.Fail(FailureReasons.ClientError, "message", "detail", validationError).Value, FailureReasons.ClientError, "message", "detail", null, [validationError]);
        AssertError(Result.Fail(FailureReasons.DatabaseError, exception, validationError).Value, FailureReasons.DatabaseError, "exception", "inner", exception, [validationError]);
        AssertError(Result.Fail(FailureReasons.ItemNotFound).Value, FailureReasons.ItemNotFound, null, null, null, null);
        AssertError(Result.Fail(FailureReasons.ClientError, "message", validationErrors).Value, FailureReasons.ClientError, "message", null, null, validationErrors);
        AssertError(Result.Fail(FailureReasons.ClientError, "message", "detail", validationErrors).Value, FailureReasons.ClientError, "message", "detail", null, validationErrors);
        AssertError(Result.Fail(FailureReasons.DatabaseError, exception, validationErrors).Value, FailureReasons.DatabaseError, "exception", "inner", exception, validationErrors);
    }

    [Fact]
    public void GenericResult_FailOverloads_PopulateError()
    {
        var validationError = new ValidationError("Field", "Error");
        IEnumerable<ValidationError> validationErrors = [validationError];
        var exception = new InvalidOperationException("exception", new Exception("inner"));

        AssertError(Result<int>.Fail(FailureReasons.ClientError, validationError).Value, FailureReasons.ClientError, null, null, null, [validationError]);
        AssertError(Result<int>.Fail(FailureReasons.ClientError, "message", validationError).Value, FailureReasons.ClientError, "message", null, null, [validationError]);
        AssertError(Result<int>.Fail(FailureReasons.ClientError, "message", "detail", validationError).Value, FailureReasons.ClientError, "message", "detail", null, [validationError]);
        AssertError(Result<int>.Fail(FailureReasons.DatabaseError, exception, validationError).Value, FailureReasons.DatabaseError, "exception", "inner", exception, [validationError]);
        AssertError(Result<int>.Fail(FailureReasons.ItemNotFound).Value, FailureReasons.ItemNotFound, null, null, null, null);
        AssertError(Result<int>.Fail(FailureReasons.ClientError, "message", validationErrors).Value, FailureReasons.ClientError, "message", null, null, validationErrors);
        AssertError(Result<int>.Fail(FailureReasons.ClientError, "message", "detail", validationErrors).Value, FailureReasons.ClientError, "message", "detail", null, validationErrors);
        AssertError(Result<int>.Fail(FailureReasons.DatabaseError, exception, validationErrors).Value, FailureReasons.DatabaseError, "exception", "inner", exception, validationErrors);
    }

    [Fact]
    public void GenericResult_ConvertedFromFailedResult_KeepsSameError()
    {
        var source = Result.Fail(FailureReasons.ItemNotFound, "message");

        Result<string> result = source;

        Assert.False(result.Success);
        Assert.Same(source.Value, result.Value);
        Assert.Throws<InvalidOperationException>(() => result.Content);
    }

    [Fact]
    public void GenericResult_ConvertedFromSuccessfulResult_IsSuccessfulWithDefaultContent()
    {
        Result<int> valueResult = Result.Ok();
        Result<string> referenceResult = Result.Ok();

        Assert.True(valueResult.Success);
        Assert.Equal(0, valueResult.Content);
        Assert.True(referenceResult.Success);
        Assert.Null(referenceResult.Content);
    }

    [Fact]
    public void GenericResult_ConvertedFromFalseResult_IsNotSuccessfulWithoutError()
    {
        Result<int> result = new Result(false);

        Assert.False(result.Success);
        Assert.False(result.HasValue);
        Assert.False(result is Error);
    }

    [Fact]
    public void GenericResult_ConvertedFromDefaultResult_IsDefault()
    {
        Result<int> result = default(Result);

        Assert.False(result.Success);
        Assert.Null(result.Value);
    }

    [Theory]
    [InlineData(true, "success")]
    [InlineData(false, "false")]
    public void Result_SwitchExpression_HandlesBooleanCases(bool value, string expected)
    {
        Assert.Equal(expected, Describe(new Result(value)));
        Assert.Equal("false", Describe(default));
        Assert.Equal("error", Describe(Result.Fail(FailureReasons.ClientError)));

        static string Describe(Result result) => result switch
        {
            true => "success",
            false => "false",
            Error => "error"
        };
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GenericResult_ReturnedFromMethod_AcceptsContentOrNonGenericFailure(bool found)
    {
        static Result<string> Find(bool found)
            => found ? "value" : Result.Fail(FailureReasons.ItemNotFound);

        var result = Find(found);

        Assert.Equal(found, result.Success);
        Assert.Equal(found, result is "value");
        Assert.Equal(!found, result is Error { FailureReason: FailureReasons.ItemNotFound });
    }

    [Fact]
    public void GenericResult_SwitchExpression_HandlesEveryCase()
    {
        static string Describe(Result<int> result)
            => result switch
            {
                int value => $"value {value}",
                Error error => $"error {error.FailureReason}",
                null => "none"
            };

        Assert.Equal("value 0", Describe(0));
        Assert.Equal($"error {FailureReasons.Conflict}", Describe(Result<int>.Fail(FailureReasons.Conflict)));
        Assert.Equal("none", Describe(default));
    }

    private static void AssertError(object? value, int failureReason, string? message, string? detail, Exception? exception, IEnumerable<ValidationError>? validationErrors)
    {
        var error = Assert.IsType<Error>(value);

        Assert.Equal(failureReason, error.FailureReason);
        Assert.Equal(message, error.ErrorMessage);
        Assert.Equal(detail, error.ErrorDetail);
        Assert.Same(exception, error.Exception);
        Assert.Equal(exception is not null, error.HasException);
        Assert.Equal(validationErrors, error.ValidationErrors);
    }
}
