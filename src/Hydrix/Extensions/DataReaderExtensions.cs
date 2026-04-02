using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Orchestrator.Resolvers;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Hydrix.Extensions
{
    /// <summary>
    /// Provides extension methods for mapping data from an IDataReader to strongly typed entities and for building
    /// ordinal maps based on the data reader's schema.
    /// </summary>
    /// <remarks>These extension methods are intended to simplify the process of materializing entities from
    /// data readers and to facilitate efficient schema mapping. The methods are designed for use with types
    /// implementing the ITable interface and support scenarios where a limited number of entities need to be
    /// materialized from data readers.</remarks>
    internal static class DataReaderExtensions
    {
        /// <summary>
        /// Maps the rows from the specified <see cref="IDataReader"/> into a list of entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The target entity type.</typeparam>
        /// <param name="dataReader">The data reader containing the rows to map.</param>
        /// <param name="limit">The maximum number of entities to materialize. Values less than or equal to zero map all rows.</param>
        /// <returns>A list with the mapped entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> is null.</exception>
        public static IList<TEntity> MapTo<TEntity>(
            this IDataReader dataReader,
            int limit = 0)
            where TEntity : ITable, new()
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(dataReader);
#else
            if (dataReader == null)
                throw new ArgumentNullException(nameof(dataReader));
#endif
            var entities = limit > 0 ? new List<TEntity>(limit) : new List<TEntity>();
            ResolvedTableBindings bindings = null;
            var count = 0;

            while (dataReader.Read())
            {
                if (limit > 0 && count >= limit)
                    break;

                bindings ??= CreateBindings<TEntity>(dataReader);

                MapCurrentEntity(
                    dataReader,
                    bindings,
                    entities);
                count++;
            }

            return entities;
        }

        /// <summary>
        /// Asynchronously maps the rows from the specified <see cref="IDataReader"/> into a list of entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The target entity type.</typeparam>
        /// <param name="dataReader">The data reader containing the rows to map. Must be a <see cref="DbDataReader"/>.</param>
        /// <param name="limit">The maximum number of entities to materialize. Values less than or equal to zero map all rows.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A list with the mapped entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataReader"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="dataReader"/> is not a <see cref="DbDataReader"/>.</exception>
        public static async Task<IList<TEntity>> MapToAsync<TEntity>(
            this IDataReader dataReader,
            int limit = 0,
            CancellationToken cancellationToken = default)
            where TEntity : ITable, new()
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(dataReader);
#else
            if (dataReader == null)
                throw new ArgumentNullException(nameof(dataReader));
#endif
            if (!(dataReader is DbDataReader dbDataReader))
                throw new InvalidOperationException("Asynchronous mapping requires a DbDataReader instance.");

            var entities = limit > 0 ? new List<TEntity>(limit) : new List<TEntity>();
            ResolvedTableBindings bindings = null;
            var count = 0;

            while (await dbDataReader
                .ReadAsync(cancellationToken)
                .ConfigureAwait(false))
            {
                if (limit > 0 && count >= limit)
                    break;

                bindings ??= CreateBindings<TEntity>(dbDataReader);

                MapCurrentEntity(
                    dbDataReader,
                    bindings,
                    entities);
                count++;
            }

            return entities;
        }

        /// <summary>
        /// Creates the schema-bound mapping plan for the current reader state.
        /// </summary>
        /// <typeparam name="TEntity">The target entity type.</typeparam>
        /// <param name="dataReader">The data reader positioned on a valid current row.</param>
        /// <returns>The resolved binding plan for the current schema.</returns>
        private static ResolvedTableBindings CreateBindings<TEntity>(
            IDataReader dataReader)
            where TEntity : ITable, new()
        {
            var metadata = EntityMetadataCache.GetOrAdd(typeof(TEntity));

            if (metadata.TryGetHotBindings(
                dataReader,
                out var bindings))
            {
                return bindings;
            }

            var ordinalMap = dataReader.BuildOrdinalMap(out var columnNames);

            var hasCachedBinding = metadata.TryGetBindings(
                ordinalMap.SchemaHash,
                out bindings);
            if (hasCachedBinding &&
                bindings.Matches(dataReader))
            {
                metadata.RememberBindings(bindings);
                return bindings;
            }

            if (hasCachedBinding)
            {
                return TableMap.Bind(
                    dataReader,
                    metadata,
                    string.Empty,
                    ordinalMap.Ordinals,
                    ordinalMap.SchemaHash,
                    columnNames);
            }

            return metadata.GetOrAddBindings(
                ordinalMap.SchemaHash,
                _ => TableMap.Bind(
                    dataReader,
                    metadata,
                    string.Empty,
                    ordinalMap.Ordinals,
                    ordinalMap.SchemaHash,
                    columnNames));
        }

        /// <summary>
        /// Materializes the current row and appends the resulting entity to the provided list.
        /// </summary>
        /// <typeparam name="TEntity">The target entity type.</typeparam>
        /// <param name="record">The current data record.</param>
        /// <param name="bindings">The pre-resolved binding plan for the current schema.</param>
        /// <param name="entities">The destination list to receive the mapped entity.</param>
        private static void MapCurrentEntity<TEntity>(
            IDataRecord record,
            ResolvedTableBindings bindings,
            ICollection<TEntity> entities)
            where TEntity : ITable, new()
        {
            var entity = new TEntity();

            TableMap.SetEntity(
                entity,
                record,
                bindings);

            entities.Add(entity);
        }

        /// <summary>
        /// Builds the ordinal map and schema hash for the current <see cref="IDataReader"/> in a single pass.
        /// </summary>
        /// <param name="reader">The data reader to inspect.</param>
        /// <param name="columnNames">Receives the ordered column names for hot-path schema matching.</param>
        /// <returns>An immutable ordinal map with the computed schema hash.</returns>
        private static OrdinalMap BuildOrdinalMap(
            this IDataReader reader,
            out string[] columnNames)
        {
            columnNames = new string[reader.FieldCount];
            var ordinals = new Dictionary<string, int>(
                reader.FieldCount,
                StringComparer.OrdinalIgnoreCase);
            var hash = new HashCode();
            hash.Add(reader.FieldCount);

            for (var index = 0; index < reader.FieldCount; index++)
            {
                var name = reader.GetName(index) ?? string.Empty;
                var fieldType = GetFieldType(reader, index);

                columnNames[index] = name;
                ordinals[name] = index;

                hash.Add(index);
                hash.Add(name, StringComparer.OrdinalIgnoreCase);
                hash.Add(fieldType);
            }

            return new OrdinalMap(
                ordinals,
                hash.ToHashCode());
        }

        /// <summary>
        /// Retrieves the provider CLR type for the specified ordinal when the reader supports it.
        /// </summary>
        /// <param name="reader">The data reader to inspect.</param>
        /// <param name="ordinal">The target field ordinal.</param>
        /// <returns>The provider CLR type for the specified ordinal, or null if the reader does not support retrieving field types.</returns>
        private static Type GetFieldType(
            IDataReader reader,
            int ordinal)
        {
            try
            {
                return reader.GetFieldType(ordinal);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }
    }
}
