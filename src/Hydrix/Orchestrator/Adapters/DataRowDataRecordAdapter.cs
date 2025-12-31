using System;
using System.Data;

namespace Hydrix.Orchestrator.Adapters
{
    /// <summary>
    /// Adapts a <see cref="DataRow"/> instance to the <see cref="IDataRecord"/> interface, allowing
    /// SQL-to-entity mapping logic to operate uniformly over both <see cref="DataRow"/> and <see
    /// cref="IDataReader"/> sources.
    ///
    /// This adapter enables reuse of mapping engines designed for streaming <see
    /// cref="IDataRecord"/> access without duplicating logic for tabular <see
    /// cref="DataTable"/>-based results.
    ///
    /// The class provides a lightweight, read-only view over the underlying <see cref="DataRow"/>,
    /// exposing column metadata and values using standard ADO.NET conventions.
    /// </summary>
    /// <remarks>
    /// This adapter is especially useful when migrating from DataTable-based materialization to
    /// DataReader-based streaming, allowing a single mapping pipeline to support both scenarios.
    ///
    /// The implementation intentionally avoids write operations and provider-specific behaviors,
    /// focusing solely on value access and metadata resolution.
    /// </remarks>
    internal sealed class DataRowDataRecordAdapter
        : IDataRecord
    {
        /// <summary>
        /// Represents a row of data in a System.Data.DataTable.
        /// </summary>
        private readonly DataRow _row;

        /// <summary>
        /// Represents a collection of System.Data.DataColumn objects for a System.Data.DataTable.
        /// </summary>
        private readonly DataColumnCollection _columns;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowDataRecordAdapter"/> class wrapping
        /// the specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="row">The <see cref="DataRow"/> instance to be exposed as an <see cref="IDataRecord"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="row"/> is <c>null</c>.</exception>
        public DataRowDataRecordAdapter(DataRow row)
        {
            _row = row ?? throw new ArgumentNullException(nameof(row));
            _columns = row.Table.Columns;
        }

        /// <summary>
        /// Gets the number of columns in the current record.
        /// </summary>
        public int FieldCount
            => _columns.Count;

        /// <summary>
        /// Gets the value of the specified column by ordinal position.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The column value.</returns>
        public object this[int i]
            => _row[i];

        /// <summary>
        /// Gets the value of the specified column by column name.
        /// </summary>
        /// <param name="name">The column name.</param>
        /// <returns>The column value.</returns>
        public object this[string name]
            => _row[name];

        /// <summary>
        /// Gets the name of the column at the specified ordinal.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The column name.</returns>
        public string GetName(int i)
            => _columns[i].ColumnName;

        /// <summary>
        /// Gets the database-specific data type name of the column at the specified ordinal.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The data type name.</returns>
        public string GetDataTypeName(int i)
            => _columns[i].DataType.Name;

        /// <summary>
        /// Gets the CLR type of the column at the specified ordinal.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The <see cref="Type"/> of the column.</returns>
        public Type GetFieldType(int i)
            => _columns[i].DataType;

        /// <summary>
        /// Gets the value of the column at the specified ordinal.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The column value.</returns>
        public object GetValue(int i)
            => _row[i];

        /// <summary>
        /// Populates an array with the values of all columns in the current record.
        /// </summary>
        /// <param name="values">The array to populate with column values.</param>
        /// <returns>The number of values copied into the array.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="values"/> is <c>null</c>.
        /// </exception>
        public int GetValues(object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var count = Math.Min(values.Length, FieldCount);

            for (int i = 0; i < count; i++)
                values[i] = _row[i];

            return count;
        }

        /// <summary>
        /// Gets the column ordinal given the column name.
        /// </summary>
        /// <param name="name">The column name.</param>
        /// <returns>The zero-based column ordinal.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the specified column name does not exist.
        /// </exception>
        public int GetOrdinal(string name)
        {
            if (!_columns.Contains(name))
                throw new IndexOutOfRangeException($"Column '{name}' not found.");

            return _columns[name].Ordinal;
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="bool"/>.
        /// </summary>
        public bool GetBoolean(int i)
            => Convert.ToBoolean(_row[i]);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="byte"/>.
        /// </summary>
        public byte GetByte(int i)
            => Convert.ToByte(_row[i]);

        /// <summary>
        /// Not supported for <see cref="DataRow"/>-based records.
        /// </summary>
        public long GetBytes(
            int i,
            long fieldOffset,
            byte[] buffer,
            int bufferoffset,
            int length)
            => throw new NotSupportedException();

        /// <summary>
        /// Gets the value of the specified column as a <see cref="char"/>.
        /// </summary>
        public char GetChar(int i)
            => Convert.ToChar(_row[i]);

        /// <summary>
        /// Not supported for <see cref="DataRow"/>-based records.
        /// </summary>
        public long GetChars(
            int i,
            long fieldoffset,
            char[] buffer,
            int bufferoffset,
            int length)
            => throw new NotSupportedException();

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Guid"/>.
        /// </summary>
        public Guid GetGuid(int i)
            => _row[i] is Guid g ? g : Guid.Parse(_row[i].ToString());

        /// <summary>
        /// Gets the value of the specified column as a <see cref="short"/>.
        /// </summary>
        public short GetInt16(int i)
            => Convert.ToInt16(_row[i]);

        /// <summary>
        /// Gets the value of the specified column as an <see cref="int"/>.
        /// </summary>
        public int GetInt32(int i)
            => Convert.ToInt32(_row[i]);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="long"/>.
        /// </summary>
        public long GetInt64(int i)
            => Convert.ToInt64(_row[i]);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="float"/>.
        /// </summary>
        public float GetFloat(int i)
            => Convert.ToSingle(_row[i]);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="double"/>.
        /// </summary>
        public double GetDouble(int i)
            => Convert.ToDouble(_row[i]);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="string"/>.
        /// </summary>
        public string GetString(int i)
            => Convert.ToString(_row[i]);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="decimal"/>.
        /// </summary>
        public decimal GetDecimal(int i)
            => Convert.ToDecimal(_row[i]);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="DateTime"/>.
        /// </summary>
        public DateTime GetDateTime(int i)
            => Convert.ToDateTime(_row[i]);

        /// <summary>
        /// Not supported for <see cref="DataRow"/>-based records.
        /// </summary>
        public IDataReader GetData(int i)
            => throw new NotSupportedException();

        /// <summary>
        /// Determines whether the specified column contains a database null value.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns><c>true</c> if the column value is <see cref="DBNull"/>; otherwise, <c>false</c>.</returns>
        public bool IsDBNull(int i)
            => Convert.IsDBNull(_row[i]);
    }
}