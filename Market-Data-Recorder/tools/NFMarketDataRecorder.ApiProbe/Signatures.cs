using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NFMarketDataRecorder.ApiProbe
{
    /// <summary>
    /// Renders metadata-only reflection objects as readable C# signatures.
    /// </summary>
    /// <remarks>
    /// Everything here must work against types loaded through
    /// <see cref="System.Reflection.MetadataLoadContext"/>, which forbids anything
    /// that would need the type to be running: no <c>Activator</c>, no
    /// <c>GetCustomAttributes()</c> (only <c>GetCustomAttributesData()</c>), and
    /// enum values read via <c>GetRawConstantValue()</c>.
    /// </remarks>
    public static class Signatures
    {
        public static string TypeName(Type t)
        {
            if (t == null) return "?";
            if (t.IsGenericType)
            {
                var sb = new StringBuilder();
                string name = t.Name;
                int tick = name.IndexOf('`');
                sb.Append(tick >= 0 ? name.Substring(0, tick) : name).Append('<');
                var args = t.GetGenericArguments();
                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(TypeName(args[i]));
                }
                sb.Append('>');
                return sb.ToString();
            }
            return t.Name;
        }

        public static string Access(MethodBase m)
        {
            if (m.IsPublic) return "public";
            if (m.IsFamily) return "protected";
            if (m.IsFamilyOrAssembly) return "protected internal";
            if (m.IsAssembly) return "internal";
            return "private";
        }

        public static string Modifiers(MethodInfo m)
        {
            var parts = new List<string>();
            if (m.IsStatic) parts.Add("static");
            if (m.IsAbstract) parts.Add("abstract");
            else if (m.IsVirtual && !m.IsFinal) parts.Add("virtual");
            else if (m.IsVirtual && m.IsFinal) parts.Add("sealed override");
            return parts.Count == 0 ? "" : string.Join(" ", parts) + " ";
        }

        public static string Method(MethodInfo m)
        {
            var sb = new StringBuilder();
            sb.Append(Access(m)).Append(' ').Append(Modifiers(m))
              .Append(TypeName(m.ReturnType)).Append(' ').Append(m.Name).Append('(');

            var ps = m.GetParameters();
            for (int i = 0; i < ps.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                if (ps[i].ParameterType.IsByRef) sb.Append(ps[i].IsOut ? "out " : "ref ");
                sb.Append(TypeName(ps[i].ParameterType)).Append(' ').Append(ps[i].Name);
            }
            sb.Append(')');
            return sb.ToString();
        }

        public static string Property(PropertyInfo p)
        {
            var getter = p.GetGetMethod(true);
            var setter = p.GetSetMethod(true);
            string acc = getter != null ? Access(getter) : (setter != null ? Access(setter) : "?");

            var sb = new StringBuilder();
            sb.Append(acc).Append(' ').Append(TypeName(p.PropertyType)).Append(' ').Append(p.Name)
              .Append(" { ");
            if (getter != null) sb.Append("get; ");
            if (setter != null) sb.Append("set; ");
            sb.Append('}');
            return sb.ToString();
        }

        public static string Field(FieldInfo f)
        {
            string acc = f.IsPublic ? "public" : f.IsFamily ? "protected" : f.IsAssembly ? "internal" : "private";
            return acc + " " + TypeName(f.FieldType) + " " + f.Name;
        }

        public static string Event(EventInfo e)
        {
            return "event " + TypeName(e.EventHandlerType) + " " + e.Name;
        }

        /// <summary>Enum members and their underlying values, read from metadata only.</summary>
        public static List<string> EnumMembers(Type enumType)
        {
            var result = new List<string>();
            foreach (var f in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                object raw;
                try { raw = f.GetRawConstantValue(); }
                catch (InvalidOperationException) { raw = null; }
                result.Add(f.Name + " = " + (raw ?? "?"));
            }
            return result;
        }

        /// <summary>
        /// The inheritance chain, so it is visible which base class actually
        /// declares a callback the adapter overrides.
        /// </summary>
        public static string BaseChain(Type t)
        {
            var parts = new List<string>();
            var cur = t;
            int guard = 0;
            while (cur != null && guard++ < 20)
            {
                parts.Add(cur.FullName ?? cur.Name);
                try { cur = cur.BaseType; }
                catch (FileNotFoundException) { parts.Add("<base unresolved>"); break; }
            }
            return string.Join("  ->  ", parts);
        }
    }
}
