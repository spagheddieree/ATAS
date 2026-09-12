using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NFMarketDataRecorder.ApiProbe
{
    /// <summary>
    /// Builds the assembly set <see cref="System.Reflection.MetadataLoadContext"/>
    /// resolves against.
    /// </summary>
    /// <remarks>
    /// <para><b>The defect this exists to fix.</b> The probe originally resolved
    /// against the ATAS directory plus <c>RuntimeEnvironment.GetRuntimeDirectory()</c>.
    /// For a plain <c>net8.0</c> console app that directory is
    /// <c>Microsoft.NETCore.App</c>, which does <b>not</b> contain
    /// <c>PresentationCore</c>, <c>PresentationFramework</c>, <c>WindowsBase</c> or
    /// <c>System.Xaml</c>. ATAS assemblies reference WPF types, so resolution threw
    /// <c>FileNotFoundException</c> before a single line of report was written —
    /// on Windows exactly as on Linux, because the resolver is built the same way
    /// on both. It was never a Linux artefact.</para>
    /// <para><b>The fix.</b> Locate the <c>Microsoft.WindowsDesktop.App</c> shared
    /// framework next to the running <c>Microsoft.NETCore.App</c> and add it to the
    /// resolver. Discovered, never hard-coded, and the operator is never asked to
    /// copy framework DLLs into their ATAS installation.</para>
    /// <para>A best-match version is chosen rather than requiring an exact one: the
    /// probe reads metadata only, so a minor-version difference in WPF reference
    /// assemblies cannot change the ATAS signatures being reported.</para>
    /// </remarks>
    public static class ResolverPaths
    {
        /// <summary>Assemblies whose absence caused the original failure.</summary>
        public static readonly string[] RequiredWpfAssemblies =
        {
            "PresentationCore",
            "PresentationFramework",
            "WindowsBase",
            "System.Xaml",
        };

        /// <summary>
        /// The <c>Microsoft.NETCore.App</c> directory of the running runtime.
        /// </summary>
        public static string CoreRuntimeDirectory()
        {
            return RuntimeEnvironment.GetRuntimeDirectory();
        }

        /// <summary>
        /// Derives the <c>Microsoft.WindowsDesktop.App</c> shared-framework root
        /// from the core runtime directory.
        /// </summary>
        /// <remarks>
        /// The two frameworks are installed as siblings:
        /// <c>&lt;dotnet&gt;/shared/Microsoft.NETCore.App/&lt;ver&gt;</c> and
        /// <c>&lt;dotnet&gt;/shared/Microsoft.WindowsDesktop.App/&lt;ver&gt;</c>.
        /// Walking up two levels and across is therefore robust to any install
        /// location, which is the point — no path is assumed.
        /// </remarks>
        public static string WindowsDesktopRoot(string coreRuntimeDirectory)
        {
            if (string.IsNullOrEmpty(coreRuntimeDirectory)) return null;

            try
            {
                var versionDir = new DirectoryInfo(coreRuntimeDirectory.TrimEnd(
                    Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                var netcoreApp = versionDir.Parent;      // Microsoft.NETCore.App
                var shared = netcoreApp?.Parent;         // shared
                if (shared == null) return null;

                string candidate = Path.Combine(shared.FullName, "Microsoft.WindowsDesktop.App");
                return Directory.Exists(candidate) ? candidate : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Picks the best WindowsDesktop version directory: the one whose major
        /// version matches the running runtime, else the highest available.
        /// </summary>
        public static string BestWindowsDesktopVersion(string windowsDesktopRoot, string coreRuntimeDirectory)
        {
            if (string.IsNullOrEmpty(windowsDesktopRoot)) return null;

            string[] versionDirs;
            try { versionDirs = Directory.GetDirectories(windowsDesktopRoot); }
            catch (Exception) { return null; }

            if (versionDirs.Length == 0) return null;

            string wantedMajor = MajorOf(Path.GetFileName(
                (coreRuntimeDirectory ?? "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)));

            string best = null;
            Version bestVersion = null;

            foreach (var dir in versionDirs)
            {
                string name = Path.GetFileName(dir);
                Version parsed = ParseVersion(name);
                if (parsed == null) continue;

                bool majorMatches = wantedMajor != null && MajorOf(name) == wantedMajor;
                bool bestMajorMatches = best != null && wantedMajor != null &&
                                        MajorOf(Path.GetFileName(best)) == wantedMajor;

                // A major-version match always wins; among equals, the highest version.
                if (best == null
                    || (majorMatches && !bestMajorMatches)
                    || (majorMatches == bestMajorMatches && parsed > bestVersion))
                {
                    best = dir;
                    bestVersion = parsed;
                }
            }

            return best;
        }

        private static string MajorOf(string versionDirName)
        {
            Version v = ParseVersion(versionDirName);
            return v == null ? null : v.Major.ToString();
        }

        private static Version ParseVersion(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            // Strip any prerelease suffix, e.g. "9.0.0-rc.1".
            int dash = name.IndexOf('-');
            if (dash > 0) name = name.Substring(0, dash);

            Version v;
            return Version.TryParse(name, out v) ? v : null;
        }

        /// <summary>
        /// The complete resolver set: the ATAS directory, the core runtime, and the
        /// WindowsDesktop framework when one can be found.
        /// </summary>
        /// <param name="missingWpf">
        /// Names of the WPF assemblies still unresolvable after the search. Empty
        /// when the set is complete. Non-empty means metadata resolution may fail,
        /// which the caller reports rather than discovering as an exception.
        /// </param>
        public static List<string> Build(string atasDirectory, out List<string> missingWpf, out string desktopDirUsed)
        {
            var paths = new List<string>();
            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddDirectory(string dir)
            {
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
                string[] files;
                try { files = Directory.GetFiles(dir, "*.dll"); }
                catch (Exception) { return; }

                foreach (var f in files)
                {
                    // First occurrence wins, so the ATAS directory always takes
                    // precedence over framework copies of a same-named assembly.
                    string simple = Path.GetFileNameWithoutExtension(f);
                    if (present.Add(simple)) paths.Add(f);
                }
            }

            AddDirectory(atasDirectory);

            string core = CoreRuntimeDirectory();
            AddDirectory(core);

            string desktopRoot = WindowsDesktopRoot(core);
            desktopDirUsed = BestWindowsDesktopVersion(desktopRoot, core);
            AddDirectory(desktopDirUsed);

            missingWpf = new List<string>();
            foreach (var required in RequiredWpfAssemblies)
                if (!present.Contains(required)) missingWpf.Add(required);

            return paths;
        }
    }
}
