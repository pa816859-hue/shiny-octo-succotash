using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public class SqlCommandExecutor : ISqlCommandExecutor
{
    public async Task<DbDataReader> ExecuteReaderAsync(SqlCommand command, CancellationToken cancellationToken)
    {
        if (command.Connection is null)
        {
            throw new InvalidOperationException("A SQL command requires an associated connection before execution.");
        }

        if (command.Connection.State != ConnectionState.Open)
        {
            await command.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken)
            .ConfigureAwait(false);
    }
}
