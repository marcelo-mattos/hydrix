using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Hydrix.Mapper;
using Hydrix.Mapper.Caching;
using Hydrix.Mapper.Configuration;
using Hydrix.Mapper.Mapping;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hydrix.Benchmarks.Benchmarks
{
    /// <summary>
    /// Compares Hydrix.Mapper and AutoMapper across flat-object widths, collection sizes, conversion scenarios, and a
    /// cold-path cache reset.
    /// </summary>
    /// <remarks>
    /// The suite configures both mappers once during <see cref="Setup"/>, warms the Hydrix plans ahead of the hot-path
    /// benchmarks, and then measures repeated steady-state mapping against identical source instances.
    /// </remarks>
    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class MapperBenchmarks
    {
        /// <summary>
        /// Stores the AutoMapper instance used by the benchmark methods.
        /// </summary>
        private IMapper _autoMapper = null!;

        /// <summary>
        /// Stores the Hydrix mapper instance used by the benchmark methods.
        /// </summary>
        private HydrixMapper _hydrixMapper = null!;

        /// <summary>
        /// Stores the reusable small flat source instance.
        /// </summary>
        private FlatSmallSrc _smallSrc = null!;

        /// <summary>
        /// Stores the reusable medium flat source instance.
        /// </summary>
        private FlatMediumSrc _mediumSrc = null!;

        /// <summary>
        /// Stores the reusable wide flat source instance.
        /// </summary>
        private FlatLargeSrc _largeSrc = null!;

        /// <summary>
        /// Stores the reusable conversion source instance.
        /// </summary>
        private ConversionSrc _conversionSrc = null!;

        /// <summary>
        /// Stores the reusable small source list with one hundred elements.
        /// </summary>
        private List<FlatSmallSrc> _smallList100 = null!;

        /// <summary>
        /// Stores the reusable small source list with one thousand elements.
        /// </summary>
        private List<FlatSmallSrc> _smallList1000 = null!;

        /// <summary>
        /// Stores the reusable medium source list with one hundred elements.
        /// </summary>
        private List<FlatMediumSrc> _mediumList100 = null!;

        /// <summary>
        /// Stores the reusable medium source list with one thousand elements.
        /// </summary>
        private List<FlatMediumSrc> _mediumList1000 = null!;

        /// <summary>
        /// Stores the reusable wide source list with one hundred elements.
        /// </summary>
        private List<FlatLargeSrc> _largeList100 = null!;

        /// <summary>
        /// Stores the reusable wide source list with one thousand elements.
        /// </summary>
        private List<FlatLargeSrc> _largeList1000 = null!;

        /// <summary>
        /// Configures both mappers, builds reusable source instances, and warms the Hydrix mapping plans.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            var autoMapperConfiguration = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<FlatSmallSrc, FlatSmallDto>();
                    cfg.CreateMap<FlatMediumSrc, FlatMediumDto>();
                    cfg.CreateMap<FlatLargeSrc, FlatLargeDto>();
                    cfg.CreateMap<ConversionSrc, ConversionDto>()
                        .ForMember(
                            destination => destination.Name,
                            options => options.MapFrom(
                                source => source.Name == null
                                    ? null
                                    : source.Name.Trim()))
                        .ForMember(
                            destination => destination.ExternalId,
                            options => options.MapFrom(
                                source => source.ExternalId.ToString("D")))
                        .ForMember(
                            destination => destination.CreatedAt,
                            options => options.MapFrom(
                                source => source.CreatedAt.ToString(
                                    "O",
                                    CultureInfo.InvariantCulture)))
                        .ForMember(
                            destination => destination.Score,
                            options => options.MapFrom(
                                source => (int)decimal.Truncate(
                                    source.Score)));
                });
            _autoMapper = autoMapperConfiguration.CreateMapper();

            var hydrixOptions = new HydrixMapperOptions();
            hydrixOptions.String.Transform = StringTransform.Trim;
            hydrixOptions.Guid.Format = GuidFormat.D;
            hydrixOptions.Guid.Case = GuidCase.Lower;
            hydrixOptions.DateTime.StringFormat = "O";
            hydrixOptions.DateTime.TimeZone = DateTimeZone.None;
            hydrixOptions.Numeric.DecimalToIntRounding = NumericRounding.Truncate;
            _hydrixMapper = new HydrixMapper(
                hydrixOptions);

            _smallSrc = BuildSmall();
            _mediumSrc = BuildMedium();
            _largeSrc = BuildLarge();
            _conversionSrc = new ConversionSrc
            {
                Name = "  Alice  ",
                ExternalId = Guid.NewGuid(),
                CreatedAt = new DateTime(
                    2024,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc),
                Score = 95.7m,
            };
            _smallList100 = BuildList(
                _smallSrc,
                100);
            _smallList1000 = BuildList(
                _smallSrc,
                1000);
            _mediumList100 = BuildList(
                _mediumSrc,
                100);
            _mediumList1000 = BuildList(
                _mediumSrc,
                1000);
            _largeList100 = BuildList(
                _largeSrc,
                100);
            _largeList1000 = BuildList(
                _largeSrc,
                1000);

            _hydrixMapper.Map<FlatSmallDto>(
                _smallSrc);
            _hydrixMapper.Map<FlatMediumDto>(
                _mediumSrc);
            _hydrixMapper.Map<FlatLargeDto>(
                _largeSrc);
            _hydrixMapper.Map<ConversionDto>(
                _conversionSrc);
        }

        /// <summary>
        /// Maps the small flat source object through AutoMapper.
        /// </summary>
        /// <returns>
        /// The destination object produced by AutoMapper for the reusable small flat source instance.
        /// </returns>
        [Benchmark(Description = "AutoMapper - flat small")]
        public FlatSmallDto AutoMapper_FlatSmall() =>
            _autoMapper.Map<FlatSmallDto>(
                _smallSrc);

        /// <summary>
        /// Maps the small flat source object through Hydrix.
        /// </summary>
        /// <returns>
        /// The destination object produced by Hydrix for the reusable small flat source instance.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - flat small")]
        public FlatSmallDto HydrixMapper_FlatSmall() =>
            _hydrixMapper.Map<FlatSmallDto>(
                _smallSrc);

        /// <summary>
        /// Maps the medium flat source object through AutoMapper.
        /// </summary>
        /// <returns>
        /// The destination object produced by AutoMapper for the reusable medium flat source instance.
        /// </returns>
        [Benchmark(Description = "AutoMapper - flat medium")]
        public FlatMediumDto AutoMapper_FlatMedium() =>
            _autoMapper.Map<FlatMediumDto>(
                _mediumSrc);

        /// <summary>
        /// Maps the medium flat source object through Hydrix.
        /// </summary>
        /// <returns>
        /// The destination object produced by Hydrix for the reusable medium flat source instance.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - flat medium")]
        public FlatMediumDto HydrixMapper_FlatMedium() =>
            _hydrixMapper.Map<FlatMediumDto>(
                _mediumSrc);

        /// <summary>
        /// Maps the wide flat source object through AutoMapper.
        /// </summary>
        /// <returns>
        /// The destination object produced by AutoMapper for the reusable wide flat source instance.
        /// </returns>
        [Benchmark(Description = "AutoMapper - flat large")]
        public FlatLargeDto AutoMapper_FlatLarge() =>
            _autoMapper.Map<FlatLargeDto>(
                _largeSrc);

        /// <summary>
        /// Maps the wide flat source object through Hydrix.
        /// </summary>
        /// <returns>
        /// The destination object produced by Hydrix for the reusable wide flat source instance.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - flat large")]
        public FlatLargeDto HydrixMapper_FlatLarge() =>
            _hydrixMapper.Map<FlatLargeDto>(
                _largeSrc);

        /// <summary>
        /// Maps the conversion-heavy source object through AutoMapper.
        /// </summary>
        /// <returns>
        /// The destination object produced by AutoMapper after applying trimming, formatting, and numeric conversion.
        /// </returns>
        [Benchmark(Description = "AutoMapper - with conversions")]
        public ConversionDto AutoMapper_WithConversions() =>
            _autoMapper.Map<ConversionDto>(
                _conversionSrc);

        /// <summary>
        /// Maps the conversion-heavy source object through Hydrix.
        /// </summary>
        /// <returns>
        /// The destination object produced by Hydrix after applying trimming, formatting, and numeric conversion.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - with conversions")]
        public ConversionDto HydrixMapper_WithConversions() =>
            _hydrixMapper.Map<ConversionDto>(
                _conversionSrc);

        /// <summary>
        /// Maps one hundred small source objects through AutoMapper.
        /// </summary>
        /// <returns>
        /// A list containing the small destination objects produced by AutoMapper.
        /// </returns>
        [Benchmark(Description = "AutoMapper - list small x100")]
        public List<FlatSmallDto> AutoMapper_ListSmall100() =>
            _autoMapper.Map<List<FlatSmallDto>>(
                _smallList100);

        /// <summary>
        /// Maps one hundred small source objects through Hydrix.
        /// </summary>
        /// <returns>
        /// A read-only list containing the small destination objects produced by Hydrix.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - list small x100")]
        public IReadOnlyList<FlatSmallDto> HydrixMapper_ListSmall100()
        {
            var sources = new List<object>(
                _smallList100.Count);

            foreach (var source in _smallList100)
            {
                sources.Add(
                    source);
            }

            return _hydrixMapper.MapList<FlatSmallDto>(
                sources);
        }

        /// <summary>
        /// Maps one thousand small source objects through AutoMapper.
        /// </summary>
        /// <returns>
        /// A list containing the small destination objects produced by AutoMapper.
        /// </returns>
        [Benchmark(Description = "AutoMapper - list small x1000")]
        public List<FlatSmallDto> AutoMapper_ListSmall1000() =>
            _autoMapper.Map<List<FlatSmallDto>>(
                _smallList1000);

        /// <summary>
        /// Maps one thousand small source objects through Hydrix.
        /// </summary>
        /// <returns>
        /// A read-only list containing the small destination objects produced by Hydrix.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - list small x1000")]
        public IReadOnlyList<FlatSmallDto> HydrixMapper_ListSmall1000()
        {
            var sources = new List<object>(
                _smallList1000.Count);

            foreach (var source in _smallList1000)
            {
                sources.Add(
                    source);
            }

            return _hydrixMapper.MapList<FlatSmallDto>(
                sources);
        }

        /// <summary>
        /// Maps one hundred medium source objects through AutoMapper.
        /// </summary>
        /// <returns>
        /// A list containing the medium destination objects produced by AutoMapper.
        /// </returns>
        [Benchmark(Description = "AutoMapper - list medium x100")]
        public List<FlatMediumDto> AutoMapper_ListMedium100() =>
            _autoMapper.Map<List<FlatMediumDto>>(
                _mediumList100);

        /// <summary>
        /// Maps one hundred medium source objects through Hydrix.
        /// </summary>
        /// <returns>
        /// A read-only list containing the medium destination objects produced by Hydrix.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - list medium x100")]
        public IReadOnlyList<FlatMediumDto> HydrixMapper_ListMedium100()
        {
            var sources = new List<object>(
                _mediumList100.Count);

            foreach (var source in _mediumList100)
            {
                sources.Add(
                    source);
            }

            return _hydrixMapper.MapList<FlatMediumDto>(
                sources);
        }

        /// <summary>
        /// Maps one thousand medium source objects through AutoMapper.
        /// </summary>
        /// <returns>
        /// A list containing the medium destination objects produced by AutoMapper.
        /// </returns>
        [Benchmark(Description = "AutoMapper - list medium x1000")]
        public List<FlatMediumDto> AutoMapper_ListMedium1000() =>
            _autoMapper.Map<List<FlatMediumDto>>(
                _mediumList1000);

        /// <summary>
        /// Maps one thousand medium source objects through Hydrix.
        /// </summary>
        /// <returns>
        /// A read-only list containing the medium destination objects produced by Hydrix.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - list medium x1000")]
        public IReadOnlyList<FlatMediumDto> HydrixMapper_ListMedium1000()
        {
            var sources = new List<object>(
                _mediumList1000.Count);

            foreach (var source in _mediumList1000)
            {
                sources.Add(
                    source);
            }

            return _hydrixMapper.MapList<FlatMediumDto>(
                sources);
        }

        /// <summary>
        /// Maps one hundred wide source objects through AutoMapper.
        /// </summary>
        /// <returns>
        /// A list containing the wide destination objects produced by AutoMapper.
        /// </returns>
        [Benchmark(Description = "AutoMapper - list large x100")]
        public List<FlatLargeDto> AutoMapper_ListLarge100() =>
            _autoMapper.Map<List<FlatLargeDto>>(
                _largeList100);

        /// <summary>
        /// Maps one hundred wide source objects through Hydrix.
        /// </summary>
        /// <returns>
        /// A read-only list containing the wide destination objects produced by Hydrix.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - list large x100")]
        public IReadOnlyList<FlatLargeDto> HydrixMapper_ListLarge100()
        {
            var sources = new List<object>(
                _largeList100.Count);

            foreach (var source in _largeList100)
            {
                sources.Add(
                    source);
            }

            return _hydrixMapper.MapList<FlatLargeDto>(
                sources);
        }

        /// <summary>
        /// Maps one thousand wide source objects through AutoMapper.
        /// </summary>
        /// <returns>
        /// A list containing the wide destination objects produced by AutoMapper.
        /// </returns>
        [Benchmark(Description = "AutoMapper - list large x1000")]
        public List<FlatLargeDto> AutoMapper_ListLarge1000() =>
            _autoMapper.Map<List<FlatLargeDto>>(
                _largeList1000);

        /// <summary>
        /// Maps one thousand wide source objects through Hydrix.
        /// </summary>
        /// <returns>
        /// A read-only list containing the wide destination objects produced by Hydrix.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - list large x1000")]
        public IReadOnlyList<FlatLargeDto> HydrixMapper_ListLarge1000()
        {
            var sources = new List<object>(
                _largeList1000.Count);

            foreach (var source in _largeList1000)
            {
                sources.Add(
                    source);
            }

            return _hydrixMapper.MapList<FlatLargeDto>(
                sources);
        }

        /// <summary>
        /// Measures the Hydrix cold path by clearing the cached plans immediately before mapping.
        /// </summary>
        /// <returns>
        /// The destination object produced by Hydrix after rebuilding the plan for the medium flat source instance.
        /// </returns>
        [Benchmark(Description = "Hydrix.Mapper - first hit (cold path)")]
        public FlatMediumDto HydrixMapper_FirstHit_ColdPath()
        {
            MapPlanCache.Clear();
            return _hydrixMapper.Map<FlatMediumDto>(
                _mediumSrc);
        }

        /// <summary>
        /// Builds the reusable small flat source instance used by the benchmark suite.
        /// </summary>
        /// <returns>
        /// A fully populated <see cref="FlatSmallSrc"/> instance representing the compact mapping scenario.
        /// </returns>
        private static FlatSmallSrc BuildSmall() =>
            new FlatSmallSrc
            {
                Id = 1,
                Name = "Alice",
                Email = "alice@example.com",
                Age = 30,
                IsActive = true,
            };

        /// <summary>
        /// Builds the reusable medium flat source instance used by the benchmark suite.
        /// </summary>
        /// <returns>
        /// A fully populated <see cref="FlatMediumSrc"/> instance representing the medium-width mapping scenario.
        /// </returns>
        private static FlatMediumSrc BuildMedium() =>
            new FlatMediumSrc
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                Phone = "+1-800-000",
                Age = 30,
                Salary = 95000m,
                IsActive = true,
                CreatedAt = new DateTime(
                    2020,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ExternalId = Guid.NewGuid(),
                Department = "Engineering",
                Level = 3,
            };

        /// <summary>
        /// Builds the reusable wide flat source instance used by the benchmark suite.
        /// </summary>
        /// <returns>
        /// A fully populated <see cref="FlatLargeSrc"/> instance representing the wide mapping scenario.
        /// </returns>
        private static FlatLargeSrc BuildLarge() =>
            new FlatLargeSrc
            {
                P01 = 1,
                P02 = "a",
                P03 = "b",
                P04 = "c",
                P05 = 5,
                P06 = 6.5m,
                P07 = 7.5,
                P08 = true,
                P09 = DateTime.UtcNow,
                P10 = Guid.NewGuid(),
                P11 = 11L,
                P12 = 12,
                P13 = 13.5f,
                P14 = "n",
                P15 = "o",
                P16 = 16,
                P17 = 17,
                P18 = 18,
                P19 = 19,
                P20 = 20,
            };

        /// <summary>
        /// Builds a list that repeats the supplied template reference the requested number of times.
        /// </summary>
        /// <typeparam name="T">
        /// The source object type to store in the generated list.
        /// </typeparam>
        /// <param name="template">
        /// The reusable object reference that should be inserted into every position of the generated list.
        /// </param>
        /// <param name="count">
        /// The number of entries that the generated list must contain.
        /// </param>
        /// <returns>
        /// A list containing <paramref name="count"/> references to <paramref name="template"/>.
        /// </returns>
        private static List<T> BuildList<T>(
            T template,
            int count)
        {
            var list = new List<T>(
                count);

            for (var index = 0; index < count; index++)
            {
                list.Add(
                    template);
            }

            return list;
        }
    }
}