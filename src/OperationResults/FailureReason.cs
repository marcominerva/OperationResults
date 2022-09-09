namespace OperationResults;

public enum FailureReason
{
    None,
    ItemNotFound,
    Forbidden,
    DatabaseError,
    ClientError,
    GenericError
}
