namespace OperationResults.Example.BusinessLayer.Services.Interfaces;

public interface IImageService
{
    Task<Result<ByteArrayFileContent>> GetImageAsync();
}
