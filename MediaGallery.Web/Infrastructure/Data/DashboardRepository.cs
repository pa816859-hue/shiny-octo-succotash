using System;
using System.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public sealed class DashboardRepository : SqlRepositoryBase, IDashboardRepository
{
    private const string SummaryQuery = @"SELECT
    (SELECT COUNT(*) FROM dbo.Messages) AS TotalMessages,
    (SELECT COUNT(*) FROM dbo.Photos) AS TotalPhotos,
    (SELECT COUNT(*) FROM dbo.Videos) AS TotalVideos,
    (SELECT COUNT(*) FROM dbo.MonitoredChannels WHERE IsActive = 1) AS ActiveChannels,
    (SELECT COUNT(*) FROM dbo.Users) AS TotalUsers,
    (SELECT MAX(SentDate) FROM dbo.Messages) AS LastMessageSentAt;";

    public DashboardRepository(IDbConnectionFactory connectionFactory, ISqlCommandExecutor commandExecutor)
        : base(connectionFactory, commandExecutor)
    {
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        using var command = new SqlCommand(SummaryQuery, connection)
        {
            CommandType = CommandType.Text
        };

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);

        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return new DashboardSummaryDto(0, 0, 0, 0, 0, null);
        }

        var totalMessages = reader.GetInt64(reader.GetOrdinal("TotalMessages"));
        var totalPhotos = reader.GetInt64(reader.GetOrdinal("TotalPhotos"));
        var totalVideos = reader.GetInt64(reader.GetOrdinal("TotalVideos"));
        var activeChannels = reader.GetInt64(reader.GetOrdinal("ActiveChannels"));
        var totalUsers = reader.GetInt64(reader.GetOrdinal("TotalUsers"));
        var lastMessageOrdinal = reader.GetOrdinal("LastMessageSentAt");
        var lastMessageSentAt = reader.IsDBNull(lastMessageOrdinal) ? (DateTime?)null : reader.GetDateTime(lastMessageOrdinal);

        return new DashboardSummaryDto(totalMessages, totalPhotos, totalVideos, activeChannels, totalUsers, lastMessageSentAt);
    }
}
