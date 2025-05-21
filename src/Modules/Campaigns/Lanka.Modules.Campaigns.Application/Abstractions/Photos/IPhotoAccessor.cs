using Lanka.Common.Domain;
using Lanka.Modules.Campaigns.Domain.Bloggers.Photos;
using Microsoft.AspNetCore.Http;

namespace Lanka.Modules.Campaigns.Application.Abstractions.Photos;

public interface IPhotoAccessor
{
    Task<Result<Photo>> AddPhotoAsync(IFormFile file);

    Task<Result> DeletePhotoAsync(string photoId);
}
