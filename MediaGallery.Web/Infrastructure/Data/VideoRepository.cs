using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Infrastructure.Data.Dto;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public class VideoRepository : SqlRepositoryBase, IVideoRepository
{
    private const string LatestVideosQuery = @"SELECT TOP (@PageSize) VideoID, FilePath, AddedOn
FROM dbo.Videos
ORDER BY AddedOn DESC, VideoID DESC;";

    public VideoRepository(IDbConnectionFactory connectionFactory, ISqlCommandExecutor commandExecutor)
        : base(connectionFactory, commandExecutor)
    {
    }

    public async Task<IReadOnlyList<VideoDto>> GetLatestVideosAsync(int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageSize = NormalizePageSize(pageSize);

        using var connection = CreateConnection();
        using var command = new SqlCommand(LatestVideosQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = normalizedPageSize });

        var videos = new List<VideoDto>(normalizedPageSize);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var idOrdinal = reader.GetOrdinal("VideoID");
        var pathOrdinal = reader.GetOrdinal("FilePath");
        var addedOnOrdinal = reader.GetOrdinal("AddedOn");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            videos.Add(new VideoDto(
                reader.GetInt64(idOrdinal),
                reader.GetString(pathOrdinal),
                reader.GetDateTime(addedOnOrdinal)));
        }

        return videos;
    }

    public async Task<IReadOnlyList<VideoDto>> GetVideosByIdsAsync(IEnumerable<long> videoIds, CancellationToken cancellationToken = default)
    {
        if (videoIds is null)
        {
            throw new ArgumentNullException(nameof(videoIds));
        }

        var distinctIds = videoIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (distinctIds.Length == 0)
        {
            return Array.Empty<VideoDto>();
        }

        var commandTextBuilder = new StringBuilder();
        commandTextBuilder.Append("SELECT VideoID, FilePath, AddedOn FROM dbo.Videos WHERE VideoID IN (");

        var parameterNames = new string[distinctIds.Length];
        for (var index = 0; index < distinctIds.Length; index++)
        {
            if (index > 0)
            {
                commandTextBuilder.Append(", ");
            }

            var parameterName = "@Id" + index.ToString(CultureInfo.InvariantCulture);
            parameterNames[index] = parameterName;
            commandTextBuilder.Append(parameterName);
        }

        commandTextBuilder.Append(") ORDER BY AddedOn DESC, VideoID DESC;");

        using var connection = CreateConnection();
        using var command = new SqlCommand(commandTextBuilder.ToString(), connection)
        {
            CommandType = CommandType.Text
        };

        for (var index = 0; index < distinctIds.Length; index++)
        {
            command.Parameters.Add(new SqlParameter(parameterNames[index], SqlDbType.BigInt) { Value = distinctIds[index] });
        }

        var videos = new List<VideoDto>(distinctIds.Length);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var idOrdinal = reader.GetOrdinal("VideoID");
        var pathOrdinal = reader.GetOrdinal("FilePath");
        var addedOnOrdinal = reader.GetOrdinal("AddedOn");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            videos.Add(new VideoDto(
                reader.GetInt64(idOrdinal),
                reader.GetString(pathOrdinal),
                reader.GetDateTime(addedOnOrdinal)));
        }

        return videos;
    }
}
