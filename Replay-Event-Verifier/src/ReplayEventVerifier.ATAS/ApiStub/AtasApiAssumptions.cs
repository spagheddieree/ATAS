// ===========================================================================
//  THIS FILE IS NOT THE ATAS SDK. IT IS A WRITTEN-DOWN SET OF ASSUMPTIONS.
// ===========================================================================
//
//  No ATAS SDK, assembly or documentation was reachable from the environment
//  ReplayVerifierIndicator.cs was authored in:
//
//    * nuget.org has no atas.indicators / atas.datafeedscore / atas.strategies
//      / utils.common package (HTTP 404 on each flat-container index)
//    * atas.net, docs.atas.net and help.atas.net are blocked by egress policy
//    * no ATAS assemblies exist anywhere on the machine
//
//  So the adapter's ATAS API surface could not be verified. Rather than ship it
//  never having been compiled at all, the surface it assumes is declared here
//  and compiled under the STUB_ATAS symbol. That buys exactly one thing: proof
//  that the adapter is internally coherent C# — correct names, arities, types
//  and control flow, no typos. It proves NOTHING about whether these signatures
//  match the real ATAS SDK.
//
//  On a machine with ATAS installed, build WITHOUT STUB_ATAS and against the
//  real assemblies. Every compile error you get is an entry in
//  docs/ATAS-API-VERIFICATION.md that needs correcting. That document lists each
//  symbol below, why it is assumed, and how to fix it.
//
//  This file must never be shipped into the real build. The .csproj excludes it
//  whenever STUB_ATAS is not defined.
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
    // rejects it -- which is exactly the error this pinning caught.
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

namespace ATAS.Indicators
{
    /// <summary>ASSUMED: kind discriminant carried by a market data event.</summary>
    public enum MarketDataType
    {
        Trade = 0,
        Bid = 1,
        Ask = 2,
    }

    /// <summary>ASSUMED: aggressor side of a trade as reported by the feed.</summary>
    public enum TradeDirection
    {
        Between = 0,
        Buy = 1,
        Sell = 2,
    }

    /// <summary>ASSUMED: drawing layer selector.</summary>
    public enum DrawingLayouts
    {
        None = 0,
        Historical = 1,
        LatestBar = 2,
        Final = 3,
    }

    /// <summary>ASSUMED: the payload of every market data callback.</summary>
    public class MarketDataArg
    {
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public MarketDataType DataType { get; set; }
        public TradeDirection Direction { get; set; }
    }

    /// <summary>ASSUMED: one price level returned by the depth API.</summary>
    public class MarketDepthRow
    {
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
    }

    /// <summary>ASSUMED: read access to the platform's own order book.</summary>
    public interface IMarketDepthInfoProvider
    {
        IEnumerable<MarketDepthRow> GetMarketDepth(MarketDataType side);
    }

    /// <summary>ASSUMED: instrument identity exposed to an indicator.</summary>
    public class InstrumentInfoData
    {
        public string Instrument { get; set; }
    }

    /// <summary>ASSUMED: the indicator base class and the members used by the adapter.</summary>
    public abstract class Indicator
    {
        protected bool EnableCustomDrawing { get; set; }
        protected bool DenyToChangePanel { get; set; }

        public InstrumentInfoData InstrumentInfo { get; set; }
        public IMarketDepthInfoProvider MarketDepthInfo { get; set; }

        protected void SubscribeToDrawingEvents(DrawingLayouts layouts) { }

        protected virtual void OnInitialize() { }
        protected abstract void OnCalculate(int bar, decimal value);
        protected virtual void OnNewTrade(MarketDataArg trade) { }
        protected virtual void MarketDepthChanged(MarketDataArg depth) { }
        protected virtual void OnDispose() { }
    }
}

#endif
