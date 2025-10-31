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
}
