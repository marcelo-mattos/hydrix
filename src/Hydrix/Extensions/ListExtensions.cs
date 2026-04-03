using Hydrix.Caching;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Extensions
{
    /// <summary>
    /// Provides extension methods for list-based operations used by Hydrix.
    /// </summary>
    internal static class ListExtensions
    {
        /// <summary>
        /// Maps an <see cref="ITable"/> list into a <see cref="DataTable"/> to improve bulk insert operations.
        /// </summary>
        /// <typeparam name="TEntity">Represents a SQL table entity that holds the data to convert.</typeparam>
        /// <param name="entities">The list of entities to convert.</param>
        /// <returns>A <see cref="DataTable"/> instance containing the mapped data.</returns>
        internal static DataTable MapTo<TEntity>(
            this IList<TEntity> entities)
            where TEntity : ITable, new()
        {
            var map = DataColumnMapCache<TEntity>.GetOrCreate();
            var dataTable = new DataTable();

            foreach (var column in map.Columns)
            {
                dataTable.Columns.Add(
                    column.ColumnName,
                    column.DataType);
            }

            if (entities == null)
                return dataTable;

            foreach (var entity in entities)
            {
                var row = dataTable.NewRow();

                for (var index = 0; index < map.Columns.Length; index++)
                {
                    var column = map.Columns[index];
                    row[index] = column.Getter(entity) ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
