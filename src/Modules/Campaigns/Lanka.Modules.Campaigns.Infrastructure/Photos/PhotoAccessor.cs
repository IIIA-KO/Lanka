using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Application.Abstractions.Photos;
using Lanka.Modules.Campaigns.Domain.Bloggers.Photos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Campaigns.Infrastructure.Photos;

internal sealed class PhotoAccessor : IPhotoAccessor
{
    private readonly Cloudinary _cloudinary;

    public PhotoAccessor(IOptions<CloudinaryOptions> cloudinaryOptions)
    {
        this._cloudinary = CreateCloudinaryAccount(cloudinaryOptions.Value);
    }

    public async Task<Result<Photo>> AddPhotoAsync(IFormFile file)
    {
        if (!IsValidFile(file))
        {
            return Result.Failure<Photo>(PhotoErrors.FailedUpload);
        }

        await using Stream stream = file.OpenReadStream();

        ImageUploadParams uploadParams = CreateImageUploadParams(file, stream);
        ImageUploadResult uploadResult = await this._cloudinary.UploadAsync(uploadParams);

        return uploadResult.Error is not null
                ? Result.Failure<Photo>(PhotoErrors.FailedUpload)
                : new Photo(uploadResult.PublicId, new Uri(uploadResult.SecureUrl.AbsoluteUri));
    }

    private static Cloudinary CreateCloudinaryAccount(CloudinaryOptions options)
    {
        var account = new Account(options.CloudName, options.ApiKey, options.ApiSecret);
        return new Cloudinary(account);
    }

    private static bool IsValidFile(IFormFile file) => file?.Length > 0;

    public async Task<Result> DeletePhotoAsync(string photoId)
    {
        var deleteParameters = new DeletionParams(photoId);
        DeletionResult result = await this._cloudinary.DestroyAsync(deleteParameters);

        return result.Result == "ok"
            ? Result.Success()
            : Result.Failure(PhotoErrors.FailedDelete);
    }

    private static ImageUploadParams CreateImageUploadParams(IFormFile file, Stream stream)
    {
        return new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = new Transformation().Height(500).Width(500).Crop("fill")
        };
    }
}
