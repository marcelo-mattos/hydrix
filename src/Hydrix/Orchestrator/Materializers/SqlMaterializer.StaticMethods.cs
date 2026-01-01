using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Metadata;
using Hydrix.Schemas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// SQL Data Handler Class
    /// </summary>
    public partial class SqlMaterializer :
        Contract.ISqlMaterializer
    {
        /// <summary>
        /// Converts a <see cref="IDataReader"/> result set into a list of
        /// <see cref="ISqlEntity"/> instances using streaming access.
        ///
        /// This method iterates through the <see cref="IDataReader"/> sequentially,
        /// materializing each row into a new entity instance based on precomputed
        /// SQL entity metadata.
        ///
        /// It is optimized for performance and low memory consumption, avoiding
        /// intermediate structures such as <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a SQL-mapped entity type that implements <see cref="ISqlEntity"/>.
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
            where TEntity : ISqlEntity, new()
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                dataReader);
#else
                if (dataReader == null)
                    throw new ArgumentNullException(nameof(dataReader));
#endif

            var entities = new List<TEntity>();

            var metadata = _entityMetadataCache.GetOrAdd(
                typeof(TEntity),
                SqlEntityMetadata.BuildEntityMetadata
            );

            while (dataReader.Read())
            {
                var entity = new TEntity();

                SqlEntityMap.SetEntity(
                    entity,
                    dataReader,
                    metadata,
                    Array.Empty<string>(),
                    _entityMetadataCache
                );

                entities.Add(entity);
            }

            return entities;
        }

        /// <summary> /// Converts a DataTable into an ISqlEntity list. /// </summary>
        /// /// <typeparam name="TEntity">Represents a Sql Table that holds the data to
        /// be parsed from the DataSet result.</typeparam> /// <param
        /// name="dataTable">An System.Data.DataTable object.</param> /// <returns>A
        /// list of Sql Table that holds the data to be parsed from the DataSet
        /// result.</returns>
        public static IList<TEntity> ConvertDataTableToEntity<TEntity>(
            DataTable dataTable)
            where TEntity : ISqlEntity, new()
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return new List<TEntity>();

            var metadata = _entityMetadataCache.GetOrAdd(
                typeof(TEntity),
                SqlEntityMetadata.BuildEntityMetadata);

            var entities = new List<TEntity>();
            foreach (var row in dataTable.Rows.Cast<DataRow>())
            {
                var entity = new TEntity();
                SqlEntityMap.SetEntity(
                    entity,
                    row,
                    metadata,
                    Array.Empty<string>(),
                    _entityMetadataCache);

                entities.Add(entity);
            }

            return entities;
        }

        /// <summary>
        /// Converts an ISqlEntity list into a DataTable to improve Bulk Insert operations.
        /// </summary>
        /// <typeparam name="TEntity">Represents a Sql Table that holds the data to be parsed from the DataSet result.</typeparam>
        /// <param name="entities">ISqlEntity list with data to convert.</param>
        /// <returns>An System.Data.DataTable object.</returns>
        public static DataTable ConvertEntityToDataTable<TEntity>(IList<TEntity> entities)
            where TEntity : ISqlEntity, new()
        {
            var dataTable = new DataTable();

            var properties = typeof(TEntity)
                .GetProperties()
                .Where(p => p.CanRead && Attribute.IsDefined(p, typeof(SqlFieldAttribute)))
                .Select(p => new
                {
                    Property = p,
                    Attribute = (SqlFieldAttribute)p
                        .GetCustomAttributes(typeof(SqlFieldAttribute), false)
                        .First()
                })
                .ToList();

            foreach (var property in properties)
            {
                var type = property.Property.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                var columnName = string.IsNullOrWhiteSpace(property.Attribute.FieldName)
                    ? property.Property.Name
                    : property.Attribute.FieldName;

                dataTable.Columns.Add(columnName, type);
            }

            if (entities == null || entities.Count == 0)
                return dataTable;

            foreach (var entity in entities)
            {
                var row = dataTable.NewRow();

                foreach (var item in properties)
                {
                    var columnName = string.IsNullOrWhiteSpace(item.Attribute.FieldName)
                        ? item.Property.Name
                        : item.Attribute.FieldName;

                    row[columnName] = item.Property.GetValue(entity) ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        /// <summary>
        /// Validates whether the specified <typeparamref name="TEntity"/> type is properly
        /// configured to be used in an ExecuteEntity operation.
        ///
        /// The method ensures that:
        /// <list type="bullet">
        /// <item>
        /// The entity type is decorated with <see cref="SqlEntityAttribute"/>.
        /// </item>
        /// <item>
        /// The entity exposes at least one readable property decorated with
        /// <see cref="SqlFieldAttribute"/>.
        /// </item>
        /// </list>
        ///
        /// If the entity does not declare <see cref="SqlEntityAttribute"/>, a
        /// <see cref="MissingMemberException"/> is thrown.
        ///
        /// The return value indicates whether the entity contains mappable SQL fields,
        /// allowing the execution flow to short-circuit when no valid fields are defined.
        /// </summary>
        /// <typeparam name="TEntity">
        /// Represents a SQL-mapped entity type that implements <see cref="ISqlEntity"/>.
        /// </typeparam>
        /// <returns>
        /// <c>true</c> if the entity contains at least one property mapped with
        /// <see cref="SqlFieldAttribute"/>; otherwise, <c>false</c>.
        /// </returns>
        private static bool ValidateEntityRequest<TEntity>()
            where TEntity : ISqlEntity, new()
        {
            var sqlEntityAttribute = (typeof(TEntity)
                .GetCustomAttributes(typeof(SqlEntityAttribute), false) as SqlEntityAttribute[])
                .FirstOrDefault();

            if (null == sqlEntityAttribute)
                throw new MissingMemberException("The SqlEntity does not have a SqlEntityAttibute decorating itself.");

            var properties = typeof(TEntity)
                .GetProperties()
                .Where(property => property
                    .GetCustomAttributes(typeof(SqlFieldAttribute), false)
                    .Any())
                .ToArray();

            return properties.Length > 0;
        }
    }
}