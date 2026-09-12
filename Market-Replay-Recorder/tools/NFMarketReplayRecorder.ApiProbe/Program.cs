using System;
using System.Collections.Generic;
using System.IO;

namespace NFMarketReplayRecorder.ApiProbe
{
    /// <summary>
    /// Entry point. Finds the ATAS installation, reads its metadata, writes a report.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            string dir = null;
            string outPath = "atas-api-report.md";
            bool listOnly = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--dir": if (i + 1 < args.Length) dir = args[++i]; break;
                    case "--out": if (i + 1 < args.Length) outPath = args[++i]; break;
                    case "--list": listOnly = true; break;
                    case "--help":
                    case "-h": Usage(); return 0;
                }
            }

            try
            {
                if (dir == null || listOnly)
                {
                    Console.WriteLine("Searching for an ATAS installation...");
                    List<string> found = InstallDiscovery.FindInstallDirectories();

                    if (found.Count == 0)
                    {
                        Console.Error.WriteLine();
                        Console.Error.WriteLine("No ATAS installation found automatically.");
                        Console.Error.WriteLine("Searched under:");
                        foreach (var r in InstallDiscovery.CandidateRoots()) Console.Error.WriteLine("  " + r);
                        Console.Error.WriteLine();
                        Console.Error.WriteLine("Re-run with the directory holding ATAS.Indicators.dll, e.g.:");
                        Console.Error.WriteLine("  atas-api-probe --dir \"<path to ATAS install>\"");
                        return 2;
                    }

                    Console.WriteLine("Found " + found.Count + " candidate director" + (found.Count == 1 ? "y" : "ies") + ":");
                    for (int i = 0; i < found.Count; i++) Console.WriteLine("  [" + i + "] " + found[i]);

                    if (listOnly) return 0;

                    dir = found[0];
                    Console.WriteLine();
                    Console.WriteLine("Using: " + dir);
                    Console.WriteLine("(pass --dir to choose a different one)");
                }

                if (!Directory.Exists(dir))
                {
                    Console.Error.WriteLine("error: directory does not exist: " + dir);
                    return 2;
                }

                Console.WriteLine();
                var probe = new Probe(dir);

                int rc;
                try
                {
                    rc = probe.Run();
                }
                catch (Exception ex)
                {
                    // Whatever the probe managed to produce is still worth having --
                    // losing the entire report to one exception was the original
                    // defect, and a partial report usually identifies the cause.
                    rc = 3;
                    Console.Error.WriteLine("probe failed partway: " + ex.GetType().Name + ": " + ex.Message);
                }

                File.WriteAllText(outPath, probe.Report);

                Console.WriteLine("Report written: " + Path.GetFullPath(outPath));
                Console.WriteLine();
                Console.WriteLine(
                    rc == 0 ? "Send that file back to close out the API-binding work package."
                    : rc == 3 ? "PARTIAL report written -- send it anyway; it names what failed."
                    : "The probe could not read ATAS metadata from that directory -- see the report.");
                return rc;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("error: " + ex.GetType().Name + ": " + ex.Message);
                return 1;
            }
        }

        private static void Usage()
        {
            Console.WriteLine(@"atas-api-probe -- reads the REAL ATAS assemblies and reports the actual API.

Reads metadata only. It never loads or executes ATAS code, never starts ATAS,
and never touches your charts, settings or data. Read-only on disk apart from
the report file it writes.

  --dir  <path>   Directory holding ATAS.Indicators.dll.
                  Omit to search for it automatically.
  --list          Only list candidate installation directories, then exit.
  --out  <file>   Report path (default: atas-api-report.md).

Typical use:

  dotnet run -c Release --project tools/NFMarketReplayRecorder.ApiProbe");
        }
    }
}
