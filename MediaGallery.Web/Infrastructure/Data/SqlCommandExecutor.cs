using System.Data;
using System.Data.Common;
using MediaGallery.Web.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace MediaGallery.Web.Infrastructure.Data;

public class SqlCommandExecutor : ISqlCommandExecutor
{
    private readonly IOptionsMonitor<DatabaseOptions> _options;

    public SqlCommandExecutor(IOptionsMonitor<DatabaseOptions> options)
    {
        _options = options;
    }

    public async Task<DbDataReader> ExecuteReaderAsync(SqlCommand command, CancellationToken cancellationToken)
    {
        if (command.Connection is null)
        {
            throw new InvalidOperationException("A SQL command requires an associated connection before execution.");
        }

        var timeoutSeconds = _options.CurrentValue.CommandTimeoutSeconds;
        if (timeoutSeconds <= 0)
        {
            timeoutSeconds = 30;
        }

        command.CommandTimeout = timeoutSeconds;

        if (command.Connection.State != ConnectionState.Open)
        {
            await command.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken)
            .ConfigureAwait(false);
    }
}
