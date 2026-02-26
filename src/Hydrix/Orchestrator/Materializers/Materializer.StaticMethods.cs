using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// Provides static methods for converting between SQL result sets and entity representations, enabling efficient
    /// materialization of data and bulk operations.
    /// </summary>
    /// <remarks>The Materializer class includes methods to convert data from IDataReader and DataTable into
    /// lists of SQL-mapped entities, as well as to create DataTable objects from entity lists. These methods are
    /// optimized for performance and low memory usage, making them suitable for scenarios such as bulk inserts and
    /// streaming data access. The class also offers validation utilities to ensure entities are properly configured for
    /// SQL operations.</remarks>
    public partial class Materializer :
        Contract.IMaterializer
    {
        /// <summary>
        /// Converts a <see cref="IDataReader"/> result set into a list of
        /// <see cref="ITable"/> instances using streaming access.
        ///
        /// This method iterates through the <see cref="IDataReader"/> sequentially,
        /// materializing each row into a new entity instance based on precomputed
        /// SQL entity metadata.
        ///
        /// It is optimized for performance and low memory consumption, avoiding
        /// intermediate structures such as <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a SQL-mapped entity type that implements <see cref="ITable"/>.
        /// </typeparam>
        /// <param name="dataReader">
        /// The <see cref="IDataReader"/> containing the result set to be mapped.
        /// </param>
        /// <returns>
        /// A list of entities populated from the data reader result set.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dataReader"/> is <c>null</c>.
        /// </exception>
        public static IList<TEntity> ConvertDataReaderToEntities<TEntity>(
            IDataReader dataReader)
            where TEntity : ITable, new()
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                dataReader);
#else
            if (dataReader == null)
                throw new ArgumentNullException(nameof(dataReader));
#endif

            var entities = new List<TEntity>();
            var type = typeof(TEntity);
            var metadata = EntityMetadataCache.GetOrAdd(type);

            var ordinals = BuildOrdinals(dataReader);

            while (dataReader.Read())
            {
                var entity = new TEntity();

                TableMap.SetEntity(
                    entity,
                    dataReader,
                    metadata,
                    string.Empty,
                    ordinals
                );

                entities.Add(entity);
            }

            return entities;
        }

        /// <summary> /// Converts a DataTable into an ITable list. /// </summary>
        /// /// <typeparam name="TEntity">Represents a Sql Table that holds the data to
        /// be parsed from the DataSet result.</typeparam> /// <param
        /// name="dataTable">An System.Data.DataTable object.</param> /// <returns>A
        /// list of Sql Table that holds the data to be parsed from the DataSet
        /// result.</returns>
        public static IList<TEntity> ConvertDataTableToEntity<TEntity>(
            DataTable dataTable)
            where TEntity : ITable, new()
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return new List<TEntity>();

            using var dataReader = dataTable.CreateDataReader();

            return ConvertDataReaderToEntities<TEntity>(
                dataReader);
        }

        /// <summary>
        /// Converts an ITable list into a DataTable to improve Bulk Insert operations.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="entities">ITable list with data to convert.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        internal static DataTable ConvertEntityToDataTable<TEntity>(IList<TEntity> entities)
            where TEntity : ITable, new()
        {
            var map = DataColumnMapCache<TEntity>.GetOrCreate();
            var dataTable = new DataTable();

            for (var index = 0; index < map.Columns.Length; index++)
            {
                var column = map.Columns[index];
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

        /// <summary>
        /// Creates a dictionary that maps column names to their ordinal positions in the specified data reader.
        /// </summary>
        /// <remarks>The returned dictionary uses ordinal, case-insensitive string comparison for column
        /// names. This can be useful for efficient column lookup when processing data from the reader.</remarks>
        /// <param name="reader">The data reader from which to retrieve column names and their ordinal positions. Must not be null.</param>
        /// <returns>A dictionary containing column names as keys and their corresponding zero-based ordinal positions as values.
        /// The dictionary is case-insensitive with respect to column names.</returns>
        private static Dictionary<string, int> BuildOrdinals(
            IDataReader reader)
        {
            var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (!string.IsNullOrWhiteSpace(name))
                    ordinals.TryAdd(name, i);
            }

            return ordinals;
        }

        /// <summary>
        /// Validates whether the specified <typeparamref name="TEntity"/> type is properly
        /// configured to be used in an ExecuteEntity operation.
        ///
        /// The method ensures that:
        /// <list type="bullet">
        /// <item>
        /// The entity type is decorated with <see cref="ForeignTableAttribute"/>.
        /// </item>
        /// <item>
        /// The entity exposes at least one readable property decorated with
        /// <see cref="ColumnAttribute"/>.
        /// </item>
        /// </list>
        ///
        /// If the entity does not declare <see cref="ForeignTableAttribute"/>, a
        /// <see cref="MissingMemberException"/> is thrown.
        ///
        /// The return value indicates whether the entity contains mappable SQL fields,
        /// allowing the execution flow to short-circuit when no valid fields are defined.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a SQL-mapped entity type that implements <see cref="ITable"/>.
        /// </typeparam>
        /// <returns>
        /// <c>true</c> if the entity contains at least one property mapped with
        /// <see cref="ColumnAttribute"/>; otherwise, <c>false</c>.
        /// </returns>
        private static bool ValidateEntityRequest<TEntity>()
            where TEntity : ITable, new()
        {
            var tableAttribute = typeof(TEntity)
                .GetCustomAttributes(typeof(TableAttribute), false)
                .Cast<TableAttribute>()
                .FirstOrDefault();

            if (null == tableAttribute)
                throw new MissingMemberException("The entity does not have a TableAttibute decorating itself.");

            var properties = typeof(TEntity)
                .GetProperties()
                .Where(property => property
                    .GetCustomAttributes(typeof(ColumnAttribute), false)
                    .Length != 0)
                .ToArray();

            return properties.Length > 0;
        }
    }
}