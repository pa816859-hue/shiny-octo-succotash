using System;
using System.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public class TagRepository : SqlRepositoryBase, ITagRepository
{
    private const string PhotoTagQuery = @"SELECT PhotoID, Tag, Score
FROM dbo.PhotoTags
WHERE PhotoID = @PhotoId
ORDER BY Score DESC, Tag ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    private const string UserTagQuery = @"SELECT UserID, Tag, Weight
FROM dbo.UserTags
WHERE UserID = @UserId
ORDER BY Weight DESC, Tag ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    private const string TagSummaryQuery = @"WITH TagCounts AS (
    SELECT pt.Tag,
           COUNT(DISTINCT pt.PhotoID) AS PhotoCount
    FROM dbo.PhotoTags AS pt
    LEFT JOIN dbo.Messages AS m ON m.PhotoID = pt.PhotoID
    WHERE (@UserId IS NULL OR m.UserID = @UserId)
    GROUP BY pt.Tag)
SELECT Tag,
       PhotoCount
FROM TagCounts
ORDER BY PhotoCount DESC, Tag ASC
OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;";

    private const string TagDetailQuery = @"SELECT pt.Tag,
       pt.PhotoID,
       p.FilePath AS PhotoPath,
       p.AddedOn,
       pt.Score,
       m.MessageID,
       m.ChannelID,
       m.SentDate,
       m.MessageText,
       m.UserID,
       n.Username,
       n.FirstName,
       n.LastName
FROM dbo.PhotoTags AS pt
JOIN dbo.Photos AS p ON p.PhotoID = pt.PhotoID
LEFT JOIN dbo.Messages AS m ON m.PhotoID = pt.PhotoID
LEFT JOIN dbo.UserNames AS n ON n.UserID = m.UserID
WHERE pt.Tag = @Tag
ORDER BY pt.Score DESC, p.AddedOn DESC, pt.PhotoID DESC
OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;";

    public TagRepository(IDbConnectionFactory connectionFactory, ISqlCommandExecutor commandExecutor)
        : base(connectionFactory, commandExecutor)
    {
    }

    public async Task<IReadOnlyList<PhotoTagDto>> GetPhotoTagsAsync(long photoId, int offset, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedOffset = NormalizeOffset(offset);
        var normalizedPageSize = NormalizePageSize(pageSize);

        using var connection = CreateConnection();
        using var command = new SqlCommand(PhotoTagQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@PhotoId", SqlDbType.BigInt) { Value = photoId });
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = normalizedOffset });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = normalizedPageSize });

        var tags = new List<PhotoTagDto>(normalizedPageSize);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var photoIdOrdinal = reader.GetOrdinal("PhotoID");
        var tagOrdinal = reader.GetOrdinal("Tag");
        var scoreOrdinal = reader.GetOrdinal("Score");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            tags.Add(new PhotoTagDto(
                reader.GetInt64(photoIdOrdinal),
                reader.GetString(tagOrdinal),
                reader.GetDouble(scoreOrdinal)));
        }

        return tags;
    }

    public async Task<IReadOnlyList<UserTagDto>> GetUserTagsAsync(long userId, int offset, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedOffset = NormalizeOffset(offset);
        var normalizedPageSize = NormalizePageSize(pageSize);

        using var connection = CreateConnection();
        using var command = new SqlCommand(UserTagQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = userId });
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = normalizedOffset });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = normalizedPageSize });

        var tags = new List<UserTagDto>(normalizedPageSize);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var userIdOrdinal = reader.GetOrdinal("UserID");
        var tagOrdinal = reader.GetOrdinal("Tag");
        var weightOrdinal = reader.GetOrdinal("Weight");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            tags.Add(new UserTagDto(
                reader.GetInt64(userIdOrdinal),
                reader.GetString(tagOrdinal),
                reader.GetInt32(weightOrdinal)));
        }

        return tags;
    }

    public async Task<IReadOnlyList<TagSummaryDto>> GetTagSummariesAsync(int offset, int limit, long? userId, CancellationToken cancellationToken = default)
    {
        var normalizedOffset = NormalizeOffset(offset);
        var normalizedLimit = NormalizePageSize(limit);

        using var connection = CreateConnection();
        using var command = new SqlCommand(TagSummaryQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = normalizedOffset });
        command.Parameters.Add(new SqlParameter("@Limit", SqlDbType.Int) { Value = normalizedLimit });
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt)
        {
            Value = userId.HasValue ? userId.Value : DBNull.Value
        });

        var summaries = new List<TagSummaryDto>(normalizedLimit);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var tagOrdinal = reader.GetOrdinal("Tag");
        var countOrdinal = reader.GetOrdinal("PhotoCount");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            summaries.Add(new TagSummaryDto(
                reader.GetString(tagOrdinal),
                reader.GetInt32(countOrdinal),
                null,
                null,
                null,
                null));
        }

        return summaries;
    }

    public async Task<IReadOnlyList<TagDetailDto>> GetTagDetailsAsync(string tag, int offset, int limit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag cannot be null or whitespace.", nameof(tag));
        }

        var normalizedOffset = NormalizeOffset(offset);
        var normalizedLimit = NormalizePageSize(limit);

        using var connection = CreateConnection();
        using var command = new SqlCommand(TagDetailQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@Tag", SqlDbType.NVarChar, 255) { Value = tag });
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = normalizedOffset });
        command.Parameters.Add(new SqlParameter("@Limit", SqlDbType.Int) { Value = normalizedLimit });

        var details = new List<TagDetailDto>(normalizedLimit);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var tagOrdinal = reader.GetOrdinal("Tag");
        var photoIdOrdinal = reader.GetOrdinal("PhotoID");
        var photoPathOrdinal = reader.GetOrdinal("PhotoPath");
        var addedOnOrdinal = reader.GetOrdinal("AddedOn");
        var scoreOrdinal = reader.GetOrdinal("Score");
        var messageIdOrdinal = reader.GetOrdinal("MessageID");
        var channelIdOrdinal = reader.GetOrdinal("ChannelID");
        var sentDateOrdinal = reader.GetOrdinal("SentDate");
        var messageTextOrdinal = reader.GetOrdinal("MessageText");
        var userIdOrdinal = reader.GetOrdinal("UserID");
        var usernameOrdinal = reader.GetOrdinal("Username");
        var firstNameOrdinal = reader.GetOrdinal("FirstName");
        var lastNameOrdinal = reader.GetOrdinal("LastName");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            details.Add(new TagDetailDto(
                reader.GetString(tagOrdinal),
                reader.GetInt64(photoIdOrdinal),
                reader.GetString(photoPathOrdinal),
                reader.GetDateTime(addedOnOrdinal),
                reader.GetDouble(scoreOrdinal),
                reader.IsDBNull(messageIdOrdinal) ? null : reader.GetInt64(messageIdOrdinal),
                reader.IsDBNull(channelIdOrdinal) ? null : reader.GetInt64(channelIdOrdinal),
                reader.IsDBNull(sentDateOrdinal) ? null : reader.GetDateTime(sentDateOrdinal),
                reader.IsDBNull(messageTextOrdinal) ? null : reader.GetString(messageTextOrdinal),
                reader.IsDBNull(userIdOrdinal) ? null : reader.GetInt64(userIdOrdinal),
                reader.IsDBNull(usernameOrdinal) ? null : reader.GetString(usernameOrdinal),
                reader.IsDBNull(firstNameOrdinal) ? null : reader.GetString(firstNameOrdinal),
                reader.IsDBNull(lastNameOrdinal) ? null : reader.GetString(lastNameOrdinal)));
        }

        return details;
    }
}
