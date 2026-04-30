using System;
using System.IO;
using System.Threading;

namespace ReSharperPlugin.SharedMeta
{
    /// <summary>
    /// Opt-in diagnostic logger for the plugin. Writes to
    /// <c>%TEMP%/sharedmeta-rider-plugin.log</c> with millisecond timestamps and a
    /// sequence id so concurrent ReSharper search threads can be untangled.
    /// <para>
    /// Disabled by default — set the environment variable
    /// <c>SHAREDMETA_RIDER_PLUGIN_DEBUG=1</c> before launching Rider to enable. The
    /// flag is sampled once at process start; flipping it requires a Rider restart.
    /// </para>
    /// <para>
    /// Bypasses JetBrains.Util.ILogger on purpose — when enabled, the user can tail
    /// the file from outside the IDE without configuring a custom log appender.
    /// Failures (locked file, permission denied) are swallowed; diagnostic logging
    /// must never break the search itself.
    /// </para>
    /// </summary>
    internal static class DiagLog
    {
        private static readonly bool Enabled =
            Environment.GetEnvironmentVariable("SHAREDMETA_RIDER_PLUGIN_DEBUG") == "1";

        private static readonly string LogPath =
            Path.Combine(Path.GetTempPath(), "sharedmeta-rider-plugin.log");

        private static int _seq;
        private static readonly object Gate = new();

        public static void Write(string line)
        {
            if (!Enabled) return;
            try
            {
                var seq = Interlocked.Increment(ref _seq);
                var ts = DateTime.UtcNow.ToString("HH:mm:ss.fff");
                var formatted = $"[{ts} #{seq:0000}] {line}{Environment.NewLine}";
                lock (Gate)
                {
                    File.AppendAllText(LogPath, formatted);
                }
            }
            catch
            {
                // Swallow — logging failure must not break Find Usages.
            }
        }
    }
}
