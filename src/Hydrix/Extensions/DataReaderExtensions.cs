using Hydrix.Orchestrator.Caching;
using Hydrix.Orchestrator.Mapping;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Extensions
{
    /// <summary>
    /// Provides extension methods for mapping data from an IDataReader to strongly typed entities and for building
    /// ordinal maps based on the data reader's schema.
    /// </summary>
    /// <remarks>These extension methods are intended to simplify the process of materializing entities from
    /// data readers and to facilitate efficient schema mapping. The methods are designed for use with types
    /// implementing the ITable interface and support scenarios where a limited number of entities need to be
    /// materialized from a data reader.</remarks>
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
            var entities = new List<TEntity>();
            var metadata = EntityMetadataCache.GetOrAdd(typeof(TEntity));

            var ordinalMap = dataReader.BuildOrdinals();
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
                    schemaHash);

                entities.Add(entity);
                count++;
            }

            return entities;
        }

        /// <summary>
        /// Builds an ordinal map for the current <see cref="IDataReader"/> schema.
        /// </summary>
        /// <param name="reader">The data reader to inspect.</param>
        /// <returns>An <see cref="OrdinalMap"/> containing ordinals and a schema hash.</returns>
        private static OrdinalMap BuildOrdinals(
            this IDataReader reader)
        {
            var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            unchecked
            {
                var hash = 17;
                for (var index = 0; index < reader.FieldCount; index++)
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
    }
}