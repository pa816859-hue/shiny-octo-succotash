using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Web.Infrastructure.Data;

public interface ISqlCommandExecutor
{
    Task<DbDataReader> ExecuteReaderAsync(SqlCommand command, CancellationToken cancellationToken);
}
