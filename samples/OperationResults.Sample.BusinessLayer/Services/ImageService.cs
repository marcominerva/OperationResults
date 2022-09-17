using OperationResults.Sample.BusinessLayer.Services.Interfaces;

namespace OperationResults.Sample.BusinessLayer.Services;

public class ImageService : IImageService
{
    public async Task<Result<ByteArrayFileContent>> GetImageAsync()
    {
        if (!File.Exists(@"D:\Image.jpg"))
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var content = await File.ReadAllBytesAsync(@"D:\Image.jpg");
        return new ByteArrayFileContent(content, "image/jpg");
    }
}
