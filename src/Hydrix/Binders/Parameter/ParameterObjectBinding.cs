using System;

namespace Hydrix.Binders.Parameter
{
    /// <summary>
    /// Represents an immutable binding between a parameter name and a function used to retrieve its value from a source
    /// object.
    /// </summary>
    /// <remarks>This struct is intended for scenarios where parameter values must be dynamically accessed
    /// using a delegate. It is thread-safe and suitable for use in concurrent environments.</remarks>
    internal readonly struct ParameterObjectBinding
    {
        /// <summary>
        /// Parameter name used to identify the binding. This name is typically used as a key in parameter collections or
        /// dictionaries.
        /// </summary>
        /// <remarks>The name should be unique within the context of its usage to avoid conflicts with other parameter bindings.</remarks>
        public string Name { get; }

        /// <summary>
        /// Gets a function that retrieves the value of a specified property from an object.
        /// </summary>
        /// <remarks>The getter function takes an object as input and returns the corresponding property
        /// value. This is useful for dynamically accessing properties in scenarios such as reflection or data
        /// binding.</remarks>
        public Func<object, object> Getter { get; }

        /// <summary>
        /// Initializes a new instance of the ParameterObjectBinding class with the specified parameter name and value
        /// retrieval function.
        /// </summary>
        /// <remarks>The getter function should be designed to handle the expected type of object it will
        /// be called with. Ensure that the function returns the appropriate value for the parameter, and consider
        /// handling invalid input or type mismatches as needed.</remarks>
        /// <param name="name">The name used to identify the parameter binding. Cannot be null.</param>
        /// <param name="getter">A function that retrieves the value of the parameter from a given object. The function must accept an object
        /// and return the corresponding value. Cannot be null.</param>
        public ParameterObjectBinding(
            string name,
            Func<object, object> getter)
        {
            Name = name;
            Getter = getter;
        }
    }
}
