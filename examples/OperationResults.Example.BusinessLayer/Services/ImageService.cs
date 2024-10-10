using OperationResults.Example.BusinessLayer.Services.Interfaces;

namespace OperationResults.Example.BusinessLayer.Services;

public class ImageService : IImageService
{
    public async Task<Result<ByteArrayFileContent>> GetImageAsync()
    {
        if (!File.Exists(@"D:\Taggia.jpg"))
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var content = await File.ReadAllBytesAsync(@"D:\Taggia.jpg");
        return new ByteArrayFileContent(content, "image/jpg");
    }
}
