using CloudinaryDotNet.Actions;

namespace backEnd.Interfaces;

public interface ICloudinaryService
{
    Task<ImageUploadResult> UploadImageAsync(IFormFile file);
    Task<VideoUploadResult> UploadVideoAsync(IFormFile file);
    Task<DeletionResult> DeleteMediaAsync(string publicId);
}
