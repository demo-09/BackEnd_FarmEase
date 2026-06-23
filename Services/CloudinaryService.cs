using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using backEnd.Interfaces;

namespace backEnd.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var acc = new Account(
            Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? config["Cloudinary:CloudName"],
            Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? config["Cloudinary:ApiKey"],
            Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? config["Cloudinary:ApiSecret"]
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(1000).Width(1000).Crop("limit"),
                Folder = "farmEase/machinery"
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    public async Task<VideoUploadResult> UploadVideoAsync(IFormFile file)
    {
        var uploadResult = new VideoUploadResult();

        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "farmEase/machinery_videos"
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    public async Task<DeletionResult> DeleteMediaAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
