using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Plans;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

#if NET6_0_OR_GREATER

using System.Runtime.InteropServices;

#endif

namespace Hydrix.Mapper
{
    /// <summary>
    /// Provides the default mapper implementation that uses cached, precompiled mapping plans for high-throughput object
    /// projection.
    /// </summary>
    /// <remarks>
    /// The first mapping operation for a source and destination pair incurs the cold-path cost of plan creation. After
    /// that, the hot path consists of a per-instance cache lookup keyed by the source and destination type pair followed
    /// by a single compiled delegate invocation.
    /// </remarks>
    public sealed class HydrixMapper :
        IHydrixMapper
    {
        /// <summary>
        /// Stores the option snapshot used when new mapping plans must be compiled.
        /// </summary>
        private readonly HydrixMapperOptions _options;

        /// <summary>
        /// Stores the pre-computed structural cache key for <see cref="_options"/>.
        /// </summary>
        private readonly MapPlanOptionsKey _optionsKey;

        /// <summary>
        /// Stores the per-instance plan cache keyed only by the source and destination type pair.
        /// </summary>
        /// <remarks>
        /// Because options are fixed for the lifetime of this instance, the per-instance cache avoids rebuilding the
        /// option snapshot key on every hot-path call. A miss falls through to the shared <see cref="MapPlanCache"/>
        /// where the full key is used exactly once.
        /// </remarks>
        private readonly ConcurrentDictionary<TypePair, MapPlan> _planCache =
            new ConcurrentDictionary<TypePair, MapPlan>();

        /// <summary>
        /// Initializes a new <see cref="HydrixMapper"/> instance.
        /// </summary>
        /// <param name="options">
        /// The option snapshot that controls how future mapping plans are compiled.
        /// </param>
        public HydrixMapper(
            HydrixMapperOptions options)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(options);
            _options = options.Clone();
#else
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            _options = options.Clone();
#endif
            _optionsKey = MapPlanOptionsKey.Create(
                _options);
        }

        /// <summary>
        /// Maps a single source object to a new destination instance.
        /// </summary>
        /// <typeparam name="TTarget">
        /// The destination type that should be created and populated.
        /// </typeparam>
        /// <param name="source">
        /// The source object that supplies the values to map.
        /// </param>
        /// <returns>
        /// A populated destination instance of type <typeparamref name="TTarget"/>.
        /// </returns>
        public TTarget Map<TTarget>(
            object source)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                source);
#else
            if (source == null)
                throw new ArgumentNullException(
                    nameof(source));
#endif
            return (TTarget)GetPlan(
                    source.GetType(),
                    typeof(TTarget))
                .Execute(
                    source);
        }

        /// <summary>
        /// Maps a single source object whose type is known at compile time to a new destination instance.
        /// </summary>
        /// <typeparam name="TSource">
        /// The compile-time source type used to resolve the cached mapping plan.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The destination type that should be created and populated.
        /// </typeparam>
        /// <param name="source">
        /// The source object that supplies the values to map.
        /// </param>
        /// <returns>
        /// A populated destination instance of type <typeparamref name="TTarget"/>.
        /// </returns>
        public TTarget Map<TSource, TTarget>(
            TSource source)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(
                source);
#else
            if (source is null)
                throw new ArgumentNullException(
                    nameof(source));
