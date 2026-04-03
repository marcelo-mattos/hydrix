using Hydrix.Mapping;
using Hydrix.Metadata.Internals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Resolvers
{
    /// <summary>
    /// Represents a set of resolved field and nested entity bindings for a table, providing access to the associated
    /// field and entity binding collections.
    /// </summary>
    internal sealed class ResolvedTableBindings
    {
        /// <summary>
        /// Gets the collection of resolved field bindings associated with the current instance.
        /// </summary>
        public ResolvedFieldBinding[] Fields { get; }

        /// <summary>
        /// Gets the collection of resolved nested bindings associated with this instance.
        /// </summary>
        public ResolvedNestedBinding[] Entities { get; }

        /// <summary>
        /// Gets the captured schema column names used for hot-path schema matching.
        /// </summary>
        public string[] ColumnNames { get; }

        /// <summary>
        /// Gets the flattened field bindings used by schema matching to avoid recursive traversal on the hot path.
        /// </summary>
        private ResolvedFieldBinding[] MatchFields { get; }

        /// <summary>
        /// Gets the compiled single-pass row materializer that assigns all scalar fields and nested leaf entities
        /// in one delegate invocation, eliminating the per-field loop on the hot path.
        /// </summary>
        /// <remarks>
        /// This is available when every nested entity in <see cref="Entities"/> has a pre-compiled
        /// <see cref="ResolvedNestedBinding.Materializer"/>. When <see langword="null"/>, the caller must
        /// fall back to the loop-based <see cref="TableMap.SetResolvedEntityFields"/> path.
        /// On runtimes with Dynamic PGO (.NET 8+) the JIT can devirtualize and inline the constant-captured
        /// delegate invocations after a few warm-up iterations, yielding near-zero indirect-call overhead.
        /// </remarks>
        public Action<object, IDataRecord> RowMaterializer { get; }

        /// <summary>
        /// Initializes a new instance of the ResolvedTableBindings class with the specified field and entity bindings.
        /// </summary>
        /// <param name="fields">An array of ResolvedFieldBinding objects representing the field bindings to include. If null, an empty array
        /// is used.</param>
        /// <param name="entities">An array of ResolvedNestedBinding objects representing the nested entity bindings to include. If null, an
        /// empty array is used.</param>
        /// <param name="columnNames">The captured schema column names used for hot-path schema matching.</param>
        public ResolvedTableBindings(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities,
            string[] columnNames = null)
        {
            Fields = fields ?? Array.Empty<ResolvedFieldBinding>();
            Entities = entities ?? Array.Empty<ResolvedNestedBinding>();
            ColumnNames = columnNames ?? Array.Empty<string>();
            MatchFields = BuildMatchFields(
                Fields,
                Entities);
            RowMaterializer = BuildRowMaterializer(
                Fields,
                Entities);
        }

        /// <summary>
        /// Builds a single compiled <see cref="Action{T1,T2}"/> that assigns all scalar fields and nested leaf
        /// entities in one delegate invocation, eliminating per-field delegate indirection on the hot path.
        /// </summary>
        /// <remarks>
        /// Attempts to build a fully-inlined expression tree first (zero delegate indirections per row),
        /// then falls back to a delegate-invocation tree when property metadata is unavailable.
        /// Returns <see langword="null"/> when the fast-path cannot be applied.
        /// </remarks>
        /// <param name="fields">The resolved scalar field bindings for this entity level.</param>
        /// <param name="entities">The resolved nested entity bindings for this entity level.</param>
        /// <returns>A compiled <see cref="Action{T1,T2}"/> covering all assignments, or <see langword="null"/> if the
        /// fast-path cannot be applied.</returns>
        private static Action<object, IDataRecord> BuildRowMaterializer(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities)
        {
            var inlined = BuildInlinedRowMaterializer(
                fields,
                entities);

            if (inlined != null)
                return inlined;

            return BuildDelegateRowMaterializer(
                fields,
                entities);
        }

        /// <summary>
        /// Builds a fully-inlined expression tree that casts the parent entity once, assigns all scalar
        /// fields directly, and inlines nested entity materialization (existence check, instantiation,
        /// and child field assignments) without any delegate indirection.
        /// </summary>
        /// <param name="fields">The resolved scalar field bindings for this entity level.</param>
        /// <param name="entities">The resolved nested entity bindings for this entity level.</param>
        /// <remarks>
        /// This path eliminates N+M+1 indirect delegate calls per row (where N = parent fields,
        /// M = child fields across all nested entities), removes redundant boxing/unboxing at
        /// delegate boundaries, and skips the <c>IsDBNull</c> check on nested primary key fields
        /// whose existence is already verified by the instantiation condition.
        /// Returns <see langword="null"/> when any required <see cref="PropertyInfo"/> is missing
        /// or when nested entities have deeper sub-entities (non-leaf).
        /// </remarks>
        /// <returns>A compiled <see cref="Action{T1,T2}"/> covering all assignments with inlined expressions,
        /// or <see langword="null"/> if the inlined path cannot be applied due to missing metadata or
        /// unsupported nesting.</returns>
        private static Action<object, IDataRecord> BuildInlinedRowMaterializer(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities)
        {
            var totalOps = fields.Length + entities.Length;
            if (totalOps == 0)
                return null;

            Type parentType = GetParentType(
                fields,
                entities);

            if (parentType == null)
                return null;

            if (!AreFieldsValid(fields) ||
                !AreEntitiesValid(entities))
            {
                return null;
            }

            var parent = Expression.Parameter(
                typeof(object),
                "parent");

            var record = Expression.Parameter(
                typeof(IDataRecord),
                "record");

            var typedParent = Expression.Variable(
                parentType,
                "typedParent");

            var variables = new List<ParameterExpression> { typedParent };
            var bodyExpressions = new List<Expression>
            {
                Expression.Assign(
                    typedParent,
                    Expression.Convert(
                        parent,
                        parentType))
            };

            AddFieldAssignments(
                bodyExpressions,
                typedParent,
                record,
                fields);

            AddEntityAssignments(
                bodyExpressions,
                variables,
                typedParent,
                record,
                entities);

            return Expression
                .Lambda<Action<object, IDataRecord>>(
                    Expression.Block(
                        variables,
                        bodyExpressions),
                    parent,
                    record)
                .Compile();
        }

        /// <summary>
        /// Gets the declaring type of the first property or navigation property found in the specified field or entity
        /// bindings.
        /// </summary>
        /// <param name="fields">An array of field bindings to search for a property with a declaring type.</param>
        /// <param name="entities">An array of nested entity bindings to search for a navigation property with a declaring type.</param>
        /// <returns>The declaring type of the first property or navigation property found; otherwise, null if none are found.</returns>
        [SuppressMessage(
            "Minor Code Smell",
            "S3267",
            Justification = "Loop intentionally used to avoid LINQ allocations and improve performance in hot path")]
        private static Type GetParentType(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities)
        {
            Type parentType = null;

            foreach (var field in fields)
            {
                if (field.Property != null)
                    parentType ??= field.Property.DeclaringType;
            }

            foreach (var entity in entities)
            {
                if (entity.NavigationProperty != null)
                    parentType ??= entity.NavigationProperty.DeclaringType;
            }

            return parentType;
        }

        /// <summary>
        /// Determines whether all elements in the specified array of field bindings have both their property and
        /// assigner set.
        /// </summary>
        /// <param name="fields">An array of field bindings to validate. Each element is checked to ensure its property and assigner are not
        /// null.</param>
        /// <returns>true if every field binding in the array has both a non-null property and assigner; otherwise, false.</returns>
        private static bool AreFieldsValid(
            ResolvedFieldBinding[] fields)
        {
            foreach (var field in fields)
            {
                if (field.Property == null ||
                    field.Assigner == null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines whether all specified entities and their child fields are valid for binding.
        /// </summary>
        /// <param name="entities">An array of entities to validate. Each entity must have a non-null navigation property, non-null bindings,
        /// and no entities in its bindings. All child fields within each entity's bindings must have a non-null
        /// property.</param>
        /// <returns>true if all entities and their child fields meet the required validation criteria; otherwise, false.</returns>
        private static bool AreEntitiesValid(
            ResolvedNestedBinding[] entities)
        {
            foreach (var entity in entities)
            {
                if (entity.NavigationProperty == null ||
                    entity.Bindings == null ||
                    entity.Bindings.Entities.Length != 0)
                {
                    return false;
                }

                foreach (var childField in entity.Bindings.Fields)
                {
                    if (childField.Property == null)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds assignment expressions for each specified field to the provided expression list.
        /// </summary>
        /// <remarks>This method is typically used when constructing expression trees for object
        /// materialization, ensuring that each field in the provided array is assigned from the corresponding data
        /// record value.</remarks>
        /// <param name="bodyExpressions">The list to which the generated field assignment expressions will be added.</param>
        /// <param name="typedParent">The parameter expression representing the strongly-typed parent object to which fields are assigned.</param>
        /// <param name="record">The parameter expression representing the data record containing field values.</param>
        /// <param name="fields">An array of resolved field bindings that define the properties to assign and their corresponding source
        /// information.</param>
        private static void AddFieldAssignments(
            List<Expression> bodyExpressions,
            ParameterExpression typedParent,
            ParameterExpression record,
            ResolvedFieldBinding[] fields)
        {
            for (var index = 0; index < fields.Length; index++)
            {
                bodyExpressions.Add(
                    CreateInlineFieldAssignment(
                        typedParent,
                        record,
                        fields[index].Property,
                        fields[index].Ordinal,
                        fields[index].SourceType));
            }
        }

        /// <summary>
        /// Adds assignment expressions to initialize and assign nested entity properties for each specified entity
        /// binding, enabling the construction of complex object graphs in expression trees.
        /// </summary>
        /// <remarks>This method is typically used when building expression trees for materializing object
        /// graphs from data records, such as in object-relational mapping scenarios. It ensures that child entities are
        /// instantiated and their properties are assigned only when the appropriate conditions are met.</remarks>
        /// <param name="bodyExpressions">The list to which generated assignment expressions for nested entities will be added.</param>
        /// <param name="variables">The list of variables used within the expression tree, to which new variables for child entities will be
        /// appended.</param>
        /// <param name="typedParent">The parameter expression representing the parent object whose navigation properties will be assigned.</param>
        /// <param name="record">The parameter expression representing the data record from which field values are read.</param>
        /// <param name="entities">An array of resolved nested bindings that describe the entity navigation properties and their associated
        /// field bindings.</param>
        private static void AddEntityAssignments(
            List<Expression> bodyExpressions,
            List<ParameterExpression> variables,
            ParameterExpression typedParent,
            ParameterExpression record,
            ResolvedNestedBinding[] entities)
        {
            for (var index = 0; index < entities.Length; index++)
            {
                var entity = entities[index];
                var navProp = entity.NavigationProperty;
                var childType = navProp.PropertyType;
                var child = Expression.Variable(
                    childType,
                    string.Concat(
                        "child",
                        index.ToString()));
                variables.Add(child);

                var childBody = new List<Expression>
                {
                    Expression.Assign(
                        child,
                        Expression.New(
                            childType)),
                    Expression.Assign(
                        Expression.Property(
                            typedParent,
                            navProp),
                        child)
                };

                var childFields = entity.Bindings.Fields;
                for (var childIndex = 0; childIndex < childFields.Length; childIndex++)
                {
                    var childField = childFields[childIndex];
                    var isPkField = entity.UsesPrimaryKey &&
                                    childField.Ordinal == entity.PrimaryKeyOrdinal;

                    childBody.Add(isPkField
                        ? CreateInlineFieldAssignmentNoNullCheck(
                            child,
                            record,
                            childField.Property,
                            childField.Ordinal,
                            childField.SourceType)
                        : CreateInlineFieldAssignment(
                            child,
                            record,
                            childField.Property,
                            childField.Ordinal,
                            childField.SourceType));
                }

                bodyExpressions.Add(
                    Expression.IfThen(
                        MetadataFactory.CreateNestedInstantiationCondition(
                            record,
                            entity.UsesPrimaryKey,
                            entity.PrimaryKeyOrdinal,
                            entity.CandidateOrdinals),
                        Expression.Block(
                            new[] { child }.AsEnumerable(),
                            childBody)));
            }
        }

        /// <summary>
        /// Creates an inline field assignment expression that reads a value from the data record
        /// and assigns it to a property on a pre-cast entity instance, with an <c>IsDBNull</c> guard.
        /// </summary>
        /// <param name="typedInstance">The pre-cast entity instance.</param>
        /// <param name="record">The data record to read values from.</param>
        /// <param name="property">The property to assign the value to.</param>
        /// <param name="ordinal">The ordinal position of the field in the data record.</param>
        /// <param name="sourceType">The source type of the field in the data record.</param>
        /// <returns>An expression representing the conditional field assignment.</returns>
        private static Expression CreateInlineFieldAssignment(
            Expression typedInstance,
            ParameterExpression record,
            PropertyInfo property,
            int ordinal,
            Type sourceType)
        {
            var ordinalExpr = Expression.Constant(
                ordinal);

            var propertyAccess = Expression.Property(
                typedInstance,
                property);

            return Expression.IfThenElse(
                Expression.Call(
                    record,
                    MetadataFactory.IsDbNullMethod,
                    ordinalExpr),
                Expression.Assign(
                    propertyAccess,
                    Expression.Default(
                        property.PropertyType)),
                Expression.Assign(
                    propertyAccess,
                    MetadataFactory.CreateValueExpression(
                        record,
                        ordinalExpr,
                        property.PropertyType,
                        sourceType)));
        }

        /// <summary>
        /// Creates an inline field assignment expression without an <c>IsDBNull</c> guard, for use
        /// with fields whose non-null status is guaranteed by a prior existence check (e.g. nested
        /// entity primary key fields).
        /// </summary>
        /// <param name="typedInstance">The pre-cast entity instance.</param>
        /// <param name="record">The data record to read values from.</param>
        /// <param name="property">The property to assign the value to.</param>
        /// <param name="ordinal">The ordinal position of the field in the data record.</param>
        /// <param name="sourceType">The source type of the field in the data record.</param>
        /// <returns>An expression representing the field assignment without a null check.</returns>
        private static Expression CreateInlineFieldAssignmentNoNullCheck(
            Expression typedInstance,
            ParameterExpression record,
            PropertyInfo property,
            int ordinal,
            Type sourceType)
        {
            var ordinalExpr = Expression.Constant(
                ordinal);

            var propertyAccess = Expression.Property(
                typedInstance,
                property);

            return Expression.Assign(
                propertyAccess,
                MetadataFactory.CreateValueExpression(
                    record,
                    ordinalExpr,
                    property.PropertyType,
                    sourceType));
        }

        /// <summary>
        /// Builds a row materializer using delegate invocations for each field assigner and nested
        /// materializer. This is the fallback path when property metadata is unavailable for
        /// fully-inlined compilation.
        /// </summary>
        /// <param name="fields">The resolved scalar field bindings for this entity level.</param>
        /// <param name="entities">The resolved nested entity bindings for this entity level.</param>
        /// <returns>A compiled <see cref="Action{T1,T2}"/> covering all assignments with delegate invocations,
        /// or <see langword="null"/> if the delegate path cannot be applied due to missing metadata or
        /// unsupported nesting.</returns>
        private static Action<object, IDataRecord> BuildDelegateRowMaterializer(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities)
        {
            for (var index = 0; index < entities.Length; index++)
            {
                if (entities[index].Materializer == null)
                    return null;
            }

            var totalOps = fields.Length + entities.Length;
            if (totalOps == 0)
                return null;

            var parent = Expression.Parameter(
                typeof(object),
                "parent");

            var record = Expression.Parameter(
                typeof(IDataRecord),
                "record");

            var body = new Expression[totalOps];

            for (var index = 0; index < fields.Length; index++)
            {
                if (fields[index].Assigner == null)
                    return null;

                body[index] = Expression.Invoke(
                    Expression.Constant(
                        fields[index].Assigner),
                    parent,
                    record);
            }

            for (var index = 0; index < entities.Length; index++)
            {
                body[fields.Length + index] = Expression.Invoke(
                    Expression.Constant(
                        entities[index].Materializer),
                    parent,
                    record);
            }

            var blockBody = totalOps == 1
                ? body[0]
                : (Expression)Expression.Block(
                    body);

            return Expression
                .Lambda<Action<object, IDataRecord>>(
                    blockBody,
                    parent,
                    record)
                .Compile();
        }

        /// <summary>
        /// Determines whether the specified data reader matches the expected column names and field types.
        /// </summary>
        /// <remarks>Column name comparisons are case-insensitive. The method returns false if the number
        /// of columns does not match or if any column name differs from the expected value.</remarks>
        /// <param name="reader">The data reader to compare against the expected column schema. Cannot be null.</param>
        /// <returns>true if the data reader's columns match the expected names and field types; otherwise, false.</returns>
        internal bool Matches(
            IDataReader reader)
        {
            if (reader == null ||
                ColumnNames.Length == 0 ||
                reader.FieldCount != ColumnNames.Length)
            {
                return false;
            }

            for (var index = 0; index < ColumnNames.Length; index++)
            {
                var currentName = reader.GetName(index) ?? string.Empty;
                if (!string.Equals(
                    currentName,
                    ColumnNames[index],
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return MatchesFieldTypes(reader);
        }

        /// <summary>
        /// Determines whether the field types in the specified data reader match the expected types defined by the
        /// current bindings.
        /// </summary>
        /// <remarks>This method checks both the type information and the actual runtime values of the
        /// fields to ensure type consistency.</remarks>
        /// <param name="reader">The data reader to compare against the expected field types. Must not be null.</param>
        /// <returns>true if all fields in the data reader match the expected types; otherwise, false.</returns>
        private bool MatchesFieldTypes(
            IDataReader reader)
        {
            for (var index = 0; index < MatchFields.Length; index++)
            {
                if (!FieldMatches(
                    reader,
                    MatchFields[index]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Builds the flattened field bindings used by schema matching.
        /// </summary>
        private static ResolvedFieldBinding[] BuildMatchFields(
            ResolvedFieldBinding[] fields,
            ResolvedNestedBinding[] entities)
        {
            if (fields.Length == 0)
            {
                if (entities.Length == 0)
                    return Array.Empty<ResolvedFieldBinding>();

                if (entities.Length == 1)
                    return entities[0].Bindings.MatchFields;
            }
            else if (entities.Length == 0)
            {
                return fields;
            }

            var totalCount = fields.Length;
            for (var index = 0; index < entities.Length; index++)
                totalCount += entities[index].Bindings.MatchFields.Length;

            if (totalCount == 0)
                return Array.Empty<ResolvedFieldBinding>();

            var matchFields = new ResolvedFieldBinding[totalCount];
            var offset = 0;

            if (fields.Length != 0)
            {
                Array.Copy(
                    fields,
                    matchFields,
                    fields.Length);
                offset = fields.Length;
            }

            for (var index = 0; index < entities.Length; index++)
            {
                var nestedMatchFields = entities[index].Bindings.MatchFields;
                if (nestedMatchFields.Length == 0)
                    continue;

                Array.Copy(
                    nestedMatchFields,
                    0,
                    matchFields,
                    offset,
                    nestedMatchFields.Length);
                offset += nestedMatchFields.Length;
            }

            return matchFields;
        }

        /// <summary>
        /// Checks if a single field matches the expected type.
        /// </summary>
        /// <param name="reader">The data reader to compare against the expected field type.</param>
        /// <param name="field">The field binding to check.</param>
        /// <returns>true if the field matches; otherwise, false.</returns>
        private static bool FieldMatches(
            IDataReader reader,
            ResolvedFieldBinding field)
        {
            if (field.Ordinal < 0)
                return true;

            if (field.SourceType == null)
                return false;

            var currentFieldType = GetFieldType(reader, field.Ordinal);

            if (currentFieldType != null)
                return currentFieldType == field.SourceType;

            if (!reader.IsDBNull(field.Ordinal) &&
                reader.GetValue(field.Ordinal)?.GetType() != field.SourceType)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves the data type of the specified column in the provided data reader.
        /// </summary>
        /// <remarks>If the data reader does not support retrieving the field type or if the operation is
        /// invalid for the current state of the reader, the method returns null instead of throwing an
        /// exception.</remarks>
        /// <param name="reader">The data reader from which to obtain the column type information. Cannot be null.</param>
        /// <param name="ordinal">The zero-based column ordinal indicating which column's data type to retrieve. Must be within the range of
        /// available columns in the reader.</param>
        /// <returns>A Type object representing the data type of the specified column, or null if the type cannot be determined.</returns>
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
