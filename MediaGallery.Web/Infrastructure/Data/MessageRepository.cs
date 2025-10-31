using System.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public class MessageRepository : SqlRepositoryBase, IMessageRepository
{
    private const string MessageQuery = @"SELECT ChannelID, MessageID, UserID, SentDate, MessageText, PhotoID, VideoID
FROM dbo.Messages
WHERE ChannelID = @ChannelId
ORDER BY SentDate DESC, MessageID DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    public MessageRepository(IDbConnectionFactory connectionFactory, ISqlCommandExecutor commandExecutor)
        : base(connectionFactory, commandExecutor)
    {
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(long channelId, int offset, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedOffset = NormalizeOffset(offset);
        var normalizedPageSize = NormalizePageSize(pageSize);

        using var connection = CreateConnection();
        using var command = new SqlCommand(MessageQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@ChannelId", SqlDbType.BigInt) { Value = channelId });
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = normalizedOffset });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = normalizedPageSize });

        var messages = new List<MessageDto>(normalizedPageSize);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var channelIdOrdinal = reader.GetOrdinal("ChannelID");
        var messageIdOrdinal = reader.GetOrdinal("MessageID");
        var userIdOrdinal = reader.GetOrdinal("UserID");
        var sentDateOrdinal = reader.GetOrdinal("SentDate");
        var messageTextOrdinal = reader.GetOrdinal("MessageText");
        var photoIdOrdinal = reader.GetOrdinal("PhotoID");
        var videoIdOrdinal = reader.GetOrdinal("VideoID");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            messages.Add(new MessageDto(
                reader.GetInt64(channelIdOrdinal),
                reader.GetInt64(messageIdOrdinal),
                reader.GetInt64(userIdOrdinal),
                reader.GetDateTime(sentDateOrdinal),
                reader.IsDBNull(messageTextOrdinal) ? null : reader.GetString(messageTextOrdinal),
                reader.IsDBNull(photoIdOrdinal) ? null : reader.GetInt64(photoIdOrdinal),
                reader.IsDBNull(videoIdOrdinal) ? null : reader.GetInt64(videoIdOrdinal)));
        }

        return messages;
    }
}
