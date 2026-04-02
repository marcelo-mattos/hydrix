using System;
using System.Data;

namespace Hydrix.Extensions
{
    /// <summary>
    /// Provides extension methods for working with integer values related to standard database types.
    /// </summary>
    /// <remarks>This class contains methods that assist in validating and interpreting integer values as
    /// standard database type identifiers, optimizing for performance in scenarios such as parameter binding.</remarks>
    internal static class IntExtensions
    {
        /// <summary>
        /// Determines whether the specified integer corresponds to a standard <see cref="DbType"/> value.
        /// </summary>
        /// <remarks>This method uses a switch expression to avoid the reflection overhead of
        /// <see cref="Enum.IsDefined(Type, object)"/> in hot paths during parameter binding.</remarks>
        /// <param name="dbType">The integer value to validate.</param>
        /// <returns><see langword="true"/> if the value maps to a known <see cref="DbType"/> member; otherwise,
        /// <see langword="false"/>.</returns>
        public static bool IsStandardDbType(
            this int dbType)
            => dbType switch
            {
                (int)DbType.AnsiString => true,
                (int)DbType.Binary => true,
                (int)DbType.Byte => true,
                (int)DbType.Boolean => true,
                (int)DbType.Currency => true,
                (int)DbType.Date => true,
                (int)DbType.DateTime => true,
                (int)DbType.Decimal => true,
                (int)DbType.Double => true,
                (int)DbType.Guid => true,
                (int)DbType.Int16 => true,
                (int)DbType.Int32 => true,
                (int)DbType.Int64 => true,
                (int)DbType.Object => true,
                (int)DbType.SByte => true,
                (int)DbType.Single => true,
                (int)DbType.String => true,
                (int)DbType.Time => true,
                (int)DbType.UInt16 => true,
                (int)DbType.UInt32 => true,
                (int)DbType.UInt64 => true,
                (int)DbType.VarNumeric => true,
                (int)DbType.AnsiStringFixedLength => true,
                (int)DbType.StringFixedLength => true,
                (int)DbType.Xml => true,
                (int)DbType.DateTime2 => true,
                (int)DbType.DateTimeOffset => true,
                _ => false
            };
    }
}
