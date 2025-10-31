using System.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public class MessageRepository : SqlRepositoryBase, IMessageRepository
{
    private const string RecentMessagesQuery = @"SELECT m.ChannelID,
       m.MessageID,
       m.UserID,
       n.Username,
       n.FirstName,
       n.LastName,
       m.SentDate,
       m.MessageText,
       m.PhotoID,
       p.FilePath AS PhotoPath,
       m.VideoID,
       v.FilePath AS VideoPath
FROM dbo.Messages AS m
LEFT JOIN dbo.UserNames AS n ON n.UserID = m.UserID
LEFT JOIN dbo.Photos AS p ON p.PhotoID = m.PhotoID
LEFT JOIN dbo.Videos AS v ON v.VideoID = m.VideoID
WHERE (@ChannelId IS NULL OR m.ChannelID = @ChannelId)
  AND (@UserId IS NULL OR m.UserID = @UserId)
  AND (@MediaOnly = 0 OR m.PhotoID IS NOT NULL OR m.VideoID IS NOT NULL)
ORDER BY
    CASE WHEN @SortAscending = 1 THEN m.SentDate END ASC,
    CASE WHEN @SortAscending = 1 THEN m.MessageID END ASC,
    CASE WHEN @SortAscending = 0 THEN m.SentDate END DESC,
    CASE WHEN @SortAscending = 0 THEN m.MessageID END DESC
OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;";

    public MessageRepository(IDbConnectionFactory connectionFactory, ISqlCommandExecutor commandExecutor)
        : base(connectionFactory, commandExecutor)
    {
    }

    public async Task<IReadOnlyList<MessageDetailDto>> GetRecentMessagesAsync(
        int offset,
        int limit,
        long? channelId,
        long? userId,
        bool sortAscending,
        bool mediaOnly,
        CancellationToken cancellationToken = default)
    {
        var normalizedOffset = NormalizeOffset(offset);
        var normalizedLimit = NormalizePageSize(limit);

        using var connection = CreateConnection();
        using var command = new SqlCommand(RecentMessagesQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = normalizedOffset });
        command.Parameters.Add(new SqlParameter("@Limit", SqlDbType.Int) { Value = normalizedLimit });
        command.Parameters.Add(new SqlParameter("@ChannelId", SqlDbType.BigInt)
        {
            Value = channelId.HasValue ? channelId.Value : DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt)
        {
            Value = userId.HasValue ? userId.Value : DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@MediaOnly", SqlDbType.Bit) { Value = mediaOnly ? 1 : 0 });
        command.Parameters.Add(new SqlParameter("@SortAscending", SqlDbType.Bit) { Value = sortAscending ? 1 : 0 });

        var messages = new List<MessageDetailDto>(normalizedLimit);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var channelIdOrdinal = reader.GetOrdinal("ChannelID");
        var messageIdOrdinal = reader.GetOrdinal("MessageID");
        var userIdOrdinal = reader.GetOrdinal("UserID");
        var usernameOrdinal = reader.GetOrdinal("Username");
        var firstNameOrdinal = reader.GetOrdinal("FirstName");
        var lastNameOrdinal = reader.GetOrdinal("LastName");
        var sentDateOrdinal = reader.GetOrdinal("SentDate");
        var messageTextOrdinal = reader.GetOrdinal("MessageText");
        var photoIdOrdinal = reader.GetOrdinal("PhotoID");
        var photoPathOrdinal = reader.GetOrdinal("PhotoPath");
        var videoIdOrdinal = reader.GetOrdinal("VideoID");
        var videoPathOrdinal = reader.GetOrdinal("VideoPath");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            messages.Add(new MessageDetailDto(
                reader.GetInt64(channelIdOrdinal),
                reader.GetInt64(messageIdOrdinal),
                reader.GetInt64(userIdOrdinal),
                reader.IsDBNull(usernameOrdinal) ? null : reader.GetString(usernameOrdinal),
                reader.IsDBNull(firstNameOrdinal) ? null : reader.GetString(firstNameOrdinal),
                reader.IsDBNull(lastNameOrdinal) ? null : reader.GetString(lastNameOrdinal),
                reader.GetDateTime(sentDateOrdinal),
                reader.IsDBNull(messageTextOrdinal) ? null : reader.GetString(messageTextOrdinal),
                reader.IsDBNull(photoIdOrdinal) ? null : reader.GetInt64(photoIdOrdinal),
                reader.IsDBNull(photoPathOrdinal) ? null : reader.GetString(photoPathOrdinal),
                reader.IsDBNull(videoIdOrdinal) ? null : reader.GetInt64(videoIdOrdinal),
                reader.IsDBNull(videoPathOrdinal) ? null : reader.GetString(videoPathOrdinal)));
        }

        return messages;
    }
}
