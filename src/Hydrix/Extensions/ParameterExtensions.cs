using System;
using System.Collections.Generic;
using System.Data;

namespace Hydrix.Extensions
{
    /// <summary>
    /// Provides extension methods for converting objects to a collection of IDataParameter instances.
    /// </summary>
    /// <remarks>This class contains methods that facilitate the conversion of various parameter types into a
    /// format compatible with data access operations. It supports both single IDataParameter instances and collections
    /// of IDataParameter.</remarks>
    internal static class ParameterExtensions
    {
        /// <summary>
        /// Converts the specified object into an enumerable collection of IDataParameter instances.
        /// </summary>
        /// <remarks>This method is useful for scenarios where a method requires a collection of
        /// IDataParameter instances, allowing for flexible input types.</remarks>
        /// <param name="parameters">The object to convert, which can be a single IDataParameter or an enumerable collection of IDataParameter
        /// instances.</param>
        /// <returns>An enumerable collection of IDataParameter instances derived from the specified object. If the object is
        /// null, an empty collection is returned.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified object is neither an IDataParameter nor an IEnumerable of IDataParameter.</exception>
        public static IEnumerable<IDataParameter> AsIDataParameters(
            this object parameters)
        {
            if (parameters == null)
                yield break;

            if (parameters is IEnumerable<IDataParameter> dbParams)
            {
                foreach (var dbParam in dbParams)
                    yield return dbParam;

                yield break;
            }

            if (parameters is IDataParameter single)
            {
                yield return single;
                yield break;
            }

            throw new ArgumentException(
                $"Parameter type '{parameters.GetType().FullName}' is not supported.",
                nameof(parameters));
        }
    }
}