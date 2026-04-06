using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
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
            "ToString",
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
            "ToString",
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
            "ToString",
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
        /// Caches the reflection metadata for <see cref="Math.Min(double,double)"/>.
        /// </summary>
        private static readonly MethodInfo MathMinDouble = typeof(Math).GetMethod(
            "Min",
            new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="Math.Max(double,double)"/>.
        /// </summary>
        private static readonly MethodInfo MathMaxDouble = typeof(Math).GetMethod(
            "Max",
            new[] { typeof(double), typeof(double) });

        /// <summary>
        /// Caches the reflection metadata for <see cref="ClampDecimalToLong(decimal,long,long)"/>.
        /// </summary>
        private static readonly MethodInfo ClampDecimalToLongMethod = typeof(ConverterFactory).GetMethod(
            nameof(ClampDecimalToLong),
            BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Caches the reflection metadata for <see cref="ClampDoubleToLong(double,long,long)"/>.
        /// </summary>
        private static readonly MethodInfo ClampDoubleToLongMethod = typeof(ConverterFactory).GetMethod(
            nameof(ClampDoubleToLong),
            BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Caches the reflection metadata for <see cref="object.ToString()"/>.
        /// </summary>
        private static readonly MethodInfo ObjectToString = typeof(object).GetMethod(
            "ToString",
            Type.EmptyTypes);

#if NET6_0_OR_GREATER
        /// <summary>
        /// Caches the reflection metadata for <see cref="DateOnly.ToString(string,IFormatProvider)"/>.
        /// </summary>
        private static readonly MethodInfo DateOnlyToStringFormat = typeof(DateOnly).GetMethod(
            "ToString",
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

            var coreExpression = BuildCoreConversion(
                sourceValue,
                srcUnderlying,
                dstUnderlying,
                options,
                attr);
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
                return Expression.Condition(
                    hasValue,
                    destinationExpression,
                    nullDestination);
            }

            if (srcIsReferenceType)
            {
                var isNull = Expression.Equal(
                    srcPropAccess,
                    Expression.Constant(
                        null,
                        srcType));
                var nullDestination = Expression.Default(
                    dstType);
                return Expression.Condition(
                    isNull,
                    nullDestination,
                    destinationExpression);
            }

            return destinationExpression;
        }

        /// <summary>
        /// Builds the non-nullable conversion expression for the supplied type pair.
        /// </summary>
        /// <param name="srcValue">The expression that yields the non-nullable source value.</param>
        /// <param name="srcType">The non-nullable source type.</param>
        /// <param name="dstType">The non-nullable destination type.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The expression that performs the requested conversion.</returns>
        private static Expression BuildCoreConversion(
            Expression srcValue,
            Type srcType,
            Type dstType,
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (srcType == dstType)
            {
                if (srcType == typeof(string))
                {
                    return BuildStringTransform(
                        srcValue,
                        ResolveStringTransform(
                            options,
                            attr));
                }

                return srcValue;
            }

            if (srcType == typeof(Guid) && dstType == typeof(string))
            {
                return BuildGuidToString(
                    srcValue,
                    ResolveGuidFormat(
                        options,
                        attr),
                    ResolveGuidCase(
                        options,
                        attr));
            }

            if (srcType == typeof(DateTime) && dstType == typeof(string))
            {
                return BuildDateTimeToString(
                    srcValue,
                    options,
                    attr);
            }

            if (srcType == typeof(DateTimeOffset) && dstType == typeof(string))
            {
                return BuildDateTimeOffsetToString(
                    srcValue,
                    options,
                    attr);
            }

#if NET6_0_OR_GREATER
            if (srcType == typeof(DateOnly) && dstType == typeof(string))
            {
                return BuildDateOnlyToString(
                    srcValue,
                    options,
                    attr);
            }
#endif

            if (srcType == typeof(bool) && dstType == typeof(string))
            {
                return BuildBoolToString(
                    srcValue,
                    options,
                    attr);
            }
            if (srcType.IsEnum && dstType == typeof(string))
            {
                return Expression.Call(
                    Expression.Convert(
                        srcValue,
                        typeof(object)),
                    ObjectToString);
            }

            if (srcType.IsEnum && IsIntegerType(dstType))
            {
                return Expression.Convert(
                    srcValue,
                    dstType);
            }

            if (dstType.IsEnum && IsIntegerType(srcType))
            {
                return Expression.Convert(
                    srcValue,
                    dstType);
            }

            if (IsDecimalFloatType(srcType) && IsIntegerType(dstType))
            {
                return BuildDecimalToInteger(
                    srcValue,
                    srcType,
                    dstType,
                    options,
                    attr);
            }

            if (IsNumericType(srcType) && IsNumericType(dstType))
            {
                return BuildNumericCast(
                    srcValue,
                    dstType,
                    ResolveOverflow(
                        options,
                        attr));
            }

            throw new NotSupportedException(
                $"Hydrix.Mapper: no built-in conversion from '{srcType.FullName}' to '{dstType.FullName}'. " +
                "Ensure the property types are compatible or use a supported conversion pair.");
        }

        /// <summary>
        /// Builds the expression that applies the configured string transformation pipeline.
        /// </summary>
        /// <param name="src">The source string expression.</param>
        /// <param name="transform">The transformation flags to apply.</param>
        /// <returns>The expression that yields the transformed string.</returns>
        private static Expression BuildStringTransform(
            Expression src,
            StringTransform transform)
        {
            if (transform == StringTransform.None)
                return src;

            var current = src;

            if ((transform & StringTransform.Trim) != 0)
            {
                current = Expression.Call(
                    current,
                    StringTrim);
            }
            else if ((transform & StringTransform.TrimStart) != 0)
            {
                current = Expression.Call(
                    current,
                    StringTrimStart);
            }
            else if ((transform & StringTransform.TrimEnd) != 0)
            {
                current = Expression.Call(
                    current,
                    StringTrimEnd);
            }

            if ((transform & StringTransform.Uppercase) != 0)
            {
                current = Expression.Call(
                    current,
                    StringToUpper);
            }
            else if ((transform & StringTransform.Lowercase) != 0)
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
        private static Expression BuildGuidToString(
            Expression src,
            GuidFormat format,
            GuidCase casing)
        {
            var formatString = format switch
            {
                GuidFormat.N => "N",
                GuidFormat.B => "B",
                GuidFormat.P => "P",
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
                : (Expression)call;
        }

        /// <summary>
        /// Builds the expression that normalizes and formats a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="src">The <see cref="DateTime"/> source expression.</param>
        /// <param name="options">The global option snapshot.</param>
        /// <param name="attr">The optional per-property override attribute.</param>
        /// <returns>The expression that yields the formatted date or time string.</returns>
        private static Expression BuildDateTimeToString(
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
        private static Expression BuildDateTimeOffsetToString(
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
        private static Expression BuildDateOnlyToString(
            Expression src,
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            var dateTimeParams = ResolveDateTimeParams(
                options,
                attr);

            return Expression.Call(
                src,
                DateOnlyToStringFormat,
                Expression.Constant(
                    dateTimeParams.format),
                Expression.Constant(
                    ResolveCulture(
                        dateTimeParams.culture),
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
        private static Expression BuildBoolToString(
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
                BoolStringFormat.LowerCase => ("true", "false"),
                BoolStringFormat.YesNo => ("Yes", "No"),
                BoolStringFormat.YN => ("Y", "N"),
                BoolStringFormat.OneZero => ("1", "0"),
                BoolStringFormat.SN => ("S", "N"),
                BoolStringFormat.SimNao => ("Sim", "Nao"),
                BoolStringFormat.TF => ("T", "F"),
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
        private static Expression BuildDecimalToInteger(
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
        private static Expression ApplyDecimalRounding(
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
        private static Expression ApplyDoubleRounding(
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
        private static Expression BuildNumericCast(
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
        private static Expression BuildClampedCast(
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
        private static StringTransform ResolveStringTransform(
            HydrixMapperOptions options,
            MapConversionAttribute attr)
        {
            if (attr != null && (attr.OverrideStringTransform || attr.StringTransform != StringTransform.None))
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
        private static IFormatProvider ResolveCulture(
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