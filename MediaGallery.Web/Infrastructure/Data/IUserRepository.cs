using MediaGallery.Web.Infrastructure.Data.Dto;

namespace MediaGallery.Web.Infrastructure.Data;

public interface IUserRepository
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserByIdAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
