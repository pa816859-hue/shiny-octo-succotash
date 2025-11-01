using MediaGallery.Web.Infrastructure.Data.Dto;

namespace MediaGallery.Web.Infrastructure.Data;

public interface IMessageRepository
{
    Task<IReadOnlyList<MessageDetailDto>> GetRecentMessagesAsync(
        int offset,
        int limit,
        long? channelId,
        long? userId,
        bool sortAscending,
        bool mediaOnly,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MessageDetailDto>> GetMediaChronologyAsync(
        long? photoId,
        long? videoId,
        CancellationToken cancellationToken = default);
}