#endif

            return ((Func<TSource, TTarget>)GetPlan(
                    typeof(TSource),
                    typeof(TTarget))
                .TypedExecute)(
                    source);
        }

        /// <summary>
        /// Maps each non-null source object from the supplied sequence to a new destination instance.
        /// </summary>
        /// <typeparam name="TTarget">
        /// The destination type that should be created for each non-null source element.
        /// </typeparam>
        /// <param name="sources">
        /// The source sequence to map. A <see langword="null"/> value yields an empty result.
        /// </param>
        /// <returns>
        /// A read-only list containing the mapped destination instances in the same iteration order as the input sequence.
        /// </returns>
        public IReadOnlyList<TTarget> MapList<TTarget>(
            IEnumerable<object> sources)
        {
            if (sources == null)
                return Array.Empty<TTarget>();

            var result = CreateResultBuffer<TTarget>(
                sources);
            var destinationType = typeof(TTarget);
            Type lastSourceType = null;
            MapPlan lastPlan = null;

            foreach (var source in sources)
            {
                if (source == null)
                    continue;

                var sourceType = source.GetType();
                if (lastPlan == null || sourceType != lastSourceType)
                {
                    lastPlan = GetPlan(
                        sourceType,
                        destinationType);
                    lastSourceType = sourceType;
                }

                result.Add(
                    (TTarget)lastPlan.Execute(
                        source));
            }

            return result;
        }

        /// <summary>
        /// Maps each non-null source object from the supplied homogeneous sequence to a new destination instance by using a
        /// single cached plan for the compile-time source type.
        /// </summary>
        /// <typeparam name="TSource">
        /// The compile-time source type used to resolve the cached mapping plan.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The destination type that should be created for each non-null source element.
        /// </typeparam>
        /// <param name="sources">
        /// The source sequence to map. A <see langword="null"/> value yields an empty result.
        /// </param>
        /// <returns>
        /// A read-only list containing the mapped destination instances in the same iteration order as the input sequence.
        /// </returns>
        public IReadOnlyList<TTarget> MapList<TSource, TTarget>(
            IEnumerable<TSource> sources)
        {
            if (sources == null)
                return Array.Empty<TTarget>();

            var typedExecute = (Func<TSource, TTarget>)GetPlan(
                    typeof(TSource),
                    typeof(TTarget))
                .TypedExecute;

#if NET6_0_OR_GREATER
            if (sources is List<TSource> list)
            {
                var span = CollectionsMarshal.AsSpan(
                    list);

                if (span.IsEmpty)
                    return Array.Empty<TTarget>();

                return MapSpan(
                    span,
                    typedExecute);
            }
#endif

            if (sources is IList<TSource> ilist)
            {
                if (ilist.Count == 0)
                    return Array.Empty<TTarget>();

                return MapIList(
                    ilist,
                    typedExecute);
            }

            return MapEnumerable(
                sources,
                typedExecute);
        }

        /// <summary>
        /// Returns the cached mapping plan for the supplied source and destination type pair, creating and caching it on
        /// first use.
        /// </summary>
        /// <remarks>
        /// The per-instance cache uses only the type pair as the key. On a miss the full option-aware
        /// <see cref="MapPlanCache"/> is consulted, which compiles the plan exactly once per unique combination of type
        /// pair and configuration.
        /// </remarks>
        /// <param name="sourceType">
        /// The source type used as the left side of the mapping-plan key.
        /// </param>
        /// <param name="destType">
        /// The destination type used as the right side of the mapping-plan key.
        /// </param>
        /// <returns>
        /// The cached or newly compiled <see cref="MapPlan"/> for the requested type pair.
        /// </returns>
        private MapPlan GetPlan(
            Type sourceType,
            Type destType)
        {
            var key = new TypePair(
                sourceType,
                destType);

            return _planCache.TryGetValue(
                key,
                out var plan)
                ? plan
                : _planCache.GetOrAdd(
                    key,
                    _ => MapPlanCache.GetOrAdd(
                        sourceType,
                        destType,
                        _options,
                        _optionsKey));
        }

        /// <summary>
        /// Creates the result buffer used by <see cref="MapList{TTarget}(IEnumerable{object})"/>, pre-sizing it when the
        /// source sequence exposes a count.
        /// </summary>
        /// <typeparam name="TTarget">
        /// The destination type stored in the result list.
        /// </typeparam>
        /// <param name="sources">
        /// The source sequence whose count may be used to size the result buffer.
        /// </param>
        /// <returns>
        /// A mutable result list sized for the known source count when available.
        /// </returns>
        private static List<TTarget> CreateResultBuffer<TTarget>(
            IEnumerable<object> sources)
        {
            if (sources is ICollection collection)
            {
                return new List<TTarget>(
                    collection.Count);
            }

            if (sources is IReadOnlyCollection<object> readOnlyCollection)
            {
                return new List<TTarget>(
                    readOnlyCollection.Count);
            }

            return new List<TTarget>();
        }

        /// <summary>
        /// Creates the result buffer used by <see cref="MapList{TSource, TTarget}(IEnumerable{TSource})"/>, pre-sizing it
        /// when the homogeneous source sequence exposes a count.
        /// </summary>
        /// <typeparam name="TSource">
        /// The source type stored in the input sequence.
        /// </typeparam>
        /// <typeparam name="TTarget">
        /// The destination type stored in the result list.
        /// </typeparam>
        /// <param name="sources">
        /// The source sequence whose count may be used to size the result buffer.
        /// </param>
        /// <returns>
        /// A mutable result list sized for the known source count when available.
        /// </returns>
        private static List<TTarget> CreateResultBuffer<TSource, TTarget>(
            IEnumerable<TSource> sources)
        {
            if (sources is ICollection<TSource> collection)
                return new List<TTarget>(
                    collection.Count);

            if (sources is IReadOnlyCollection<TSource> readOnlyCollection)
                return new List<TTarget>(
                    readOnlyCollection.Count);

            if (sources is ICollection nonGenericCollection)
                return new List<TTarget>(
                    nonGenericCollection.Count);

            return new List<TTarget>();
        }

