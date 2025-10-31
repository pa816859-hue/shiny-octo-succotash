using System.Data;
using MediaGallery.Web.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace MediaGallery.Web.Services;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly IOptionsMonitor<DatabaseOptions> _options;

    public SqlConnectionFactory(IOptionsMonitor<DatabaseOptions> options)
    {
        _options = options;
    }

    public IDbConnection CreateConnection()
    {
        var connectionString = _options.CurrentValue.Default;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The default database connection string has not been configured.");
        }

        return new SqlConnection(connectionString);
    }
}
