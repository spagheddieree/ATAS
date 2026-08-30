using System;
using System.Collections.Generic;

namespace NQVolatility.Core
{
    /// <summary>One OHLC bar.</summary>
    public readonly struct Ohlc
    {
        public Ohlc(DateTime openTimeUtc, double open, double high, double low, double close)
        {
            OpenTimeUtc = DateTime.SpecifyKind(openTimeUtc, DateTimeKind.Utc);
            Open = open; High = high; Low = low; Close = close;
        }

        public DateTime OpenTimeUtc { get; }
        public double Open { get; }
        public double High { get; }
        public double Low { get; }
        public double Close { get; }
    }

    /// <summary>
    /// Pine <c>ta.atr(length)</c> == <c>ta.rma(ta.tr, length)</c>, Wilder smoothing.
    /// Returns null (Pine <c>na</c>) until the RMA seed is available.
    /// </summary>
    public sealed class WilderAtr
    {
        private readonly int _length;
        private readonly List<double> _seedBuffer;
        private double? _prev;
        private double? _prevClose;
        private int _count;

        public WilderAtr(int length)
        {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length), "minval=1 (Pine line 24)");
            _length = length;
            _seedBuffer = new List<double>(length);
        }

        /// <summary>
        /// Pine <c>ta.tr</c>: on the first bar (no previous close) it degenerates to high-low.
        /// </summary>
        public static double TrueRange(double high, double low, double? prevClose)
        {
            if (prevClose is null) return high - low;
            var pc = prevClose.Value;
            return Math.Max(high - low, Math.Max(Math.Abs(high - pc), Math.Abs(low - pc)));
        }

        /// <summary>Feed one completed bar; returns the ATR after that bar, or null if not yet seeded.</summary>
        public double? Update(double high, double low, double close)
        {
            var tr = TrueRange(high, low, _prevClose);
            _prevClose = close;
            _count++;

            if (_prev is null)
            {
                _seedBuffer.Add(tr);
                if (_seedBuffer.Count < _length) return null;

                // ta.rma seeds with the SMA of the first `length` values.
                double sum = 0;
                foreach (var v in _seedBuffer) sum += v;
                _prev = sum / _length;
                return _prev;
            }

            _prev = (_prev.Value * (_length - 1) + tr) / _length;
            return _prev;
        }

        public double? Current => _prev;
        public int BarsSeen => _count;
    }

    /// <summary>
    /// Models Pine lines 219-221:
    /// <code>
    /// atrDaily     = request.security(tickerid, "D", ta.atr(n), lookahead=barmerge.lookahead_off)
    /// yesterdayATR = atrDaily[1]
    /// </code>
    /// <para>
    /// Two distinct semantics are combined and must not be conflated
    /// (PINE-PARITY-SPEC 6):
    /// </para>
    /// <list type="number">
    /// <item>lookahead_off -- the daily value is published to the intraday series only
    /// AFTER the daily bar closes, so during daily period D the series carries
    /// ATR-through-(D-1).</item>
    /// <item><c>[1]</c> indexes the INTRADAY CHART BAR series, not daily bars. So
    /// yesterdayATR is the value the series held on the PREVIOUS CHART BAR.</item>
    /// </list>
    /// <para>
    /// The two differ only on the first chart bar of a new daily period.
    /// </para>
    /// </summary>
    public sealed class DailyAtrSeries
    {
        private readonly WilderAtr _atr;
        private double? _published;      // atrDaily on the current chart bar
        private double? _previousBar;    // atrDaily[1]
        private bool _hasCurrent;

        public DailyAtrSeries(int atrLength)
        {
            _atr = new WilderAtr(atrLength);
        }

        /// <summary>
        /// Call when a daily bar CLOSES. The resulting ATR becomes visible to
        /// intraday bars from the next chart bar onward (lookahead_off).
        /// </summary>
        public void OnDailyBarClosed(double high, double low, double close)
        {
            _atr.Update(high, low, close);
        }

        /// <summary>
        /// Advance to the next chart bar. Returns <c>yesterdayATR</c> for that bar --
        /// i.e. the value of the published series as of the PREVIOUS chart bar.
        /// </summary>
        public double? AdvanceChartBar()
        {
            _previousBar = _hasCurrent ? _published : null;
            _published = _atr.Current;
            _hasCurrent = true;
            return _previousBar;
        }

        /// <summary>The published daily ATR on the current chart bar (Pine <c>atrDaily</c>).</summary>
        public double? Published => _published;

        /// <summary><c>atrDaily[1]</c> -- what the engine consumes.</summary>
        public double? YesterdayAtr => _previousBar;
    }
}
