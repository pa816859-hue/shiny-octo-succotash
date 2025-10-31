using System.Data;
using MediaGallery.Web.Infrastructure.Data.Dto;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public class UserRepository : SqlRepositoryBase, IUserRepository
{
    private const string UserQuery = @"SELECT u.UserID, u.LastUpdate, n.FirstName, n.LastName, n.Username
FROM dbo.Users AS u
LEFT JOIN dbo.UserNames AS n ON n.UserID = u.UserID
ORDER BY u.LastUpdate DESC, u.UserID DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    public UserRepository(IDbConnectionFactory connectionFactory, ISqlCommandExecutor commandExecutor)
        : base(connectionFactory, commandExecutor)
    {
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(int offset, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedOffset = NormalizeOffset(offset);
        var normalizedPageSize = NormalizePageSize(pageSize);

        using var connection = CreateConnection();
        using var command = new SqlCommand(UserQuery, connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = normalizedOffset });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = normalizedPageSize });

        var users = new List<UserDto>(normalizedPageSize);

        await using var reader = await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        var userIdOrdinal = reader.GetOrdinal("UserID");
        var lastUpdateOrdinal = reader.GetOrdinal("LastUpdate");
        var firstNameOrdinal = reader.GetOrdinal("FirstName");
        var lastNameOrdinal = reader.GetOrdinal("LastName");
        var usernameOrdinal = reader.GetOrdinal("Username");

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            users.Add(new UserDto(
                reader.GetInt64(userIdOrdinal),
                reader.GetDateTime(lastUpdateOrdinal),
                reader.IsDBNull(firstNameOrdinal) ? null : reader.GetString(firstNameOrdinal),
                reader.IsDBNull(lastNameOrdinal) ? null : reader.GetString(lastNameOrdinal),
                reader.IsDBNull(usernameOrdinal) ? null : reader.GetString(usernameOrdinal)));
        }

        return users;
    }
}
