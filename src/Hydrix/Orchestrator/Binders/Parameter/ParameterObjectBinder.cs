using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Hydrix.Orchestrator.Binders.Parameter
{
    /// <summary>
    /// Binds parameters from a specified object to a database command using defined bindings.
    /// </summary>
    /// <remarks>This class is designed to facilitate the binding of parameters to an IDbCommand by iterating
    /// over an array of ParameterObjectBinding instances. Each binding defines how to extract a value from the
    /// parameters object. The method allows for customization of parameter names through a prefix and supports various
    /// rules for handling null values and collections.</remarks>
    internal sealed class ParameterObjectBinder
    {
        /// <summary>
        /// Represents the possible states of the SQL parser during scanning operations.
        /// </summary>
        /// <remarks>This enumeration is used to track the current context while parsing SQL statements,
        /// such as whether the parser is in normal code, inside single or double quotes, or within a comment. Correctly
        /// identifying the scan state is essential for accurate tokenization and parsing of SQL scripts.</remarks>
        private enum SqlScanState
        {
            /// <summary>
            /// Represents the normal state of an operation or process.
            /// </summary>
            Normal,

            /// <summary>
            /// Gets the single quote character used in string literals.
            /// </summary>
            SingleQuote,

            /// <summary>
            /// Gets the string representation of a double quote character.
            /// </summary>
            DoubleQuote,

            /// <summary>
            /// Represents a single-line comment in the source code, typically used to annotate or explain code for
            /// developers.
            /// </summary>
            /// <remarks>Line comments are ignored by the compiler and do not affect program
            /// execution. They are commonly used to provide context, clarify intent, or temporarily disable code during
            /// development.</remarks>
            LineComment,

            /// <summary>
            /// Represents a block comment in the code, typically used to provide detailed explanations or annotations.
            /// </summary>
            /// <remarks>Block comments can span multiple lines and are useful for documenting complex
            /// logic or providing context for sections of code. They are often used to enhance code readability and
            /// maintainability.</remarks>
            BlockComment
        }

        /// <summary>
        /// Gets the array of parameter object bindings used for processing input parameters.
        /// </summary>
        /// <remarks>This field is initialized with the parameter bindings necessary for the operation of
        /// the associated method or class. It is intended for internal use and should not be modified
        /// directly.</remarks>
        private readonly ParameterObjectBinding[] _bindings;

        /// <summary>
        /// Initializes a new instance of the ParameterObjectBinder class using the specified parameter bindings.
        /// </summary>
        /// <param name="bindings">An array of ParameterObjectBinding instances that define the parameter bindings to be used by the binder.
        /// Cannot be null.</param>
        public ParameterObjectBinder(
            ParameterObjectBinding[] bindings)
            => _bindings = bindings;

        /// <summary>
        /// Binds the specified parameters to the provided database command using the given prefix and parameter
        /// addition logic.
        /// </summary>
        /// <remarks>This method iterates over the available parameter bindings and applies the provided
        /// logic for adding each parameter to the command. It is the caller's responsibility to ensure that the
        /// parameters object contains the expected properties and that the addParameter action correctly handles null
        /// values and collections as needed.</remarks>
        /// <param name="command">The database command to which the parameters will be bound. Must not be null.</param>
        /// <param name="parameters">An object containing the parameter values to bind. The properties of this object are used to retrieve
        /// parameter values.</param>
        /// <param name="prefix">A string prefix to prepend to each parameter name when binding to the command. Can be empty if no prefix is
        /// required.</param>
        /// <param name="addParameter">An action that defines how to add each parameter to the command. Receives the command, the full parameter
        /// name (including prefix), and the parameter value.</param>
        public void Bind(
            IDbCommand command,
            object parameters,
            string prefix,
            Action<IDbCommand, string, object> addParameter)
        {
            for (int index = 0; index < _bindings.Length; index++)
            {
                var binder = _bindings[index];
                var value = binder.Getter(parameters);

                if (IsEnumerableParameter(value))
                {
                    ExpandEnumerableParameter(
                        command,
                        binder.Name,
                        prefix,
                        (IEnumerable)value);

                    continue;
                }

                addParameter(command, $"{prefix}{binder.Name}", value);
            }
        }

        /// <summary>
        /// Determines whether the specified object is an enumerable collection, excluding strings and byte arrays.
        /// </summary>
        /// <remarks>This method returns false for strings and byte arrays, as they are not considered
        /// enumerable collections.</remarks>
        /// <param name="value">The object to evaluate for enumerable status. Must not be null.</param>
        /// <returns>true if the object is an enumerable collection other than a string or byte array; otherwise, false.</returns>
        private static bool IsEnumerableParameter(object value)
        {
            if (value == null)
                return false;

            if (value is string || value is byte[])
                return false;

            return value is IEnumerable;
        }

        /// <summary>
        /// Expands an enumerable parameter into individual parameters for a database command, replacing the placeholder
        /// in the command text with the generated parameter names.
        /// </summary>
        /// <remarks>This method generates unique parameter names by combining the specified prefix and
        /// base name with an index. Null values in the collection are assigned as DBNull.Value. The method updates the
        /// command text by replacing the original parameter placeholder with a comma-separated list of the generated
        /// parameter names.</remarks>
        /// <param name="command">The database command to which the parameters will be added.</param>
        /// <param name="name">The base name for the parameters to be generated and inserted into the command text.</param>
        /// <param name="prefix">A prefix to prepend to each generated parameter name, allowing for grouping or disambiguation.</param>
        /// <param name="values">An enumerable collection of values to be added as individual parameters. Each value is assigned to a
        /// separate parameter.</param>
        private static void ExpandEnumerableParameter(
            IDbCommand command,
            string name,
            string prefix,
            IEnumerable values)
        {
            var parameterNames = new List<string>();
            int index = 0;

            foreach (var item in values)
            {
                string parameterName = $"{prefix}{name}_{index++}";
                parameterNames.Add(parameterName);

                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = item ?? DBNull.Value;

                command.Parameters.Add(parameter);
            }

            if (parameterNames.Count == 0)
                throw new InvalidOperationException(
                    $"Enumerable parameter '{name}' is empty. Provide at least one value.");

            var token = $"{prefix}{name}";
            var replacement = string.Join(", ", parameterNames);

            command.CommandText = ReplaceParameterToken(
                command.CommandText,
                token,
                replacement);
        }

        /// <summary>
        /// Replaces all occurrences of a specified token in a SQL string with a given replacement, excluding tokens
        /// found within string literals or comments.
        /// </summary>
        /// <remarks>This method ensures that replacements are only performed outside of string literals
        /// and both line and block comments, preserving the syntactic correctness of the SQL statement.</remarks>
        /// <param name="sql">The SQL string in which to search for and replace the token. Cannot be null or empty.</param>
        /// <param name="token">The token to be replaced in the SQL string. Cannot be null or empty.</param>
        /// <param name="replacement">The string to replace each occurrence of the token with.</param>
        /// <returns>A new SQL string with all valid occurrences of the specified token replaced by the replacement string. If
        /// the input SQL string or token is null or empty, the original SQL string is returned unchanged.</returns>
        private static string ReplaceParameterToken(
            string sql,
            string token,
            string replacement)
        {
            if (string.IsNullOrEmpty(sql) || string.IsNullOrEmpty(token))
                return sql;

            var state = SqlScanState.Normal;
            var length = sql.Length;
            var tokenLength = token.Length;

            var builder = new StringBuilder(
                sql.Length + Math.Max(0, replacement.Length - tokenLength));

            for (var index = 0; index < length; index++)
            {
                var @char = sql[index];

                if (TryHandleCommentOrString(
                    sql,
                    builder,
                    ref state,
                    ref index))
                    continue;

                if (TryReplaceToken(
                    sql,
                    token,
                    replacement,
                    builder,
                    ref index))
                    continue;

                builder.Append(@char);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Attempts to process the current character in the SQL string as part of a comment or string literal, updating
        /// the scan state and output accordingly.
        /// </summary>
        /// <remarks>This method is typically used as part of a SQL parsing routine to correctly identify
        /// and process line comments, block comments, and string literals. It updates the scan state and output builder
        /// as appropriate, ensuring that comments and quoted strings are preserved or handled according to SQL syntax
        /// rules.</remarks>
        /// <param name="sql">The SQL string being scanned for comments and string literals.</param>
        /// <param name="builder">The StringBuilder instance that accumulates the processed SQL output.</param>
        /// <param name="state">The current scan state, indicating whether the parser is inside a comment, string literal, or normal SQL
        /// text. This parameter is updated to reflect state transitions.</param>
        /// <param name="index">The current position in the SQL string. This parameter may be incremented if multi-character tokens are
        /// handled.</param>
        /// <returns>true if the character was handled as part of a comment or string literal; otherwise, false.</returns>
        private static bool TryHandleCommentOrString(
            string sql,
            StringBuilder builder,
            ref SqlScanState state,
            ref int index)
        {
            var length = sql.Length;
            var @char = sql[index];

            switch (state)
            {
                case SqlScanState.LineComment:
                    builder.Append(@char);
                    if (@char == '\n')
                        state = SqlScanState.Normal;
                    return true;

                case SqlScanState.BlockComment:
                    builder.Append(@char);
                    if (@char == '*' && index + 1 < length && sql[index + 1] == '/')
                    {
                        builder.Append('/');
                        index++;
                        state = SqlScanState.Normal;
                    }
                    return true;

                case SqlScanState.SingleQuote:
                    builder.Append(@char);

                    if (@char == '\'' && !(index + 1 < length && sql[index + 1] == '\''))
                        state = SqlScanState.Normal;

                    if (@char == '\'' && index + 1 < length && sql[index + 1] == '\'')
                    {
                        builder.Append('\'');
                        index++;
                    }
                    return true;

                case SqlScanState.DoubleQuote:
                    builder.Append(@char);
                    if (@char == '"')
                        state = SqlScanState.Normal;
                    return true;

                case SqlScanState.Normal:
                    if (@char == '-' && index + 1 < length && sql[index + 1] == '-')
                    {
                        builder.Append("--");
                        index++;
                        state = SqlScanState.LineComment;
                        return true;
                    }

                    if (@char == '/' && index + 1 < length && sql[index + 1] == '*')
                    {
                        builder.Append("/*");
                        index++;
                        state = SqlScanState.BlockComment;
                        return true;
                    }

                    if (@char == '\'')
                    {
                        builder.Append(@char);
                        state = SqlScanState.SingleQuote;
                        return true;
                    }

                    if (@char == '"')
                    {
                        builder.Append(@char);
                        state = SqlScanState.DoubleQuote;
                        return true;
                    }
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Attempts to replace a specified token in the SQL string with a replacement string at the current index,
        /// updating the provided StringBuilder and advancing the index if the replacement is successful.
        /// </summary>
        /// <remarks>The method only replaces the token if it is found at the specified index and is not
        /// part of a larger word, as determined by token boundary checks.</remarks>
        /// <param name="sql">The SQL string in which to search for the token.</param>
        /// <param name="token">The token to search for and replace within the SQL string.</param>
        /// <param name="replacement">The string to use as a replacement for the specified token.</param>
        /// <param name="builder">The StringBuilder instance to which the replacement string is appended if the token is replaced.</param>
        /// <param name="index">A reference to the current index in the SQL string. If the token is replaced, this value is updated to the
        /// position after the replacement.</param>
        /// <returns>true if the token was found at the current index and replaced; otherwise, false.</returns>
        private static bool TryReplaceToken(
            string sql,
            string token,
            string replacement,
            StringBuilder builder,
            ref int index)
        {
            var length = sql.Length;
            var tokenLength = token.Length;

            if (sql[index] != token[0] ||
                index + tokenLength > length)
                return false;

            for (var idx = 1; idx < tokenLength; idx++)
            {
                if (sql[index + idx] != token[idx])
                    return false;
            }

            char previous = index > 0 ? sql[index - 1] : '\0';
            char next = (index + tokenLength) < length ? sql[index + tokenLength] : '\0';

            if (!IsTokenBoundary(previous) || !IsTokenBoundary(next))
                return false;

            builder.Append(replacement);
            index += tokenLength - 1;
            return true;
        }

        /// <summary>
        /// Determines whether the specified character represents a token boundary for parsing purposes.
        /// </summary>
        /// <remarks>This method is useful when identifying token boundaries during lexical analysis or
        /// parsing operations, such as when splitting input into meaningful tokens.</remarks>
        /// <param name="char">The character to evaluate as a potential token boundary.</param>
        /// <returns>true if the character is a null character or is not a letter, digit, or underscore; otherwise, false.</returns>
        private static bool IsTokenBoundary(
            char @char)
        {
            if (@char == '\0')
                return true;

            return !(char.IsLetterOrDigit(@char) ||
                @char == '_');
        }
    }
}