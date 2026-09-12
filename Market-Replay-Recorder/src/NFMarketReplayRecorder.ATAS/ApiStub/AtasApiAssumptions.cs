// ===========================================================================
//  MEASURED ATAS API SURFACE — no longer assumptions.
// ===========================================================================
//
//  Every signature below was READ FROM THE REAL ATAS ASSEMBLIES, copied
//  byte-for-byte from the Windows install at
//  C:\Program Files (x86)\ATAS Platform and SHA-256 verified:
//
//    ATAS.Indicators.dll     cc721fb118c3b6cca94ae02ed4cf89f53c7076729d3ab1cfa8030b95f0952756
//    ATAS.DataFeedsCore.dll  b598f2987437dcdc423acfe03237a01972f6c6891e0174e385b267d369b05f3f
//    ATAS.Types.dll          355bac332cd0f00f28093972cc0743077c73aca6419176dd2b0f39c02319be76
//    OFT.Core.dll            479ce76138ebde434db9a9adccab5dc0f004c118b3d7c92b25d85cb0fbd698aa
//    Utils.Common.dll        97a9590f03989769a16d5a02b55d5b1409c3e191710b0985da5db0d063860dc1
//
//  Source: docs/evidence/atas-api-report.md (sections 1-6), produced by
//  tools/NFMarketReplayRecorder.ApiProbe via MetadataLoadContext. ATAS was never
//  started and no ATAS code executed.
//
//  ---------------------------------------------------------------------------
//  WHAT THIS FILE IS AND IS NOT
//  ---------------------------------------------------------------------------
//
//  IS:     a faithful transcription of the CLR metadata -- type names,
//          namespaces, member names, signatures, accessibility, enum values.
//          Compiling the adapter against it is a real check of the binding.
//
//  IS NOT: evidence of RUNTIME BEHAVIOUR. Metadata says MarketDataArg.Time
//          exists and is a DateTime. It says nothing about whether that value is
//          exchange time, replay time, or arrival time. Every such question is
//          still open and is marked UNKNOWN in the field register.
//
//  The probe also ran on Linux rather than Windows (see the provenance record
//  alongside the report). That is sound for metadata -- MetadataLoadContext
//  reads PE/CLI tables and never executes the target -- but it is one more
//  reason nothing here may be read as a runtime guarantee.
//
//  On a Windows machine with ATAS installed, build WITHOUT STUB_ATAS to compile
//  against the real assemblies. A mismatch now indicates a transcription error
//  here or a different ATAS version, not an unknown API.
//
// ===========================================================================

#if STUB_ATAS

using System;
using System.Collections.Generic;

// ---------------------------------------------------------------------------
//  Framework attributes.
//
//  System.ComponentModel.DataAnnotations is a framework assembly under net472,
//  which is what the real build targets, so the real build gets these from the
//  framework reference in the .csproj. netstandard2.0 does not carry them and
//  the only alternative would be a NuGet package added purely to satisfy a stub
//  build, so the minimal shapes the adapter uses are declared here instead.
// ---------------------------------------------------------------------------
namespace System.ComponentModel.DataAnnotations
{
    // AttributeUsage is copied from the real framework attribute, deliberately. A
    // permissive stub would let the adapter apply [Display] where the real build
    // rejects it -- which is exactly the error this pinning caught once already.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field
                    | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class DisplayAttribute : Attribute
    {
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class RangeAttribute : Attribute
    {
        public RangeAttribute(int minimum, int maximum) { Minimum = minimum; Maximum = maximum; }
        public object Minimum { get; private set; }
        public object Maximum { get; private set; }
    }
}

namespace ATAS.DataFeedsCore
{
    /// <summary>
    /// MEASURED (report section 3): <c>ATAS.DataFeedsCore.MarketDataType</c>,
    /// underlying Int32. Note both the namespace and the ordinal values -- the
    /// earlier assumption placed this in ATAS.Indicators with Trade first.
    /// </summary>
    public enum MarketDataType
    {
        Bid = 0,
        Ask = 1,
        Trade = 2,
    }

    /// <summary>
    /// MEASURED (report section 3): <c>ATAS.DataFeedsCore.TradeDirection</c>,
    /// underlying Int32.
    /// </summary>
    public enum TradeDirection
    {
        Between = 0,
        Buy = 1,
        Sell = 2,
    }
}

namespace ATAS.Indicators
{
    using ATAS.DataFeedsCore;

