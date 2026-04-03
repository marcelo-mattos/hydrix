using Hydrix.Attributes.Schemas;
using Hydrix.Extensions;
using Hydrix.Mapping;
using Hydrix.Metadata.Materializers;
using Hydrix.Schemas.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Metadata.Internals
{
    /// <summary>
    /// Provides factory methods for creating compiled metadata
    /// components used during entity materialization.
    /// </summary>
    internal static class MetadataFactory
    {
        /// <summary>
        /// Represents the MethodInfo for the IDataRecord.IsDBNull method.
        /// </summary>
        /// <remarks>This field can be used to invoke the IsDBNull method via reflection when working with
        /// IDataRecord instances. It is useful for scenarios where method access is required dynamically at
        /// runtime.</remarks>
        internal static readonly MethodInfo IsDbNullMethod =
            typeof(IDataRecord).GetMethod(
                nameof(IDataRecord.IsDBNull));

        /// <summary>
        /// Represents the cached MethodInfo for the IDataRecord.GetValue method.
        /// </summary>
        /// <remarks>This field provides efficient access to the GetValue method of the IDataRecord
        /// interface via reflection. It can be used to dynamically invoke the method on IDataRecord instances without
        /// repeated reflection lookups.</remarks>
        private static readonly MethodInfo GetValueMethod =
            typeof(IDataRecord).GetMethod(
                nameof(IDataRecord.GetValue));

        /// <summary>
        /// Provides a mapping between common .NET types and their corresponding strongly-typed getter methods on the
        /// IDataRecord interface.
        /// </summary>
        /// <remarks>This dictionary enables efficient lookup of the appropriate IDataRecord getter method
        /// for a given type, facilitating type-safe data retrieval from data records. Only a predefined set of types is
        /// supported; attempting to retrieve a method for an unsupported type will result in a
        /// KeyNotFoundException.</remarks>
        private static readonly Dictionary<Type, MethodInfo> TypedGetterMethods = new Dictionary<Type, MethodInfo>()
        {
            [typeof(bool)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetBoolean)),
            [typeof(byte)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetByte)),
            [typeof(int)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt32)),
            [typeof(long)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt64)),
            [typeof(short)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetInt16)),
            [typeof(float)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetFloat)),
            [typeof(double)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDouble)),
            [typeof(decimal)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDecimal)),
            [typeof(Guid)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetGuid)),
            [typeof(DateTime)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetDateTime)),
            [typeof(string)] = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetString)),
        };

        /// <summary>
        /// Creates a new instance of the column materialization metadata for the specified property and column
        /// attribute.
        /// </summary>
        /// <param name="property">The property for which to create the materialization metadata. Cannot be null.</param>
        /// <param name="attribute">The column attribute that provides mapping information for the property. Cannot be null.</param>
        /// <returns>A new instance of the column materialization metadata representing the specified property and its associated
        /// column attribute.</returns>
        public static ColumnMaterializeMetadata CreateField(
            PropertyInfo property,
            ColumnAttribute attribute)
            => new ColumnMaterializeMetadata(
                    property,
                    CreateSetter(property),
                    property.PropertyType,
                    attribute);

        /// <summary>
        /// Creates a new instance of the TableMaterializeMetadata class using the specified column and entity mappings.
        /// </summary>
        /// <param name="fields">A read-only list of column mappings that define the fields to be materialized.</param>
        /// <param name="entities">A read-only list of table mappings that represent the entities to be included in the metadata.</param>
        /// <returns>A TableMaterializeMetadata instance initialized with the provided column and entity mappings.</returns>
        public static TableMaterializeMetadata CreateEntity(
            IReadOnlyList<ColumnMap> fields,
            IReadOnlyList<TableMap> entities)
            => new TableMaterializeMetadata(
                    fields,
                    entities);

        /// <summary>
        /// Creates a new instance of the foreign table materialization metadata for a nested entity property.
        /// </summary>
        /// <remarks>Use this method to generate metadata required for materializing nested entities in
        /// object-relational mapping scenarios. The returned metadata encapsulates the property, its mapping attribute,
        /// and delegates for instantiation and assignment.</remarks>
        /// <param name="property">The property information representing the navigation property to the nested entity. Cannot be null.</param>
        /// <param name="attribute">The attribute that defines the foreign table mapping for the nested entity. Cannot be null.</param>
        /// <returns>A new instance of the <see cref="ForeignTableMaterializeMetadata"/> configured for the specified property
        /// and attribute.</returns>
        public static ForeignTableMaterializeMetadata CreateNestedEntity(
            PropertyInfo property,
            ForeignTableAttribute attribute)
            => new ForeignTableMaterializeMetadata(
                    property,
                    attribute,
                    CreateFactory(property.PropertyType),
                    CreateSetter(property));

        /// <summary>
        /// Creates a delegate that retrieves the value of the specified property from a given object instance.
        /// </summary>
        /// <remarks>The returned delegate uses expression trees to provide efficient, strongly-typed
        /// access to the property value at runtime. The input object must be of the type that declares the property or
        /// a compatible derived type; otherwise, an exception may be thrown at invocation.</remarks>
        /// <param name="property">The property metadata for which to create a getter delegate. Must not be null and must represent a property
        /// of the declaring type.</param>
        /// <returns>A delegate that takes an object instance and returns the value of the specified property as an object.</returns>
        internal static Func<object, object> CreateGetter(
            PropertyInfo property)
        {
            var parameter = Expression.Parameter(
                typeof(object),
                "entity");

            var cast = Expression.Convert(
                parameter,
                property.DeclaringType ?? throw new InvalidOperationException(
                    $"Property '{property.Name}' does not have a declaring type."));

            var propertyAccess = Expression.Property(
                cast,
                property);

            var convert = Expression.Convert(
                propertyAccess,
                typeof(object));

            return Expression
                .Lambda<Func<object, object>>(
                    convert,
                    parameter)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that sets the value of a specified property on a given object instance.
        /// </summary>
        /// <remarks>This method uses expression trees to generate a setter delegate, which can improve
        /// performance when setting property values dynamically. The returned delegate performs type conversions as
        /// needed based on the property's declaring type and property type.</remarks>
        /// <param name="property">The <see cref="PropertyInfo"/> representing the property to set. Must not be null and must refer to a
        /// writable property.</param>
        /// <returns>An <see cref="Action{T1, T2}"/> delegate that takes an object instance and a value, and sets the
        /// specified property on the instance to the provided value.</returns>
        internal static Action<object, object> CreateSetter(
            PropertyInfo property)
        {
            var instance = Expression.Parameter(
                typeof(object),
                "instance");

            var value = Expression.Parameter(
                typeof(object),
                "value");

            var castInstance = Expression.Convert(
                instance,
                property.DeclaringType ?? throw new InvalidOperationException(
                    $"Property '{property.Name}' does not have a declaring type."));

            var castValue = Expression.Convert(
                value,
                property.PropertyType);

            var assign = Expression.Assign(
                Expression.Property(
                    castInstance,
                    property),
                castValue
            );

            return Expression
                .Lambda<Action<object, object>>(
                    assign,
                    instance,
                    value)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that instantiates a nested entity and assigns it to the parent in a single invocation.
        /// </summary>
        /// <param name="property">The navigation property that receives the nested entity.</param>
        /// <returns>A delegate that creates the nested entity, assigns it to the parent, and returns it as <see cref="ITable"/>.</returns>
        internal static Func<object, ITable> CreateNestedEntityActivator(
            PropertyInfo property)
        {
            var parent = Expression.Parameter(
                typeof(object),
                "parent");

            var child = Expression.Variable(
                property.PropertyType,
                "child");

            var assignChild = Expression.Assign(
                child,
                Expression.New(property.PropertyType));

            var assignProperty = Expression.Assign(
                Expression.Property(
                    Expression.Convert(
                        parent,
                        property.DeclaringType ?? throw new InvalidOperationException(
                            $"Property '{property.Name}' does not have a declaring type.")),
                    property),
                child);

            var body = Expression.Block(
                new[] { child }.AsEnumerable(),
                assignChild,
                assignProperty,
                Expression.Convert(
                    child,
                    typeof(ITable)));

            return Expression
                .Lambda<Func<object, ITable>>(
                    body,
                    parent)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that materializes a nested leaf entity in a single compiled action.
        /// </summary>
        /// <param name="property">The navigation property that receives the nested entity.</param>
        /// <param name="usesPrimaryKey">Indicates whether the nested entity uses a primary key to decide instantiation.</param>
        /// <param name="primaryKeyOrdinal">The ordinal of the primary key column when available.</param>
        /// <param name="candidateOrdinals">Candidate ordinals used when no explicit primary key alias is configured.</param>
        /// <param name="fieldAssigners">The resolved scalar field assigners for the nested entity.</param>
        /// <returns>A compiled delegate that instantiates and populates the nested entity when the current row contains nested data.</returns>
        internal static Action<object, IDataRecord> CreateNestedEntityMaterializer(
            PropertyInfo property,
            bool usesPrimaryKey,
            int primaryKeyOrdinal,
            int[] candidateOrdinals,
            Action<object, IDataRecord>[] fieldAssigners)
        {
            var parent = Expression.Parameter(
                typeof(object),
                "parent");

            var record = Expression.Parameter(
                typeof(IDataRecord),
                "record");

            var child = Expression.Variable(
                property.PropertyType,
                "child");

            var propertyAccess = Expression.Property(
                Expression.Convert(
                    parent,
                    property.DeclaringType ?? throw new InvalidOperationException(
                        $"Property '{property.Name}' does not have a declaring type.")),
                property);

            var body = new List<Expression>
            {
                Expression.Assign(
                    child,
                    Expression.New(property.PropertyType)),
                Expression.Assign(
                    propertyAccess,
                    child)
            };

            var boxedChild = Expression.Convert(
                child,
                typeof(object));

            if (fieldAssigners != null)
            {
                for (var index = 0; index < fieldAssigners.Length; index++)
                {
                    var fieldAssigner = fieldAssigners[index];
                    if (fieldAssigner == null)
                        continue;

                    body.Add(
                        Expression.Invoke(
                            Expression.Constant(fieldAssigner),
                            boxedChild,
                            record));
                }
            }

            var materializerBody = Expression.IfThen(
                CreateNestedInstantiationCondition(
                    record,
                    usesPrimaryKey,
                    primaryKeyOrdinal,
                    candidateOrdinals),
                Expression.Block(
                    new[] { child }.AsEnumerable(),
                    body));

            return Expression
                .Lambda<Action<object, IDataRecord>>(
                    materializerBody,
                    parent,
                    record)
                .Compile();
        }

        /// <summary>
        /// Creates an expression that determines whether a nested object should be instantiated based on the presence
        /// of non-null values in the specified fields of a data record.
        /// </summary>
        /// <remarks>If <paramref name="usesPrimaryKey"/> is <see langword="true"/>, the expression checks
        /// the primary key field for a non-null value. Otherwise, it checks the specified candidate fields and returns
        /// <see langword="true"/> if any are non-null.</remarks>
        /// <param name="record">The parameter expression representing the data record to inspect for null values.</param>
        /// <param name="usesPrimaryKey">A value indicating whether the primary key should be used to determine instantiation.</param>
        /// <param name="primaryKeyOrdinal">The ordinal position of the primary key field in the data record. Must be non-negative if <paramref
        /// name="usesPrimaryKey"/> is <see langword="true"/>.</param>
        /// <param name="candidateOrdinals">An array of ordinal positions for candidate fields to check for non-null values when the primary key is not
        /// used. Can be null or empty.</param>
        /// <returns>An expression that evaluates to <see langword="true"/> if the nested object should be instantiated;
        /// otherwise, <see langword="false"/>.</returns>
        internal static Expression CreateNestedInstantiationCondition(
            ParameterExpression record,
            bool usesPrimaryKey,
            int primaryKeyOrdinal,
            int[] candidateOrdinals)
        {
            if (usesPrimaryKey)
            {
                if (primaryKeyOrdinal < 0)
                    return Expression.Constant(false);

                return Expression.Not(
                    Expression.Call(
                        record,
                        IsDbNullMethod,
                        Expression.Constant(primaryKeyOrdinal)));
            }

            if (candidateOrdinals == null)
                return Expression.Constant(false);

            Expression condition = null;

            for (var index = 0; index < candidateOrdinals.Length; index++)
            {
                var notDbNull = Expression.Not(
                    Expression.Call(
                        record,
                        IsDbNullMethod,
                        Expression.Constant(candidateOrdinals[index])));

                condition = condition == null
                    ? (Expression)notDbNull
                    : Expression.OrElse(
                        condition,
                        notDbNull);
            }

            return condition ?? Expression.Constant(false);
        }

        /// <summary>
        /// Creates a delegate that assigns a value from an IDataRecord to a specified property of an object, using the
        /// given column ordinal and source type.
        /// </summary>
        /// <remarks>If the value in the data record at the specified ordinal is DBNull, the property is
        /// set to its default value. Otherwise, the value is converted from the source type and assigned to the
        /// property.</remarks>
        /// <param name="property">The PropertyInfo representing the property to assign the value to. Must not be null and must have a
        /// declaring type.</param>
        /// <param name="ordinal">The zero-based column ordinal in the IDataRecord from which to retrieve the value.</param>
        /// <param name="sourceType">The type of the value in the data record column. Used to determine how to convert the value before
        /// assignment.</param>
        /// <returns>An Action delegate that assigns the value from the specified column of the IDataRecord to the given property
        /// of the target object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property does not have a declaring type.</exception>
        internal static Action<object, IDataRecord> CreateRecordAssigner(
            PropertyInfo property,
            int ordinal,
            Type sourceType)
        {
            var instance = Expression.Parameter(
                typeof(object),
                "instance");

            var record = Expression.Parameter(
                typeof(IDataRecord),
                "record");

            var propertyAccess = Expression.Property(
                Expression.Convert(
                    instance,
                    property.DeclaringType ?? throw new InvalidOperationException(
                        $"Property '{property.Name}' does not have a declaring type.")),
                property);

            var ordinalExpression = Expression.Constant(ordinal);
            var assignDefault = Expression.Assign(
                propertyAccess,
                Expression.Default(property.PropertyType));
            var assignValue = Expression.Assign(
                propertyAccess,
                CreateValueExpression(
                    record,
                    ordinalExpression,
                    property.PropertyType,
                    sourceType));

            var body = Expression.IfThenElse(
                Expression.Call(
                    record,
                    IsDbNullMethod,
                    ordinalExpression),
                assignDefault,
                assignValue);

            return Expression
                .Lambda<Action<object, IDataRecord>>(
                    body,
                    instance,
                    record)
                .Compile();
        }

        /// <summary>
        /// Creates a factory delegate that instantiates an object of the specified type using its parameterless
        /// constructor.
        /// </summary>
        /// <remarks>This method uses expression trees to generate a factory delegate at runtime. If the
        /// specified type does not have a public parameterless constructor, a runtime exception will occur when the
        /// delegate is invoked.</remarks>
        /// <param name="type">The type of object to instantiate. Must have a public parameterless constructor.</param>
        /// <returns>A delegate that creates a new instance of the specified type when invoked.</returns>
        internal static Func<object> CreateFactory(
            Type type)
            => Expression
                .Lambda<Func<object>>(
                    Expression.New(
                        type))
                .Compile();

        /// <summary>
        /// Creates a factory function that returns the default value for the specified type.
        /// </summary>
        /// <remarks>This method uses expression trees to compile a lambda function that produces the
        /// default value. The returned factory can be used to obtain a new instance of the default value each time it
        /// is invoked.</remarks>
        /// <param name="type">The type for which to generate a default value factory. If the type is a reference type, the factory will
        /// return null; if it is a value type, the factory will return the default value for that type.</param>
        /// <returns>A function that returns the default value of the specified type, or null if the type is a reference type.</returns>
        public static Func<object> CreateDefaultValueFactory(
            Type type)
        {
            if (!type.IsValueType)
                return () => null;

            var body = Expression.Convert(
                Expression.Default(type),
                typeof(object));

            return Expression
                .Lambda<Func<object>>(body)
                .Compile();
        }

        /// <summary>
        /// Creates an expression that retrieves and converts a value from a data record at the specified ordinal to the
        /// desired target type.
        /// </summary>
        /// <remarks>If the target type is an enumeration, the method handles conversion from the
        /// underlying type. The returned expression may include type conversions as necessary to match the requested
        /// target type.</remarks>
        /// <param name="record">The parameter expression representing the data record from which to retrieve the value.</param>
        /// <param name="ordinal">A constant expression specifying the zero-based column ordinal of the value to retrieve.</param>
        /// <param name="targetType">The type to which the retrieved value should be converted.</param>
        /// <param name="sourceType">The original type of the value in the data record.</param>
        /// <returns>An expression that, when executed, retrieves the value at the specified ordinal from the data record and
        /// converts it to the target type.</returns>
        internal static Expression CreateValueExpression(
            ParameterExpression record,
            ConstantExpression ordinal,
            Type targetType,
            Type sourceType)
        {
            var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            var valueExpression = conversionType.IsEnum
                ? CreateEnumExpression(
                    record,
                    ordinal,
                    conversionType,
                    sourceType,
                    targetType)
                : CreateNonEnumExpression(
                    record,
                    ordinal,
                    conversionType,
                    sourceType,
                    targetType);

            return EnsureTargetType(
                valueExpression,
                targetType);
        }

        /// <summary>
        /// Creates an expression that retrieves and converts a value from a data record to a specified enumeration
        /// type.
        /// </summary>
        /// <remarks>The returned expression handles conversion from the underlying type or source type to
        /// the enumeration type, using direct conversion or value conversion as appropriate.</remarks>
        /// <param name="record">The parameter expression representing the data record from which the value is retrieved.</param>
        /// <param name="ordinal">The constant expression specifying the zero-based column ordinal of the value in the data record.</param>
        /// <param name="enumType">The type of the enumeration to which the value should be converted.</param>
        /// <param name="sourceType">The type of the value as stored in the data record.</param>
        /// <param name="targetType">The target type for the conversion, typically matching the property or field type being assigned.</param>
        /// <returns>An expression that, when executed, retrieves the value from the specified column in the data record and
        /// converts it to the specified enumeration type.</returns>
        private static Expression CreateEnumExpression(
            ParameterExpression record,
            ConstantExpression ordinal,
            Type enumType,
            Type sourceType,
            Type targetType)
        {
            var underlyingType = Enum.GetUnderlyingType(enumType);

            if (TryCreateTypedGetterCall(
                record,
                ordinal,
                underlyingType,
                sourceType,
                out var enumValue))
                return Expression.Convert(enumValue, enumType);

            if (TryCreateDirectConversionGetterCall(
                record,
                ordinal,
                sourceType,
                enumType,
                out var converted))
                return converted;

            return CreateConvertedValueExpression(
                record,
                ordinal,
                targetType);
        }

        /// <summary>
        /// Creates an expression that retrieves and converts a value from a data record, handling non-enum types.
        /// </summary>
        /// <remarks>This method attempts to optimize value retrieval and conversion by selecting the most
        /// direct approach available for the given types.</remarks>
        /// <param name="record">The parameter expression representing the data record from which the value is retrieved.</param>
        /// <param name="ordinal">The constant expression specifying the zero-based column ordinal in the data record.</param>
        /// <param name="conversionType">The type to which the value should be converted before assignment.</param>
        /// <param name="sourceType">The original type of the value as stored in the data record.</param>
        /// <param name="targetType">The final target type for the value after conversion.</param>
        /// <returns>An expression that retrieves and converts the value from the specified record and ordinal to the desired
        /// type.</returns>
        private static Expression CreateNonEnumExpression(
            ParameterExpression record,
            ConstantExpression ordinal,
            Type conversionType,
            Type sourceType,
            Type targetType)
        {
            if (TryCreateTypedGetterCall(
                record,
                ordinal,
                conversionType,
                sourceType,
                out var typed))
                return typed;

            if (TryCreateDirectConversionGetterCall(
                record,
                ordinal,
                sourceType,
                conversionType,
                out var converted))
                return converted;

            return CreateConvertedValueExpression(
                record,
                ordinal,
                targetType);
        }

        /// <summary>
        /// Ensures that the specified expression is of the given target type, converting it if necessary.
        /// </summary>
        /// <param name="expression">The expression to check and potentially convert.</param>
        /// <param name="targetType">The type to which the expression should be converted if it does not already match.</param>
        /// <returns>An expression of the specified target type. If the original expression is already of the target type, it is
        /// returned unchanged; otherwise, a converted expression is returned.</returns>
        private static Expression EnsureTargetType(
            Expression expression,
            Type targetType)
            => expression.Type == targetType
                ? expression
                : Expression.Convert(
                    expression,
                    targetType);

        /// <summary>
        /// Attempts to create an expression that calls a typed getter method for the specified target type on the given
        /// record expression.
        /// </summary>
        /// <param name="record">The expression representing the data record instance from which to retrieve the value.</param>
        /// <param name="ordinal">The expression representing the ordinal or index of the field to retrieve.</param>
        /// <param name="targetType">The type of the value to be retrieved from the record.</param>
        /// <param name="sourceType">The type of the value as stored in the record. If null or equal to targetType, a typed getter will be used.</param>
        /// <param name="valueExpression">When this method returns, contains the expression representing the typed getter call if successful;
        /// otherwise, null.</param>
        /// <returns>true if a suitable typed getter call expression was created and assigned to valueExpression; otherwise,
        /// false.</returns>
        private static bool TryCreateTypedGetterCall(
            Expression record,
            Expression ordinal,
            Type targetType,
            Type sourceType,
            out Expression valueExpression)
        {
            if ((sourceType == null || sourceType == targetType) &&
                TypedGetterMethods.TryGetValue(targetType, out var getterMethod) &&
                getterMethod != null)
            {
                valueExpression = Expression.Call(
                    record,
                    getterMethod,
                    ordinal);
                return true;
            }

            valueExpression = null;
            return false;
        }

        /// <summary>
        /// Attempts to create an expression that retrieves and directly converts a value from a data record to the
        /// specified target type.
        /// </summary>
        /// <remarks>This method supports direct conversions between compatible types, including enum
        /// types if the underlying type is compatible. If a direct conversion is not possible, the method returns false
        /// and sets the output expression to null.</remarks>
        /// <param name="record">An expression representing the data record from which the value will be retrieved.</param>
        /// <param name="ordinal">An expression representing the ordinal position of the field within the data record.</param>
        /// <param name="sourceType">The type of the value as stored in the data record.</param>
        /// <param name="targetType">The type to which the value should be converted.</param>
        /// <param name="valueExpression">When this method returns, contains the expression that retrieves and converts the value if the operation
        /// succeeds; otherwise, null.</param>
        /// <returns>true if a direct conversion expression could be created; otherwise, false.</returns>
        private static bool TryCreateDirectConversionGetterCall(
            Expression record,
            Expression ordinal,
            Type sourceType,
            Type targetType,
            out Expression valueExpression)
        {
            if (sourceType == null ||
                !TypedGetterMethods.TryGetValue(sourceType, out var getterMethod) ||
                getterMethod == null)
            {
                valueExpression = null;
                return false;
            }

            if (targetType.IsEnum)
            {
                var enumUnderlyingType = Enum.GetUnderlyingType(targetType);
                if (!CanUseDirectTypedConversion(sourceType, enumUnderlyingType))
                {
                    valueExpression = null;
                    return false;
                }

                var sourceValueExpression = Expression.Call(
                    record,
                    getterMethod,
                    ordinal);

                valueExpression = Expression.Convert(
                    Expression.Convert(
                        sourceValueExpression,
                        enumUnderlyingType),
                    targetType);

                return true;
            }

            if (!CanUseDirectTypedConversion(sourceType, targetType))
            {
                valueExpression = null;
                return false;
            }

            valueExpression = Expression.Convert(
                Expression.Call(
                    record,
                    getterMethod,
                    ordinal),
                targetType);

            return true;
        }

        /// <summary>
        /// Determines whether a direct typed conversion can be performed between the specified source and target types.
        /// </summary>
        /// <remarks>This method considers a direct conversion possible if both types are numeric or if
        /// they are exactly the same type.</remarks>
        /// <param name="sourceType">The type of the source value to be converted.</param>
        /// <param name="targetType">The type to which the source value is to be converted.</param>
        /// <returns>true if the source and target types are the same or both are numeric types; otherwise, false.</returns>
        private static bool CanUseDirectTypedConversion(
            Type sourceType,
            Type targetType)
            => sourceType == targetType ||
               IsNumericType(sourceType) && IsNumericType(targetType);

        /// <summary>
        /// Determines whether the specified type is a supported numeric type.
        /// </summary>
        /// <remarks>This method considers only the primitive numeric types commonly used in .NET. It does
        /// not include unsigned types, BigInteger, or custom numeric types.</remarks>
        /// <param name="type">The type to evaluate for numeric compatibility. Must not be null.</param>
        /// <returns>true if the type is one of the supported numeric types (byte, short, int, long, float, double, or decimal);
        /// otherwise, false.</returns>
        private static bool IsNumericType(
            Type type)
            => type == typeof(byte) ||
               type == typeof(short) ||
               type == typeof(int) ||
               type == typeof(long) ||
               type == typeof(float) ||
               type == typeof(double) ||
               type == typeof(decimal);

        /// <summary>
        /// Creates a unary expression that retrieves a value from a record at the specified ordinal position and
        /// converts it to the given target type.
        /// </summary>
        /// <remarks>The returned expression uses a converter appropriate for the specified target type to
        /// ensure correct type conversion. This is useful when dynamically constructing expressions for data
        /// materialization scenarios.</remarks>
        /// <param name="record">The expression representing the data record from which to retrieve the value.</param>
        /// <param name="ordinal">The expression representing the zero-based ordinal position of the value within the record.</param>
        /// <param name="targetType">The type to which the retrieved value will be converted.</param>
        /// <returns>A unary expression that, when executed, invokes a type-specific converter on the value obtained from the
        /// record at the specified ordinal.</returns>
        private static UnaryExpression CreateConvertedValueExpression(
            Expression record,
            Expression ordinal,
            Type targetType)
            => Expression.Convert(
                Expression.Invoke(
                    Expression.Constant(
                        ObjectExtensions.GetConverter(targetType)),
                    Expression.Call(
                        record,
                        GetValueMethod,
                        ordinal)),
                targetType);
    }
}
