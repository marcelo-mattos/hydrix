using Hydrix.Caching;
using Hydrix.Internals;
using Hydrix.Mapping;
using Hydrix.Metadata.Materializers;
using Hydrix.Resolvers;
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
            Func<IDataRecord, TEntity> factory = null;

            if (limit <= 0)
            {
                while (dataReader.Read())
                {
                    if (bindings == null)
                    {
                        bindings = CreateBindings<TEntity>(dataReader);
                        factory = bindings.GetOrBuildTypedFactory<TEntity>();
                    }

                    AppendEntity(
                        dataReader,
                        bindings,
                        factory,
                        entities);
                }
            }
            else
            {
                var count = 0;
                while (count < limit &&
                    dataReader.Read())
                {
                    if (bindings == null)
                    {
                        bindings = CreateBindings<TEntity>(dataReader);
                        factory = bindings.GetOrBuildTypedFactory<TEntity>();
                    }

                    AppendEntity(
                        dataReader,
                        bindings,
                        factory,
                        entities);

                    count++;
                }
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
            Func<IDataRecord, TEntity> factory = null;

            if (limit <= 0)
            {
                while (await dbDataReader
                    .ReadAsync(cancellationToken)
                    .ConfigureAwait(false))
                {
                    if (bindings == null)
                    {
                        bindings = CreateBindings<TEntity>(dbDataReader);
                        factory = bindings.GetOrBuildTypedFactory<TEntity>();
                    }

                    AppendEntity(
                        dbDataReader,
                        bindings,
                        factory,
                        entities);
                }
            }
            else
            {
                var count = 0;
                while (count < limit &&
                    await dbDataReader
                        .ReadAsync(cancellationToken)
                        .ConfigureAwait(false))
                {
                    if (bindings == null)
                    {
                        bindings = CreateBindings<TEntity>(dbDataReader);
                        factory = bindings.GetOrBuildTypedFactory<TEntity>();
                    }

                    AppendEntity(
                        dbDataReader,
                        bindings,
                        factory,
                        entities);

                    count++;
                }
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
                return RebuildAndReplaceBindings(
                    dataReader,
                    metadata,
                    ordinalMap,
                    columnNames);
            }

            var built = metadata.GetOrAddBindings(
                ordinalMap.SchemaHash,
                _ => TableMap.Bind(
                    dataReader,
                    metadata,
                    string.Empty,
                    ordinalMap.Ordinals,
                    ordinalMap.SchemaHash,
                    columnNames));

            // The schemaHash is only a non-authoritative bucket key (32 bits); Matches is the authoritative gate.
            // Under a concurrent insert with a genuine hash collision, GetOrAddBindings can return a plan built for a
            // different schema, so validate the result and rebuild when it does not match the current reader.
            if (built.Matches(dataReader))
                return built;

            return RebuildAndReplaceBindings(
                dataReader,
                metadata,
                ordinalMap,
                columnNames);
        }

        /// <summary>
        /// Rebuilds the binding plan for the current reader schema and replaces any cached entry stored under the same
        /// schema hash, then records the rebuilt plan as the hot bindings.
        /// </summary>
        /// <remarks>Used both when a cached plan fails schema validation (hash collision or schema drift) and when
        /// a concurrently inserted plan does not match, so the corrected plan is cached for subsequent requests.</remarks>
        /// <param name="dataReader">The data reader positioned on a valid current row.</param>
        /// <param name="metadata">The entity materialization metadata that owns the binding cache.</param>
        /// <param name="ordinalMap">The ordinal map and schema hash computed for the current reader.</param>
        /// <param name="columnNames">The ordered column names captured for hot-path schema matching.</param>
        /// <returns>The rebuilt binding plan for the current schema.</returns>
        private static ResolvedTableBindings RebuildAndReplaceBindings(
            IDataReader dataReader,
            TableMaterializeMetadata metadata,
            OrdinalMap ordinalMap,
            string[] columnNames)
        {
            var rebuilt = TableMap.Bind(
                dataReader,
                metadata,
                string.Empty,
                ordinalMap.Ordinals,
                ordinalMap.SchemaHash,
                columnNames);

            metadata.ReplaceBindings(
                ordinalMap.SchemaHash,
                rebuilt);

            metadata.RememberBindings(
                rebuilt);

            return rebuilt;
        }

        /// <summary>
        /// Materializes the current row and appends the resulting entity to the provided list, using the compiled
        /// typed factory when available and falling back to construct-then-populate otherwise.
        /// </summary>
        /// <remarks>The destination is typed as the concrete <see cref="List{T}"/> rather than
        /// <see cref="ICollection{T}"/> so the per-row <c>Add</c> binds to the inlinable list method instead of an
        /// interface dispatch. When <paramref name="factory"/> is non-null it constructs and populates the entity in a
        /// single call (matching Dapper's per-row shape); otherwise the converter/setter fallback path is used.</remarks>
        /// <typeparam name="TEntity">The target entity type.</typeparam>
        /// <param name="record">The current data record.</param>
        /// <param name="bindings">The pre-resolved binding plan for the current schema.</param>
        /// <param name="factory">The compiled typed factory for the current schema, or <see langword="null"/> when the fast path is unavailable.</param>
        /// <param name="entities">The destination list to receive the mapped entity.</param>
        private static void AppendEntity<TEntity>(
            IDataRecord record,
            ResolvedTableBindings bindings,
            Func<IDataRecord, TEntity> factory,
            List<TEntity> entities)
            where TEntity : ITable, new()
        {
            if (factory != null)
            {
                entities.Add(factory(record));
                return;
            }

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
                var fieldType = FieldTypeHelper.GetFieldType(
                    reader,
                    index);

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
    }
}
