using System;
using System.Collections.Generic;
using System.IO;

namespace ReplayEventVerifier.ApiProbe
{
    /// <summary>
    /// Finds the ATAS installation by looking, not by assuming.
    /// </summary>
    /// <remarks>
    /// A hard-coded install path is exactly the kind of guess this whole work
    /// package exists to eliminate. The candidate list below is a set of places
    /// to <em>look</em>; a directory is only reported when it actually contains
    /// an assembly whose name starts with <c>ATAS.</c>. If none of them match,
    /// the probe says so and asks for <c>--dir</c> rather than picking one.
    /// </remarks>
    public static class InstallDiscovery
    {
        /// <summary>Assembly name prefixes that mark a directory as an ATAS installation.</summary>
        private static readonly string[] Markers = { "ATAS.", "OFT.", "Utils.Common" };

        public static List<string> CandidateRoots()
        {
            var roots = new List<string>();

            void Add(string path)
            {
                if (!string.IsNullOrEmpty(path) && !roots.Contains(path)) roots.Add(path);
            }

            // Environment.SpecialFolder resolves correctly on the Windows machine
            // this runs on; no path is spelled out literally where the OS can say.
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            foreach (var baseDir in new[] { programFilesX86, programFiles, localAppData, appData, documents })
            {
                if (string.IsNullOrEmpty(baseDir)) continue;
                Add(baseDir);
            }

            return roots;
        }

        /// <summary>
        /// Returns every directory under the candidate roots that holds ATAS
        /// assemblies, most-recently-written first.
        /// </summary>
        public static List<string> FindInstallDirectories(int maxDepth = 4)
        {
            var found = new List<string>();

            foreach (var root in CandidateRoots())
            {
                if (!SafeDirExists(root)) continue;
                Scan(root, 0, maxDepth, found);
            }

            found.Sort((a, b) => LastWrite(b).CompareTo(LastWrite(a)));
            return found;
        }

        private static void Scan(string dir, int depth, int maxDepth, List<string> found)
        {
            if (depth > maxDepth) return;

            if (LooksLikeAtasInstall(dir) && !found.Contains(dir)) found.Add(dir);

            string[] subdirs;
            try { subdirs = Directory.GetDirectories(dir); }
            catch (UnauthorizedAccessException) { return; }
            catch (IOException) { return; }

            foreach (var sub in subdirs)
            {
                // Only descend into directories whose name plausibly relates, except
                // at the top level where we cannot yet tell.
                string name = Path.GetFileName(sub);
                if (depth >= 1 && name.IndexOf("ATAS", StringComparison.OrdinalIgnoreCase) < 0
                               && name.IndexOf("Order", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                Scan(sub, depth + 1, maxDepth, found);
            }
        }

        public static bool LooksLikeAtasInstall(string dir)
        {
            string[] files;
            try { files = Directory.GetFiles(dir, "*.dll"); }
            catch (UnauthorizedAccessException) { return false; }
            catch (IOException) { return false; }

            foreach (var f in files)
            {
                string name = Path.GetFileName(f);
                foreach (var marker in Markers)
                {
                    if (name.StartsWith(marker, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
            return false;
        }

        private static bool SafeDirExists(string d)
        {
            try { return Directory.Exists(d); }
            catch (IOException) { return false; }
        }

        private static DateTime LastWrite(string d)
        {
            try { return Directory.GetLastWriteTimeUtc(d); }
            catch (IOException) { return DateTime.MinValue; }
        }
    }
}
