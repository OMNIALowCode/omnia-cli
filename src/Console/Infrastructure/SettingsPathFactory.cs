using System;

namespace Omnia.CLI.Infrastructure
{
    internal static class SettingsPathFactory
    {
        public static string Path()
            => System.IO.Path.Combine(OperationSystemFolder(),
                "omnia", "cli");

        public static string OperationSystemPathSlash()
            => Environment.OSVersion.Platform == PlatformID.Win32NT ? "\\" : "/";

        private static string OperationSystemFolder()
            => Environment.OSVersion.Platform == PlatformID.Unix ? Environment.GetEnvironmentVariable("HOME") : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }
}
