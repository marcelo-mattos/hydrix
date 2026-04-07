using System;

namespace Hydrix.Mapper.Attributes
{
    /// <summary>
    /// Marks a destination DTO class with its corresponding source entity type, enabling automatic nested mapping
    /// without explicit registration in <see cref="Configuration.HydrixMapperOptions"/>.
    /// </summary>
    /// <remarks>
    /// When the plan builder encounters a property type mismatch between a source and destination, it checks whether
    /// the destination property type carries this attribute. If the attribute's <see cref="SourceType"/> matches the
    /// source property type, the nested mapping plan is resolved and compiled automatically.
    ///
    /// This attribute is read exclusively at plan-compile time and has zero runtime cost.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MapFromAttribute :
        Attribute
    {
        /// <summary>Gets the source entity type that this DTO is mapped from.</summary>
        public Type SourceType { get; }

        /// <summary>
        /// Initializes a new <see cref="MapFromAttribute"/> with the specified source entity type.
        /// </summary>
        /// <param name="sourceType">The source entity type that maps to this DTO class.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceType"/> is null.</exception>
        public MapFromAttribute(Type sourceType)
        {
            SourceType = sourceType ??
                throw new ArgumentNullException(nameof(sourceType));
        }
    }
}
