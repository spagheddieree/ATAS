using System.Text;

namespace NFMarketDataRecorder.Core
{
    /// <summary>
    /// Renders a <see cref="RawEvent"/> as one JSON line.
    /// </summary>
    /// <remarks>
    /// Field order is fixed here and nowhere else. Two captures of the same events
    /// are byte-identical, which is what lets the 1x vs accelerated comparison be
    /// a plain text diff rather than a semantic one.
    /// </remarks>
    public static class EventSerializer
    {
        public static void Write(StringBuilder sb, RawEvent e)
        {
            var j = new JsonLine(sb);
            j.Num("recorder_seq", e.Seq)
             .Str("kind", e.Kind)
             .Time("src_ts", e.SourceUtc)
             .Time("recv_ts", e.ReceivedUtc);

            switch (e.Kind)
            {
                case EventKind.Trade:
                    j.Num("price", e.Price)
                     .Num("volume", e.Volume)
                     .Str("aggressor", e.AggressorSide ?? Aggressor.Unknown);
                    break;

                case EventKind.Depth:
                    j.Str("side", e.BookSide)
                     .Num("price", e.Price)
                     .Num("volume", e.Volume);
                    break;

                case EventKind.Snapshot:
                    j.Num("depth_limit", e.DepthLimit)
                     .Levels("bids", e.Bids)
                     .Levels("asks", e.Asks);
                    break;
            }

            j.End();
        }

        public static string ToLine(RawEvent e)
        {
            var sb = new StringBuilder(160);
            Write(sb, e);
            return sb.ToString();
        }

        /// <summary>
        /// The canonical form used for comparing two captures: the event line with
        /// <c>recorder_seq</c> and <c>recv_ts</c> removed.
        /// </summary>
        /// <remarks>
        /// Both excluded fields are properties of the observer, not of the market.
        /// <c>recv_ts</c> is wall clock and must differ between a 1x and an
        /// accelerated run. <c>seq</c> is capture order, which can legitimately
        /// differ when the platform dispatches two same-instant events on different
        /// threads. Including either would report a mismatch on every run and make
        /// the comparison useless.
        /// </remarks>
        public static void WriteCanonical(StringBuilder sb, RawEvent e)
        {
            var j = new JsonLine(sb);
            j.Str("kind", e.Kind)
             .Time("src_ts", e.SourceUtc);

            switch (e.Kind)
            {
                case EventKind.Trade:
                    j.Num("price", e.Price)
                     .Num("volume", e.Volume)
                     .Str("aggressor", e.AggressorSide ?? Aggressor.Unknown);
                    break;

                case EventKind.Depth:
                    j.Str("side", e.BookSide)
                     .Num("price", e.Price)
                     .Num("volume", e.Volume);
                    break;

                case EventKind.Snapshot:
                    j.Num("depth_limit", e.DepthLimit)
                     .Levels("bids", e.Bids)
                     .Levels("asks", e.Asks);
                    break;
            }

            j.End();
        }

        public static string ToCanonical(RawEvent e)
        {
            var sb = new StringBuilder(160);
            WriteCanonical(sb, e);
            return sb.ToString();
        }
    }
}
