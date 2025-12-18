using System;
using System.Collections.Generic;
using System.Linq;
using Hydrix.Attributes.Schemas;
using Hydrix.Schemas;

namespace Hydrix.Orchestrator.Mappers
{
    /// <summary>
    /// Represents the cached metadata of a SQL-mapped entity type.
    /// </summary>
    /// <remarks>
    /// This class centralizes all reflection-derived information required to map a <see
    /// cref="System.Data.DataTable"/> or <see cref="System.Data.DataRow"/> into an <see
    /// cref="ISqlEntity"/> instance.
    ///
    /// The metadata is built once per entity type and reused through an internal cache,
    /// significantly reducing reflection overhead during large result set processing.
    ///
    /// It contains:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// A collection of mapped scalar fields decorated with <see cref="SqlFieldAttribute"/>,
    /// including their resolved target types and property accessors.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A collection of nested entity mappings decorated with <see cref="SqlEntityAttribute"/>,
    /// enabling recursive object graph construction from flattened SQL projections.
    /// </description>
    /// </item>
    /// </list>
    /// This structure is intentionally immutable after construction to ensure thread safety and
    /// predictable behavior during concurrent mapping operations.
    /// </remarks>
    internal sealed class SqlEntityMetadata
    {
        /// <summary>
        /// Gets the collection of scalar field mappings for the entity.
        /// </summary>
        /// <remarks>
        /// Each item in this collection represents a writable property decorated with <see
        /// cref="SqlFieldAttribute"/>, including:
        /// <list type="bullet">
        /// <item>
        /// <description>The reflected <see cref="System.Reflection.PropertyInfo"/>.</description>
        /// </item>
        /// <item>
        /// <description>The associated <see cref="SqlFieldAttribute"/> instance.</description>
        /// </item>
        /// <item>
        /// <description>
        /// The resolved target CLR type used for value conversion, with <see cref="Nullable{T}"/>
        /// already unwrapped when applicable.
        /// </description>
        /// </item>
        /// </list>
        /// These mappings are used to assign column values from a <see cref="System.Data.DataRow"/>
        /// to the corresponding entity properties in a safe and performant manner.
        /// </remarks>
        public IReadOnlyList<SqlFieldMap> Fields { get; private set; }

        /// <summary>
        /// Gets the collection of nested entity mappings for the entity.
        /// </summary>
        /// <remarks>
        /// Each item in this collection represents a writable property decorated with <see
        /// cref="SqlEntityAttribute"/>, defining a composition relationship between the current
        /// entity and another <see cref="ISqlEntity"/>.
        ///
        /// These mappings allow the data handler to:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Conditionally instantiate nested entities based on primary key presence, preventing the
        /// creation of empty objects when LEFT JOINs return null values.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Recursively populate object graphs using flattened column aliases (e.g. <c>Parent.Child.Property</c>).
        /// </description>
        /// </item>
        /// </list>
        /// This mechanism enables complex projections without requiring an ORM or provider-specific features.
        /// </remarks>
        public IReadOnlyList<SqlEntityMap> Entities { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlEntityMetadata"/> class.
        /// </summary>
        /// <param name="fields">
        /// The collection of scalar field mappings associated with the entity type, typically
        /// derived from properties decorated with <see cref="SqlFieldAttribute"/>.
        /// </param>
        /// <param name="entities">
        /// The collection of nested entity mappings associated with the entity type, typically
        /// derived from properties decorated with <see cref="SqlEntityAttribute"/>.
        /// </param>
        /// <remarks>
        /// This constructor is intended to be invoked exclusively by the metadata builder
        /// responsible for analyzing entity types via reflection.
        ///
        /// Once created, the metadata instance should be treated as read-only and reused across
        /// multiple mapping operations to ensure optimal performance and consistency.
        /// </remarks>
        public SqlEntityMetadata(
            IReadOnlyList<SqlFieldMap> fields,
            IReadOnlyList<SqlEntityMap> entities)
        {
            this.Fields = fields;
            this.Entities = entities;
        }

        /// <summary>
        /// Builds and returns the metadata definition for a SQL-mapped entity type.
        /// </summary>
        /// <param name="type">
        /// The CLR type representing an entity that implements <see cref="ISqlEntity"/> and is
        /// decorated with SQL mapping attributes.
        /// </param>
        /// <returns>
        /// A fully populated <see cref="SqlEntityMetadata"/> instance containing all scalar field
        /// and nested entity mappings associated with the specified type.
        /// </returns>
        /// <remarks>
        /// This method performs a one-time reflection analysis over the specified entity type to
        /// discover all properties participating in SQL-to-entity mapping.
        ///
        /// The resulting metadata includes:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Scalar field mappings for properties decorated with <see cref="SqlFieldAttribute"/>,
        /// including resolved target types for safe value conversion.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Nested entity mappings for properties decorated with <see cref="SqlEntityAttribute"/>,
        /// enabling recursive object graph materialization from flattened SQL projections.
        /// </description>
        /// </item>
        /// </list>
        /// The metadata produced by this method is intended to be cached and reused across multiple
        /// mapping operations, eliminating repetitive reflection overhead and significantly
        /// improving performance when processing large result sets.
        ///
        /// Nullable property types are normalized by unwrapping their underlying CLR type, ensuring
        /// compatibility with <see cref="System.Convert"/> during runtime value conversion.
        /// </remarks>
        internal static SqlEntityMetadata BuildEntityMetadata(Type type)
        {
            var fields = type
                .GetProperties()
                .Where(p => p.CanWrite && Attribute.IsDefined(p, typeof(SqlFieldAttribute)))
                .Select(p =>
                {
                    var attr = (SqlFieldAttribute)p
                        .GetCustomAttributes(typeof(SqlFieldAttribute), false)
                        .First();

                    var targetType = p.PropertyType;

                    if (targetType.IsGenericType &&
                        targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        targetType = Nullable.GetUnderlyingType(targetType);

                    return new SqlFieldMap(
                        p,
                        attr,
                        targetType);
                })
                .ToList();

            var entities = type
                .GetProperties()
                .Where(p => p.CanWrite && Attribute.IsDefined(p, typeof(SqlEntityAttribute)))
                .Select(p => new SqlEntityMap(
                    p,
                    (SqlEntityAttribute)p
                        .GetCustomAttributes(typeof(SqlEntityAttribute), false)
                        .First()))
                .ToList();

            return new SqlEntityMetadata(
                fields,
                entities);
        }
    }
}