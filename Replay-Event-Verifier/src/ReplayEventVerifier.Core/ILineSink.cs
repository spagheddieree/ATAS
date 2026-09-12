using System;
using System.IO;
using System.Text;

namespace ReplayEventVerifier.Core
{
    /// <summary>
    /// Where serialized lines go. Abstracted so that write-failure handling can be
    /// tested deterministically instead of by contriving a full disk.
    /// </summary>
    public interface ILineSink : IDisposable
    {
        void WriteLine(string line);

        /// <summary>Pushes buffered data all the way to durable storage.</summary>
        void Flush(bool durable);
    }

    /// <summary>UTF-8, LF-terminated, no BOM. LF is fixed regardless of host OS so a
    /// capture taken on Windows is byte-comparable with one taken anywhere else.</summary>
    public sealed class FileLineSink : ILineSink
    {
        private readonly FileStream _fs;
        private readonly StreamWriter _w;

        public FileLineSink(string path)
        {
            _fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 1 << 16);
            _w = new StreamWriter(_fs, new UTF8Encoding(false), 1 << 16);
            _w.NewLine = "\n";
        }

        public void WriteLine(string line)
        {
            _w.Write(line);
            _w.Write('\n');
        }

        public void Flush(bool durable)
        {
            _w.Flush();
            if (durable) _fs.Flush(true);
        }

        public void Dispose()
        {
            try { _w.Flush(); _fs.Flush(true); }
            catch (IOException) { /* nothing useful left to do at teardown */ }
            _w.Dispose();
        }
    }
}
