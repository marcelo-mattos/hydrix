using Hydrix.Schemas.Contract;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Extensions
{
    /// <summary>
    /// Provides extension methods for mapping rows from a DataTable to strongly typed entity lists.
    /// </summary>
    internal static class DataTableExtensions
    {
        /// <summary>
        /// Maps the rows from a <see cref="DataTable"/> into a list of entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The target entity type.</typeparam>
        /// <param name="dataTable">The table containing the rows to map.</param>
        /// <returns>A list of mapped entities, or an empty list when the table is null or has no rows.</returns>
        public static IList<TEntity> MapTo<TEntity>(
            this DataTable dataTable)
            where TEntity : ITable, new()
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return new List<TEntity>();

            using var dataReader = dataTable.CreateDataReader();
            return dataReader.MapTo<TEntity>();
        }
    }
}
