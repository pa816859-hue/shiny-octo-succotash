using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public interface IUserService
{
    Task<UserProfileViewModel> GetUserProfileAsync(
        long userId,
        int pageNumber,
        int pageSize,
        long? channelId,
        bool mediaOnly,
        MessageSortOrder sortOrder,
        CancellationToken cancellationToken = default);
}
