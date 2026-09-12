using System;
using System.Globalization;
using System.Text;

namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// A minimal, strictly deterministic JSON line builder.
    /// </summary>
    /// <remarks>
    /// Hand-rolled on purpose, for three reasons that a general JSON library does
    /// not give us:
    /// <list type="number">
    /// <item>No dependency is added to an assembly that loads inside the trading
    /// platform's own process.</item>
    /// <item>Field order, number formatting and timestamp formatting are fixed by
    /// this file rather than by a serializer's defaults, so two captures of the
    /// same events are byte-identical and can be diffed directly.</item>
    /// <item>netstandard2.0 has no built-in JSON writer, so the alternative would
    /// be a package reference.</item>
    /// </list>
    /// </remarks>
    public sealed class JsonLine
    {
        private readonly StringBuilder _sb;
        private bool _first = true;

        public JsonLine(StringBuilder sb)
        {
            _sb = sb;
            _sb.Append('{');
        }

        private void Key(string name)
        {
            if (!_first) _sb.Append(',');
            _first = false;
            _sb.Append('"').Append(name).Append("\":");
        }

        public JsonLine Str(string name, string value)
        {
            Key(name);
            WriteString(_sb, value);
            return this;
        }

        public JsonLine Num(string name, long value)
        {
            Key(name);
            _sb.Append(value.ToString(CultureInfo.InvariantCulture));
            return this;
        }

        public JsonLine Num(string name, decimal value)
        {
            Key(name);
            _sb.Append(FormatDecimal(value));
            return this;
        }

        public JsonLine Bool(string name, bool value)
        {
            Key(name);
            _sb.Append(value ? "true" : "false");
            return this;
        }

        public JsonLine Time(string name, DateTime utc)
        {
            Key(name);
            _sb.Append('"').Append(FormatTime(utc)).Append('"');
            return this;
        }

        /// <summary>Writes a depth ladder as <c>[[price,volume],...]</c>.</summary>
        public JsonLine Levels(string name, DomLevel[] levels)
        {
            Key(name);
            _sb.Append('[');
            if (levels != null)
            {
                for (int i = 0; i < levels.Length; i++)
                {
                    if (i > 0) _sb.Append(',');
                    _sb.Append('[')
                       .Append(FormatDecimal(levels[i].Price))
                       .Append(',')
                       .Append(FormatDecimal(levels[i].Volume))
                       .Append(']');
                }
            }
            _sb.Append(']');
            return this;
        }

        public void End()
        {
            _sb.Append('}');
        }

        /// <summary>
        /// UTC, ISO-8601, fixed 7 fractional digits (100 ns, the full resolution of
        /// <see cref="DateTime"/>). Fixed width matters: it makes the text form sort
        /// in the same order as the value, so a capture can be checked for
        /// monotonicity with <c>sort -c</c> and no parsing.
        /// </summary>
        public static string FormatTime(DateTime utc)
        {
            if (utc.Kind == DateTimeKind.Local) utc = utc.ToUniversalTime();
            return utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Canonical decimal text: invariant culture, never exponential, and with
        /// trailing fractional zeros removed so that <c>1.50</c> and <c>1.5</c> —
        /// the same value at different scales — cannot produce two different lines.
        /// </summary>
        public static string FormatDecimal(decimal value)
        {
            string s = value.ToString(CultureInfo.InvariantCulture);
            if (s.IndexOf('.') >= 0)
            {
                s = s.TrimEnd('0');
                if (s.Length > 0 && s[s.Length - 1] == '.') s = s.Substring(0, s.Length - 1);
            }
            if (s.Length == 0) s = "0";
            if (s == "-0") s = "0";
            return s;
        }

        public static void WriteString(StringBuilder sb, string value)
        {
            sb.Append('"');
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        default:
                            if (c < 0x20 || c == 0x7F)
                                sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            else
                                sb.Append(c);
                            break;
                    }
                }
            }
            sb.Append('"');
        }
    }
}
