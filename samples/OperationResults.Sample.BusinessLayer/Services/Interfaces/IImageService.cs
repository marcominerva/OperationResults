namespace OperationResults.Sample.BusinessLayer.Services.Interfaces;

public interface IImageService
{
    Task<Result<ByteArrayFileContent>> GetImageAsync();
}
