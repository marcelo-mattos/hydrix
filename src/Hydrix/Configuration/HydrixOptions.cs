using Microsoft.Extensions.Logging;

namespace Hydrix.Configuration
{
    /// <summary>
    /// Provides configuration options for customizing command execution and logging behavior in the Hydrix framework.
    /// </summary>
    /// <remarks>Use this class to specify settings such as the command timeout duration, whether logging is
    /// enabled, and the logger instance to use for logging operations. The default command timeout is 30
    /// seconds.</remarks>
    public sealed class HydrixOptions
    {
        /// <summary>
        /// The default wait time (in seconds) before terminating the attempt to execute a command and generating an error.
        /// </summary>
        internal const int DefaultTimeout = 30;

        /// <summary>
        /// The default prefix used for SQL parameters.
        /// </summary>
        internal const string DefaultParameterPrefix = "@";

        /// <summary>
        /// Gets or sets the maximum time, in seconds, to wait for a command to execute before timing out.
        /// </summary>
        /// <remarks>The default value is 30 seconds. If the command does not complete within this period,
        /// an exception is thrown. Adjust this value based on the expected duration of commands to balance
        /// responsiveness and reliability.</remarks>
        public int CommandTimeout { get; set; } = DefaultTimeout;

        /// <summary>
        /// Gets or sets the default prefix used for SQL parameters.
        /// </summary>
        /// <remarks>The default value is "@". Adjust this value if your SQL parameters use a different prefix.</remarks>
        public string ParameterPrefix { get; set; } = DefaultParameterPrefix;

        /// <summary>
        /// Gets or sets a value indicating whether per-command logging is enabled.
        /// </summary>
        /// <remarks>When <see langword="false"/> (the default), the logging path is skipped entirely before any
        /// string allocation or StringBuilder work occurs, regardless of the configured log level. Enable only during
        /// development or targeted diagnostics to avoid per-command allocation overhead in high-throughput
        /// production scenarios.</remarks>
        public bool EnableCommandLogging { get; set; } = false;

        /// <summary>
        /// Gets or sets the logger instance to use for logging operations.
        /// </summary>
        /// <remarks>If <see cref="EnableCommandLogging"/> is false this property is ignored.
        /// Must be set together with <see cref="EnableCommandLogging"/> to produce any output.</remarks>
        public ILogger Logger { get; set; }
    }
}
