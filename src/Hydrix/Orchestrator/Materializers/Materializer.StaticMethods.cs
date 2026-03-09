using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;

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
        /// Converts the data from the specified IDataReader into a list of entities of type TEntity.
        /// </summary>
        /// <remarks>Each record in the dataReader is mapped to a new instance of TEntity using entity
        /// metadata. Ensure that the dataReader is positioned before the first record and is open before calling this
        /// method.</remarks>
        /// <typeparam name="TEntity">The type of entity to create for each record. TEntity must implement the ITable interface and have a
        /// parameterless constructor.</typeparam>
        /// <param name="dataReader">The IDataReader instance that provides the data to be converted. This parameter cannot be null.</param>
        /// <param name="limit">The maximum number of entities to create. If zero or negative, all records are converted.</param>
        /// <returns>A list of TEntity instances populated with data from the IDataReader. The list will be empty if the
        /// dataReader contains no records.</returns>
        public static IList<TEntity> ConvertDataReaderToEntities<TEntity>(
            IDataReader dataReader,
            int limit = 0)
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

            var ordinalMap = BuildOrdinals(dataReader);
            var ordinals = ordinalMap.Ordinals;
            var schemaHash = ordinalMap.SchemaHash;

            var count = 0;
            while (dataReader.Read())
            {
                if (limit > 0 && count >= limit)
                    break;

                var entity = new TEntity();

                TableMap.SetEntity(
                    entity,
                    dataReader,
                    metadata,
                    string.Empty,
                    ordinals,
                    schemaHash
                );

                entities.Add(entity);
                count++;
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
        private static OrdinalMap BuildOrdinals(
            IDataReader reader)
        {
            var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            unchecked
            {
                int hash = 17;
                for (int index = 0; index < reader.FieldCount; index++)
                {
                    var name = reader.GetName(index) ?? string.Empty;

                    ordinals[name] = index;

                    hash = (hash * 31) + index;
                    hash = (hash * 31) + name.Length;
                    hash = (hash * 31) + StringComparer.OrdinalIgnoreCase.GetHashCode(name);
                }
                hash = (hash * 31) + reader.FieldCount;

                return new OrdinalMap(
                    ordinals,
                    hash);
            }
        }

        /// <summary>
        /// Determines whether the entity request for the specified entity type is valid.
        /// </summary>
        /// <remarks>This method uses a validation cache to assess whether the request for the given
        /// entity type meets the required criteria. It is typically used to ensure that entity requests conform to
        /// expected validation rules before processing.</remarks>
        /// <typeparam name="TEntity">The type of entity to validate. Must implement the ITable interface and have a parameterless constructor.</typeparam>
        /// <returns>true if the entity request is valid; otherwise, false.</returns>
        private static bool ValidateEntityRequest<TEntity>()
            where TEntity : ITable, new()
            => EntityRequestValidationCache
                .Validate(typeof(TEntity));
    }
}