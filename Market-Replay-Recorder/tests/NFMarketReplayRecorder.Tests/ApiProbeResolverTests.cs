using System.Collections.Generic;
using System.IO;
using System.Linq;

using NFMarketReplayRecorder.ApiProbe;
using Xunit;

namespace NFMarketReplayRecorder.Tests
{
    /// <summary>
    /// The probe's assembly resolver. These cover the defect that made the probe
    /// throw before writing any report: the WindowsDesktop shared framework was
    /// never in the resolver, so ATAS's WPF references could not resolve.
    /// </summary>
    public class ApiProbeResolverTests
    {
        [Fact]
        public void The_wpf_assemblies_that_caused_the_failure_are_the_ones_checked_for()
        {
            Assert.Contains("PresentationCore", ResolverPaths.RequiredWpfAssemblies);
            Assert.Contains("PresentationFramework", ResolverPaths.RequiredWpfAssemblies);
            Assert.Contains("WindowsBase", ResolverPaths.RequiredWpfAssemblies);
            Assert.Contains("System.Xaml", ResolverPaths.RequiredWpfAssemblies);
        }

        [Fact]
        public void The_core_runtime_directory_is_discovered_and_real()
        {
            string core = ResolverPaths.CoreRuntimeDirectory();
            Assert.False(string.IsNullOrEmpty(core));
            Assert.True(Directory.Exists(core));
        }

        [Fact]
        public void The_windows_desktop_root_is_derived_as_a_sibling_of_the_core_runtime()
        {
            // The layout the derivation relies on:
            //   <dotnet>/shared/Microsoft.NETCore.App/<ver>
            //   <dotnet>/shared/Microsoft.WindowsDesktop.App/<ver>
            using var tmp = new TempDir();
            string shared = Path.Combine(tmp.Path, "shared");
            string core = Path.Combine(shared, "Microsoft.NETCore.App", "8.0.31");
            string desktop = Path.Combine(shared, "Microsoft.WindowsDesktop.App");
            Directory.CreateDirectory(core);
            Directory.CreateDirectory(Path.Combine(desktop, "8.0.31"));

            Assert.Equal(desktop, ResolverPaths.WindowsDesktopRoot(core));
        }

        [Fact]
        public void A_missing_windows_desktop_root_yields_null_rather_than_throwing()
        {
            using var tmp = new TempDir();
            string core = Path.Combine(tmp.Path, "shared", "Microsoft.NETCore.App", "8.0.31");
            Directory.CreateDirectory(core);

            Assert.Null(ResolverPaths.WindowsDesktopRoot(core));
        }

        [Fact]
        public void A_malformed_core_runtime_path_yields_null_rather_than_throwing()
        {
            Assert.Null(ResolverPaths.WindowsDesktopRoot(null));
            Assert.Null(ResolverPaths.WindowsDesktopRoot(""));
        }

        [Fact]
        public void The_desktop_version_matching_the_running_major_is_preferred()
        {
            // A machine with several runtimes installed must not pick, say, 6.x
            // reference assemblies for an 8.x process.
            using var tmp = new TempDir();
            string shared = Path.Combine(tmp.Path, "shared");
            string core = Path.Combine(shared, "Microsoft.NETCore.App", "8.0.31");
            string desktop = Path.Combine(shared, "Microsoft.WindowsDesktop.App");
            Directory.CreateDirectory(core);
            foreach (var v in new[] { "6.0.36", "7.0.20", "8.0.11", "8.0.31" })
                Directory.CreateDirectory(Path.Combine(desktop, v));

            string chosen = ResolverPaths.BestWindowsDesktopVersion(desktop, core);

            Assert.Equal(Path.Combine(desktop, "8.0.31"), chosen);
        }

        [Fact]
        public void The_highest_version_is_used_when_no_major_matches()
        {
            using var tmp = new TempDir();
            string shared = Path.Combine(tmp.Path, "shared");
            string core = Path.Combine(shared, "Microsoft.NETCore.App", "9.0.0");
            string desktop = Path.Combine(shared, "Microsoft.WindowsDesktop.App");
            Directory.CreateDirectory(core);
            Directory.CreateDirectory(Path.Combine(desktop, "6.0.36"));
            Directory.CreateDirectory(Path.Combine(desktop, "8.0.31"));

            Assert.Equal(Path.Combine(desktop, "8.0.31"),
                         ResolverPaths.BestWindowsDesktopVersion(desktop, core));
        }