#if NET6_0_OR_GREATER

        /// <summary>
        /// Maps each non-null element in the supplied span to a destination instance using the compiled delegate.
        /// </summary>
        private static List<TTarget> MapSpan<TSource, TTarget>(
            Span<TSource> span,
            Func<TSource, TTarget> execute)
        {
            var result = new List<TTarget>(
                span.Length);

            for (var index = 0; index < span.Length; index++)
            {
                if (span[index] is null)
                    continue;

                result.Add(
                    execute(
                        span[index]));
            }

            return result;
        }

#endif

        /// <summary>
        /// Maps each non-null element in the supplied index-addressable list to a destination instance using the compiled
        /// delegate.
        /// </summary>
        private static List<TTarget> MapIList<TSource, TTarget>(
            IList<TSource> ilist,
            Func<TSource, TTarget> execute)
        {
            var count = ilist.Count;
            var result = new List<TTarget>(
                count);

            for (var index = 0; index < count; index++)
            {
                var item = ilist[index];
                if (item is null)
                    continue;

                result.Add(
                    execute(
                        item));
            }

            return result;
        }

        /// <summary>
        /// Maps each non-null element from an arbitrary enumerable sequence to a destination instance using the compiled
        /// delegate.
        /// </summary>
        private static List<TTarget> MapEnumerable<TSource, TTarget>(
            IEnumerable<TSource> sources,
            Func<TSource, TTarget> execute)
        {
            var result = CreateResultBuffer<TSource, TTarget>(
                sources);

            foreach (var source in sources)
            {
                if (source is null)
                    continue;

                result.Add(
                    execute(
                        source));
            }

            return result;
        }

        /// <summary>
        /// Represents the lightweight value-type key used by the per-instance plan cache, combining a source type and a
        /// destination type.
        /// </summary>
        private readonly struct TypePair :
            IEquatable<TypePair>
        {
            /// <summary>
            /// Stores the source type portion of the key.
            /// </summary>
            private readonly Type _source;

            /// <summary>
            /// Stores the destination type portion of the key.
            /// </summary>
            private readonly Type _target;

            /// <summary>
            /// Initializes a new <see cref="TypePair"/> key.
            /// </summary>
            /// <param name="source">
            /// The source type component of the key.
            /// </param>
            /// <param name="target">
            /// The destination type component of the key.
            /// </param>
            internal TypePair(
                Type source,
                Type target)
            {
                _source = source;
                _target = target;
            }

            /// <summary>
            /// Determines whether the current key matches another <see cref="TypePair"/>.
            /// </summary>
            /// <param name="other">
            /// The other key to compare with the current instance.
            /// </param>
            /// <returns>
            /// <see langword="true"/> when both keys contain the same source and destination types; otherwise,
            /// <see langword="false"/>.
            /// </returns>
            public bool Equals(
                TypePair other) =>
                _source == other._source &&
                _target == other._target;

            /// <summary>
            /// Determines whether the current key matches another object instance.
            /// </summary>
            /// <param name="obj">
            /// The object to compare with the current key.
            /// </param>
            /// <returns>
            /// <see langword="true"/> when <paramref name="obj"/> is a <see cref="TypePair"/> with the same source and
            /// destination types; otherwise, <see langword="false"/>.
            /// </returns>
            public override bool Equals(
                object obj) =>
                obj is TypePair pair && Equals(
                    pair);

            /// <summary>
            /// Returns the hash code used by the concurrent dictionary for this key.
            /// </summary>
            /// <returns>
            /// A stable hash code computed from the runtime handles of the source and destination types.
            /// </returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    return (_source.GetHashCode() * 397) ^ _target.GetHashCode();
                }
            }
        }
    }
}
