using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace NFMarketDataRecorder.ApiProbe
{
    /// <summary>
    /// Reads the real ATAS assemblies' metadata and reports what the API actually is.
    /// </summary>
    /// <remarks>
    /// The report is organised around the recorder's needs, and every target is
    /// answered twice:
    /// <list type="number">
    /// <item><b>Named lookup</b> — is the specific member the adapter currently
    /// assumes actually there?</item>
    /// <item><b>Keyword sweep</b> — what members exist whose names relate to
    /// trades, depth, time or sequencing, regardless of what we assumed?</item>
    /// </list>
    /// The sweep is the important half. A named lookup can only confirm or deny a
    /// guess; the sweep can find the member we should have been using. If the
    /// adapter's assumed name is wrong, the sweep is what reveals the right one.
    /// </remarks>
    public sealed class Probe
    {
        private readonly string _dir;
        private readonly StringBuilder _md = new StringBuilder();
        private MetadataLoadContext _ctx;
        private readonly List<Assembly> _atasAssemblies = new List<Assembly>();

        public Probe(string atasDirectory)
        {
            _dir = atasDirectory;
        }

        public string Report { get { return _md.ToString(); } }

        public int Run()
        {
            H1("ATAS API probe report");
            Line("Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"));
            Line("Probe host OS: " + RuntimeInformation.OSDescription);
            Line("ATAS directory: `" + _dir + "`");
            Blank();
            Line("This report is **measured metadata**, not assumption. Every signature below");
            Line("was read from the assemblies in the directory named above.");
            Blank();

            var dlls = Directory.GetFiles(_dir, "*.dll");
            if (dlls.Length == 0)
            {
                Line("**FAILED: no .dll files in that directory.**");
                return 2;
            }

            // MetadataLoadContext needs the core library plus everything referenced.
            // Bundling the running runtime's assemblies alongside the ATAS ones lets
            // .NET Framework metadata resolve against .NET 8 reference assemblies,
            // which is enough for signature inspection.
            var resolverPaths = new List<string>(dlls);
            resolverPaths.AddRange(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"));

            _ctx = new MetadataLoadContext(new PathAssemblyResolver(resolverPaths));

            H2("0 · Assemblies loaded");
            foreach (var path in dlls.OrderBy(p => p))
            {
                string name = Path.GetFileName(path);
                bool interesting = name.StartsWith("ATAS.", StringComparison.OrdinalIgnoreCase)
                                   || name.StartsWith("OFT.", StringComparison.OrdinalIgnoreCase)
                                   || name.StartsWith("Utils.", StringComparison.OrdinalIgnoreCase);
                if (!interesting) continue;

                try
                {
                    var asm = _ctx.LoadFromAssemblyPath(path);
                    _atasAssemblies.Add(asm);
                    Line("- `" + name + "`  — " + SafeTypeCount(asm) + " public types");
                }
                catch (Exception ex)
                {
                    Line("- `" + name + "`  — **could not load metadata**: " + ex.GetType().Name + ": " + ex.Message);
                }
            }
            Blank();

            if (_atasAssemblies.Count == 0)
            {
                Line("**FAILED: no ATAS/OFT/Utils assemblies could be loaded from that directory.**");
                return 2;
            }

            ReportIndicatorBase();
            ReportMarketDataArg();
            ReportEnums();
            ReportDepthApi();
            ReportInstrument();
            ReportAttributes();
            ReportSweep();

            H2("Next step");
            Line("Send this file back. Each section maps onto a numbered row in");
            Line("`docs/ATAS-API-VERIFICATION.md` §3, and the adapter is corrected against it.");
            return 0;
        }

        // ------------------------------------------------------------------ 1

        private void ReportIndicatorBase()
        {
            H2("1 · Indicator base type and lifecycle");

            var indicator = FindType("ATAS.Indicators.Indicator") ?? FindTypeByName("Indicator");
            if (indicator == null)
            {
                Line("**NOT FOUND: no type named `Indicator`.** The adapter's base class assumption is wrong.");
                Line("Types whose name ends in `Indicator`:");
                foreach (var t in AllTypes().Where(t => t.Name.EndsWith("Indicator", StringComparison.Ordinal)).Take(40))
                    Line("- `" + t.FullName + "`");
                Blank();
                return;
            }

            Line("Found: `" + indicator.FullName + "` in `" + indicator.Assembly.GetName().Name + "`");
            Line("Inheritance: `" + Signatures.BaseChain(indicator) + "`");
            Blank();

            Line("### Members the adapter overrides or calls");
            Blank();
            string[] wanted =
            {
                "OnNewTrade", "MarketDepthChanged", "OnCalculate", "OnInitialize", "OnDispose",
                "SubscribeToDrawingEvents", "EnableCustomDrawing", "DenyToChangePanel",
                "MarketDepthInfo", "InstrumentInfo",
            };

            foreach (var name in wanted)
            {
                var hits = MembersNamed(indicator, name).ToList();
                if (hits.Count == 0)
                {
                    Line("- `" + name + "` — **NOT FOUND**");
                }
                else
                {
                    foreach (var h in hits) Line("- `" + h + "`");
                }
            }
            Blank();

            Line("### Every virtual/abstract member on the hierarchy (what CAN be overridden)");
            Blank();
            Line("```");
            foreach (var m in AllMethods(indicator)
                         .Where(m => (m.IsVirtual || m.IsAbstract) && !m.IsFinal && !m.IsSpecialName)
                         .Select(Signatures.Method).Distinct().OrderBy(s => s))
                _md.AppendLine(m);
            Line("```");
            Blank();
        }

        // ------------------------------------------------------------------ 2

        private void ReportMarketDataArg()
        {
            H2("2 · Market data event payload");

            var arg = FindType("ATAS.Indicators.MarketDataArg") ?? FindTypeByName("MarketDataArg");
            if (arg == null)
            {
                Line("**NOT FOUND: no type named `MarketDataArg`.**");
                Line("Candidate payload types (name contains Trade/Tick/Depth/MarketData):");
                foreach (var t in AllTypes().Where(t =>
                             Contains(t.Name, "MarketData") || Contains(t.Name, "Tick") ||
                             Contains(t.Name, "TradeArg") || Contains(t.Name, "DepthArg")).Take(40))
                    Line("- `" + t.FullName + "`");
                Blank();
                return;
            }

            Line("Found: `" + arg.FullName + "`  (inheritance: `" + Signatures.BaseChain(arg) + "`)");
            Blank();
            Line("### All public properties and fields — with exact CLR types");
            Blank();
            Line("```");
            foreach (var p in AllProperties(arg).Select(Signatures.Property).Distinct().OrderBy(s => s))
                _md.AppendLine(p);
            foreach (var f in arg.GetFields(BindingFlags.Public | BindingFlags.Instance)
                         .Select(Signatures.Field).Distinct().OrderBy(s => s))
                _md.AppendLine(f);
            Line("```");
            Blank();

            Line("### Classification against the recorder contract");
            Blank();
            Line("| Recorder needs | Candidate member(s) found | Verdict |");
            Line("|---|---|---|");
            ClassifyRow(arg, "source timestamp", new[] { "time", "date", "timestamp" });
            ClassifyRow(arg, "price", new[] { "price" });
            ClassifyRow(arg, "volume", new[] { "volume", "size", "qty", "quantity" });
            ClassifyRow(arg, "aggressor / direction", new[] { "direction", "aggress", "side", "type" });
            ClassifyRow(arg, "source sequence / exchange id", new[] { "seq", "id", "number", "ordinal" });
            Blank();
            Line("A row reading NOT FOUND means the field is **VERIFIED UNAVAILABLE** from this");
            Line("payload type and must be recorded as unavailable, never inferred.");
            Blank();
        }

        private void ClassifyRow(Type t, string need, string[] keywords)
        {
            var hits = AllProperties(t).Select(p => p.Name)
                .Concat(t.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name))
                .Where(n => keywords.Any(k => Contains(n, k)))
                .Distinct().OrderBy(s => s).ToList();

            Line("| " + need + " | " + (hits.Count == 0 ? "—" : "`" + string.Join("`, `", hits) + "`") +
                 " | " + (hits.Count == 0 ? "**NOT FOUND**" : "present") + " |");
        }

        // ------------------------------------------------------------------ 3

        private void ReportEnums()
        {
            H2("3 · Enums (book side and trade direction)");

            foreach (var name in new[] { "MarketDataType", "TradeDirection" })
            {
                var t = FindTypeByName(name);
                if (t == null || !t.IsEnum)
                {
                    Line("- `" + name + "` — **NOT FOUND as an enum**");
                    continue;
                }
                Line("**`" + t.FullName + "`** (underlying " + Signatures.TypeName(t.GetEnumUnderlyingType()) + ")");
                Line("```");
                foreach (var m in Signatures.EnumMembers(t)) _md.AppendLine(m);
                Line("```");
            }
            Blank();

            Line("### Any other enum containing Bid/Ask/Trade members");
            Blank();
            foreach (var t in AllTypes().Where(t => t.IsEnum))
            {
                List<string> members;
                try { members = Signatures.EnumMembers(t); }
                catch (Exception) { continue; }

                bool relevant = members.Any(m => m.StartsWith("Bid", StringComparison.Ordinal)
                                              || m.StartsWith("Ask", StringComparison.Ordinal)
                                              || m.StartsWith("Trade", StringComparison.Ordinal));
                if (!relevant) continue;
                Line("- `" + t.FullName + "`: " + string.Join(", ", members));
            }
            Blank();
        }

        // ------------------------------------------------------------------ 4

        private void ReportDepthApi()
        {
            H2("4 · Depth snapshot API");

            Line("Every member across ATAS types whose name mentions depth, book or level.");
            Line("The snapshot must come from the platform's own book, so the exact shape and");
            Line("**ordering** of what these return decides how `ReadSide` is written.");
            Blank();
            Line("```");
            foreach (var t in AllTypes().Where(t => Contains(t.Name, "Depth") || Contains(t.Name, "Book")).Take(60))
            {
                _md.AppendLine("TYPE " + t.FullName);
                foreach (var p in AllProperties(t).Take(30)) _md.AppendLine("    " + Signatures.Property(p));
                foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                             .Where(m => !m.IsSpecialName).Take(30))
                    _md.AppendLine("    " + Signatures.Method(m));
            }
            Line("```");
            Blank();
        }

        // ------------------------------------------------------------------ 5

        private void ReportInstrument()
        {
            H2("5 · Instrument metadata");

            var candidates = AllTypes()
                .Where(t => Contains(t.Name, "Instrument") || Contains(t.Name, "Security"))
                .Take(20).ToList();

            if (candidates.Count == 0) { Line("**No instrument/security type found.**"); Blank(); return; }

            Line("```");
            foreach (var t in candidates)
            {
                _md.AppendLine("TYPE " + t.FullName);
                foreach (var p in AllProperties(t).Take(25)) _md.AppendLine("    " + Signatures.Property(p));
            }
            Line("```");
            Blank();
        }

        // ------------------------------------------------------------------ 6

        private void ReportAttributes()
        {
            H2("6 · Configuration / UI attributes ATAS actually uses");

            Line("If ATAS ships its own attribute types, the adapter's settings should use those");
            Line("rather than the framework ones.");
            Blank();
            Line("```");
            foreach (var t in AllTypes()
                         .Where(t => t.Name.EndsWith("Attribute", StringComparison.Ordinal))
                         .OrderBy(t => t.FullName).Take(60))
                _md.AppendLine(t.FullName);
            Line("```");
            Blank();
        }

        // ------------------------------------------------------------------ 7

        private void ReportSweep()
        {
            H2("7 · Keyword sweep — trade and depth entry points");

            Line("**The most important section.** A named lookup can only confirm or deny the");
            Line("adapter's current guess. This sweep lists every method and event across the");
            Line("ATAS types whose name relates to trades or depth, so the correct entry point");
            Line("is visible even when the assumed name is wrong.");
            Blank();

            string[] keys = { "NewTrade", "OnTrade", "Trade", "Tick", "Print", "MarketDepth", "DepthChanged", "BestBid", "BestAsk" };

            Line("```");
            foreach (var t in AllTypes())
            {
                var hits = new List<string>();

                foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                               BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (m.IsPrivate || m.IsSpecialName) continue;
                    if (keys.Any(k => Contains(m.Name, k))) hits.Add(Signatures.Method(m));
                }

                foreach (var e in t.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (keys.Any(k => Contains(e.Name, k))) hits.Add(Signatures.Event(e));
                }

                if (hits.Count == 0) continue;

                _md.AppendLine("TYPE " + t.FullName);
                foreach (var h in hits.Distinct().OrderBy(s => s)) _md.AppendLine("    " + h);
            }
            Line("```");
            Blank();

            Line("Read this for two specific findings the objective depends on:");
            Blank();
            Line("1. **Is there a per-print trade callback, or only an aggregated one?** A method");
            Line("   taking a collection or a bar index rather than a single event argument");
            Line("   suggests aggregation. If only aggregated trades are exposed, the research");
            Line("   objective is not achievable through this API and that is the finding.");
            Line("2. **Is depth incremental or a whole-book refresh?** A callback carrying one");
            Line("   price level is incremental; one carrying a collection, or none at all with");
            Line("   only a pollable book, means individual depth changes are not observable.");
            Blank();
        }

        // -------------------------------------------------------------- helpers

        private IEnumerable<Type> AllTypes()
        {
            foreach (var asm in _atasAssemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
                catch (Exception) { continue; }

                foreach (var t in types) if (t != null) yield return t;
            }
        }

        private Type FindType(string fullName)
        {
            return AllTypes().FirstOrDefault(t => string.Equals(t.FullName, fullName, StringComparison.Ordinal));
        }

        private Type FindTypeByName(string name)
        {
            return AllTypes().FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.Ordinal));
        }

        /// <summary>Walks the inheritance chain so inherited members are reported too.</summary>
        private static IEnumerable<MethodInfo> AllMethods(Type t)
        {
            var cur = t;
            int guard = 0;
            while (cur != null && guard++ < 20)
            {
                MethodInfo[] ms;
                try
                {
                    ms = cur.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                        BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }
                catch (Exception) { yield break; }

                foreach (var m in ms) if (!m.IsPrivate) yield return m;

                try { cur = cur.BaseType; } catch (Exception) { yield break; }
            }
        }

        private static IEnumerable<PropertyInfo> AllProperties(Type t)
        {
            var cur = t;
            int guard = 0;
            while (cur != null && guard++ < 20)
            {
                PropertyInfo[] ps;
                try
                {
                    ps = cur.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }
                catch (Exception) { yield break; }

                foreach (var p in ps) yield return p;

                try { cur = cur.BaseType; } catch (Exception) { yield break; }
            }
        }

        private static IEnumerable<string> MembersNamed(Type t, string name)
        {
            foreach (var m in AllMethods(t))
                if (string.Equals(m.Name, name, StringComparison.Ordinal)) yield return Signatures.Method(m);

            foreach (var p in AllProperties(t))
                if (string.Equals(p.Name, name, StringComparison.Ordinal)) yield return Signatures.Property(p);
        }

        private static int SafeTypeCount(Assembly asm)
        {
            try { return asm.GetTypes().Length; }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Count(t => t != null); }
            catch (Exception) { return -1; }
        }

        private static bool Contains(string haystack, string needle)
        {
            return haystack != null && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void H1(string s) { _md.AppendLine("# " + s); _md.AppendLine(); }
        private void H2(string s) { _md.AppendLine(); _md.AppendLine("## " + s); _md.AppendLine(); }
        private void Line(string s) { _md.AppendLine(s); }
        private void Blank() { _md.AppendLine(); }
    }
}
