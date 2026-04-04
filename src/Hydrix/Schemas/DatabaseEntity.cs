using Hydrix.Builders.Query;
using Hydrix.Builders.Query.Conditions;
using Hydrix.Caching;
using Hydrix.Metadata.Builders;
using Hydrix.Metadata.Snapshots;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace Hydrix.Schemas
{
    /// <summary>
    /// Represents the abstract base class for all database entities, providing a common contract for database-related
    /// objects.
    /// </summary>
    /// <remarks>Classes that derive from <see cref="DatabaseEntity"/> must implement the <see cref="IEntity"/>
    /// interface to define specific behaviors and properties required for database entities. The class also contains a
    /// process-wide hot cache for builder metadata so validation and query generation can reuse the most recently
    /// requested entity metadata efficiently.</remarks>
    public abstract class DatabaseEntity :
        IEntity
    {
        /// <summary>
        /// Holds the process-wide hot cache snapshot for the most recently requested entity metadata.
        /// </summary>
        /// <remarks>This field is read and written using volatile operations to provide a lock-free fast
        /// path for repeated metadata lookups in high-throughput and async scenarios. The cached snapshot also stores
        /// the Entity Framework metadata cache version so registrations translated from <c>OnModelCreating</c> can
        /// invalidate the hot cache safely.</remarks>
        private static EntityMetadataSnapshot _cache;

        /// <summary>
        /// Validates the current object and returns a value that indicates whether the object is in a valid state.
        /// </summary>
        /// <typeparam name="T">The entity type expected by the external validator.</typeparam>
        /// <remarks>Use this method to determine whether the object's state meets all defined validation
        /// criteria. The method first performs the standard DataAnnotations-based validation and then executes the
        /// provided external validator delegate, if any, appending any additional validation errors to the same result
        /// list.</remarks>
        /// <param name="results">When this method returns, contains a list of <see cref="ValidationResult"/> objects that describe any
        /// validation errors encountered. The list is empty if the object is valid.</param>
        /// <param name="externalValidator">An external validator delegate to execute additional validation logic.</param>
        /// <returns><see langword="true"/> if the object passes all validation checks; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown when the current instance cannot be cast to <typeparamref name="T"/> before invoking
        /// <paramref name="externalValidator"/>.</exception>
        public virtual bool IsValid<T>(
            out List<ValidationResult> results,
            Func<T, IEnumerable<ValidationResult>> externalValidator)
            where T : IEntity
        {
            IsValid(out results);

            if (externalValidator == null)
                return results.Count == 0;

            T entity;
            try
            {
                entity = (T)(object)this;
            }
            catch (InvalidCastException exception)
            {
                throw new InvalidCastException(
                    $"External validator type mismatch. Expected entity assignable to '{typeof(T).FullName}', but actual runtime type is '{GetType().FullName}'.",
                    exception);
            }

            var externalResults = externalValidator(entity);
            if (externalResults != null)
                results.AddRange(externalResults);

            return results.Count == 0;
        }

        /// <summary>
        /// Validates the current object and returns a value that indicates whether the object is in a valid state.
        /// </summary>
        /// <remarks>Use this method to determine whether the object's state meets all defined validation
        /// criteria. The method populates the provided list with detailed validation errors, if any are
        /// found.</remarks>
        /// <param name="results">When this method returns, contains a list of <see cref="ValidationResult"/> objects that describe any
        /// validation errors encountered. The list is empty if the object is valid.</param>
        /// <returns><see langword="true"/> if the object passes all validation checks; otherwise, <see langword="false"/>.</returns>
        public virtual bool IsValid(
            out List<ValidationResult> results)
        {
            results = new List<ValidationResult>();
            ValidateInternal(results);
            return results.Count == 0;
        }

        /// <summary>
        /// Validates the public instance properties of the current object and adds any validation errors to the
        /// specified results list.
        /// </summary>
        /// <remarks>This method examines all public instance properties of the current object using the
        /// builder metadata resolved for the runtime type. That means the validation path respects both attribute-based
        /// mappings and metadata translated from Entity Framework registrations.</remarks>
        /// <param name="results">A list that receives the validation results for each property that fails validation. Must not be null.</param>
        private void ValidateInternal(
            List<ValidationResult> results)
        {
            var metadata = GetOrAddEntityMetadata(GetType());

            foreach (var column in metadata.Columns)
            {
                var value = column.Getter(this);
                var context = new ValidationContext(this)
                {
                    MemberName = column.PropertyName
                };

                Validator.TryValidateProperty(
                    value,
                    context,
                    results);
            }
        }

        /// <summary>
        /// Retrieves the metadata associated with the specified entity type, adding it to the cache if it does not
        /// already exist.
        /// </summary>
        /// <remarks>This method optimizes repeated access by caching the most recently requested entity
        /// type and its metadata. Subsequent calls with the same entity type and the same Entity Framework cache
        /// version return the cached metadata, improving performance for frequent lookups while still allowing external
        /// metadata registrations to refresh the hot cache.</remarks>
        /// <param name="entityType">The type of the entity for which metadata is requested. Cannot be null.</param>
        /// <returns>The metadata object for the specified entity type. If the metadata is already cached, returns the cached
        /// instance; otherwise, retrieves and caches the metadata before returning it.</returns>
        private static EntityBuilderMetadata GetOrAddEntityMetadata(
            Type entityType)
        {
            var snapshot = Volatile.Read(ref _cache);
            var cacheVersion = EntityFrameworkMetadataCache.Version;

            if (snapshot != null &&
                ReferenceEquals(snapshot.EntityType, entityType) &&
                snapshot.Version == cacheVersion)
            {
                return snapshot.Metadata;
            }

            var metadata = EntityBuilderMetadataCache.GetMetadata(entityType);

            Volatile.Write(
                ref _cache,
                new EntityMetadataSnapshot(
                    entityType,
                    metadata,
                    cacheVersion));

            return metadata;
        }

        /// <summary>
        /// Generates a SQL SELECT query string for the current entity type, including optional table aliases and left
        /// joins for related foreign tables.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity for which the SQL query is being generated. Must implement <see cref="ITable"/>.</typeparam>
        /// <remarks>The generated query reflects the table and relationship structure defined either by
        /// Hydrix attributes or by metadata translated from Entity Framework registrations. Foreign tables referenced by
        /// the resolved metadata are automatically joined using the semantics defined by the existing query builder
        /// pipeline.</remarks>
        /// <param name="where">A <see cref="WhereBuilder"/> instance that specifies the conditions to apply to the WHERE clause of the generated SQL
        /// query.</param>
        /// <returns>A string containing the complete SQL SELECT query, including any specified WHERE conditions and joins
        /// for related tables.</returns>
        public static string BuildQuery<TEntity>(
            WhereBuilder where = null)
            where TEntity : ITable
            => QueryBuilder.Build<TEntity>(where);
    }
}