        [Fact]
        public void A_prerelease_version_directory_does_not_break_selection()
        {
            using var tmp = new TempDir();
            string shared = Path.Combine(tmp.Path, "shared");
            string core = Path.Combine(shared, "Microsoft.NETCore.App", "8.0.31");
            string desktop = Path.Combine(shared, "Microsoft.WindowsDesktop.App");
            Directory.CreateDirectory(core);
            Directory.CreateDirectory(Path.Combine(desktop, "8.0.0-rc.2"));
            Directory.CreateDirectory(Path.Combine(desktop, "8.0.31"));

            Assert.Equal(Path.Combine(desktop, "8.0.31"),
                         ResolverPaths.BestWindowsDesktopVersion(desktop, core));
        }

        [Fact]
        public void An_empty_or_absent_desktop_root_yields_null()
        {
            using var tmp = new TempDir();
            string empty = Path.Combine(tmp.Path, "Microsoft.WindowsDesktop.App");
            Directory.CreateDirectory(empty);

            Assert.Null(ResolverPaths.BestWindowsDesktopVersion(empty, "/x/Microsoft.NETCore.App/8.0.31"));
            Assert.Null(ResolverPaths.BestWindowsDesktopVersion(null, "/x/Microsoft.NETCore.App/8.0.31"));
        }

        [Fact]
        public void The_resolver_set_includes_the_atas_directory_and_the_core_runtime()
        {
            using var tmp = new TempDir();
            File.WriteAllText(Path.Combine(tmp.Path, "ATAS.Fake.dll"), "not a real assembly");

            List<string> missing;
            string desktopUsed;
            var paths = ResolverPaths.Build(tmp.Path, out missing, out desktopUsed);

            Assert.Contains(paths, p => Path.GetFileName(p) == "ATAS.Fake.dll");
            Assert.Contains(paths, p => Path.GetFileName(p) == "System.Runtime.dll");
            Assert.True(paths.Count > 10);
        }

        [Fact]
        public void The_atas_directory_wins_over_a_framework_assembly_of_the_same_name()
        {
            // ATAS must never be shadowed by a framework copy: the point of the probe
            // is to report ATAS's own metadata.
            using var tmp = new TempDir();
            File.WriteAllText(Path.Combine(tmp.Path, "System.Runtime.dll"), "shadowing copy");

            List<string> missing;
            string desktopUsed;
            var paths = ResolverPaths.Build(tmp.Path, out missing, out desktopUsed);

            var chosen = paths.Single(p => Path.GetFileName(p) == "System.Runtime.dll");
            Assert.StartsWith(tmp.Path, chosen);
        }

        [Fact]
        public void No_duplicate_assembly_names_reach_the_resolver()
        {
            // PathAssemblyResolver throws on duplicate simple names, so the build
            // must de-duplicate or the probe fails at construction.
            using var tmp = new TempDir();

            List<string> missing;
            string desktopUsed;
            var paths = ResolverPaths.Build(tmp.Path, out missing, out desktopUsed);

            var names = paths.Select(Path.GetFileNameWithoutExtension).ToList();
            Assert.Equal(names.Count, names.Distinct(System.StringComparer.OrdinalIgnoreCase).Count());
        }

        [Fact]
        public void Missing_wpf_assemblies_are_reported_rather_than_thrown()
        {
            // The behaviour that replaces the original silent failure: the caller is
            // told what is missing and can still emit a useful partial report.
            using var tmp = new TempDir();

            List<string> missing;
            string desktopUsed;
            ResolverPaths.Build(tmp.Path, out missing, out desktopUsed);

            Assert.NotNull(missing);
            foreach (var m in missing)
                Assert.Contains(m, ResolverPaths.RequiredWpfAssemblies);
        }
    }
}
