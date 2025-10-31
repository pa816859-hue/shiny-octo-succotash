using System.Collections;
using System.Data.Common;

namespace MediaGallery.Tests.Infrastructure.Data;

internal sealed class FakeDbDataReader : DbDataReader
{
    private readonly IReadOnlyList<FakeColumn> _columns;
    private readonly IReadOnlyList<object?[]> _rows;
    private int _position = -1;

    private FakeDbDataReader(IReadOnlyList<FakeColumn> columns, IReadOnlyList<object?[]> rows)
    {
        _columns = columns;
        _rows = rows;
    }

    public override int FieldCount => _columns.Count;

    public override bool HasRows => _rows.Count > 0;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override bool Read()
    {
        if (_position + 1 >= _rows.Count)
        {
            return false;
        }

        _position++;
        return true;
    }

    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        => Task.FromResult(Read());

    public override bool NextResult() => false;

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        => Task.FromResult(false);

    public override int Depth => 0;

    public override bool IsClosed => false;

    public override int RecordsAffected => 0;

    public override string GetName(int ordinal) => _columns[ordinal].Name;

    public override string GetDataTypeName(int ordinal) => _columns[ordinal].Type.Name;

    public override Type GetFieldType(int ordinal) => _columns[ordinal].Type;

    public override object GetValue(int ordinal)
    {
        if (_position < 0 || _position >= _rows.Count)
        {
            throw new InvalidOperationException("No current row is available.");
        }

        return _rows[_position][ordinal] ?? DBNull.Value;
    }

    public override int GetValues(object[] values)
    {
        if (_position < 0 || _position >= _rows.Count)
        {
            return 0;
        }

        var currentRow = _rows[_position];
        var length = Math.Min(values.Length, currentRow.Length);
        Array.Copy(currentRow, values, length);
        return length;
    }

    public override bool IsDBNull(int ordinal) => GetValue(ordinal) is DBNull;

    public override int GetOrdinal(string name)
    {
        for (var i = 0; i < _columns.Count; i++)
        {
            if (string.Equals(_columns[i].Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new IndexOutOfRangeException($"Column '{name}' was not found.");
    }

    public override IEnumerator GetEnumerator() => new DbEnumerator(this, closeReader: false);

    public override string GetString(int ordinal) => Convert.ToString(GetValue(ordinal)) ?? string.Empty;

    public override long GetInt64(int ordinal) => Convert.ToInt64(GetValue(ordinal));

    public override int GetInt32(int ordinal) => Convert.ToInt32(GetValue(ordinal));

    public override double GetDouble(int ordinal) => Convert.ToDouble(GetValue(ordinal));

    public override DateTime GetDateTime(int ordinal) => Convert.ToDateTime(GetValue(ordinal));

    public override ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    private readonly record struct FakeColumn(string Name, Type Type);

    public sealed class Builder
    {
        private readonly List<FakeColumn> _columns = new();
        private readonly List<object?[]> _rows = new();

        public Builder WithColumn(string name, Type type)
        {
            _columns.Add(new FakeColumn(name, type));
            return this;
        }

        public Builder WithRow(params object?[] values)
        {
            if (values.Length != _columns.Count)
            {
                throw new InvalidOperationException("Row value count must match the number of configured columns.");
            }

            _rows.Add(values);
            return this;
        }

        public FakeDbDataReader Build() => new(_columns, _rows);
    }
}
