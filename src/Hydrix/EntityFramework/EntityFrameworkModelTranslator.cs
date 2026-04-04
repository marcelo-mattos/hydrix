using Hydrix.Caching;
using Hydrix.Mapping;
using Hydrix.Metadata.Builders;
using Hydrix.Metadata.EntityFramework;
using Hydrix.Metadata.Internals;
using Hydrix.Schemas.Contract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hydrix.EntityFramework
{
    /// <summary>
    /// Translates Entity Framework metadata into the internal Hydrix metadata contracts.
    /// </summary>
    /// <remarks>The translator intentionally uses reflection so Hydrix can cooperate with Entity
    /// Framework models without taking a hard compile-time dependency on EF abstractions inside the main runtime
    /// assembly. The output of this translator is cached in <see cref="EntityFrameworkMetadataCache"/> and then
    /// consumed by the existing Hydrix caches.</remarks>
    internal static class EntityFrameworkModelTranslator
    {
        /// <summary>
        /// Represents the relational table-name annotation used by Entity Framework.
        /// </summary>
        private const string TableNameAnnotation = "Relational:TableName";

        /// <summary>
        /// Represents the relational schema annotation used by Entity Framework.
        /// </summary>
        private const string SchemaAnnotation = "Relational:Schema";

        /// <summary>
        /// Represents the relational column-name annotation used by Entity Framework.
        /// </summary>
        private const string ColumnNameAnnotation = "Relational:ColumnName";

        /// <summary>
        /// Represents the metadata property name used by Entity Framework for key and foreign-key property collections.
        /// </summary>
        private const string PropertiesMemberName = "Properties";

        /// <summary>
        /// Translates the supplied Entity Framework model object into a collection of Hydrix registrations.
        /// </summary>
        /// <remarks>The translation process first materializes the scalar metadata for every compatible CLR
        /// type, then performs a second pass to resolve reference navigations between those types.</remarks>
        /// <param name="dbContextOrModel">A <c>DbContext</c> instance or model object exposed by Entity Framework.</param>
        /// <returns>A read-only collection containing the metadata registrations translated for the compatible entity types.</returns>
        public static IReadOnlyCollection<RegisteredEntityMetadata> Translate(
            object dbContextOrModel)
        {
            var model = ResolveModel(dbContextOrModel);
            var entities = new Dictionary<Type, ReflectedEntityModel>();

            foreach (var entityType in InvokeEnumerable(model, "GetEntityTypes"))
            {
                var clrType = GetClrType(entityType);
                if (clrType == null || !typeof(ITable).IsAssignableFrom(clrType))
                    continue;

                entities[clrType] = BuildEntityModel(
                    entityType,
                    clrType);
            }

            foreach (var entity in entities.Values)
            {
                PopulateNavigations(
                    entity,
                    entities);
            }

            return entities.Values
                .Select(CreateRegisteredMetadata)
                .ToArray();
        }

        /// <summary>
        /// Builds the intermediate reflected entity model for the specified Entity Framework entity type.
        /// </summary>
        /// <param name="entityType">The Entity Framework metadata object that describes the entity type.</param>
        /// <param name="clrType">The CLR type represented by the Entity Framework metadata.</param>
        /// <returns>A <see cref="ReflectedEntityModel"/> containing the translated scalar metadata.</returns>
        private static ReflectedEntityModel BuildEntityModel(
            object entityType,
            Type clrType)
        {
            var primaryKeys = new HashSet<string>(
                GetPrimaryKeyPropertyNames(entityType),
                StringComparer.Ordinal);
            var fields = new List<ReflectedScalarProperty>();

            foreach (var property in InvokeEnumerable(entityType, "GetProperties"))
            {
                var propertyInfo = ResolvePropertyInfo(
                    property,
                    clrType);

                if (propertyInfo == null ||
                    !propertyInfo.CanWrite ||
                    propertyInfo.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                fields.Add(new ReflectedScalarProperty(
                    propertyInfo,
                    ResolveColumnName(
                        property,
                        propertyInfo),
                    primaryKeys.Contains(propertyInfo.Name),
                    IsRequired(property)));
            }

            return new ReflectedEntityModel(
                entityType,
                clrType,
                GetAnnotationValue(entityType, TableNameAnnotation) ?? clrType.Name,
                GetAnnotationValue(entityType, SchemaAnnotation),
                fields,
                GetPrimaryKeyColumns(entityType));
        }

        /// <summary>
        /// Resolves the reference navigations for the specified reflected entity model.
        /// </summary>
        /// <remarks>Collection navigations are ignored because the current Hydrix materialization pipeline
        /// only supports nested reference mappings. Navigations that target types outside the translated set are also
        /// ignored intentionally.</remarks>
        /// <param name="entity">The reflected entity model whose navigations should be populated.</param>
        /// <param name="entities">The reflected entity models indexed by CLR type.</param>
        private static void PopulateNavigations(
            ReflectedEntityModel entity,
            IReadOnlyDictionary<Type, ReflectedEntityModel> entities)
        {
            var navigations = new List<ReflectedNavigation>();

            foreach (var navigation in InvokeEnumerable(entity.EntityType, "GetNavigations"))
            {
                var reflectedNavigation = TryCreateNavigation(
                    entity,
                    entities,
                    navigation);
                if (reflectedNavigation != null)
                    navigations.Add(reflectedNavigation);
            }

            entity.SetNavigations(navigations);
        }

        /// <summary>
        /// Attempts to translate a single Entity Framework navigation into the intermediate Hydrix representation.
        /// </summary>
        /// <param name="entity">The reflected entity model currently being populated.</param>
        /// <param name="entities">The reflected entity models indexed by CLR type.</param>
        /// <param name="navigation">The Entity Framework navigation metadata object.</param>
        /// <returns>A <see cref="ReflectedNavigation"/> instance when the navigation is compatible with Hydrix;
        /// otherwise, <see langword="null"/>.</returns>
        private static ReflectedNavigation TryCreateNavigation(
            ReflectedEntityModel entity,
            IReadOnlyDictionary<Type, ReflectedEntityModel> entities,
            object navigation)
        {
            var propertyInfo = ResolveNavigationProperty(
                navigation,
                entity.ClrType);
            if (propertyInfo == null ||
                !entities.TryGetValue(propertyInfo.PropertyType, out var targetEntity))
            {
                return null;
            }

            var foreignKey = GetPropertyValue(
                navigation,
                "ForeignKey");
            if (foreignKey == null ||
                !TryResolveNavigationColumns(
                    navigation,
                    foreignKey,
                    entity.ClrType,
                    out var mainColumns,
                    out var targetColumns,
                    out var isRequiredJoin))
            {
                return null;
            }

            return new ReflectedNavigation(
                propertyInfo,
                targetEntity,
                targetEntity.PrimaryKeyColumns.FirstOrDefault(),
                mainColumns,
                targetColumns,
                isRequiredJoin);
        }

        /// <summary>
        /// Resolves the CLR navigation property when it is compatible with Hydrix nested mapping.
        /// </summary>
        /// <param name="navigation">The Entity Framework navigation metadata object.</param>
        /// <param name="clrType">The CLR type that owns the navigation.</param>
        /// <returns>The resolved <see cref="PropertyInfo"/> when the navigation is writable, non-indexed, and not a
        /// collection; otherwise, <see langword="null"/>.</returns>
        private static PropertyInfo ResolveNavigationProperty(
            object navigation,
            Type clrType)
        {
            var propertyInfo = ResolvePropertyInfo(
                navigation,
                clrType);
            if (propertyInfo == null ||
                !propertyInfo.CanWrite ||
                propertyInfo.GetIndexParameters().Length != 0 ||
                IsCollectionNavigation(
                    navigation,
                    propertyInfo))
            {
                return null;
            }

            return propertyInfo;
        }

        /// <summary>
        /// Resolves the main and target columns associated with a translated navigation.
        /// </summary>
        /// <param name="navigation">The Entity Framework navigation metadata object.</param>
        /// <param name="foreignKey">The foreign-key metadata object associated with the navigation.</param>
        /// <param name="currentClrType">The CLR type currently being translated.</param>
        /// <param name="mainColumns">When this method returns, contains the column names on the main side of the relationship.</param>
        /// <param name="targetColumns">When this method returns, contains the column names on the target side of the relationship.</param>
        /// <param name="isRequiredJoin">When this method returns, indicates whether the translated navigation should be emitted as a required join.</param>
        /// <returns><see langword="true"/> when the relationship columns were resolved successfully; otherwise,
        /// <see langword="false"/>.</returns>
        private static bool TryResolveNavigationColumns(
            object navigation,
            object foreignKey,
            Type currentClrType,
            out string[] mainColumns,
            out string[] targetColumns,
            out bool isRequiredJoin)
        {
            var isOnDependent = IsNavigationOnDependent(
                navigation,
                foreignKey,
                currentClrType);
            mainColumns = isOnDependent
                ? GetForeignKeyColumns(foreignKey)
                : GetPrincipalKeyColumns(foreignKey);
            targetColumns = isOnDependent
                ? GetPrincipalKeyColumns(foreignKey)
                : GetForeignKeyColumns(foreignKey);

            isRequiredJoin = isOnDependent && IsForeignKeyRequired(foreignKey);

            return mainColumns.Length != 0 &&
                targetColumns.Length != 0 &&
                mainColumns.Length == targetColumns.Length;
        }

        /// <summary>
        /// Creates the final cache entry consumed by Hydrix from the supplied reflected entity model.
        /// </summary>
        /// <param name="entity">The reflected entity model to translate into Hydrix metadata.</param>
        /// <returns>A <see cref="RegisteredEntityMetadata"/> instance that can be stored in the Entity Framework cache.</returns>
        private static RegisteredEntityMetadata CreateRegisteredMetadata(
            ReflectedEntityModel entity)
        {
            var fields = entity.Fields
                .Select(field => new ColumnMap(
                    field.ColumnName,
                    MetadataFactory.CreateSetter(field.Property),
                    FieldReaderFactory.Create(field.Property.PropertyType),
                    field.Property.PropertyType,
                    field.Property))
                .ToArray();

            var nestedEntities = entity.Navigations
                .Select(navigation => new TableMap(
                    navigation.Property,
                    navigation.PrimaryKeyColumn))
                .ToArray();

            var columns = entity.Fields
                .Select(field => new ColumnBuilderMetadata(
                    field.Property.Name,
                    field.ColumnName,
                    field.IsPrimaryKey,
                    field.IsRequired,
                    MetadataFactory.CreateGetter(field.Property)))
                .ToArray();

            var joins = entity.Navigations
                .Select(navigation => new JoinBuilderMetadata(
                    navigation.Property.Name,
                    navigation.Target.Table,
                    navigation.Target.Schema,
                    navigation.TargetColumns,
                    navigation.MainColumns,
                    navigation.IsRequiredJoin,
                    navigation.Target.Fields
                        .Select(field => new ForeignColumnMetadata(
                            field.ColumnName,
                            $"{navigation.Property.Name}.{field.ColumnName}"))
                        .ToArray()))
                .ToArray();

            return new RegisteredEntityMetadata(
                entity.ClrType,
                MetadataFactory.CreateEntity(
                    fields,
                    nestedEntities),
                new EntityBuilderMetadata(
                    entity.ClrType.Name,
                    entity.ClrType,
                    entity.Table,
                    entity.Schema,
                    columns,
                    joins));
        }

        /// <summary>
        /// Resolves the actual Entity Framework model object from the supplied input.
        /// </summary>
        /// <param name="dbContextOrModel">A <c>DbContext</c> instance or a model object.</param>
        /// <returns>The resolved Entity Framework model object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the supplied value does not expose an Entity Framework model.</exception>
        private static object ResolveModel(
            object dbContextOrModel)
        {
            if (HasMethod(
                dbContextOrModel,
                "GetEntityTypes"))
            {
                return dbContextOrModel;
            }

            var model = GetPropertyValue(
                dbContextOrModel,
                "Model");
            if (model != null &&
                HasMethod(
                    model,
                    "GetEntityTypes"))
            {
                return model;
            }

            throw new InvalidOperationException(
                "Hydrix could not resolve an Entity Framework model from the supplied instance. Pass a DbContext or DbContext.Model.");
        }

        /// <summary>
        /// Retrieves the CLR type represented by the supplied Entity Framework entity metadata object.
        /// </summary>
        /// <param name="entityType">The Entity Framework metadata object.</param>
        /// <returns>The CLR type represented by the metadata, or <see langword="null"/> when it cannot be determined.</returns>
        private static Type GetClrType(
            object entityType)
        {
            TryGetPropertyValue(
                entityType,
                "ClrType",
                out Type clrType);

            return clrType;
        }

        /// <summary>
        /// Determines whether the supplied navigation is located on the dependent side of the relationship.
        /// </summary>
        /// <param name="navigation">The navigation metadata object.</param>
        /// <param name="foreignKey">The foreign-key metadata object associated with the navigation.</param>
        /// <param name="currentClrType">The CLR type currently being translated.</param>
        /// <returns><see langword="true"/> when the navigation belongs to the dependent side; otherwise,
        /// <see langword="false"/>.</returns>
        private static bool IsNavigationOnDependent(
            object navigation,
            object foreignKey,
            Type currentClrType)
        {
            if (TryGetPropertyValue(
                navigation,
                "IsOnDependent",
                out bool isOnDependent))
            {
                return isOnDependent;
            }

            return GetClrType(
                GetPropertyValue(
                    foreignKey,
                    "DeclaringEntityType")) == currentClrType;
        }

        /// <summary>
        /// Determines whether the supplied foreign key is marked as required in Entity Framework metadata.
        /// </summary>
        /// <param name="foreignKey">The foreign-key metadata object.</param>
        /// <returns><see langword="true"/> when the foreign key is required; otherwise, <see langword="false"/>.</returns>
        private static bool IsForeignKeyRequired(
            object foreignKey)
            => TryGetPropertyValue(
                foreignKey,
                "IsRequired",
                out bool isRequired) && isRequired;

        /// <summary>
        /// Retrieves the foreign-key column names defined by the supplied Entity Framework foreign-key metadata.
        /// </summary>
        /// <param name="foreignKey">The foreign-key metadata object.</param>
        /// <returns>An array containing the foreign-key column names.</returns>
        private static string[] GetForeignKeyColumns(
            object foreignKey)
            => GetSequence(
                    GetPropertyValue(
                        foreignKey,
                        PropertiesMemberName))
                .Select(property => ResolveColumnName(property))
                .ToArray();

        /// <summary>
        /// Retrieves the principal-key column names defined by the supplied Entity Framework foreign-key metadata.
        /// </summary>
        /// <param name="foreignKey">The foreign-key metadata object.</param>
        /// <returns>An array containing the principal-key column names.</returns>
        private static string[] GetPrincipalKeyColumns(
            object foreignKey)
            => GetSequence(
                    GetPropertyValue(
                        GetPropertyValue(
                            foreignKey,
                            "PrincipalKey"),
                        PropertiesMemberName))
                .Select(property => ResolveColumnName(property))
                .ToArray();

        /// <summary>
        /// Retrieves the primary-key property names declared for the supplied Entity Framework entity metadata.
        /// </summary>
        /// <param name="entityType">The Entity Framework entity metadata object.</param>
        /// <returns>An enumerable containing the CLR property names that participate in the primary key.</returns>
        private static IEnumerable<string> GetPrimaryKeyPropertyNames(
            object entityType)
        {
            var primaryKey = InvokeMethod(
                entityType,
                "FindPrimaryKey");
            if (primaryKey == null)
                return Array.Empty<string>();

            return GetSequence(
                    GetPropertyValue(
                        primaryKey,
                        PropertiesMemberName))
                .Select(property =>
                    TryGetPropertyValue(
                        property,
                        "Name",
                        out string name)
                            ? name
                            : null)
                .Where(name => !string.IsNullOrWhiteSpace(name));
        }

        /// <summary>
        /// Retrieves the primary-key column names declared for the supplied Entity Framework entity metadata.
        /// </summary>
        /// <param name="entityType">The Entity Framework entity metadata object.</param>
        /// <returns>An array containing the database column names that compose the primary key.</returns>
        private static string[] GetPrimaryKeyColumns(
            object entityType)
        {
            var primaryKey = InvokeMethod(
                entityType,
                "FindPrimaryKey");
            if (primaryKey == null)
                return Array.Empty<string>();

            return GetSequence(
                    GetPropertyValue(
                        primaryKey,
                        PropertiesMemberName))
                .Select(property => ResolveColumnName(property))
                .ToArray();
        }

        /// <summary>
        /// Resolves the database column name represented by the supplied Entity Framework property metadata.
        /// </summary>
        /// <remarks>The method first checks for the relational column annotation, then falls back to the
        /// metadata property name, and finally to the CLR property name when a <see cref="PropertyInfo"/> is supplied.</remarks>
        /// <param name="property">The Entity Framework property metadata object.</param>
        /// <param name="propertyInfo">The CLR property info associated with the metadata, when already resolved.</param>
        /// <returns>The resolved database column name, or <see langword="null"/> when no name can be determined.</returns>
        private static string ResolveColumnName(
            object property,
            PropertyInfo propertyInfo = null)
        {
            var columnName = GetAnnotationValue(
                property,
                ColumnNameAnnotation);
            if (!string.IsNullOrWhiteSpace(columnName))
                return columnName;

            if (TryGetPropertyValue(
                property,
                "Name",
                out string propertyName) &&
                !string.IsNullOrWhiteSpace(propertyName))
            {
                return propertyName;
            }

            return propertyInfo?.Name;
        }

        /// <summary>
        /// Resolves the CLR property represented by the supplied Entity Framework metadata object.
        /// </summary>
        /// <remarks>The method first attempts to read a direct <see cref="PropertyInfo"/> reference from
        /// the metadata and falls back to searching by the metadata name on the target CLR type.</remarks>
        /// <param name="metadata">The Entity Framework metadata object.</param>
        /// <param name="clrType">The CLR type that owns the property.</param>
        /// <returns>The resolved <see cref="PropertyInfo"/>, or <see langword="null"/> when it cannot be determined.</returns>
        private static PropertyInfo ResolvePropertyInfo(
            object metadata,
            Type clrType)
        {
            if (TryGetPropertyValue(
                metadata,
                "PropertyInfo",
                out PropertyInfo propertyInfo))
            {
                return propertyInfo;
            }

            if (TryGetPropertyValue(
                metadata,
                "Name",
                out string propertyName) &&
                !string.IsNullOrWhiteSpace(propertyName))
            {
                return clrType.GetProperty(
                    propertyName,
                    BindingFlags.Instance | BindingFlags.Public);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the supplied Entity Framework property is required.
        /// </summary>
        /// <param name="property">The Entity Framework property metadata object.</param>
        /// <returns><see langword="true"/> when the property is required; otherwise, <see langword="false"/>.</returns>
        private static bool IsRequired(
            object property)
            => TryGetPropertyValue(
                property,
                "IsNullable",
                out bool isNullable) && !isNullable;

        /// <summary>
        /// Determines whether the supplied navigation represents a collection navigation.
        /// </summary>
        /// <remarks>The method prefers Entity Framework's explicit <c>IsCollection</c> metadata when
        /// available and falls back to inspecting the CLR property type.</remarks>
        /// <param name="navigation">The Entity Framework navigation metadata object.</param>
        /// <param name="propertyInfo">The CLR property associated with the navigation.</param>
        /// <returns><see langword="true"/> when the navigation represents a collection; otherwise,
        /// <see langword="false"/>.</returns>
        private static bool IsCollectionNavigation(
            object navigation,
            PropertyInfo propertyInfo)
        {
            if (TryGetPropertyValue(
                navigation,
                "IsCollection",
                out bool isCollection))
            {
                return isCollection;
            }

            return propertyInfo.PropertyType != typeof(string) &&
                typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType);
        }

        /// <summary>
        /// Invokes a parameterless instance method on the supplied object.
        /// </summary>
        /// <param name="instance">The object that exposes the target method.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <returns>The return value produced by the method, or <see langword="null"/> when the method does not exist or the
        /// instance is null.</returns>
        private static object InvokeMethod(
            object instance,
            string methodName)
        {
            if (instance == null)
                return null;

            var method = instance.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                Type.EmptyTypes,
                modifiers: null);

            return method?.Invoke(
                instance,
                null);
        }

        /// <summary>
        /// Invokes a parameterless method and returns the result as an enumerable sequence.
        /// </summary>
        /// <param name="instance">The object that exposes the target method.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <returns>An enumerable sequence with the values returned by the method, or an empty sequence when the return value is not
        /// enumerable.</returns>
        private static IEnumerable<object> InvokeEnumerable(
            object instance,
            string methodName)
        {
            if (!(InvokeMethod(
                instance,
                methodName) is IEnumerable values))
            {
                return Array.Empty<object>();
            }

            return values.Cast<object>();
        }

        /// <summary>
        /// Determines whether the supplied object exposes the specified parameterless instance method.
        /// </summary>
        /// <param name="instance">The object to inspect.</param>
        /// <param name="methodName">The name of the method to search for.</param>
        /// <returns><see langword="true"/> when the method exists; otherwise, <see langword="false"/>.</returns>
        private static bool HasMethod(
            object instance,
            string methodName)
            => instance != null &&
               instance.GetType().GetMethod(
                   methodName,
                   BindingFlags.Instance | BindingFlags.Public,
                   binder: null,
                   Type.EmptyTypes,
                   modifiers: null) != null;

        /// <summary>
        /// Converts the supplied value into an enumerable sequence of boxed objects.
        /// </summary>
        /// <param name="value">The value to treat as a sequence.</param>
        /// <returns>An enumerable sequence containing the boxed items, or an empty sequence when the value is not enumerable.</returns>
        private static IEnumerable<object> GetSequence(
            object value)
        {
            if (!(value is IEnumerable enumerable))
                return Array.Empty<object>();

            return enumerable.Cast<object>();
        }

        /// <summary>
        /// Retrieves the value of an Entity Framework annotation from the supplied annotatable metadata object.
        /// </summary>
        /// <param name="annotatable">The metadata object that may expose the annotation.</param>
        /// <param name="annotationName">The annotation name to search for.</param>
        /// <returns>The annotation value as a string, or <see langword="null"/> when the annotation is not available.</returns>
        private static string GetAnnotationValue(
            object annotatable,
            string annotationName)
        {
            if (annotatable == null)
                return null;

            var findAnnotation = annotatable.GetType().GetMethod(
                "FindAnnotation",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                new[] { typeof(string) },
                modifiers: null);
            if (findAnnotation == null)
                return null;

            var annotation = findAnnotation.Invoke(
                annotatable,
                new object[] { annotationName });

            return GetPropertyValue(
                annotation,
                "Value") as string;
        }

        /// <summary>
        /// Retrieves the value of the specified property from the supplied object.
        /// </summary>
        /// <param name="instance">The object that owns the property.</param>
        /// <param name="propertyName">The name of the property to read.</param>
        /// <returns>The property value, or <see langword="null"/> when the instance is null or the property cannot be found.</returns>
        private static object GetPropertyValue(
            object instance,
            string propertyName)
        {
            if (instance == null)
                return null;

            var property = instance.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);

            return property?.GetValue(instance);
        }

        /// <summary>
        /// Attempts to read a strongly typed property value from the supplied object.
        /// </summary>
        /// <typeparam name="T">The expected property type.</typeparam>
        /// <param name="instance">The object that owns the property.</param>
        /// <param name="propertyName">The name of the property to read.</param>
        /// <param name="value">When this method returns, contains the strongly typed property value if the operation succeeds.</param>
        /// <returns><see langword="true"/> when the property exists and its value is assignable to <typeparamref name="T"/>;
        /// otherwise, <see langword="false"/>.</returns>
        private static bool TryGetPropertyValue<T>(
            object instance,
            string propertyName,
            out T value)
        {
            var rawValue = GetPropertyValue(
                instance,
                propertyName);
            if (rawValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }
    }
}
