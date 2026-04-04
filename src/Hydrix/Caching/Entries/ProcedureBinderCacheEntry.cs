using Hydrix.Binders.Procedure;
using System;

namespace Hydrix.Caching.Entries
{
    /// <summary>
    /// Represents an immutable hot-cache entry for a procedure binder.
    /// </summary>
    /// <remarks>This type stores the procedure CLR type together with its resolved binder so the
    /// process-wide hot cache can update and read both values atomically through a single reference.</remarks>
    internal sealed class ProcedureBinderCacheEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureBinderCacheEntry"/> class.
        /// </summary>
        /// <param name="procedureType">The CLR type associated with the cached binder.</param>
        /// <param name="binder">The cached binder for the procedure type.</param>
        public ProcedureBinderCacheEntry(
            Type procedureType,
            ProcedureBinder binder)
        {
            ProcedureType = procedureType;
            Binder = binder;
        }

        /// <summary>
        /// Gets the CLR type associated with the cached binder.
        /// </summary>
        public Type ProcedureType { get; }

        /// <summary>
        /// Gets the cached procedure binder.
        /// </summary>
        public ProcedureBinder Binder { get; }
    }
}
