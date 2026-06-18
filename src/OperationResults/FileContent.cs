namespace OperationResults;

/// <summary>
/// Represents file content backed by a <see cref="Stream"/>, so services can return downloadable or renderable files without depending on a specific presentation framework.
/// </summary>
/// <param name="Content">The stream containing the file payload. Ownership and disposal remain with the consuming adapter unless documented otherwise by the producer.</param>
/// <param name="ContentType">The media type that describes how the payload should be interpreted by the consumer.</param>
/// <param name="DownloadFileName">An optional file name that presentation layers can use when exposing the payload as a download.</param>
public record class StreamFileContent(Stream Content, string ContentType, string? DownloadFileName = null);

/// <summary>
/// Represents file content backed by an in-memory byte array,so services can return downloadable or renderable files without depending on a specific presentation framework.
/// </summary>
/// <param name="Content">The binary payload to expose to the consumer.</param>
/// <param name="ContentType">The media type that describes how the payload should be interpreted by the consumer.</param>
/// <param name="DownloadFileName">An optional file name that presentation layers can use when exposing the payload as a download.</param>
public record class ByteArrayFileContent(byte[] Content, string ContentType, string? DownloadFileName = null);
