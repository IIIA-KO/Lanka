using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.Photos;

public static class PhotoErrors
{
    public static Error PhotoNotFound =>
        Error.NotFound("Photo.NotFound", "User has no profile photo with provided indentifier");

    public static Error FailedUpload =>
        Error.Problem("Photo.FailedUpload", "Error occured while uploading photo");

    public static Error FailedDelete =>
        Error.Problem("Photo.FailedDelete", "Error occured while deleting photo");
}