    /// <summary>MEASURED (section 1): layer selector passed to SubscribeToDrawingEvents.</summary>
    [Flags]
    public enum DrawingLayouts
    {
        None = 0,
        Historical = 1,
        LatestBar = 2,
        Final = 4,
    }

    /// <summary>
    /// MEASURED (section 2): the payload of every market-data callback, and also
    /// the row type returned by the depth snapshot.
    /// </summary>
    /// <remarks>
    /// <para>The measured member list is reproduced here in full.</para>
    /// <para><b>There is no sequence member.</b> The only <c>Sequence</c> property
    /// anywhere in the 2,122-line report belongs to
    /// <c>OFT.Phemex.WsMessages.Pushes.WsDepthPush</c> -- a crypto-exchange
    /// websocket message type not reachable from the indicator API. The exchange
    /// order ids below are ORDER IDENTIFIERS and are <b>not</b> a feed sequence
    /// number; mapping either to source_sequence would fabricate ordering
    /// information the feed never supplied.</para>
    /// </remarks>
    public class MarketDataArg
    {
        public bool IsAsk { get; }
        public bool IsBid { get; }
        public DateTime Time { get; set; }
        public decimal OpenInterest { get; set; }
        public decimal OriginPrice { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public MarketDataType DataType { get; set; }
        public long? AggressorExchangeOrderId { get; set; }
        public long? ExchangeOrderId { get; set; }
        public TradeDirection Direction { get; set; }
    }

    /// <summary>
    /// MEASURED (section 4): depth access exposed to an indicator.
    /// </summary>
    /// <remarks>
    /// The snapshot returns a <b>flat</b> sequence of <see cref="MarketDataArg"/> --
    /// not separate bid and ask ladders -- so each row's side must be read from the
    /// row itself. Row ORDERING is not expressed in metadata and remains
    /// unverified: nothing here licenses assuming best-first, bids-descending or
    /// asks-ascending.
    /// </remarks>
    public interface IMarketDepthInfoProvider
    {
        decimal CumulativeDomAsks { get; }
        decimal CumulativeDomBids { get; }
        IEnumerable<MarketDataArg> GetMarketDepthSnapshot();
    }

    /// <summary>MEASURED (section 5): instrument metadata exposed to an indicator.</summary>
    public interface IInstrumentInfo
    {
        string Instrument { get; }
        string Exchange { get; }
        decimal TickSize { get; }
        int TimeZone { get; }
    }

    /// <summary>
    /// MEASURED (section 1): the indicator base class, reduced to the members the
    /// adapter uses. Real inheritance chain:
    /// <c>Indicator -&gt; ExtendedIndicator -&gt; BaseIndicator -&gt; ChartObject
    /// -&gt; Filters.TrackedPropertyBase -&gt; Object</c>.
    /// </summary>
    /// <remarks>
    /// Accessibility is measured, and it corrects two earlier assumptions:
    /// <see cref="EnableCustomDrawing"/> and <see cref="DenyToChangePanel"/> are
    /// <b>public</b>, while <see cref="MarketDepthInfo"/> and
    /// <see cref="InstrumentInfo"/> are <b>protected and get-only</b>.
    /// </remarks>
    public abstract class Indicator
    {
        public bool EnableCustomDrawing { get; set; }
        public bool DenyToChangePanel { get; set; }

        protected IMarketDepthInfoProvider MarketDepthInfo { get; }
        protected IInstrumentInfo InstrumentInfo { get; }

        protected void SubscribeToDrawingEvents(DrawingLayouts flags) { }

        // --- single-event callbacks: what the recorder binds for capture ---
        protected virtual void OnNewTrade(MarketDataArg trade) { }
        protected virtual void MarketDepthChanged(MarketDataArg depth) { }
        protected virtual void OnBestBidAskChanged(MarketDataArg depth) { }

        // --- batch callbacks: measured to exist, deliberately NOT used to capture ---
        protected virtual void OnNewTrades(IEnumerable<MarketDataArg> trades) { }
        protected virtual void MarketDepthsChanged(IEnumerable<MarketDataArg> depths) { }

        // --- lifecycle ---
        protected virtual void OnInitialize() { }
        protected abstract void OnCalculate(int bar, decimal value);
        protected virtual void OnDispose() { }
        public virtual void Dispose() { }
    }
}

#endif
