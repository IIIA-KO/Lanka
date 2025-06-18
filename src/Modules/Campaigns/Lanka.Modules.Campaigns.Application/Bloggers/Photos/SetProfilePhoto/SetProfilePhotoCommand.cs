using Lanka.Common.Application.Messaging;
using Microsoft.AspNetCore.Http;

namespace Lanka.Modules.Campaigns.Application.Bloggers.Photos.SetProfilePhoto;

public sealed record SetProfilePhotoCommand(IFormFile File) : ICommand;
