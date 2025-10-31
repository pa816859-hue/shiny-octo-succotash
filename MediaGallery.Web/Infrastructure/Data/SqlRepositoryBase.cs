using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public abstract class SqlRepositoryBase
{
    private const int DefaultMaxPageSize = 500;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ISqlCommandExecutor _commandExecutor;

    protected SqlRepositoryBase(IDbConnectionFactory connectionFactory, ISqlCommandExecutor commandExecutor)
    {
        _connectionFactory = connectionFactory;
        _commandExecutor = commandExecutor;
    }

    protected SqlConnection CreateConnection()
    {
        if (_connectionFactory.CreateConnection() is not SqlConnection connection)
        {
            throw new InvalidOperationException("The configured connection factory must return a SqlConnection instance.");
        }

        return connection;
    }

    protected Task<DbDataReader> ExecuteReaderAsync(SqlCommand command, CancellationToken cancellationToken)
        => _commandExecutor.ExecuteReaderAsync(command, cancellationToken);

    protected static int NormalizeOffset(int offset)
        => offset < 0 ? 0 : offset;

    protected static int NormalizePageSize(int pageSize, int? maxPageSize = null)
    {
        var limit = maxPageSize.GetValueOrDefault(DefaultMaxPageSize);
        if (pageSize <= 0)
        {
            return 1;
        }

        if (pageSize > limit)
        {
            return limit;
        }

        return pageSize;
    }
}
