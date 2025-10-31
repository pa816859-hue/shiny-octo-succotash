using MediaGallery.Web.Infrastructure.Data.Dto;

namespace MediaGallery.Web.Infrastructure.Data;

public interface IMessageRepository
{
    Task<IReadOnlyList<MessageDto>> GetMessagesAsync(
        long channelId,
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default);
}
