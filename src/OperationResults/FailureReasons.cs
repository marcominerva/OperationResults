namespace OperationResults;

public static class FailureReasons
{
    public const int None = 0;
    public const int ItemNotFound = 1;
    public const int Forbidden = 2;
    public const int DatabaseError = 3;
    public const int ClientError = 4;
    public const int InvalidFile = 5;
    public const int Conflict = 6;
    public const int Unauthorized = 7;
    public const int GenericError = 8;
}
