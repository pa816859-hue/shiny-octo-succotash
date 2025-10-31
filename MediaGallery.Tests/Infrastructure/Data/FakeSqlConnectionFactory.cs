using System.Data;
using MediaGallery.Web.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Tests.Infrastructure.Data;

internal sealed class FakeSqlConnectionFactory : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection();
}
