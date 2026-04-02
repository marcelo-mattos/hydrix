using System;
using System.Data;
using System.Data.Common;

namespace Hydrix.Wrappers
{
    /// <summary>
    /// Wraps a reader so the originating command remains alive until the reader is closed or disposed.
    /// </summary>
    internal static class CommandOwningReader
    {
        /// <summary>
        /// Attaches command lifetime to the provided reader.
        /// </summary>
        /// <param name="command">The command that created the reader.</param>
        /// <param name="reader">The reader returned by the provider.</param>
        /// <returns>A reader wrapper that disposes both reader and command together.</returns>
        public static IDataReader Wrap(
            IDbCommand command,
            IDataReader reader)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(reader);
#else
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
#endif
            if (reader is DbDataReader dbDataReader)
                return new CommandOwningDbDataReader(command, dbDataReader);

            return new CommandOwningDataReader(command, reader);
        }
    }
}
