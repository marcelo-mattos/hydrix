using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Hydrix.Orchestrator.Materializers
{
    /// <summary>
    /// SQL Data Handler Class
    /// </summary>
    public partial class SqlMaterializer :
        Contract.ISqlMaterializer
    {
        /// <summary>
        /// Determines whether the specified parameter value represents a collection that should be
        /// expanded into multiple SQL parameters (e.g. for <c>IN</c> clauses).
        ///
        /// This method is used during parameter binding to distinguish between:
        /// <list type="bullet">
        /// <item>Scalar values, which are bound as a single SQL parameter.</item>
        /// <item>
        /// Enumerable values (such as arrays or lists), which must be expanded into multiple SQL
        /// parameters (e.g. <c>@ids_0, @ids_1, @ids_2</c>) to support SQL <c>IN</c> expressions.
        /// </item>
        /// </list>
        /// The following types are explicitly excluded from enumeration handling:
        /// <list type="bullet">
        /// <item>
        /// <see cref="string"/>, which implements <see cref="System.Collections.IEnumerable"/> but
        /// must always be treated as a scalar value.
        /// </item>
        /// <item>
        /// <see cref="T:System.Byte[]"/>, which represents binary data and must also be treated as
        /// a scalar parameter.
        /// </item>
        /// </list>
        /// Any other value implementing <see cref="System.Collections.IEnumerable"/> is considered
        /// eligible for parameter expansion.
        /// </summary>
        /// <param name="value">The parameter value to evaluate.</param>
        /// <returns>
        /// <c>true</c> if the value represents a collection that should be expanded into multiple
        /// SQL parameters; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsEnumerableParameter(object value)
        {
            if (value == null)
                return false;

            if (value is string || value is byte[])
                return false;

            return value is System.Collections.IEnumerable;
        }

        /// <summary>
        /// Creates and adds a scalar SQL parameter to the specified command.
        ///
        /// The parameter name is automatically prefixed with '@' and the provided value is assigned
        /// directly, falling back to <see cref="DBNull.Value"/> when the value is <c>null</c>.
        ///
        /// This method is intended for simple (non-collection) values and is used internally by the
        /// parameter binding infrastructure, including dynamic object-based parameter mapping and
        /// SQL <c>IN</c> expansion support.
        /// </summary>
        /// <param name="command">
        /// The <see cref="IDbCommand"/> instance to which the parameter will be added.
        /// </param>
        /// <param name="name">The logical parameter name without the '@' prefix.</param>
        /// <param name="value">
        /// The value to assign to the parameter, or <c>null</c> to represent a database NULL.
        /// </param>
        private void AddScalarParameter(
            IDbCommand command,
            string name,
            object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"{_parameterPrefix}{name}";
            parameter.Value = value ?? DBNull.Value;

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Expands a collection parameter into multiple scalar SQL parameters to support <c>IN</c>
        /// clauses in SQL statements.
        ///
        /// This method replaces a single placeholder parameter (e.g. <c>@ids</c>) in the command
        /// text with a comma-separated list of generated parameters (e.g. <c>@ids_0, @ids_1,
        /// @ids_2</c>) and adds each corresponding value to the command's parameter collection.
        ///
        /// Each element in the provided collection results in a distinct SQL parameter, ensuring
        /// full compatibility with ADO.NET providers while preventing SQL injection risks.
        ///
        /// This approach mirrors the behavior popularized by lightweight ORMs such as Dapper, while
        /// remaining fully provider-agnostic and compatible with .NET Core 3.1.
        /// </summary>
        /// <param name="command">
        /// The <see cref="IDbCommand"/> instance whose command text and parameters will be modified
        /// to accommodate the expanded collection.
        /// </param>
        /// <param name="name">
        /// The logical name of the collection parameter without the '@' prefix. This name must
        /// match the placeholder used in the SQL statement (e.g. <c>IN (@ids)</c>).
        /// </param>
        /// <param name="values">
        /// The collection of values to be expanded into individual SQL parameters. Each element
        /// represents a single value in the resulting <c>IN</c> clause.
        /// </param>
        /// <remarks>
        /// <para>
        /// The method assumes that the SQL statement contains the parameter placeholder prefixed
        /// with '@'. The replacement is performed using a simple string substitution and therefore
        /// requires correct usage of parentheses in the SQL (e.g. <c>IN (@ids)</c>).
        /// </para>
        /// <para>
        /// Strings and binary values are intentionally excluded from this expansion logic elsewhere
        /// to avoid accidental character-based parameterization.
        /// </para>
        /// <para>
        /// Providing an empty collection may result in invalid SQL and should be validated by the
        /// caller or guarded at a higher level.
        /// </para>
        /// </remarks>
        private void ExpandEnumerableParameter(
            IDbCommand command,
            string name,
            System.Collections.IEnumerable values)
        {
            var parameterNames = new List<string>();
            int index = 0;

            foreach (var item in values)
            {
                string parameterName = $"{_parameterPrefix}{name}_{index++}";
                parameterNames.Add(parameterName);

                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = item ?? DBNull.Value;

                command.Parameters.Add(parameter);
            }

            command.CommandText = command.CommandText.Replace(
                $"{_parameterPrefix}{name}",
                string.Join(", ", parameterNames)
            );
        }

        /// <summary>
        /// Adds a parameter to the specified command, automatically determining whether the
        /// provided value represents a scalar or a collection.
        ///
        /// When the value is a collection (excluding strings and binary data), the parameter is
        /// expanded into multiple scalar parameters to support SQL <c>IN</c> clauses. Otherwise,
        /// the value is treated as a simple scalar parameter and added directly to the command.
        ///
        /// This method acts as the central decision point for parameter binding, ensuring
        /// consistent behavior across all command creation and execution paths within the ORM.
        /// </summary>
        /// <param name="command">
        /// The <see cref="IDbCommand"/> instance to which the parameter(s) will be added.
        /// </param>
        /// <param name="name">
        /// The logical parameter name without the '@' prefix. This name must match the placeholder
        /// used in the SQL statement.
        /// </param>
        /// <param name="value">
        /// The value to bind to the parameter. If the value is an enumerable, it will be expanded
        /// into multiple SQL parameters; otherwise, it will be bound as a single scalar value.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method relies on <see cref="IsEnumerableParameter(object)"/> to determine whether
        /// the value should be expanded. Strings and byte arrays are intentionally excluded from
        /// enumeration to avoid unintended behavior.
        /// </para>
        /// <para>
        /// The actual parameter creation is delegated to <see
        /// cref="ExpandEnumerableParameter(IDbCommand, string, System.Collections.IEnumerable)"/>
        /// and <see cref="AddScalarParameter(IDbCommand, string, object)"/>, preserving separation
        /// of concerns and maintainability.
        /// </para>
        /// </remarks>
        private void AddParameter(
            IDbCommand command,
            string name,
            object value)
        {
            if (IsEnumerableParameter(value))
            {
                ExpandEnumerableParameter(
                    command,
                    name,
                    (System.Collections.IEnumerable)value);

                return;
            }

            AddScalarParameter(
                command,
                name,
                value);
        }

        /// <summary>
        /// Binds parameters to the specified command using a plain object as the parameter source.
        ///
        /// Each public instance property of the provided object is interpreted as a SQL parameter,
        /// where the property name corresponds to the parameter name and the property value
        /// corresponds to the parameter value.
        ///
        /// This method enables a lightweight, Dapper-style syntax for parameter binding, allowing
        /// callers to pass anonymous objects or simple DTOs instead of manually constructing <see
        /// cref="IDataParameter"/> instances.
        ///
        /// Collection-valued properties are automatically expanded to support SQL <c>IN</c>
        /// clauses, while scalar values are bound as single parameters.
        /// </summary>
        /// <param name="command">
        /// The <see cref="IDbCommand"/> instance to which the parameters will be bound.
        /// </param>
        /// <param name="parameters">
        /// An object whose public instance properties represent SQL parameters. If <c>null</c>, no
        /// parameters are bound.
        /// </param>
        /// <remarks>
        /// <para>
        /// Property names are used verbatim (with an '@' prefix added internally) to match
        /// placeholders in the SQL statement.
        /// </para>
        /// <para>
        /// This method delegates the actual parameter creation logic to <see
        /// cref="AddParameter(IDbCommand, string, object)"/>, ensuring consistent handling of
        /// scalar values and collection expansion across the ORM.
        /// </para>
        /// <para>
        /// Reflection metadata is retrieved only once per invocation and does not depend on
        /// provider-specific features, maintaining full ADO.NET compatibility.
        /// </para>
        /// </remarks>
        private void BindParametersFromObject(
            IDbCommand command,
            object parameters)
        {
            if (parameters == null)
                return;

            var properties = parameters
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                var value = property.GetValue(parameters);
                AddParameter(
                    command,
                    property.Name,
                    value);
            }
        }

        /// <summary>
        /// Formats a parameter value into a SQL-friendly string representation suitable for logging
        /// and diagnostic purposes.
        ///
        /// This method is intended exclusively for **debugging and logging** scenarios (e.g. SQL
        /// command tracing), and must **never** be used to build executable SQL statements, as it
        /// does not provide protection against SQL injection.
        ///
        /// The formatting rules follow common SQL conventions:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <c>null</c> or <see cref="DBNull.Value"/> values are represented as the literal <c>NULL</c>.
        /// </description>
        /// </item>
        /// <item>
        /// <description><see cref="string"/> values are enclosed in single quotes.</description>
        /// </item>
        /// <item>
        /// <description>
        /// <see cref="DateTime"/> values are formatted using <c>yyyy-MM-dd HH:mm:ss.fff</c> to
        /// preserve precision and readability.
        /// </description>
        /// </item>
        /// <item>
        /// <description><see cref="Guid"/> values are enclosed in single quotes.</description>
        /// </item>
        /// <item>
        /// <description>
        /// <see cref="bool"/> values are converted to <c>1</c> or <c>0</c>, following common SQL conventions.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// All other types fall back to their <see cref="object.ToString"/> representation.
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="value">The parameter value to be formatted for SQL logging.</param>
        /// <returns>
        /// A string representing the formatted value, suitable for inclusion in SQL command logs.
        /// </returns>
        private static string FormatParameterValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";

            return value switch
            {
                string s => $"'{s}'",
                DateTime d => $"'{d:yyyy-MM-dd HH:mm:ss.fff}'",
                Guid g => $"'{g}'",
                bool b => b ? "1" : "0",
                _ => value.ToString()
            };
        }
    }
}