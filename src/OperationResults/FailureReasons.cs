namespace OperationResults;

public static class FailureReasons
{
    public const int None = 0;
    public const int ClientError = 1;
    public const int Unauthorized = 2;
    public const int Forbidden = 3;
    public const int ItemNotFound = 4;
    public const int InvalidRequest = 5;
    public const int Timeout = 6;
    public const int Conflict = 7;
    public const int InvalidFile = 8;
    public const int InvalidContent = 9;
    public const int DatabaseError = 10;
    public const int NetworkError = 11;
    public const int ServiceUnavailable = 12;
    public const int GenericError = 1000;
}
