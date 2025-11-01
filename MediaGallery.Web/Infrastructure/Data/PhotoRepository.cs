using System;
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

public class PhotoRepository : SqlRepositoryBase, IPhotoRepository
{
    private const string RandomPhotosQuery = @"SELECT TOP (@PageSize) PhotoID, FilePath, AverageHash, DifferenceHash, PerceptualHash, AddedOn
FROM dbo.Photos
ORDER BY NEWID();";

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

    public async Task<IReadOnlyList<PhotoDto>> GetRandomPhotosAsync(int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageSize = NormalizePageSize(pageSize);

        using var connection = CreateConnection();
        using var command = new SqlCommand(RandomPhotosQuery, connection)
        {
            CommandType = CommandType.Text
        };

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

    public async Task<IReadOnlyList<PhotoDto>> GetPhotosByIdsAsync(IEnumerable<long> photoIds, CancellationToken cancellationToken = default)
    {
        if (photoIds is null)
        {
            throw new ArgumentNullException(nameof(photoIds));
        }

        var distinctIds = photoIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (distinctIds.Length == 0)
        {
            return Array.Empty<PhotoDto>();
        }

        var commandTextBuilder = new StringBuilder();
        commandTextBuilder.Append("SELECT PhotoID, FilePath, AverageHash, DifferenceHash, PerceptualHash, AddedOn FROM dbo.Photos WHERE PhotoID IN (");

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

        commandTextBuilder.Append(") ORDER BY AddedOn DESC, PhotoID DESC;");

        using var connection = CreateConnection();
        using var command = new SqlCommand(commandTextBuilder.ToString(), connection)
        {
            CommandType = CommandType.Text
        };

        for (var index = 0; index < distinctIds.Length; index++)
        {
            command.Parameters.Add(new SqlParameter(parameterNames[index], SqlDbType.BigInt) { Value = distinctIds[index] });
        }

        var photos = new List<PhotoDto>(distinctIds.Length);

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
