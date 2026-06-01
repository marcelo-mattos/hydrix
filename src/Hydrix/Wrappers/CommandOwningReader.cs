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

        /// <summary>
        /// Returns the underlying provider reader for a command-owning wrapper so the materialization loop can read
        /// columns without paying the wrapper's per-call virtual forwarding overhead.
        /// </summary>
        /// <remarks>The returned reader is owned by the wrapper and must not be disposed by the caller; disposing
        /// the wrapper (for example via a <c>using</c> statement) still disposes both the provider reader and the
        /// originating command. When <paramref name="reader"/> is not a Hydrix wrapper it is returned unchanged.</remarks>
        /// <param name="reader">The reader to unwrap. May be a <see cref="CommandOwningDbDataReader"/>, a
        /// <see cref="CommandOwningDataReader"/>, or any other <see cref="IDataReader"/>.</param>
        /// <returns>The inner provider reader when <paramref name="reader"/> is a command-owning wrapper; otherwise
        /// <paramref name="reader"/> itself.</returns>
        public static IDataReader Unwrap(
            IDataReader reader)
        {
            switch (reader)
            {
                case CommandOwningDbDataReader owningDbReader:
                    return owningDbReader.InnerReader;
                case CommandOwningDataReader owningReader:
                    return owningReader.InnerReader;
                default:
                    return reader;
            }
        }
    }
}
