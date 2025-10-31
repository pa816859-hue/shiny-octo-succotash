using System.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public class PhotoRepository : SqlRepositoryBase, IPhotoRepository
{
    private const string PhotoQuery = @"SELECT PhotoID, FilePath, AverageHash, DifferenceHash, PerceptualHash, AddedOn
FROM dbo.Photos
ORDER BY AddedOn DESC, PhotoID DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    public PhotoRepository(IDbConnectionFactory connectionFactory, ISqlCommandExecutor commandExecutor)
        : base(connectionFactory, commandExecutor)
    {
    }

    public async Task<IReadOnlyList<PhotoDto>> GetPhotosAsync(int offset, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedOffset = NormalizeOffset(offset);
        var normalizedPageSize = NormalizePageSize(pageSize);

        using var connection = CreateConnection();
        using var command = new SqlCommand(PhotoQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = normalizedOffset });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = normalizedPageSize });

        var photos = new List<PhotoDto>(normalizedPageSize);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var photoIdOrdinal = reader.GetOrdinal("PhotoID");
        var filePathOrdinal = reader.GetOrdinal("FilePath");
        var averageHashOrdinal = reader.GetOrdinal("AverageHash");
        var differenceHashOrdinal = reader.GetOrdinal("DifferenceHash");
        var perceptualHashOrdinal = reader.GetOrdinal("PerceptualHash");
        var addedOnOrdinal = reader.GetOrdinal("AddedOn");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            photos.Add(new PhotoDto(
                reader.GetInt64(photoIdOrdinal),
                reader.GetString(filePathOrdinal),
                reader.GetInt64(averageHashOrdinal),
                reader.GetInt64(differenceHashOrdinal),
                reader.GetInt64(perceptualHashOrdinal),
                reader.GetDateTime(addedOnOrdinal)));
        }

        return photos;
    }
}
