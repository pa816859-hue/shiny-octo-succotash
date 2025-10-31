using System.Data.Common;
using MediaGallery.Web.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace MediaGallery.Tests.Infrastructure.Data;

internal sealed class FakeSqlCommandExecutor : ISqlCommandExecutor
{
    private readonly Queue<DbDataReader> _resultSets;

    public FakeSqlCommandExecutor(IEnumerable<DbDataReader> resultSets)
    {
        _resultSets = new Queue<DbDataReader>(resultSets);
    }

    public List<IReadOnlyDictionary<string, object?>> CapturedParameters { get; } = new();

    public Task<DbDataReader> ExecuteReaderAsync(SqlCommand command, CancellationToken cancellationToken)
    {
        CapturedParameters.Add(command.Parameters
            .OfType<SqlParameter>()
            .ToDictionary(p => p.ParameterName, p => p.Value));

        if (_resultSets.Count > 0)
        {
            return Task.FromResult(_resultSets.Dequeue());
        }

        var builder = new FakeDbDataReader.Builder();
        return Task.FromResult<DbDataReader>(builder.Build());
    }
}
