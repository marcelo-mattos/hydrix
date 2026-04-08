using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Primitives;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Hydrix.Mapper.Converters
{
    /// <summary>
    /// Builds the expression trees responsible for converting a source property value into the target destination type.
    /// </summary>
    /// <remarks>
    /// Reflection lookup, option resolution, and expression-tree composition are all concentrated here so the hot path
    /// can execute only compiled delegates.
    /// </remarks>
    internal static class ConverterFactory
    {
        /// <summary>
        /// Stores the method name shared by all <c>ToString</c> reflection lookups in this class.
        /// </summary>
        private const string ToStringMethodName = "ToString";

        /// <summary>
        /// Caches the reflection metadata for <see cref="string.Trim()"/>.
        /// </summary>
        private static readonly MethodInfo StringTrim = typeof(string).GetMethod(
            "Trim",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="string.TrimStart()"/>.
        /// </summary>
        private static readonly MethodInfo StringTrimStart = typeof(string).GetMethod(
            "TrimStart",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="string.TrimEnd()"/>.
        /// </summary>
        private static readonly MethodInfo StringTrimEnd = typeof(string).GetMethod(
            "TrimEnd",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="string.ToUpperInvariant()"/>.
        /// </summary>
        private static readonly MethodInfo StringToUpper = typeof(string).GetMethod(
            "ToUpperInvariant",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="string.ToLowerInvariant()"/>.
        /// </summary>
        private static readonly MethodInfo StringToLower = typeof(string).GetMethod(
            "ToLowerInvariant",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="Guid.ToString(string)"/>.
        /// </summary>
        private static readonly MethodInfo GuidToStringFormat = typeof(Guid).GetMethod(
            ToStringMethodName,
            new[] { typeof(string) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="DateTime.ToUniversalTime()"/>.
        /// </summary>
        private static readonly MethodInfo DateTimeToUtc = typeof(DateTime).GetMethod(
            "ToUniversalTime",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="DateTime.ToLocalTime()"/>.
        /// </summary>
        private static readonly MethodInfo DateTimeToLocal = typeof(DateTime).GetMethod(
            "ToLocalTime",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="DateTime.ToString(string,IFormatProvider)"/>.
        /// </summary>
        private static readonly MethodInfo DateTimeToStringFormat = typeof(DateTime).GetMethod(
            ToStringMethodName,
            new[] { typeof(string), typeof(IFormatProvider) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="DateTimeOffset.ToUniversalTime()"/>.
        /// </summary>
        private static readonly MethodInfo DateTimeOffsetToUtc = typeof(DateTimeOffset).GetMethod(
            "ToUniversalTime",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="DateTimeOffset.ToLocalTime()"/>.
        /// </summary>
        private static readonly MethodInfo DateTimeOffsetToLocal = typeof(DateTimeOffset).GetMethod(
            "ToLocalTime",
            Type.EmptyTypes);

        /// <summary>
        /// Caches the reflection metadata for <see cref="DateTimeOffset.ToString(string,IFormatProvider)"/>.
        /// </summary>
        private static readonly MethodInfo DateTimeOffsetToStringFormat = typeof(DateTimeOffset).GetMethod(
            ToStringMethodName,
            new[] { typeof(string), typeof(IFormatProvider) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Ceiling(double)"/>.
        /// </summary>
        private static readonly MethodInfo MathCeilingDouble = typeof(Math).GetMethod(
            "Ceiling",
            new[] { typeof(double) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Floor(double)"/>.
        /// </summary>
        private static readonly MethodInfo MathFloorDouble = typeof(Math).GetMethod(
            "Floor",
            new[] { typeof(double) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Truncate(double)"/>.
        /// </summary>
        private static readonly MethodInfo MathTruncateDouble = typeof(Math).GetMethod(
            "Truncate",
            new[] { typeof(double) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Round(double,MidpointRounding)"/>.
        /// </summary>
        private static readonly MethodInfo MathRoundDouble = typeof(Math).GetMethod(
            "Round",
            new[] { typeof(double), typeof(MidpointRounding) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Ceiling(decimal)"/>.
        /// </summary>
        private static readonly MethodInfo MathCeilingDecimal = typeof(Math).GetMethod(
            "Ceiling",
            new[] { typeof(decimal) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Floor(decimal)"/>.
        /// </summary>
        private static readonly MethodInfo MathFloorDecimal = typeof(Math).GetMethod(
            "Floor",
            new[] { typeof(decimal) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Truncate(decimal)"/>.
        /// </summary>
        private static readonly MethodInfo MathTruncateDecimal = typeof(Math).GetMethod(
            "Truncate",
            new[] { typeof(decimal) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Round(decimal,MidpointRounding)"/>.
        /// </summary>
        private static readonly MethodInfo MathRoundDecimal = typeof(Math).GetMethod(
            "Round",
            new[] { typeof(decimal), typeof(MidpointRounding) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Min(long,long)"/>.
        /// </summary>
        private static readonly MethodInfo MathMinLong = typeof(Math).GetMethod(
            "Min",
            new[] { typeof(long), typeof(long) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Max(long,long)"/>.
        /// </summary>
        private static readonly MethodInfo MathMaxLong = typeof(Math).GetMethod(
            "Max",
            new[] { typeof(long), typeof(long) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="ClampDecimalToLong(decimal,long,long)"/>.
        /// </summary>
        private static readonly MethodInfo ClampDecimalToLongMethod =
            ((Func<decimal, long, long, long>)ClampDecimalToLong).Method;

        /// <summary>
        /// Caches the reflection metadata for <see cref="ClampDoubleToLong(double,long,long)"/>.
        /// </summary>
        private static readonly MethodInfo ClampDoubleToLongMethod =
            ((Func<double, long, long, long>)ClampDoubleToLong).Method;

        /// <summary>
        /// Caches the reflection metadata for <see cref="object.ToString()"/>.
        /// </summary>
        private static readonly MethodInfo ObjectToString = typeof(object).GetMethod(
            ToStringMethodName,
            Type.EmptyTypes);

#if NET6_0_OR_GREATER

        /// <summary>
        /// Caches the reflection metadata for <see cref="DateOnly.ToString(string,IFormatProvider)"/>.
        /// </summary>
        private static readonly MethodInfo DateOnlyToStringFormat = typeof(DateOnly).GetMethod(
            ToStringMethodName,
            new[] { typeof(string), typeof(IFormatProvider) });

#endif

        /// <summary>
        /// Builds the conversion expression for the supplied source property access.
        /// </summary>
        /// <param name="srcPropAccess">The expression that reads the source property value.</param>
        /// <param name="srcType">The source property type.</param>
        /// <param name="dstType">The destination property type.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>An expression that reads, converts, and null-propagates the source value.</returns>
        internal static Expression BuildConversionExpression(
            Expression srcPropAccess,
            Type srcType,
            Type dstType,
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (TryBuildConversionExpression(
                    srcPropAccess,
                    srcType,
                    dstType,
                    options,
                    attr,
                    out var expression,
                    out var errorMessage))
            {
                return expression;
            }

            throw new NotSupportedException(
                errorMessage);
        }

        /// <summary>
        /// Attempts to build the conversion expression for the supplied source property access.
        /// </summary>
        /// <param name="srcPropAccess">The expression that reads the source property value.</param>
        /// <param name="srcType">The source property type.</param>
        /// <param name="dstType">The destination property type.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <param name="expression">
        /// When this method returns <see langword="true"/>, contains the expression that reads, converts, and
        /// null-propagates the source value.
        /// </param>
        /// <param name="errorMessage">
        /// When this method returns <see langword="false"/>, contains the reason the conversion could not be built.
        /// </param>
        /// <returns><see langword="true"/> when the conversion could be built; otherwise, <see langword="false"/>.</returns>
        internal static bool TryBuildConversionExpression(
            Expression srcPropAccess,
            Type srcType,
            Type dstType,
            HydrixMapperOptions options,
            MapConversionAttribute attr,
            out Expression expression,
            out string errorMessage)
        {
            var srcUnderlying = Nullable.GetUnderlyingType(
                srcType) ?? srcType;
            var dstUnderlying = Nullable.GetUnderlyingType(
                dstType) ?? dstType;
            var srcIsNullable = srcType != srcUnderlying;
            var dstIsNullable = dstType != dstUnderlying;
            var srcIsReferenceType = !srcType.IsValueType;

            var sourceValue = srcIsNullable
                ? Expression.Property(
                    srcPropAccess,
                    "Value")
                : srcPropAccess;

            if (!TryBuildCoreConversion(
                    sourceValue,
                    srcUnderlying,
                    dstUnderlying,
                    options,
                    attr,
                    out var coreExpression,
                    out errorMessage))
            {
                expression = null;
                return false;
            }

            var destinationExpression = dstIsNullable
                ? Expression.Convert(
                    coreExpression,
                    dstType)
                : coreExpression;

            if (srcIsNullable)
            {
                var hasValue = Expression.Property(
                    srcPropAccess,
                    "HasValue");
                var nullDestination = Expression.Default(
                    dstType);
                expression = Expression.Condition(
                    hasValue,
                    destinationExpression,
                    nullDestination);
                return true;
            }

            if (srcIsReferenceType)
            {
                // When no conversion was applied the core expression is the same object as sourceValue.
                // Direct assignment handles null naturally, so the explicit null check can be skipped.
                if (ReferenceEquals(
                        coreExpression,
                        sourceValue))
                {
                    expression = srcPropAccess;
                    return true;
                }

                var isNull = Expression.Equal(
                    srcPropAccess,
                    Expression.Constant(
                        null,
                        srcType));
                var nullDestination = Expression.Default(
                    dstType);
                expression = Expression.Condition(
                    isNull,
                    nullDestination,
                    destinationExpression);
                return true;
            }

            expression = destinationExpression;
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Builds the non-nullable conversion expression for the supplied type pair.
        /// </summary>
        /// <param name="srcValue">The expression that yields the non-nullable source value.</param>
        /// <param name="srcType">The non-nullable source type.</param>
        /// <param name="dstType">The non-nullable destination type.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <param name="expression">The resulting conversion expression.</param>
        /// <param name="errorMessage">When the conversion cannot be built, contains the reason why.</param>
        /// <returns>The expression that performs the requested conversion.</returns>
        private static bool TryBuildCoreConversion(
            Expression srcValue,
            Type srcType,
            Type dstType,
            HydrixMapperOptions options,
            MapConversionAttribute attr,
            out Expression expression,
            out string errorMessage)
        {
            if (srcType == dstType)
            {
                expression = srcType == typeof(string)
                    ? BuildStringTransform(
                        srcValue,
                        ResolveStringTransform(
                            options,
                            attr))
                    : srcValue;
                errorMessage = null;
                return true;
            }

            if (dstType == typeof(string))
            {
                return TryBuildToStringConversion(
                    srcValue,
                    srcType,
                    options,
                    attr,
                    out expression,
                    out errorMessage);
            }

            if (srcType.IsEnum && IsIntegerType(dstType))
            {
                expression = Expression.Convert(
                    srcValue,
                    dstType);
                errorMessage = null;
                return true;
            }

            if (dstType.IsEnum && IsIntegerType(srcType))
            {
                expression = Expression.Convert(
                    srcValue,
                    dstType);
                errorMessage = null;
                return true;
            }

            if (IsDecimalFloatType(srcType) && IsIntegerType(dstType))
            {
                expression = BuildDecimalToInteger(
                    srcValue,
                    srcType,
                    dstType,
                    options,
                    attr);
                errorMessage = null;
                return true;
            }

            if (IsNumericType(srcType) && IsNumericType(dstType))
            {
                expression = BuildNumericCast(
                    srcValue,
                    dstType,
                    ResolveOverflow(
                        options,
                        attr));
                errorMessage = null;
                return true;
            }

            expression = null;
            errorMessage =
                $"Hydrix.Mapper: no built-in conversion from '{srcType.FullName}' to '{dstType.FullName}'. " +
                "Ensure the property types are compatible or use a supported conversion pair.";
            return false;
        }

        /// <summary>
        /// Attempts to build the conversion expression for any source type whose destination is <see cref="string"/>.
        /// </summary>
        /// <param name="srcValue">The expression that yields the non-nullable source value.</param>
        /// <param name="srcType">The non-nullable source type.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <param name="expression">When this method returns <see langword="true"/>, contains the expression that yields the formatted string value.</param>
        /// <param name="errorMessage">When this method returns <see langword="false"/>, contains the reason the conversion could not be built.</param>
        /// <returns>The expression that yields the formatted string value.</returns>
        private static bool TryBuildToStringConversion(
            Expression srcValue,
            Type srcType,
            HydrixMapperOptions options,
            MapConversionAttribute attr,
            out Expression expression,
            out string errorMessage)
        {
            if (srcType == typeof(Guid))
            {
                expression = BuildGuidToString(
                    srcValue,
                    ResolveGuidFormat(
                        options,
                        attr),
                    ResolveGuidCase(
                        options,
                        attr));
                errorMessage = null;
                return true;
            }

            if (srcType == typeof(DateTime))
            {
                expression = BuildDateTimeToString(
                    srcValue,
                    options,
                    attr);
                errorMessage = null;
                return true;
            }

            if (srcType == typeof(DateTimeOffset))
            {
                expression = BuildDateTimeOffsetToString(
                    srcValue,
                    options,
                    attr);
                errorMessage = null;
                return true;
            }

#if NET6_0_OR_GREATER
            if (srcType == typeof(DateOnly))
            {
                expression = BuildDateOnlyToString(
                    srcValue,
                    options,
                    attr);
                errorMessage = null;
                return true;
            }
#endif

            if (srcType == typeof(bool))
            {
                expression = BuildBoolToString(
                    srcValue,
                    options,
                    attr);
                errorMessage = null;
                return true;
            }

            if (srcType.IsEnum)
            {
                expression = Expression.Call(
                    Expression.Convert(
                        srcValue,
                        typeof(object)),
                    ObjectToString);
                errorMessage = null;
                return true;
            }

            expression = null;
            errorMessage =
                $"Hydrix.Mapper: no built-in conversion from '{srcType.FullName}' to 'System.String'. " +
                "Ensure the property types are compatible or use a supported conversion pair.";
            return false;
        }

        /// <summary>
        /// Builds the expression that applies the configured string transformation pipeline.
        /// </summary>
        /// <param name="src">The source string expression.</param>
        /// <param name="transform">The transformation flags to apply.</param>
        /// <returns>The expression that yields the transformed string.</returns>
        private static Expression BuildStringTransform(
            Expression src,
            StringTransforms transform)
        {
            if (transform == StringTransforms.None)
                return src;

            var current = src;

            if ((transform & StringTransforms.Trim) != 0)
            {
                current = Expression.Call(
                    current,
                    StringTrim);
            }
            else if ((transform & StringTransforms.TrimStart) != 0)
            {
                current = Expression.Call(
                    current,
                    StringTrimStart);
            }
            else if ((transform & StringTransforms.TrimEnd) != 0)
            {
                current = Expression.Call(
                    current,
                    StringTrimEnd);
            }

            if ((transform & StringTransforms.Uppercase) != 0)
            {
                current = Expression.Call(
                    current,
                    StringToUpper);
            }
            else if ((transform & StringTransforms.Lowercase) != 0)
            {
                current = Expression.Call(
                    current,
                    StringToLower);
            }

            return current;
        }

        /// <summary>
        /// Builds the expression that formats a Guid value as text.
        /// </summary>
        /// <param name="src">The Guid source expression.</param>
        /// <param name="format">The configured Guid format.</param>
        /// <param name="casing">The configured Guid casing.</param>
        /// <returns>The expression that yields the formatted Guid string.</returns>
        private static MethodCallExpression BuildGuidToString(
            Expression src,
            GuidFormat format,
            GuidCase casing)
        {
            var formatString = format switch
            {
                GuidFormat.DigitsOnly => "N",
                GuidFormat.Braces => "B",
                GuidFormat.Parentheses => "P",
                _ => "D",
            };

            var call = Expression.Call(
                src,
                GuidToStringFormat,
                Expression.Constant(
                    formatString));

            return casing == GuidCase.Upper
                ? Expression.Call(
                    call,
                    StringToUpper)
                : call;
        }

        /// <summary>
        /// Builds the expression that normalizes and formats a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="src">The <see cref="DateTime"/> source expression.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The expression that yields the formatted date or time string.</returns>
        private static MethodCallExpression BuildDateTimeToString(
            Expression src,
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            var (format, timeZone, culture) = ResolveDateTimeParams(
                options,
                attr);
            var dateTime = src;

            if (timeZone == DateTimeZone.ToUtc)
            {
                dateTime = Expression.Call(
                    dateTime,
                    DateTimeToUtc);
            }
            else if (timeZone == DateTimeZone.ToLocal)
            {
                dateTime = Expression.Call(
                    dateTime,
                    DateTimeToLocal);
            }

            return Expression.Call(
                dateTime,
                DateTimeToStringFormat,
                Expression.Constant(
                    format),
                Expression.Constant(
                    ResolveCulture(
                        culture),
                    typeof(IFormatProvider)));
        }

        /// <summary>
        /// Builds the expression that normalizes and formats a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="src">The <see cref="DateTimeOffset"/> source expression.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The expression that yields the formatted offset string.</returns>
        private static MethodCallExpression BuildDateTimeOffsetToString(
            Expression src,
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            var (format, timeZone, culture) = ResolveDateTimeParams(
                options,
                attr);
            var dateTimeOffset = src;

            if (timeZone == DateTimeZone.ToUtc)
            {
                dateTimeOffset = Expression.Call(
                    dateTimeOffset,
                    DateTimeOffsetToUtc);
            }
            else if (timeZone == DateTimeZone.ToLocal)
            {
                dateTimeOffset = Expression.Call(
                    dateTimeOffset,
                    DateTimeOffsetToLocal);
            }

            return Expression.Call(
                dateTimeOffset,
                DateTimeOffsetToStringFormat,
                Expression.Constant(
                    format),
                Expression.Constant(
                    ResolveCulture(
                        culture),
                    typeof(IFormatProvider)));
        }

#if NET6_0_OR_GREATER

        /// <summary>
        /// Builds the expression that formats a <see cref="DateOnly"/> value.
        /// </summary>
        /// <param name="src">The <see cref="DateOnly"/> source expression.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The expression that yields the formatted date string.</returns>
        private static MethodCallExpression BuildDateOnlyToString(
            Expression src,
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            var (format, _, culture) = ResolveDateTimeParams(
                options,
                attr);

            return Expression.Call(
                src,
                DateOnlyToStringFormat,
                Expression.Constant(
                    format),
                Expression.Constant(
                    ResolveCulture(
                        culture),
                    typeof(IFormatProvider)));
        }

#endif

        /// <summary>
        /// Builds the expression that formats a boolean value as text.
        /// </summary>
        /// <param name="src">The boolean source expression.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The expression that yields the configured true or false text.</returns>
        private static ConditionalExpression BuildBoolToString(
            Expression src,
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            BoolStringFormat format;
            string trueValue;
            string falseValue;

            if (attr != null && attr.OverrideBool)
            {
                format = attr.BoolFormat;
                trueValue = attr.TrueValue;
                falseValue = attr.FalseValue;
            }
            else
            {
                format = options.Bool.StringFormat;
                trueValue = options.Bool.TrueValue;
                falseValue = options.Bool.FalseValue;
            }

            var (resolvedTrue, resolvedFalse) = ResolveBoolStrings(
                format,
                trueValue,
                falseValue);
            return Expression.Condition(
                src,
                Expression.Constant(
                    resolvedTrue),
                Expression.Constant(
                    resolvedFalse));
        }

        /// <summary>
        /// Resolves the concrete true and false strings for the supplied bool formatting preset.
        /// </summary>
        /// <param name="format">The preset that identifies the desired output pair.</param>
        /// <param name="customTrue">The caller-provided custom true string.</param>
        /// <param name="customFalse">The caller-provided custom false string.</param>
        /// <returns>The resolved strings emitted for true and false values.</returns>
        private static (string trueString, string falseString) ResolveBoolStrings(
            BoolStringFormat format,
            string customTrue,
            string customFalse)
        {
            return format switch
            {
                BoolStringFormat.LowercaseTrueOrFalse => ("true", "false"),
                BoolStringFormat.YesOrNo => ("Yes", "No"),
                BoolStringFormat.YOrN => ("Y", "N"),
                BoolStringFormat.OneOrZero => ("1", "0"),
                BoolStringFormat.TOrF => ("T", "F"),
                BoolStringFormat.Custom => (customTrue ?? "true", customFalse ?? "false"),
                _ => ("True", "False"),
            };
        }

        /// <summary>
        /// Builds the conversion expression used when a decimal, double, or float source maps to an integral type.
        /// </summary>
        /// <param name="srcValue">The source numeric expression.</param>
        /// <param name="srcType">The source numeric type.</param>
        /// <param name="dstType">The destination integral type.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The expression that rounds, clamps, and converts the source value when required.</returns>
        private static UnaryExpression BuildDecimalToInteger(
            Expression srcValue,
            Type srcType,
            Type dstType,
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            var rounding = ResolveRounding(
                options,
                attr);
            var overflow = ResolveOverflow(
                options,
                attr);

            if (srcType == typeof(decimal))
            {
                var rounded = ApplyDecimalRounding(
                    srcValue,
                    rounding);

                if (overflow == NumericOverflow.Clamp)
                {
                    return BuildClampedCast(
                        rounded,
                        dstType);
                }

                var asLong = Expression.Convert(
                    rounded,
                    typeof(long));
                return BuildNumericCast(
                    asLong,
                    dstType,
                    overflow);
            }

            var asDouble = srcType == typeof(double)
                ? srcValue
                : Expression.Convert(
                    srcValue,
                    typeof(double));
            var roundedDouble = ApplyDoubleRounding(
                asDouble,
                rounding);

            if (overflow == NumericOverflow.Clamp)
            {
                return BuildClampedCast(
                    roundedDouble,
                    dstType);
            }

            var asLongFromDouble = Expression.Convert(
                roundedDouble,
                typeof(long));
            return BuildNumericCast(
                asLongFromDouble,
                dstType,
                overflow);
        }

        /// <summary>
        /// Applies the configured rounding strategy to a decimal expression.
        /// </summary>
        /// <param name="src">The decimal source expression.</param>
        /// <param name="rounding">The rounding strategy to apply.</param>
        /// <returns>The expression that yields the rounded decimal value.</returns>
        private static MethodCallExpression ApplyDecimalRounding(
            Expression src,
            NumericRounding rounding)
        {
            return rounding switch
            {
                NumericRounding.Ceiling => Expression.Call(
                    MathCeilingDecimal,
                    src),
                NumericRounding.Floor => Expression.Call(
                    MathFloorDecimal,
                    src),
                NumericRounding.Nearest => Expression.Call(
                    MathRoundDecimal,
                    src,
                    Expression.Constant(
                        MidpointRounding.AwayFromZero)),
                NumericRounding.Banker => Expression.Call(
                    MathRoundDecimal,
                    src,
                    Expression.Constant(
                        MidpointRounding.ToEven)),
                _ => Expression.Call(
                    MathTruncateDecimal,
                    src),
            };
        }

        /// <summary>
        /// Applies the configured rounding strategy to a double expression.
        /// </summary>
        /// <param name="src">The double source expression.</param>
        /// <param name="rounding">The rounding strategy to apply.</param>
        /// <returns>The expression that yields the rounded double value.</returns>
        private static MethodCallExpression ApplyDoubleRounding(
            Expression src,
            NumericRounding rounding)
        {
            return rounding switch
            {
                NumericRounding.Ceiling => Expression.Call(
                    MathCeilingDouble,
                    src),
                NumericRounding.Floor => Expression.Call(
                    MathFloorDouble,
                    src),
                NumericRounding.Nearest => Expression.Call(
                    MathRoundDouble,
                    src,
                    Expression.Constant(
                        MidpointRounding.AwayFromZero)),
                NumericRounding.Banker => Expression.Call(
                    MathRoundDouble,
                    src,
                    Expression.Constant(
                        MidpointRounding.ToEven)),
                _ => Expression.Call(
                    MathTruncateDouble,
                    src),
            };
        }

        /// <summary>
        /// Builds the final numeric cast expression according to the configured overflow behavior.
        /// </summary>
        /// <param name="src">The numeric source expression to cast.</param>
        /// <param name="dstType">The numeric destination type.</param>
        /// <param name="overflow">The configured overflow behavior.</param>
        /// <returns>The expression that performs the appropriate checked, clamped, or unchecked conversion.</returns>
        private static UnaryExpression BuildNumericCast(
            Expression src,
            Type dstType,
            NumericOverflow overflow)
        {
            switch (overflow)
            {
                case NumericOverflow.Throw:
                    return Expression.ConvertChecked(
                        src,
                        dstType);

                case NumericOverflow.Clamp:
                    return BuildClampedCast(
                        src,
                        dstType);

                default:
                    return Expression.Convert(
                        src,
                        dstType);
            }
        }

        /// <summary>
        /// Builds the expression that clamps an intermediate long, decimal, or double value into the valid destination range.
        /// </summary>
        /// <param name="src">The intermediate numeric expression.</param>
        /// <param name="dstType">The integral destination type whose range should be enforced.</param>
        /// <returns>The expression that yields the clamped destination value.</returns>
        private static UnaryExpression BuildClampedCast(
            Expression src,
            Type dstType)
        {
            var minValue = GetMinLong(
                dstType);
            var maxValue = GetMaxLong(
                dstType);

            if (src.Type == typeof(long))
            {
                var clampedLong = Expression.Call(
                    MathMinLong,
                    Expression.Constant(
                        maxValue),
                    Expression.Call(
                        MathMaxLong,
                        Expression.Constant(
                            minValue),
                        src));
                return Expression.Convert(
                    clampedLong,
                    dstType);
            }

            if (src.Type == typeof(decimal))
            {
                var clampedLongFromDecimal = Expression.Call(
                    ClampDecimalToLongMethod,
                    src,
                    Expression.Constant(
                        minValue),
                    Expression.Constant(
                        maxValue));
                return Expression.Convert(
                    clampedLongFromDecimal,
                    dstType);
            }

            var clampedLongFromDouble = Expression.Call(
                ClampDoubleToLongMethod,
                src.Type == typeof(double)
                    ? src
                    : Expression.Convert(
                        src,
                        typeof(double)),
                Expression.Constant(
                    minValue),
                Expression.Constant(
                    maxValue));
            return Expression.Convert(
                clampedLongFromDouble,
                dstType);
        }

        /// <summary>
        /// Converts a rounded decimal value into a clamped long without overflowing the intermediate cast.
        /// </summary>
        /// <param name="value">The rounded decimal value to clamp.</param>
        /// <param name="minValue">The inclusive minimum supported value.</param>
        /// <param name="maxValue">The inclusive maximum supported value.</param>
        /// <returns>The clamped long value.</returns>
        private static long ClampDecimalToLong(
            decimal value,
            long minValue,
            long maxValue)
        {
            if (value <= minValue)
                return minValue;

            if (value >= maxValue)
                return maxValue;

            return (long)value;
        }

        /// <summary>
        /// Converts a rounded floating-point value into a clamped long without overflowing the intermediate cast.
        /// </summary>
        /// <param name="value">The rounded floating-point value to clamp.</param>
        /// <param name="minValue">The inclusive minimum supported value.</param>
        /// <param name="maxValue">The inclusive maximum supported value.</param>
        /// <returns>The clamped long value.</returns>
        private static long ClampDoubleToLong(
            double value,
            long minValue,
            long maxValue)
        {
            if (double.IsNaN(value))
                return 0L;

            if (value <= minValue)
                return minValue;

            if (value >= maxValue)
                return maxValue;

            return (long)value;
        }

        /// <summary>
        /// Resolves the string transformation to use for the current property.
        /// </summary>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The effective string transformation.</returns>
        private static StringTransforms ResolveStringTransform(
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (attr != null && (attr.OverrideStringTransform || attr.StringTransform != StringTransforms.None))
                return attr.StringTransform;

            return options.String.Transform;
        }

        /// <summary>
        /// Resolves the Guid format to use for the current property.
        /// </summary>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The effective Guid format.</returns>
        private static GuidFormat ResolveGuidFormat(
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (attr != null && attr.OverrideGuid)
                return attr.GuidFormat;

            return options.Guid.Format;
        }

        /// <summary>
        /// Resolves the Guid casing to use for the current property.
        /// </summary>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The effective Guid casing.</returns>
        private static GuidCase ResolveGuidCase(
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (attr != null && attr.OverrideGuid)
                return attr.GuidCase;

            return options.Guid.Case;
        }

        /// <summary>
        /// Resolves the numeric rounding strategy to use for the current property.
        /// </summary>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The effective numeric rounding strategy.</returns>
        private static NumericRounding ResolveRounding(
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (attr != null && attr.OverrideNumeric)
                return attr.NumericRounding;

            return options.Numeric.DecimalToIntRounding;
        }

        /// <summary>
        /// Resolves the numeric overflow behavior to use for the current property.
        /// </summary>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The effective numeric overflow behavior.</returns>
        private static NumericOverflow ResolveOverflow(
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (attr != null && attr.OverrideNumeric)
                return attr.NumericOverflow;

            return options.Numeric.Overflow;
        }

        /// <summary>
        /// Resolves the effective date and time formatting tuple for the current property.
        /// </summary>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>A tuple containing the format string, timezone normalization, and culture name to apply.</returns>
        private static (string format, DateTimeZone timeZone, string culture) ResolveDateTimeParams(
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (attr != null && attr.OverrideDateTime)
            {
                return (
                    attr.DateFormat ?? options.DateTime.StringFormat ?? "O",
                    attr.DateTimeZone,
                    attr.Culture ?? options.DateTime.Culture ?? string.Empty);
            }

            return (
                options.DateTime.StringFormat ?? "O",
                options.DateTime.TimeZone,
                options.DateTime.Culture ?? string.Empty);
        }

        /// <summary>
        /// Resolves the format provider for the supplied culture name.
        /// </summary>
        /// <param name="cultureName">The configured culture name.</param>
        /// <returns>The format provider that should be used for string formatting.</returns>
        private static CultureInfo ResolveCulture(
            string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                return CultureInfo.InvariantCulture;

            return CultureInfo.GetCultureInfo(
                cultureName);
        }

        /// <summary>
        /// Determines whether the supplied type is one of the integral numeric types handled by the mapper.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns><see langword="true"/> when the type is integral; otherwise, <see langword="false"/>.</returns>
        internal static bool IsIntegerType(
            Type type) =>
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(short) ||
            type == typeof(byte) ||
            type == typeof(sbyte) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong);

        /// <summary>
        /// Determines whether the supplied type is a decimal or floating-point numeric type.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns><see langword="true"/> when the type is decimal, double, or float; otherwise, <see langword="false"/>.</returns>
        internal static bool IsDecimalFloatType(
            Type type) =>
            type == typeof(decimal) ||
            type == typeof(double) ||
            type == typeof(float);

        /// <summary>
        /// Determines whether the supplied type belongs to the numeric families handled by the mapper.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns><see langword="true"/> when the type is numeric; otherwise, <see langword="false"/>.</returns>
        internal static bool IsNumericType(
            Type type) =>
            IsIntegerType(
                type) || IsDecimalFloatType(
                type);

        /// <summary>
        /// Returns the effective minimum value, expressed as a long, for the supplied integral destination type.
        /// </summary>
        /// <param name="type">The destination integral type.</param>
        /// <returns>The minimum destination value normalized to <see cref="long"/>.</returns>
        private static long GetMinLong(
            Type type)
        {
            if (type == typeof(int))
                return int.MinValue;
            if (type == typeof(short))
                return short.MinValue;
            if (type == typeof(byte))
                return byte.MinValue;
            if (type == typeof(sbyte))
                return sbyte.MinValue;
            if (type == typeof(ushort))
                return ushort.MinValue;
            if (type == typeof(uint))
                return uint.MinValue;
            if (type == typeof(ulong))
                return (long)ulong.MinValue;

            return long.MinValue;
        }

        /// <summary>
        /// Returns the effective maximum value, expressed as a long, for the supplied integral destination type.
        /// </summary>
        /// <param name="type">The destination integral type.</param>
        /// <returns>The maximum destination value normalized to <see cref="long"/>.</returns>
        private static long GetMaxLong(
            Type type)
        {
            if (type == typeof(int))
                return int.MaxValue;
            if (type == typeof(short))
                return short.MaxValue;
            if (type == typeof(byte))
                return byte.MaxValue;
            if (type == typeof(sbyte))
                return sbyte.MaxValue;
            if (type == typeof(ushort))
                return ushort.MaxValue;
            if (type == typeof(uint))
                return uint.MaxValue;
            if (type == typeof(ulong))
                return long.MaxValue;

            return long.MaxValue;
        }
    }
}
