using Hydrix.Mapper.Attributes;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Primitives;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Hydrix.Mapper.UnitTests.Converters
{
    /// <summary>
    /// Covers ConverterFactory helper branches that are difficult to reach exclusively through the mapper public surface.
    /// </summary>
    public class ConverterFactoryCoverageTests
    {
        /// <summary>
        /// Stores the runtime type for the internal ConverterFactory implementation.
        /// </summary>
        private static readonly Type ConverterFactoryType = typeof(HydrixMapper).Assembly.GetType(
            "Hydrix.Mapper.Converters.ConverterFactory",
            throwOnError: true);

        /// <summary>
        /// Stores the private BuildClampedCast method used by the numeric clamp pipeline.
        /// </summary>
        private static readonly MethodInfo BuildClampedCastMethod = GetRequiredPrivateStaticMethod(
            "BuildClampedCast");

        /// <summary>
        /// Stores the private BuildNumericCast method used by the numeric overflow pipeline.
        /// </summary>
        private static readonly MethodInfo BuildNumericCastMethod = GetRequiredPrivateStaticMethod(
            "BuildNumericCast");

#if NET6_0_OR_GREATER

        /// <summary>
        /// Stores the private BuildDateOnlyToString method used by DateOnly formatting.
        /// </summary>
        private static readonly MethodInfo BuildDateOnlyToStringMethod = GetRequiredPrivateStaticMethod(
            "BuildDateOnlyToString");

#endif

        /// <summary>
        /// Stores the private ResolveDateTimeParams method used to combine global and attribute-based date settings.
        /// </summary>
        private static readonly MethodInfo ResolveDateTimeParamsMethod = GetRequiredPrivateStaticMethod(
            "ResolveDateTimeParams");

        /// <summary>
        /// Stores the private ResolveStringTransform method used to combine global and attribute-based string transforms.
        /// </summary>
        private static readonly MethodInfo ResolveStringTransformMethod = GetRequiredPrivateStaticMethod(
            "ResolveStringTransform");

        /// <summary>
        /// Stores the private BuildToStringConversion method used by string destination conversions.
        /// </summary>
        private static readonly MethodInfo BuildToStringConversionMethod = GetRequiredPrivateStaticMethod(
            "BuildToStringConversion");

        /// <summary>
        /// Verifies that BuildNumericCast delegates clamp conversions for long inputs and respects the destination range.
        /// </summary>
        [Fact]
        public void BuildNumericCast_Clamp_WithLongInput_ClampsToIntegralRanges()
        {
            var intExpression = (Expression)BuildNumericCastMethod.Invoke(
                null,
                new object[] { Expression.Constant(long.MaxValue), typeof(int), NumericOverflow.Clamp });
            var shortExpression = (Expression)BuildNumericCastMethod.Invoke(
                null,
                new object[] { Expression.Constant(long.MinValue), typeof(short), NumericOverflow.Clamp });

            Assert.Equal(
                int.MaxValue,
                Execute<int>(
                    intExpression));
            Assert.Equal(
                short.MinValue,
                Execute<short>(
                    shortExpression));
        }

        /// <summary>
        /// Verifies that BuildClampedCast uses the floating-point conversion path for a float source and preserves in-range values.
        /// </summary>
        [Fact]
        public void BuildClampedCast_WithFloatInput_UsesFloatingPointFallbackPath()
        {
            var expression = (Expression)BuildClampedCastMethod.Invoke(
                null,
                new object[] { Expression.Constant(42f), typeof(int) });

            Assert.Equal(
                42,
                Execute<int>(
                    expression));
        }

        /// <summary>
        /// Verifies that BuildClampedCast normalizes NaN values to zero through the floating-point helper.
        /// </summary>
        [Fact]
        public void BuildClampedCast_WithNaN_ReturnsZero()
        {
            var expression = (Expression)BuildClampedCastMethod.Invoke(
                null,
                new object[] { Expression.Constant(double.NaN), typeof(int) });

            Assert.Equal(
                0,
                Execute<int>(
                    expression));
        }

        /// <summary>
        /// Verifies that BuildClampedCast reaches the default long bounds when the destination type is long itself.
        /// </summary>
        [Fact]
        public void BuildClampedCast_WithLongDestination_UsesLongBounds()
        {
            var expression = (Expression)BuildClampedCastMethod.Invoke(
                null,
                new object[] { Expression.Constant(double.MaxValue), typeof(long) });

            Assert.Equal(
                long.MaxValue,
                Execute<long>(
                    expression));
        }

#if NET6_0_OR_GREATER

        /// <summary>
        /// Verifies that a passive DateOnly attribute keeps using the global format and culture settings.
        /// </summary>
        [Fact]
        public void BuildDateOnlyToString_WithPassiveAttribute_UsesGlobalSettings()
        {
            var options = new HydrixMapperOptions();
            options.DateTime.StringFormat = "yyyy-MM-dd";
            options.DateTime.Culture = "en-US";

            var expression = (Expression)BuildDateOnlyToStringMethod.Invoke(
                null,
                new object[]
                {
                    Expression.Constant(new DateOnly(2024, 6, 15)),
                    options,
                    new MapConversionAttribute()
                });

            Assert.Equal(
                "2024-06-15",
                Execute<string>(
                    expression));
        }

        /// <summary>
        /// Verifies that an overriding DateOnly attribute without explicit format or culture falls back to the global settings.
        /// </summary>
        [Fact]
        public void BuildDateOnlyToString_WithOverrideAndNoValues_FallsBackToGlobalSettings()
        {
            var options = new HydrixMapperOptions();
            options.DateTime.StringFormat = "dd/MM/yyyy";
            options.DateTime.Culture = "pt-BR";

            var expression = (Expression)BuildDateOnlyToStringMethod.Invoke(
                null,
                new object[]
                {
                    Expression.Constant(new DateOnly(2024, 6, 15)),
                    options,
                    new MapConversionAttribute { OverrideDateTime = true }
                });

            Assert.Equal(
                new DateOnly(2024, 6, 15).ToString(
                    "dd/MM/yyyy",
                    CultureInfo.GetCultureInfo("pt-BR")),
                Execute<string>(
                    expression));
        }

#endif

        /// <summary>
        /// Verifies that ResolveDateTimeParams falls back to the built-in defaults when an overriding attribute omits format and culture.
        /// </summary>
        [Fact]
        public void ResolveDateTimeParams_WithOverrideAndNoValues_UsesBuiltInDefaults()
        {
            var options = new HydrixMapperOptions();
            options.DateTime.StringFormat = null;
            options.DateTime.Culture = null;

            var result = ((string format, DateTimeZone timeZone, string culture))ResolveDateTimeParamsMethod.Invoke(
                null,
                new object[]
                {
                    options,
                    new MapConversionAttribute { OverrideDateTime = true }
                });

            Assert.Equal(
                "O",
                result.format);
            Assert.Equal(
                DateTimeZone.None,
                result.timeZone);
            Assert.Equal(
                string.Empty,
                result.culture);
        }

        /// <summary>
        /// Verifies that ResolveStringTransform preserves the global configuration when a passive attribute is present.
        /// </summary>
        [Fact]
        public void ResolveStringTransform_WithPassiveAttribute_UsesGlobalTransform()
        {
            var options = new HydrixMapperOptions();
            options.String.Transform = StringTransforms.Uppercase;

            var transform = (StringTransforms)ResolveStringTransformMethod.Invoke(
                null,
                new object[]
                {
                    options,
                    new MapConversionAttribute()
                });

            Assert.Equal(
                StringTransforms.Uppercase,
                transform);
        }

        /// <summary>
        /// Verifies that BuildToStringConversion throws for unsupported source types and reaches the fallback branch.
        /// </summary>
        [Fact]
        public void BuildToStringConversion_ThrowsNotSupportedException_WhenSourceTypeIsUnsupported()
        {
            var exception = Assert.Throws<TargetInvocationException>(
                () => BuildToStringConversionMethod.Invoke(
                    null,
                    new object[]
                    {
                        Expression.Constant(123),
                        typeof(int),
                        new HydrixMapperOptions(),
                        null,
                    }));

            var inner = Assert.IsType<NotSupportedException>(
                exception.InnerException);
            Assert.Contains(
                "System.Int32",
                inner.Message);
            Assert.Contains(
                "System.String",
                inner.Message);
        }

        /// <summary>
        /// Resolves a required private static method from ConverterFactory.
        /// </summary>
        /// <param name="name">The method name to resolve.</param>
        /// <returns>The resolved method metadata.</returns>
        private static MethodInfo GetRequiredPrivateStaticMethod(
            string name) =>
            ConverterFactoryType.GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException(
                $"Method '{name}' was not found on ConverterFactory.");

        /// <summary>
        /// Compiles and executes the supplied expression as a parameterless lambda.
        /// </summary>
        /// <typeparam name="TResult">The expected result type.</typeparam>
        /// <param name="expression">The expression to compile and execute.</param>
        /// <returns>The expression result.</returns>
        private static TResult Execute<TResult>(
            Expression expression) =>
            Expression.Lambda<Func<TResult>>(
                expression).Compile().Invoke();
    }
}
