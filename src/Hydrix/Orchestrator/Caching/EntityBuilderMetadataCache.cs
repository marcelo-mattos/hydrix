using Hydrix.Attributes.Schemas;
using Hydrix.Orchestrator.Metadata.Builders;
using Hydrix.Orchestrator.Metadata.Internals;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Hydrix.Orchestrator.Caching
{
    /// <summary>
    /// Provides a thread-safe cache for storing and retrieving metadata associated with entity builders, optimizing
    /// performance by avoiding redundant metadata construction.
    /// </summary>
    /// <remarks>This class is intended for internal use and ensures efficient, concurrent access to entity
    /// metadata in multi-threaded scenarios. By caching metadata for each entity type, it reduces overhead when
    /// building entities repeatedly within the application.</remarks>
    internal static class EntityBuilderMetadataCache
    {
        /// <summary>
        /// Stores cached metadata for entity builders, indexed by entity type.
        /// </summary>
        /// <remarks>This field is implemented as a thread-safe concurrent dictionary to ensure safe
        /// access in multi-threaded scenarios. Caching metadata improves performance by avoiding repeated lookups when
        /// building entities.</remarks>
        private static readonly ConcurrentDictionary<Type, EntityBuilderMetadata> _cache
            = new ConcurrentDictionary<Type, EntityBuilderMetadata>();

        /// <summary>
        /// Retrieves the metadata associated with the specified entity type, utilizing caching to optimize repeated
        /// access.
        /// </summary>
        /// <remarks>This method uses an internal cache to avoid reconstructing metadata for the same
        /// type, improving performance when called multiple times with the same argument.</remarks>
        /// <param name="type">The type of the entity for which metadata is to be retrieved. Must be a valid entity type; cannot be null.</param>
        /// <returns>An instance of EntityBuilderMetadata containing the metadata for the specified entity type.</returns>
        public static EntityBuilderMetadata GetMetadata(
            Type type)
            => _cache.GetOrAdd(
                type,
                BuildMetadata);

        /// <summary>
        /// Resolves the schema name, primary key columns, and foreign key columns for a specified navigation property
        /// using the provided foreign table attributes.
        /// </summary>
        /// <remarks>If the schema is not specified in the attribute or table metadata, the method
        /// defaults to using "[dbo]" as the schema name. The method ensures that both primary and foreign keys are
        /// present and that their counts match, enforcing referential integrity for the navigation property.</remarks>
        /// <param name="mainType">The type of the main entity containing the navigation property. Must not be null.</param>
        /// <param name="navigationProperty">The navigation property representing the relationship for which foreign metadata is being resolved. Must not
        /// be null.</param>
        /// <param name="attr">The attribute containing schema, primary key, and foreign key information for the foreign table. Must not be
        /// null.</param>
        /// <returns>A tuple containing the resolved schema name, an array of primary key column names, and an array of foreign
        /// key column names associated with the navigation property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the primary keys or foreign keys cannot be resolved, or if the number of primary keys does not
        /// match the number of foreign keys.</exception>
        private static (string schema, string[] primaryKeys, string[] foreignKeys) ResolveForeignMetadata(
            Type mainType,
            PropertyInfo navigationProperty,
            ForeignTableAttribute attr)
        {
            if (navigationProperty is null)
                throw new ArgumentNullException(
                    nameof(navigationProperty),
                    "Navigation property cannot be null.");

            var foreignType = navigationProperty.PropertyType;

            var mainTable = mainType.GetCustomAttribute<TableAttribute>();
            var foreignTable = foreignType.GetCustomAttribute<TableAttribute>();

            var primaryKeys = attr.PrimaryKeys
                ?? foreignType.GetProperties()
                    .Where(p => p.GetCustomAttribute<KeyAttribute>() != null)
                    .Select(p => p.Name)
                    .ToArray();

            var foreignKeys = attr.ForeignKeys
                ?? mainType.GetProperties()
                    .Where(p =>
                        p.GetCustomAttribute<ForeignKeyAttribute>()?.Name ==
                        navigationProperty.Name)
                    .Select(p => p.Name)
                    .ToArray();

            if (primaryKeys.Length == 0)
                throw new InvalidOperationException(
                    $"Primary key not resolved for {foreignType.Name}");

            if (foreignKeys.Length == 0)
                throw new InvalidOperationException(
                    $"Foreign key not resolved for {mainType.Name}");

            if (primaryKeys.Length != foreignKeys.Length)
                throw new InvalidOperationException(
                    $"PrimaryKeys and ForeignKeys count mismatch.");

            return (attr.Schema, primaryKeys, foreignKeys);
        }

        /// <summary>
        /// Builds metadata for the specified entity type, including table name, schema, columns, and joins.
        /// </summary>
        /// <remarks>This method inspects the properties of the provided type to determine which columns
        /// and joins are relevant for the entity's database representation. It skips properties marked with
        /// NotMappedAttribute and resolves foreign key relationships using ForeignTableAttribute.</remarks>
        /// <param name="type">The type of the entity for which metadata is being built. This type must be a class that represents a
        /// database entity.</param>
        /// <returns>An instance of EntityBuilderMetadata containing the metadata for the specified entity type.</returns>
        private static EntityBuilderMetadata BuildMetadata(
            Type type)
        {
            var tableAttr = type.GetCustomAttribute<TableAttribute>();

            var table = tableAttr?.Name ?? type.Name;
            var schema = tableAttr?.Schema;

            var validatableColumns = new List<ColumnBuilderMetadata>();
            var columns = new List<ColumnBuilderMetadata>();
            var joins = new List<JoinBuilderMetadata>();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetCustomAttribute<NotMappedAttribute>() != null)
                    continue;

                var foreignAttr = property.GetCustomAttribute<ForeignTableAttribute>();
                if (foreignAttr != null)
                {
                    if (foreignAttr.PrimaryKeys == null || foreignAttr.PrimaryKeys.Length == 0)
                        throw new InvalidOperationException(
                            $"ForeignTable '{property.Name}' must define at least one PrimaryKey.");

                    var (resolvedSchema, primaryKeys, foreignKeys) =
                        ResolveForeignMetadata(
                            type,
                            property,
                            foreignAttr);

                    var isRequiredJoin = foreignKeys.All(fk =>
                    {
                        var fkProp = type.GetProperty(fk);
                        return fkProp.GetCustomAttribute<RequiredAttribute>() != null;
                    });

                    var foreignSelectColumns = property.PropertyType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p =>
                            p.GetCustomAttribute<NotMappedAttribute>() == null &&
                            p.GetCustomAttribute<ForeignTableAttribute>() == null)
                        .Select(p =>
                        {
                            var colAttr = p.GetCustomAttribute<ColumnAttribute>();
                            var colName = colAttr?.Name ?? p.Name;

                            return new ForeignColumnMetadata(
                                colName,
                                $"{foreignAttr.Name}.{colName}");
                        })
                        .ToList();

                    joins.Add(new JoinBuilderMetadata(
                        property.Name,
                        foreignAttr.Name,
                        resolvedSchema,
                        primaryKeys,
                        foreignKeys,
                        isRequiredJoin,
                        foreignSelectColumns));

                    continue;
                }

                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

                columns.Add(new ColumnBuilderMetadata(
                    property.Name,
                    columnAttr?.Name ?? property.Name,
                    property.GetCustomAttribute<KeyAttribute>() != null,
                    property.GetCustomAttribute<RequiredAttribute>() != null,
                    MetadataFactory.CreateGetter(property)));
            }

            return new EntityBuilderMetadata(
                type.Name,
                type,
                table,
                schema,
                columns,
                joins);
        }
    }
}
