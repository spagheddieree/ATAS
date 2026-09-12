# ATAS API probe report

Generated: 2026-09-12T02:07:25Z
Probe host OS: Ubuntu 24.04.4 LTS
ATAS directory: `/home/claude/atas-probe-input`

This report is **measured metadata**, not assumption. Every signature below
was read from the assemblies in the directory named above.


## 0 · Assemblies loaded

- `ATAS.DataFeedsCore.dll`  — 469 public types
- `ATAS.Indicators.Other.dll`  — 229 public types
- `ATAS.Indicators.Technical.dll`  — 521 public types
- `ATAS.Indicators.dll`  — 153 public types
- `ATAS.Strategies.dll`  — 119 public types
- `ATAS.Types.dll`  — 66 public types
- `OFT.Attributes.dll`  — 43 public types
- `OFT.Binance.dll`  — 284 public types
- `OFT.Bitfinex.dll`  — 167 public types
- `OFT.Bitget.dll`  — 151 public types
- `OFT.Bitmex.dll`  — 122 public types
- `OFT.Bybit.dll`  — 189 public types
- `OFT.Controls.dll`  — 136 public types
- `OFT.Core.dll`  — 719 public types
- `OFT.Cqg.dll`  — 560 public types
- `OFT.Crypto.dll`  — 44 public types
- `OFT.Docking.dll`  — 412 public types
- `OFT.DxFeed.dll`  — 166 public types
- `OFT.DxFeedPropTrading.dll`  — 165 public types
- `OFT.Editors.dll`  — 67 public types
- `OFT.Fix.dll`  — 86 public types
- `OFT.FixServer.dll`  — 34 public types
- `OFT.IQFeed.dll`  — 30 public types
- `OFT.InteractiveBrokers2.dll`  — 72 public types
- `OFT.Localization.dll`  — 2 public types
- `OFT.Lua.v86.dll`  — 904 public types
- `OFT.MT5.dll`  — 108 public types
- `OFT.Models.dll`  — 31 public types
- `OFT.NinjaTrader.dll`  — 63 public types
- `OFT.Okx.dll`  — 129 public types
- `OFT.Phemex.dll`  — 173 public types
- `OFT.Platform.Core.dll`  — 2695 public types
- `OFT.Platform.dll`  — 1548 public types
- `OFT.Rendering.OpenGL.dll`  — 67 public types
- `OFT.Rendering.Vortice.dll`  — 40 public types
- `OFT.Rendering.Wpf.dll`  — 32 public types
- `OFT.Rendering.dll`  — 123 public types
- `OFT.Rithmic.dll`  — 176 public types
- `OFT.Sbe.dll`  — 148 public types
- `OFT.SystemStatistics.dll`  — 70 public types
- `OFT.TaiPan.dll`  — 29 public types
- `OFT.TradingTechnology.dll`  — 57 public types
- `OFT.Transaq.dll`  — 49 public types
- `OFT.Whitebit.dll`  — 136 public types
- `Utils.Common.dll`  — 455 public types
- `Utils.Windows.dll`  — 111 public types


## 1 · Indicator base type and lifecycle

Found: `ATAS.Indicators.Indicator` in `ATAS.Indicators`
Inheritance: `ATAS.Indicators.Indicator  ->  ATAS.Indicators.ExtendedIndicator  ->  ATAS.Indicators.BaseIndicator  ->  ATAS.Indicators.ChartObject  ->  ATAS.Indicators.Filters.TrackedPropertyBase  ->  System.Object`

### Members the adapter overrides or calls

- `protected virtual Void OnNewTrade(MarketDataArg trade)`
- `protected virtual Void MarketDepthChanged(MarketDataArg depth)`
- `protected abstract Void OnCalculate(Int32 bar, Decimal value)`
- `protected virtual Void OnInitialize()`
- `protected virtual Void OnDispose()`
- `protected Void SubscribeToDrawingEvents(DrawingLayouts flags)`
- `public Boolean EnableCustomDrawing { get; set; }`
- `public Boolean DenyToChangePanel { get; set; }`
- `protected IMarketDepthInfoProvider MarketDepthInfo { get; }`
- `protected IInstrumentInfo InstrumentInfo { get; }`

### Every virtual/abstract member on the hierarchy (what CAN be overridden)

```
protected abstract Void OnCalculate(Int32 bar, Decimal value)
protected virtual Void Calculate(Int32 bar, Decimal value)
protected virtual Void Finalize()
protected virtual Void LockedOnChanged()
protected virtual Void MarketDepthChanged(MarketDataArg depth)
protected virtual Void MarketDepthsChanged(IEnumerable<MarketDataArg> depths)
protected virtual Void OnApplyDefaultColors()
protected virtual Void OnBestBidAskChanged(MarketDataArg depth)
protected virtual Void OnChangeProperty(String propertyName)
protected virtual Void OnContainerChanged(IIndicatorContainer container)
protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
protected virtual Void OnDataProviderChanged(IIndicatorDataProvider oldDataProvider, IIndicatorDataProvider newDataProvider)
protected virtual Void OnDispose()
protected virtual Void OnFinishRecalculate()
protected virtual Void OnFixedProfilesResponse(IndicatorCandle fixedProfile, FixedProfilePeriods period)
protected virtual Void OnFixedProfilesResponse(IndicatorCandle fixedProfileScaled, IndicatorCandle fixedProfileOriginScale, FixedProfilePeriods period)
protected virtual Void OnInitialize()
protected virtual Void OnMarketByOrdersChanged(IEnumerable<MarketByOrder> values)
protected virtual Void OnNewMyTrade(MyTrade myTrade)
protected virtual Void OnNewOrder(Order order)
protected virtual Void OnNewTrade(MarketDataArg trade)
protected virtual Void OnNewTrades(IEnumerable<MarketDataArg> trades)
protected virtual Void OnOrderCancelFailed(Order order, String message)
protected virtual Void OnOrderChanged(Order order)
protected virtual Void OnOrderModifyFailed(Order order, Order newOrder, String error)
protected virtual Void OnOrderRegisterFailed(Order order, String message)
protected virtual Void OnPortfolioChanged(Portfolio portfolio)
protected virtual Void OnPositionChanged(Position position)
protected virtual Void OnRecalculate()
protected virtual Void OnRender(RenderContext context, DrawingLayouts layout)
protected virtual Void OnSourceChanged()
protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
protected virtual Void OnVisibleChanged()
protected virtual Void RecalculateValues()
public virtual Boolean Equals(Object obj)
public virtual Boolean ProcessKeyDown(KeyEventArgs e)
public virtual Boolean ProcessKeyUp(KeyEventArgs e)
public virtual Boolean ProcessMouseClick(RenderControlMouseEventArgs e)
public virtual Boolean ProcessMouseDoubleClick(RenderControlMouseEventArgs e)
public virtual Boolean ProcessMouseDown(RenderControlMouseEventArgs e)
public virtual Boolean ProcessMouseMove(RenderControlMouseEventArgs e)
public virtual Boolean ProcessMouseUp(RenderControlMouseEventArgs e)
public virtual Boolean ProcessMouseWheel(Int32 delta)
public virtual Int32 GetHashCode()
public virtual StdCursor GetCursor(RenderControlMouseEventArgs e)
public virtual String ToString()
public virtual Void Dispose()
public virtual Void RefreshData()
```


## 2 · Market data event payload

Found: `ATAS.Indicators.MarketDataArg`  (inheritance: `ATAS.Indicators.MarketDataArg  ->  System.Object`)

### All public properties and fields — with exact CLR types

```
public Boolean IsAsk { get; }
public Boolean IsBid { get; }
public DateTime Time { get; set; }
public Decimal OpenInterest { get; set; }
public Decimal OriginPrice { get; set; }
public Decimal Price { get; set; }
public Decimal Volume { get; set; }
public MarketDataType DataType { get; set; }
public Nullable<Int64> AggressorExchangeOrderId { get; set; }
public Nullable<Int64> ExchangeOrderId { get; set; }
public TradeDirection Direction { get; set; }
```

### Classification against the recorder contract

| Recorder needs | Candidate member(s) found | Verdict |
|---|---|---|
| source timestamp | `Time` | present |
| price | `OriginPrice`, `Price` | present |
| volume | `Volume` | present |
| aggressor / direction | `AggressorExchangeOrderId`, `DataType`, `Direction` | present |
| source sequence / exchange id | `AggressorExchangeOrderId`, `ExchangeOrderId`, `IsBid` | present |

A row reading NOT FOUND means the field is **VERIFIED UNAVAILABLE** from this
payload type and must be recorded as unavailable, never inferred.


## 3 · Enums (book side and trade direction)

**`ATAS.DataFeedsCore.MarketDataType`** (underlying Int32)
```
Bid = 0
Ask = 1
Trade = 2
```
**`ATAS.DataFeedsCore.TradeDirection`** (underlying Int32)
```
Buy = 1
Sell = 2
Between = 0
```

### Any other enum containing Bid/Ask/Trade members

- `ATAS.DataFeedsCore.EntityAction`: Logon = 0, Logout = 1, UserAdd = 2, UserChange = 3, UserRemove = 4, UserLock = 29, UserKick = 30, UserChangeExpiration = 47, UserGroupAdd = 5, UserGroupChange = 6, UserGroupRemove = 7, UserRoleAdd = 8, UserRoleChange = 9, UserRoleRemove = 10, ExchangeAdd = 11, ExchangeChange = 12, ExchangeRemove = 13, CommissionGroupAdd = 17, CommissionGroupChange = 18, CommissionGroupRemove = 19, SecurityMarginAdd = 20, SecurityMarginChange = 21, SecurityMarginRemove = 22, PortfolioAdd = 23, PortfolioChange = 32, PortfolioValue = 24, PortfolioLock = 25, OrderRegister = 26, OrderMove = 27, OrderCancel = 28, NewsSend = 31, ServerSettings = 33, RequestStatistics = 34, OrderCancelled = 35, OrderMatched = 36, ServerPnL = 37, PortfolioSuspend = 38, PortfolioViewerAdd = 39, ConnectorAdd = 40, ConnectorChange = 41, ConnectorRemove = 42, ConnectorEnable = 43, PortfolioViewerRemove = 44, TradeAllowed = 45, PortfolioReset = 46, GetUserChangeHistory = 48, GetPortfolioChangeHistory = 49, UserGroupSuspendPortfolios = 50
- `ATAS.DataFeedsCore.EntityType`: Security = 0, SecurityMargin = 1, Portfolio = 2, Position = 3, MarketDepth = 4, Trade = 5, Order = 6, MyTrade = 7, News = 8, UserRole = 9, UserGroup = 10, User = 11, UserChange = 12, CommissionGroup = 13, WorkingTime = 14, Exchange = 15, PortfolioChange = 16, TradingOptions = 17, PortfolioState = 18, PositionState = 19, Connector = 20, MarketByOrder = 21
- `ATAS.DataFeedsCore.MarketDataType`: Bid = 0, Ask = 1, Trade = 2
- `ATAS.Indicators.Technical.OrderFlowRhythm+Mode`: Volume = 0, BidAsk = 1
- `ATAS.Indicators.Technical.ActiveVolume+CalcMode`: BidAsk = 0, Bid = 1, Ask = 2
- `ATAS.Indicators.Technical.BarVolumeFilter+VolumeType`: Volume = 0, Ticks = 1, Delta = 2, Bid = 3, Ask = 4
- `ATAS.Indicators.Technical.BidAskVR+Mode`: AskBid = 0, BidAsk = 1
- `ATAS.Indicators.Technical.ClusterSearch+CalcMode`: Bid = 0, Ask = 1, Delta = 2, Volume = 3, Tick = 4, MaxVolume = 5, Time = 6
- `ATAS.Indicators.Technical.ClusterStatistic+DataType`: Ask = 0, Bid = 1, Delta = 2, DeltaVolume = 3, SessionDelta = 4, SessionDeltaVolume = 5, MaxDelta = 6, MinDelta = 7, DeltaChange = 8, Volume = 9, VolumeSecond = 10, SessionVolume = 11, Trades = 12, Height = 13, Time = 14, Duration = 15, None = 16
- `ATAS.Indicators.Technical.DynamicLevels+MiddleClusterType`: Bid = 0, Ask = 1, Delta = 2, Volume = 3, Tick = 4, Time = 5
- `ATAS.Indicators.Technical.Exhaustion+CalcModes`: Bid = 0, Ask = 1, BidAndAsk = 2, Volume = 3
- `ATAS.Indicators.Technical.MaxLevels+MaxLevelType`: Bid = 0, Ask = 1, PositiveDelta = 2, NegativeDelta = 3, Volume = 4, Tick = 5, Time = 6
- `ATAS.Indicators.Technical.TapePattern+TicksType`: Any = 0, Bid = 1, Ask = 2, Between = 3, BidOrAsk = 4, BidAndAsk = 5
- `ATAS.Indicators.Technical.UpDownVolumeRatio+CalculationMode`: UpDownVolume = 0, AskBidVolume = 1
- `ATAS.Indicators.Technical.VerticalHorizontalFilter+InputType`: Volume = 0, Ticks = 1, Asks = 2, Bids = 3, Open = 4, High = 5, Low = 6, Close = 7, OHLCAverage = 8, HLCAverage = 9, HLAverage = 10
- `ATAS.Indicators.Technical.Volume+InputType`: Volume = 0, Ticks = 1, Asks = 2, Bids = 3
- `ATAS.Indicators.Technical.VWAP+VolumeType`: Total = 0, Bid = 1, Ask = 2
- `ATAS.Indicators.FootprintColorSchemes`: Delta = 0, Solid = 10, VolumeProportion = 20, TradesProportion = 30, VolumeBasedBidAsk = 50, HeatMapVolume = 60, HeatMapTrades = 70, HeatMapDelta = 80, None = 90
- `ATAS.Indicators.FootprintContentModes`: Volume = 0, Trades = 10, VolumeTrades = 30, VolumeDelta = 40, Delta = 50, DeltaCentered = 51, BidXAsk = 60, BidAskCentered = 70, BidAsk = 80, None = 100
- `ATAS.Indicators.FootprintVisualModes`: FullRow = 0, BidAskHistogram = 1, VolumeHistogram = 2, TradesHistogram = 3, DeltaHistogram = 4, BidAskLadder = 5, PositiveNegativeDeltaProfile = 6
- `ATAS.Indicators.MarketDataType`: Bid = 0, Ask = 1, Trade = 2
- `ATAS.Indicators.SelectionType`: Full = 0, Bid = 1, Ask = 2
- `ATAS.Types.ClusterType`: Bid = 0, Ask = 1, Delta = 2, Volume = 3, Tick = 4, BidAsk = 5, VolDelta = 6
- `ATAS.Types.MarketDataType`: Bid = 0, Ask = 1
- `ATAS.Types.Tick+DataType`: Print = 0, Ask = 50, Bid = 51
- `OFT.Binance.Common.ExecutionTypes`: New = 0, Canceled = 1, Replaced = 2, Rejected = 3, Trade = 4, Expired = 5, Amendment = 6
- `OFT.Binance.SystemMessages.MessageChannelTypes`: ChannelConnected = 0, ChannelDisconnected = 1, Error = 2, SecurityFilter = 3, ExchangeInfo = 4, LastPriceSnapshot = 5, GetFuturesMode = 6, UserDataStream = 7, UserDataStreamEmpty = 8, Depth = 9, DepthSnapshot = 10, Trade = 11, BookTicker = 12, SpotAccount = 13, SpotAccountUpdate = 14, SpotPositionUpdate = 15, SpotOrder = 16, SpotIfNewUser = 17, MarginAccount = 18, FuturesAccount = 19, FuturesPosition = 20, FuturesAccountUpdate = 21, FuturesAccountConfigurationUpdate = 22, FuturesOrder = 23, FuturesMarginCall = 24, FuturesLeverageBrackets = 25, FuturesLiquidations = 26, FuturesIfNewUser = 27, SetFuturesMode = 28, FuturesExecution = 29, OrderRestResponse = 30, OrderCancelRestResponse = 31, StopOrderRejected = 32, Ping = 33, OrderSnapshot = 34, ServerTime = 35, GetRebateRecentResponse = 36, GetRebateOverviewResponse = 37, ChangeInitialLeverage = 38, ChangeMarginTypeResponse = 39, ChangeIsolatedMargin = 40, MarkPrice = 41, OpenInterest = 42, HistoricalTrades = 43, UserTrades = 44, GetSpotDailyStatistics = 45, GetFutDailyStatistics = 46, MiniTicker = 47
- `OFT.Bitfinex.SystemMessages.MessageChannelTypes`: Rest = 0, AuthOk = 1, AuthFail = 2, ChannelDisconnected = 3, ChannelError = 4, ChannelException = 5, Pong = 6, Info = 7, NotParsedResponse = 8, ChannelMessage = 9, SymbolNames = 10, SymbolDetails = 11, TickersData = 12, SecurityFilter = 13, BookResponse = 14, BookSnapshot = 15, BookUpdate = 16, TradeResponse = 17, TradeSnapshot = 18, TradeUpdate = 19, TickerResponse = 20, TickerSnapshot = 21, TickerUpdate = 22, PositionSnapshot = 23, PositionUpdate = 24, WalletsSnapshot = 25, WalletsUpdate = 26, MyTradeTe = 27, MyTradeTu = 28, OrderNewRest = 29, OrderReplaceRest = 30, OrderCancelRest = 31, OrdersSnapshot = 32, OrdersSnapshotSingle = 33, OrderUpdate = 34, OrderNotification = 35, FundingOffersSnapshot = 36, FundingOffersUpdate = 37, FundingCreditsSnapshot = 38, FundingCreditsUpdate = 39, FundingLoansSnapshot = 40, FundingLoansUpdate = 41, FundingTradesSnapshot = 42, FundingTradesUpdate = 43
- `OFT.Bitmex.SystemMessages.MessageChannelTypes`: Info = 0, Warning = 1, Error = 2, Status = 3, SubscriptionResult = 4, Auth = 5, SecurityFilter = 6, Instrument = 7, BookUpdate = 8, TradeUpdate = 9, QuotesUpdate = 10, InstrumentUpdate = 11, Wallet = 12, Position = 13, PositionUpdate = 14, OrderUpdate = 15, OrderSnapshot = 16, PostOrderResult = 17, PutOrderResult = 18, DeleteOrderResult = 19, MyTrade = 20
- `Advanced_Time_And_Sales.Indicators.other.SimpleClusterType`: Bid = 0, Ask = 1, Volume = 2, Tick = 3, Delta = 4
- `Advanced_Time_And_Sales.TradingModule.BestPriceType`: Hide = 0, Bid = 1, Ask = 2
- `Advanced_Time_And_Sales.TradingModule.Columns.DomChanges+ChangesType`: BidChanges = 0, AskChanges = 1, Both = 2
- `Advanced_Time_And_Sales.TradingModule.Columns.InformationalColumn+InformationType`: OpenInterest = 2, Trades = 3, Buys = 4, Sells = 5, Betweens = 6, AboveAsks = 7, BelowBids = 8
- `Advanced_Time_And_Sales.TradingModule.Columns.Queue+QueueVisualType`: Bid = 1, Offer = 2, Both = 3
- `Advanced_Time_And_Sales.TradingModule.Side`: Bid = 0, Offer = 1
- `OFT.Core.Models.EntityTypes`: MarketDepth = 0, Trade = 1, WorkingTime = 2, Exchange = 3, Candle = 4, VolumeCandle = 5, BigTrade = 6, User = 7, UserGroup = 8, UserRole = 9, MarketDepthSnapshot = 10, TradingSession = 11, TradingSessionWorkingTime = 12
- `OFT.Core.Models.MarketDataTypes`: Bid = 0, Ask = 1, Trade = 2
- `OFT.Core.Sbe.Messages.SbeMarketDataTypes`: Bid = 0, Offer = 1, Trade = 2, OpenPrice = 3, HighPrice = 4, LowPrice = 5, OpenInterest = 6, EmprtMarketBook = 7, HighBid = 8, LowOffer = 9, NULL_VALUE = 255
- `OFT.Core.Server.MarketData.SubscriptionTypes`: Trade = 0, BigTrade = 1, Candle = 2, VolumeCandle = 3, NULL_VALUE = 255
- `OFT.Core.Storage.Messages.MarketDepthTypes`: Bid = 0, Ask = 1, NULL_VALUE = 255
- `Shared1.OrderStatus+Status`: InTransit = 1, Rejected = 2, Working = 3, Expired = 4, InCancel = 5, InModify = 6, Cancelled = 7, Filled = 8, Suspended = 9, Disconnected = 10, Activeat = 11, ApproveRequired = 12, ApprovedByExchange = 13, ApproveRejected = 14, Matched = 15, PartiallyMatched = 16, TradeBroken = 17
- `Shared1.TransactionStatus+Status`: InTransit = 1, Rejected = 2, AckPlace = 3, Expired = 4, InCancel = 5, AckCancel = 6, RejectCancel = 7, InModify = 8, AckModify = 9, RejectModify = 10, Fill = 11, Suspend = 12, FillCorrect = 13, FillCancel = 14, FillBust = 15, Activeat = 16, Disconnect = 17, SyntheticActivated = 18, Update = 19, SyntheticFailed = 20, SyntheticOverfill = 21, SyntheticHang = 22, Approving = 23, ApproveRequested = 24, ApprovedByExchange = 25, RejectedByUser = 26, Matched = 27, TradeBroken = 28, TradeAmended = 29
- `PropTradingProtocol.AccountHistoricalEntityEnum`: All = 0, Orders = 1, Trades = 2, Fills = 3, FillTrades = 4
- `PropTradingProtocol.SymbolSpreadTypeEnum`: Native = 0, BidDifference = 1, AskDifference = 2
- `NinjaTrader.Data.MarketDataType`: Ask = 0, Bid = 1, Last = 2, DailyHigh = 3, DailyLow = 4, DailyVolume = 5, LastClose = 6, Opening = 7, OpenInterest = 8, Settlement = 9, Unknown = 10
- `OFT.NinjaTrader.MessageType`: Portfolio = 0, Order = 1, Trade = 2, Position = 3, Balance = 4, Depth = 5, Tick = 6, BidAsk = 7, NewSecurity = 8, PositionInfo = 9, Pnl = 10, Connected = 11, Disconnected = 12, SearchSecurity = 13, InstrumentConnectionError = 14, Summary = 15
- `Advanced_Time_And_Sales.ClusterSettings+MaxVolSelectionType`: Volume = 0, Trades = 1, Bid = 2, Ask = 3, NegativeDelta = 4, PositiveDelta = 5, Time = 6
- `Advanced_Time_And_Sales.ClusterVisualizationType`: BidAsk = 0, BidAskLadder = 1, BidAskHistogram = 2, BidAskDigitalHistogram = 3, BidAskProfile = 4, GraduatedVolume = 6, DeltaColoredVolume = 7, VolumeHistogtram = 8, DigitalVolumeHistogram = 9, GraduatedTrades = 10, DeltaColoredTrades = 11, TradesHistogram = 12, DigitalTradesHistogram = 13, DeltaHistogram = 14, VolumexDelta = 15, VolumexTrades = 16, Delta = 5, DeltaColoredVolumeHistogram = 17, DeltaColoredTradesHistogram = 18, DeltaProfile = 19, GraduatedTime = 20, DeltaColoredTime = 21, TimeHistogram = 22, DigitalTimeHistogram = 23, BidAskDeltaProfile = 24, BidAskImbalance = 25
- `Advanced_Time_And_Sales.EjectoinClusterType`: Bid = 0, Ask = 1, Volume = 2, Trades = 3, RelativeVolume = 4, RelativeTrades = 5
- `Advanced_Time_And_Sales.Indicators.IndicatorsParameters.MarketProfiliesSettings+MaxLevelType`: Volume = 0, Trades = 1, Time = 2, Delta = 3, PositiveDelta = 4, NegativeDelta = 5, Bid = 6, Ask = 7
- `Advanced_Time_And_Sales.Indicators.IndicatorsParameters.MarketProfiliesSettings+ProfileType`: Volume = 0, Trades = 1, Time = 2, Delta = 3, DeltaV2 = 4, BidAsk = 5
- `Advanced_Time_And_Sales.Indicators.IndicatorsParameters.TapePatternsSettings+TicksType`: Any = 0, Bid = 1, Ask = 2, Between = 3, BidOrAsk = 4, BidAndAsk = 5
- `Advanced_Time_And_Sales.Indicators.other.MiddleClusterType`: Bid = 0, Ask = 1, Delta = 2, Volume = 3, Tick = 4, Time = 5
- `Advanced_Time_And_Sales.Indicators.other.WideClusterType`: Bid = 0, Ask = 1, PositiveDelta = 2, NegativeDelta = 3, Volume = 4, Tick = 5, Time = 6
- `OFT.Controls.Chart.enums.ClustersColorSchemes`: Delta = 0, Solid = 10, VolumeProportion = 20, TradesProportion = 30, TimeProportion = 40, VolumeBasedBidAsk = 50, HeatMapVolume = 60, HeatMapTrades = 70, HeatMapDelta = 80, None = 90
- `OFT.Controls.Chart.enums.ClustersContentModes`: Volume = 0, Trades = 10, Time = 20, VolumeTrades = 30, VolumeDelta = 40, Delta = 50, DeltaCentered = 51, BidXAsk = 60, BidAskCentered = 70, BidAsk = 80, None = 100
- `OFT.Controls.Chart.enums.ClustersVisualModes`: FullRow = 0, BidAskHistogram = 1, VolumeHistogram = 2, TradesHistogram = 3, TimeHistogram = 4, DeltaHistogram = 5, BidAskLadder = 6, PositiveNegativeDeltaProfile = 7
- `OFT.Platform.Core.DataFeedManager.Replay.ReplayMarketDataTypes`: CandlesAndGeneratedBestPrices = 0, TradesAndGeneratedBestPrices = 1, TradesAndDepths = 2
- `OFT.Platform.Core.Models.PlatformModules`: Chart = 0, SmartDOM = 1, SmartTape = 2, AllPrices = 3, BidAskTape = 4, ForwardCurve = 5, OptionsBoard = 6, Positions = 7, Watchlist = 8
- `OFT.Platform.DrawingObjects.CustomHistogramm+MaxLevelType`: None = -1, Volume = 0, Trades = 1, Time = 2, Delta = 3, PositiveDelta = 4, NegativeDelta = 5, Bid = 6, Ask = 7
- `OFT.Platform.DrawingObjects.CustomHistogramm+ProfileType`: Volume = 0, Trades = 1, Time = 2, Delta = 3, DeltaV2 = 4, BidAsk = 5
- `OFT.Platform.Models.Instrument+#=zN53xzH9_yCmlubfk4w==`: Trade = 0, Depth = 1, DomUpdate = 2, DomClear = 3, Mbo = 4, Summary = 5
- `OFT.Platform.ViewModels.ApplicationMenu.ApplicationMenuItems`: Chart = 0, Watchlist = 1, SmartDOM = 2, SmartTape = 3, BidAskTape = 4, AllPrices = 5, Positions = 6, Education = 7, Connectors = 8, HowToTrade = 9, Video = 10, HelpCenter = 11, Community = 12, SubmitIdea = 13, WhatsNew = 14, ContactUs = 15, Tutorials = 16, CryptoMentors = 17, GettingStarted = 18, Templates = 19
- `-.dje_z3U2A2U9GHUBUJ8BRTEYZ6FFNQB9JEYHANW_ejd+OptionFields`: Volume = 0, OpenInterest = 1, Last = 2, Bid = 3, Ask = 4, Strike = 5, IV = 6, BidIV = 7, AskIV = 8, Delta = 9, Gamma = 10, Thetta = 11, Vega = 12
- `-.dje_zH376P6ANZHT4KMYW42NGA72DTLKDDT3CGUAVJ6TU_ejd`: None = 0, Layout = 1, Chart = 2, SmartDOM = 3, SmartTape = 4, BidAskTape = 5, AllPrices = 6, DrawingObjectsList = 7, DrawingObject = 8, Watchlist = 9, Diamond = 10, Dot = 11, DownArrow = 12, FiboRetracement = 13, GlobalHLine = 14, HLine = 15, HighlightX = 16, HighlightY = 17, Label = 18, LeftPriceMetka = 19, MyTradeObject = 20, PriceChannel = 21, RectangleObj = 22, RightPriceMetka = 23, Ruler = 24, Square = 25, TextMetka = 26, TrendLine = 27, Triangle = 28, UpArrow = 29, VerticalLine = 30, CustomHistogramm = 31, EllipseObj = 32, FiboExtensions = 33, HistoryVerticalLine = 34, HorizontalRay = 35, Logs = 36, Portfolios = 37, MyTrades = 38, Positions = 39, Strategies = 40, Statistics = 41, Panel = 42, Orders = 43, News = 44, Alerts = 45, Layouts = 46, Replay = 47, MissingPanel = 48, CopyTrading = 49, MissingDrawingObject = 50, DrawingShortPosition = 51, DrawingLongPosition = 52, ElliotWaveImpulse = 53, ElliotWaveCorrection = 54, ElliotWaveTriangle = 55, ElliotWaveDouble = 56, ElliotWaveTriple = 57, CrossLine = 58, AnchoredVWAP = 59, Angle = 60, CVDCorrelation = 61, DynamicPoc = 62, FiboFans = 63
- `-.dje_zZU268KK7JVYCMW6UUP3HPUQDYY4AGQJCRL6YS34D_ejd`: Time = 0, Price = 1, PlusMinus = 2, Volume = 3, Bid = 4, Ask = 5, Sizes = 6, Avg = 7, Mm = 8, Oi = 9, DeltaOi = 10, Speed = 11, BuySell = 12, VolumeOI = 13
- `OFT.Platform.Settings.OldSettings.NewTape.TapeAlertType`: CumulativeTrade = 0, Bid = 4, Ask = 5, Best_Bid = 1, Best_Ask = 2, Best_BidAsk = 3
- `OFT.Platform.ViewModels.SmartTape.SmartTapeAlertType`: CumulativeTrade = 0, Bid = 4, Ask = 5, Best_Bid = 1, Best_Ask = 2, Best_BidAsk = 3
- `OFT.Rithmic.MessageTypes`: OrderFill = 0, OrderUpdate = 1, ReplayPnL = 2, PnlUpdate = 3, Alert = 4, Error = 5, ExecutionsReplay = 6, OrderFailure = 7, OrderNotCancelled = 8, OrderReject = 9, ChangeSubscription = 10, OrderCancel = 11, AccountListInfo = 12, OrderReplay = 13, PnLReplay = 14, TradeRoutes = 15, ModifyReport = 16, RefDataInfo = 17, PriceIncrInfo = 18, AskInfo = 19, BidInfo = 20, BestAskInfo = 21, BestBidInfo = 22, TradeInfo = 23, OrderBookInfo = 24, CheckConnection = 25, CheckLoginComplete = 26, OrderActionError = 27, SubscribeError = 28, PositionExitInfo = 29, REngineDisposed = 30, Dbo = 31, OrderHistoryDatesInfo = 32, OptionsList = 33, OpenInterest = 34, OpenPrice = 35, HighPrice = 36, LowPrice = 37, ClosePrice = 38, SettlementPrice = 39, TradeVolume = 40
- `OFT.Sbe.SbeMessageType`: Logon = 1, Logout = 2, Heartbeat = 10, Message = 11, SecurityList3 = 18, SecurityListRequest2 = 19, SecurityListRequest = 20, SecurityList2 = 21, SecurityList = 22, MarketDataRequest = 23, MarketDataRequestReject = 24, SecurityListReject = 25, AccountsRequest = 26, AccountsReport = 27, AccountReport = 28, PositionReport = 29, MarketDataSnapshotFullRefresh = 30, MarketDataIncrementalRefresh = 31, MarketDataIncrementalRefreshSingle = 32, MarketDataOIIncrementalRefreshSingle = 33, OrdersRequest = 40, OrderReport = 41, OrderRefresh = 42, OrderReject = 43, TradeReport = 44, NewOrder = 45, OrderCancelRequest = 46, OrderCancelReplaceRequest = 47, ResetAccountRequest = 48, MyTradesRequest = 50, MyTradesResponse = 51, NewOrderWithReduceFlag = 104, ClosePositionRequest = 107
- `OFT.SystemStatistics.Models.StatisticsDataTypes`: NeedEqualize = 0, EqualizeTradesAdded = 1, TradesOpenVolumeReseted = 2
- `OFT.SystemStatistics.Models.UserActionSources`: Platform = 0, Connectors = 1, Chart = 2, SmartDOM = 3, SmartTape = 4, AllPrices = 5, BidAskTape = 6, Indicators = 7, Strategies = 8, ChartTrader = 9, DOMTrader = 10, AutoTrade = 11, DrawingObject = 12


## 4 · Depth snapshot API

Every member across ATAS types whose name mentions depth, book or level.
The snapshot must come from the platform's own book, so the exact shape and
**ordering** of what these return decides how `ReadSide` is written.

```
TYPE ATAS.DataFeedsCore.IMarketDepth
    public DateTime Time { get; set; }
    public Int32 Type { get; set; }
    public Decimal Price { get; set; }
    public Decimal Volume { get; set; }
    public Boolean IsAsk { get; }
    public Boolean IsBid { get; }
    public abstract IMarketDepth Clone()
TYPE ATAS.DataFeedsCore.MarketDepth
    public EntityType EntityType { get; }
    public String ECN { get; set; }
    public Decimal Price { get; set; }
    public Decimal Volume { get; set; }
    public Int32 OrdersCount { get; set; }
    public DateTime Time { get; set; }
    public MarketDataType Type { get; set; }
    public Security Security { get; set; }
    public Boolean IsAsk { get; }
    public Boolean IsBid { get; }
    public sealed override IMarketDepth Clone()
    public virtual String ToString()
TYPE ATAS.DataFeedsCore.MarketDepthComparer
    public sealed override Int32 Compare(Decimal x, Decimal y)
TYPE ATAS.Indicators.Technical.TradingDepthOfMarket
    public Boolean AlredyResetFilter { get; set; }
    public Boolean EnableAutoCenter { get; set; }
    public OrderFlowVisualTypes OrderFlowVisualType { get; set; }
    public FilterHeatmapTypes HeatmapType { get; set; }
    public FilterColor DomPolygonColor { get; set; }
    public FilterInt UpperCutOff { get; set; }
    public FilterInt Contrast { get; set; }
    public VerticalSmoothingModes SmoothingMode { get; set; }
    public FilterInt VerticalSmoothing { get; set; }
    public Int32 AnimationSpeed { get; set; }
    public FilterInt PowScale { get; set; }
    public Boolean LowLevelTransparency { get; set; }
    public FilterInt CustomColorStep { get; set; }
    public Filter CustomFilter { get; set; }
    public PenSettings BestBidLine { get; set; }
    public PenSettings BestAskLine { get; set; }
    public FilterInt OrderFlowSize { get; set; }
    public Filter OrderFlowCustomMaximumValue { get; set; }
    public Color HeatmapBuyTrades { get; set; }
    public Color HeatmapSellTrades { get; set; }
    public Boolean SpeedIndicatorEnabled { get; set; }
    public FilterInt OrderSpeedInterval { get; set; }
    public Color HeatmapSpeedBuys { get; set; }
    public Color HeatmapSpeedSells { get; set; }
    public PenSettings HeatmapBuyTradesBorder { get; set; }
    public PenSettings HeatmapSellTradesBorder { get; set; }
    public FilterBool CumulativeModeFilter { get; set; }
    public Filter OrderFlowFilter { get; set; }
    public Boolean HideFilteredTrades { get; set; }
    public FilterColor OrderFlowTextColorFilter { get; set; }
    public virtual Boolean ProcessMouseMove(RenderControlMouseEventArgs e)
    public virtual Boolean ProcessMouseDown(RenderControlMouseEventArgs e)
    public virtual Boolean ProcessMouseUp(RenderControlMouseEventArgs e)
    public virtual Boolean ProcessMouseClick(RenderControlMouseEventArgs e)
    public virtual Boolean ProcessKeyDown(KeyEventArgs e)
    public virtual Boolean ProcessMouseWheel(Int32 delta)
    public virtual Boolean ProcessMouseDoubleClick(RenderControlMouseEventArgs e)
    public virtual StdCursor GetCursor(RenderControlMouseEventArgs e)
    public Void DrawToolTip(RenderContext g, String text, Int32 x, Int32 y, Color color, RenderFont font, Color background, Boolean right, Int32 yOffset, Int32 xOffset, Color textColor)
    public sealed override IHeatmapColorCalculator CreateColorCalculator()
TYPE ATAS.Indicators.Technical.OrderBookAlerts
    public Decimal Filter { get; set; }
    public Filter TimeFilter { get; set; }
    public PriceOffsetMode POMode { get; set; }
    public Int32 PriceOffset { get; set; }
    public Boolean UseAlerts { get; set; }
    public String AlertFile { get; set; }
    public Color AlertForeColor { get; set; }
    public Color AlertBGColor { get; set; }
    public Boolean ShowOnChart { get; set; }
    public Single CoolDownPeriod { get; set; }
    public String Category { get; }
    protected IInstrumentInfo InstrumentInfo { get; }
    protected ITradingManager TradingManager { get; }
    public IPlatformSettings PlatformSettings { get; }
    protected IMarketDepthInfoProvider MarketDepthInfo { get; }
    public IChart ChartInfo { get; }
    public ITradingStatisticsProvider TradingStatisticsProvider { get; }
    protected Rectangle ChartArea { get; }
    protected IMouseLocationInfo MouseLocationInfo { get; }
    protected Int32 FirstVisibleBarNumber { get; }
    protected Int32 LastVisibleBarNumber { get; }
    protected Int32 VisibleBarsCount { get; }
    protected Decimal CumulativeDomAsks { get; }
    protected Decimal CumulativeDomBids { get; }
    public String Instrument { get; }
    protected Decimal TickSize { get; }
    public String DataPath { get; }
    public Int32 ValueAreaPercent { get; }
    public String ChartType { get; }
    public String TimeFrame { get; }
TYPE ATAS.Indicators.IMarketDepthInfoProvider
    public Decimal CumulativeDomAsks { get; }
    public Decimal CumulativeDomBids { get; }
    public abstract IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
TYPE ATAS.Indicators.MarketDepthInfoProvider
    public Decimal CumulativeDomAsks { get; }
    public Decimal CumulativeDomBids { get; }
    public sealed override IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
TYPE ATAS.Indicators.MarketDepthSnapshot
    public DateTime StartTime { get; set; }
    public IReadOnlyCollection<MarketDataArg> Bids { get; set; }
    public IReadOnlyCollection<MarketDataArg> Asks { get; set; }
TYPE ATAS.Indicators.MarketDepthSnapshotRequest
    public Int32 RequestId { get; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public TimeSpan Period { get; set; }
TYPE ATAS.Types.ATASMarketDepthEventArgs
    public ATASMarketDepthEventArgs GetCopy()
    public virtual String ToString()
TYPE OFT.Binance.Common.MarketDepthLimits
TYPE OFT.Binance.Rest.Commands.GetDepthSnapshot
    public String Symbol { get; set; }
    public MarketDepthLimits Limit { get; set; }
    public HttpMethods HttpMethod { get; }
    public Type ResponseType { get; }
    public Boolean UseApiKey { get; }
    public Object UserData { get; set; }
    public Nullable<Int32> Weight { get; set; }
    public CommandType CommandType { get; set; }
    public virtual String GetCommand(ApiTypes apiType)
    public virtual Int32 GetWeight(ApiTypes apiType)
TYPE OFT.Binance.Rest.Commands.GetDepthSnapshotResponse
    public DepthSnapshot Data { get; set; }
    public Type DataType { get; }
    public TimeSpan ElapsedTime { get; set; }
    public DateTime ReceivedAt { get; set; }
    public Object Data { get; set; }
    public Object UserData { get; set; }
    public MessageChannelTypes ChannelType { get; set; }
    public ApiTypes ApiType { get; set; }
TYPE OFT.Binance.Rest.Model.Depth.DepthSnapshot
    public Int64 LastUpdateId { get; set; }
    public List<DepthTradeData> BidDepthDeltas { get; set; }
    public List<DepthTradeData> AskDepthDeltas { get; set; }
TYPE OFT.Binance.WebSocket.Model.BookTickerData
    public Int64 UpdateId { get; set; }
    public DateTime EventTime { get; set; }
    public DateTime TransactionTime { get; set; }
    public String Symbol { get; set; }
    public Decimal BestBidPrice { get; set; }
    public Decimal BestBidQuantity { get; set; }
    public Decimal BestAskPrice { get; set; }
    public Decimal BestAskQuantity { get; set; }
TYPE OFT.Binance.WebSocket.Model.DepthData
    public String EventType { get; set; }
    public DateTime EventTime { get; set; }
    public String Symbol { get; set; }
    public Int64 UpdateId { get; set; }
    public List<DepthTradeData> BidDepthDeltas { get; set; }
    public List<DepthTradeData> AskDepthDeltas { get; set; }
TYPE OFT.Binance.WebSocket.Model.DepthTradeData
    public Decimal Price { get; set; }
    public Decimal Quantity { get; set; }
    public virtual String ToString()
    public virtual Int32 GetHashCode()
    public virtual Boolean Equals(Object obj)
    public sealed override Boolean Equals(DepthTradeData other)
    public Void Deconstruct(out Decimal& Price, out Decimal& Quantity)
TYPE OFT.Binance.WebSocket.Model.PartialDepthData
    public Int64 LastUpdateId { get; set; }
    public List<DepthTradeData> BidDepthDeltas { get; set; }
    public List<DepthTradeData> AskDepthDeltas { get; set; }
TYPE OFT.Bitget.WsMessages.DepthInfo
    public String[][] Asks { get; set; }
    public String[][] Bids { get; set; }
    public Int64 Checksum { get; set; }
    public String Ts { get; set; }
TYPE OFT.Bitmex.WebSocket.V1.Book.DepthBuilder
    public IEnumerable<String> MessageSymbols(Update update)
    public ICollection<MarketDepth> Update(Update update, Security security, DateTime time)
TYPE OFT.Bitmex.WebSocket.V1.Book.DepthBuilder+Depth
    public Int64 Id { get; set; }
    public Decimal Price { get; set; }
    public OrderDirections Direction { get; set; }
    public Decimal Amount { get; set; }
TYPE OFT.Core.DataProvider.Requests.MarketDepthSnapshotsRequest
    public TimeSpan SnapshotsPeriod { get; set; }
    public ScaleSettings ScaleSettings { get; set; }
    public Int64 RequestId { get; set; }
    public Int64 ContractId { get; set; }
    public Contract Contract { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public ITradingSession TradingSession { get; set; }
    public virtual String ToString()
TYPE OFT.Core.DataProvider.Requests.MarketDepthsRequest
    public Int64 RequestId { get; set; }
    public Int64 ContractId { get; set; }
    public Contract Contract { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public ITradingSession TradingSession { get; set; }
    public virtual String ToString()
TYPE OFT.Core.Models.IMarketDepthSnapshot
    public Contract Contract { get; }
    public IReadOnlyCollection<MarketDepth> Asks { get; }
    public IReadOnlyCollection<MarketDepth> Bids { get; }
    public IEnumerable<MarketDepth> Quotes { get; }
    public Int32 Count { get; }
    public DateTime StartTime { get; }
    public Boolean IsEmpty { get; }
    public Boolean IsFullState { get; }
TYPE OFT.Core.Models.MarketDepth
    public EntityTypes EntityType { get; }
    public DateTime Time { get; set; }
    public MarketDataTypes Type { get; set; }
    public Contract Contract { get; set; }
    public Decimal Price { get; set; }
    public Decimal Volume { get; set; }
    public EcnId Ecn { get; set; }
    public Boolean IsAsk { get; }
    public Boolean IsBid { get; }
    public sealed override IMarketDepth Clone()
    public virtual String ToString()
    public virtual Boolean Equals(Object obj)
    public virtual Int32 GetHashCode()
TYPE OFT.Core.Models.MarketDepthSnapshot
    public EntityTypes EntityType { get; }
    public Contract Contract { get; }
    public DateTime StartTime { get; }
    public Boolean IsFullState { get; }
    public Boolean IsEmpty { get; }
    public IReadOnlyCollection<MarketDepth> Asks { get; set; }
    public IReadOnlyCollection<MarketDepth> Bids { get; set; }
    public IEnumerable<MarketDepth> Quotes { get; }
    public Int32 Count { get; }
    public virtual Boolean Equals(Object obj)
    public virtual Int32 GetHashCode()
TYPE OFT.Core.Models.MarketDepthSnapshotExtensions
TYPE OFT.Core.Models.SuperficialMarketDepthEqualityComparer
    public sealed override Boolean Equals(MarketDepth one, MarketDepth another)
    public sealed override Int32 GetHashCode(MarketDepth obj)
TYPE OFT.Core.Server.MarketData.MarketDepthSnapshotsRequestMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 RequestId { get; set; }
    public Int64 ContractId { get; set; }
    public Int64 From { get; set; }
    public Int64 To { get; set; }
    public Int32 Scale { get; set; }
    public Byte ScaleByLower { get; set; }
    public Int64 Period { get; set; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
TYPE OFT.Core.Server.MarketData.MarketDepthsRequestMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 RequestId { get; set; }
    public Int64 ContractId { get; set; }
    public Int64 From { get; set; }
    public Int64 To { get; set; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
TYPE OFT.Core.Storage.Messages.CoinMarketDepthMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 Time { get; set; }
    public MarketDepthTypes Type { get; set; }
    public UInt64 Price { get; set; }
    public UInt64 Volume { get; set; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
TYPE OFT.Core.Storage.Messages.CoinMarketDepthSnapshotDiffMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 StartTime { get; set; }
    public AsksGroup Asks { get; }
    public BidsGroup Bids { get; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
    public AsksGroup AsksCount(Int32 count)
    public BidsGroup BidsCount(Int32 count)
TYPE OFT.Core.Storage.Messages.CoinMarketDepthSnapshotMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 StartTime { get; set; }
    public AsksGroup Asks { get; }
    public BidsGroup Bids { get; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
    public AsksGroup AsksCount(Int32 count)
    public BidsGroup BidsCount(Int32 count)
TYPE OFT.Core.Storage.Messages.MarketDepthMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 Time { get; set; }
    public MarketDepthTypes Type { get; set; }
    public Int32 Price { get; set; }
    public UInt32 Volume { get; set; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
TYPE OFT.Core.Storage.Messages.MarketDepthSnapshotDiffMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 StartTime { get; set; }
    public AsksGroup Asks { get; }
    public BidsGroup Bids { get; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
    public AsksGroup AsksCount(Int32 count)
    public BidsGroup BidsCount(Int32 count)
TYPE OFT.Core.Storage.Messages.MarketDepthSnapshotMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 StartTime { get; set; }
    public AsksGroup Asks { get; }
    public BidsGroup Bids { get; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
    public AsksGroup AsksCount(Int32 count)
    public BidsGroup BidsCount(Int32 count)
TYPE OFT.Core.Storage.Messages.MarketDepthStorageContextMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Double TickSize { get; set; }
    public Double LotSize { get; set; }
    public Int64 LastTime { get; set; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
TYPE OFT.Core.Storage.Messages.MarketDepthTypes
TYPE OFT.Core.Storage.Messages.StockMarketDepthMessage
    public UInt16 BlockLength { get; }
    public UInt16 TemplateId { get; }
    public UInt16 SchemaId { get; }
    public UInt16 SchemaVersion { get; }
    public String SemanticType { get; }
    public DirectBuffer DirectBuffer { get; }
    public Int32 Offset { get; }
    public Int32 Size { get; }
    public Int32 Limit { get; set; }
    public Int64 Time { get; set; }
    public MarketDepthTypes Type { get; set; }
    public Int32 Price { get; set; }
    public UInt32 Volume { get; set; }
    public Byte Ecn { get; set; }
    public sealed override Void WrapForEncode(DirectBuffer buffer, Int32 offset)
    public Void WrapForDecode(DirectBuffer buffer, Int32 offset, Int32 actingBlockLength, Int32 actingVersion)
TYPE Otc1.HedgeBookDetailsReport
    public Boolean IsSnapshot { get; set; }
    public Boolean IsLastPart { get; set; }
    public List<BalanceItem> BalanceItems { get; }
    public List<BalanceItemsLink> ItemsLinks { get; }
    public Boolean ShouldSerializeIsSnapshot()
    public Void ResetIsSnapshot()
    public Boolean ShouldSerializeIsLastPart()
    public Void ResetIsLastPart()
TYPE Otc1.HedgeBookDetailsSubscription
    public Boolean Subscribe { get; set; }
    public UInt32 OtcInstanceId { get; set; }
    public HedgeBalanceKey HedgeBalanceKey { get; set; }
    public Nullable<DateTime> FromUtcTimestamp { get; set; }
    public String ArchiveId { get; set; }
    public Boolean ShouldSerializeSubscribe()
    public Void ResetSubscribe()
    public Boolean ShouldSerializeOtcInstanceId()
    public Void ResetOtcInstanceId()
    public Boolean ShouldSerializeArchiveId()
    public Void ResetArchiveId()
TYPE Otc1.HedgeBooksReport
    public Boolean IsSnapshot { get; set; }
    public Boolean IsLastPart { get; set; }
    public List<HedgeBalanceDetails> HedgeBalanceDetails { get; }
    public List<ArchivedHedgeBalanceDetails> ArchivedHedgeBalanceDetails { get; }
    public List<GroupBalanceDetails> GroupBalanceDetails { get; }
    public Boolean ShouldSerializeIsSnapshot()
    public Void ResetIsSnapshot()
    public Boolean ShouldSerializeIsLastPart()
    public Void ResetIsLastPart()
TYPE Otc1.HedgeBooksSubscription
    public Boolean Subscribe { get; set; }
    public UInt32 OtcInstanceId { get; set; }
    public Boolean ShouldSerializeSubscribe()
    public Void ResetSubscribe()
    public Boolean ShouldSerializeOtcInstanceId()
    public Void ResetOtcInstanceId()
TYPE OFT.DxFeed.IOrderBook
    public Object Symbol { get; }
    public Int32 Depth { get; }
    public IList<MarketDepth> Asks { get; }
    public IList<MarketDepth> Bids { get; }
    public abstract Void Clear()
TYPE OFT.DxFeed.MultipleMarketDepthModel
    public Guid LoggerId { get; set; }
    public String LoggerName { get; set; }
    public LoggingLevel LoggingLevel { get; set; }
    public ILoggerSource ParentLoggerSource { get; set; }
    public ICachedCollection<ILoggerSource> ChildLoggerSources { get; }
    public Void AddSymbol(String symbol, OrderSource[] sources)
    public Void RemoveSymbol(String symbol)
    public Boolean TryGetBook(String security, out OrderBook& book)
    public sealed override Void Dispose()
TYPE OFT.DxFeed.OrderBook
    public Object Symbol { get; }
    public Int32 Depth { get; }
    public IList<MarketDepth> Asks { get; }
    public IList<MarketDepth> Bids { get; }
    public sealed override Void Clear()
    public Void Update(IReadOnlyList<Order> changes, out ICollection`1& updates, out ICollection`1& deletions)
TYPE OFT.Models.Education.Book
    public Int32 Number { get; set; }
    public LocalizedValue Title { get; set; }
    public LocalizedValue Description { get; set; }
    public LocalizedValue DocumentDark { get; set; }
    public LocalizedValue DocumentLight { get; set; }
    public LocalizedValue PreviewImageDark { get; set; }
    public LocalizedValue PreviewImageLight { get; set; }
    public String FeatureName { get; set; }
TYPE OFT.Okx.Common.DepthLimitType
TYPE OFT.Phemex.WsMessages.Pushes.Book
    public Int64[][] Asks { get; set; }
    public Int64[][] Bids { get; set; }
TYPE OFT.Phemex.WsMessages.Pushes.BookP
    public String[][] Asks { get; set; }
    public String[][] Bids { get; set; }
TYPE OFT.Phemex.WsMessages.Pushes.WsDepthPush
    public Book Book { get; set; }
    public BookP BookP { get; set; }
    public Int32 Depth { get; set; }
    public Int64 Timestamp { get; set; }
    public Int64 Sequence { get; set; }
    public String Symbol { get; set; }
    public String Type { get; set; }
TYPE OFT.Platform.Core.DataFeedManager.Replay.MarketDepthsEntity
    public EntityTypes EntityType { get; }
    public Contract Contract { get; }
    public DateTime Time { get; }
    public IReadOnlyCollection<MarketDepth> Depths { get; }
TYPE OFT.Platform.Core.Providers.IMarketDepthSnapshotsProvider
    public abstract Task<IEnumerable<IMarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Platform.Core.Providers.MarketDepthSnapshotsProvider
    public Guid LoggerId { get; set; }
    public String LoggerName { get; set; }
    public LoggingLevel LoggingLevel { get; set; }
    public ILoggerSource ParentLoggerSource { get; set; }
    public ICachedCollection<ILoggerSource> ChildLoggerSources { get; }
    public sealed override Task<IEnumerable<IMarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
```


## 5 · Instrument metadata

```
TYPE ATAS.DataFeedsCore.ISecurityPositionManager
    public Position Position { get; }
    public Boolean IsPositionInitialized { get; }
TYPE ATAS.DataFeedsCore.ISecurityTradingOptions
    public TimeInForce TimeInForce { get; }
    public TriggerPriceType TriggerPriceTypes { get; }
TYPE ATAS.DataFeedsCore.InstrumentExchange
    public String Code { get; set; }
    public String ExchangeCode { get; set; }
    public Exchange Exchange { get; set; }
TYPE ATAS.DataFeedsCore.Security
    public EntityType EntityType { get; }
    public String SecurityId { get; set; }
    public String ConnectorId { get; set; }
    public String Code { get; set; }
    public String Exchange { get; set; }
    public String Instrument { get; set; }
    public DateTime Expiration { get; set; }
    public SecType Type { get; set; }
    public Decimal TickSize { get; set; }
    public Decimal TickCost { get; set; }
    public Decimal LotSize { get; set; }
    public Nullable<Decimal> LotMinSize { get; set; }
    public Nullable<Decimal> LotMaxSize { get; set; }
    public Int32 Digits { get; set; }
    public Decimal MinPrice { get; set; }
    public Decimal MaxPrice { get; set; }
    public Decimal MarginBuy { get; set; }
    public Decimal MarginSell { get; set; }
    public Int64 Id { get; set; }
    public Int64 IsinId { get; set; }
    public Decimal BestAskPrice { get; set; }
    public Decimal BestAskVolume { get; set; }
    public Decimal BestBidPrice { get; set; }
    public Decimal BestBidVolume { get; set; }
    public Nullable<Decimal> LastTradePrice { get; set; }
TYPE ATAS.DataFeedsCore.SecurityFilter
    public Int64 RequestId { get; set; }
    public String Code { get; set; }
    public String Exchange { get; set; }
    public String Id { get; set; }
    public SecType Type { get; set; }
    public Int64 ContractId { get; set; }
    public String ContractCode { get; set; }
    public String ContractExchange { get; set; }
    public Nullable<DateTime> Expiration { get; set; }
TYPE ATAS.DataFeedsCore.SecurityMargin
    public String SecurityId { get; set; }
    public Boolean IsContract { get; set; }
    public Security Security { get; set; }
    public DateTime Date { get; set; }
    public Decimal IntradayInitialMarginBuy { get; set; }
    public Decimal IntradayInitialMarginSell { get; set; }
    public Decimal IntradayMarginBuy { get; set; }
    public Decimal IntradayMarginSell { get; set; }
    public Decimal InitialMarginBuy { get; set; }
    public Decimal InitialMarginSell { get; set; }
    public Decimal MarginBuy { get; set; }
    public Decimal MarginSell { get; set; }
    public EntityType EntityType { get; }
TYPE ATAS.DataFeedsCore.SecurityPositionManager
    public Position Position { get; }
    public Boolean IsPositionInitialized { get; }
    public PositionAveragePriceValueTypes AveragePriceValueType { get; set; }
    public Decimal AveragePrice { get; set; }
    public Decimal Volume { get; set; }
    public Boolean CalculateVolume { get; set; }
    public Boolean CalculateAveragePrice { get; set; }
    public Boolean CalculateOpenedPnL { get; set; }
    public Boolean CalculateClosedPnL { get; set; }
    public Boolean AllowSubscribeLevel1 { get; set; }
TYPE ATAS.DataFeedsCore.SecurityRoute
    public Int64 TradingOptionsId { get; set; }
    public String Code { get; set; }
    public String Exchange { get; set; }
    public String Account { get; set; }
    public String Name { get; }
TYPE ATAS.DataFeedsCore.SecurityRouteCache
TYPE ATAS.DataFeedsCore.SecuritySummary
    public Security Security { get; set; }
    public Nullable<Decimal> BestAskPrice { get; set; }
    public Nullable<Decimal> BestAskVolume { get; set; }
    public Nullable<Decimal> BestBidPrice { get; set; }
    public Nullable<Decimal> BestBidVolume { get; set; }
    public Nullable<Decimal> LastTradePrice { get; set; }
    public Nullable<Decimal> LastTradeVolume { get; set; }
    public Nullable<Decimal> SettlementPrice { get; set; }
    public Nullable<Decimal> OpenInterest { get; set; }
    public Nullable<Decimal> CurrentDayOpenPrice { get; set; }
    public Nullable<Decimal> CurrentDayHighPrice { get; set; }
    public Nullable<Decimal> CurrentDayLowPrice { get; set; }
    public Nullable<Decimal> CurrentDayTotalVolume { get; set; }
    public Nullable<Decimal> CurrentDayTurnover { get; set; }
    public Nullable<Decimal> PrevDayClosePrice { get; set; }
    public Nullable<Decimal> PrevDayTotalVolume { get; set; }
    public Nullable<Decimal> Last24OpenPrice { get; set; }
    public Nullable<Decimal> Last24HighPrice { get; set; }
    public Nullable<Decimal> Last24LowPrice { get; set; }
    public Nullable<Decimal> Last24TotalVolume { get; set; }
    public Nullable<Decimal> Last24Turnover { get; set; }
    public Nullable<Decimal> MarkPrice { get; set; }
    public Nullable<Decimal> FundingRate { get; set; }
    public Nullable<DateTimeOffset> NextFundingTime { get; set; }
TYPE ATAS.DataFeedsCore.SecurityTradingOptions
    public TimeInForce TimeInForce { get; set; }
    public TriggerPriceType TriggerPriceTypes { get; set; }
TYPE ATAS.DataFeedsCore.TradingOptionsSecurity
    public Int64 TradingOptionsId { get; set; }
    public String Security { get; set; }
TYPE ATAS.Indicators.IInstrumentInfo
    public String Instrument { get; }
    public String Exchange { get; }
    public Decimal TickSize { get; }
    public Int32 TimeZone { get; }
TYPE ATAS.Indicators.InstrumentInfo
    public String Instrument { get; }
    public String Exchange { get; }
    public Decimal TickSize { get; }
    public Int32 TimeZone { get; set; }
TYPE Advanced_Time_And_Sales.IInstrument
    public Int32 WindowsUsedDom { get; set; }
    public DomManager Dom { get; }
    public Exchange Exchange { get; set; }
    public Exchange CoreExchange { get; }
    public Int32 Digits { get; }
    public Boolean IsRussianInstrument { get; }
    public String Name { get; }
    public SecType SecurityType { get; }
    public SymbolType Type { get; }
    public String Class { get; }
    public Int32 DefaultScale { get; }
    public String UniqueName { get; }
    public String UniqueClass { get; }
    public Double TickSize { get; }
    public Decimal TickCost { get; }
    public Boolean IsUSBond { get; }
    public Boolean IsCrypto { get; }
    public Boolean CanBeConvertedToUSD { get; }
TYPE OFT.Binance.BinanceFuturesSecurityTradingOptions
    public TimeInForce TimeInForce { get; }
    public TriggerPriceType TriggerPriceTypes { get; }
TYPE OFT.Binance.BinanceSpotSecurityTradingOptions
    public TimeInForce TimeInForce { get; }
    public TriggerPriceType TriggerPriceTypes { get; }
TYPE OFT.Bitget.Common.BitgetSecurityTradingOptions
    public TimeInForce TimeInForce { get; }
    public TriggerPriceType TriggerPriceTypes { get; }
TYPE OFT.Bitget.Models.IBitgetInstrument
    public String Symbol { get; set; }
    public String BaseCoin { get; set; }
    public String QuoteCoin { get; set; }
    public String TakerFeeRate { get; set; }
    public String MakerFeeRate { get; set; }
    public String InstType { get; set; }
    public String Status { get; set; }
TYPE OFT.Bitget.RestMessages.BitgetContractInstrument
    public String Symbol { get; set; }
    public String BaseCoin { get; set; }
    public String QuoteCoin { get; set; }
    public String BuyLimitPriceRatio { get; set; }
    public String SellLimitPriceRatio { get; set; }
    public String FeeRateUpRatio { get; set; }
    public String MakerFeeRate { get; set; }
    public String TakerFeeRate { get; set; }
    public String OpenCostUpRatio { get; set; }
    public String[] SupportMarginCoins { get; set; }
    public String MinTradeNum { get; set; }
    public String PriceEndStep { get; set; }
    public String VolumePlace { get; set; }
    public String PricePlace { get; set; }
    public String SizeMultiplier { get; set; }
    public String SymbolType { get; set; }
    public String MinTradeUSDT { get; set; }
    public String MaxSymbolOrderNum { get; set; }
    public String MaxProductOrderNum { get; set; }
    public String MaxPositionNum { get; set; }
    public String Status { get; set; }
    public String OffTime { get; set; }
    public String LimitOpenTime { get; set; }
    public String DeliveryTime { get; set; }
    public String DeliveryStartTime { get; set; }
```


## 6 · Configuration / UI attributes ATAS actually uses

If ATAS ships its own attribute types, the adapter's settings should use those
rather than the framework ones.

```
ATAS.Indicators.ParameterAttribute
ATAS.Types.CryptoParameterAttribute
Advanced_Time_And_Sales.EnumDisplayNameAttribute
Microsoft.CodeAnalysis.EmbeddedAttribute
Microsoft.CodeAnalysis.EmbeddedAttribute
OFT.Attributes.Editors.CheckEditorAttribute
OFT.Attributes.Editors.ComboBoxEditorAttribute
OFT.Attributes.Editors.DataSeriesEditorAttribute
OFT.Attributes.Editors.IsExpandedAttribute
OFT.Attributes.Editors.MaskAttribute
OFT.Attributes.Editors.NumericEditorAttribute
OFT.Attributes.Editors.PostValueModeAttribute
OFT.Attributes.Editors.SelectDirectoryEditorAttribute
OFT.Attributes.Editors.SelectFileEditorAttribute
OFT.Attributes.Editors.SoundComboBoxEditorAttribute
OFT.Attributes.Editors.TextEditorAttribute
OFT.Attributes.FeatureIdAttribute
OFT.Attributes.HelpLinkAttribute
OFT.Attributes.IgnoreCloneAttribute
OFT.Attributes.LogoAttribute
OFT.Attributes.MappingAttribute
OFT.Attributes.ParameterAttribute
OFT.Attributes.ReferralLinkAttribute
OFT.Platform.DrawingObjects.ForceSetAttribute
OFT.Platform.DrawingObjects.IgnoreOnResetAttribute
OFT.Platform.Validation.RangeDateAttribute
OFT.Sbe.IR.Generated.MetaAttribute
OFT.Sbe.Messages.MetaAttribute
OFT.SourceGenerators.GenerateToStringAttribute
System.Runtime.CompilerServices.RefSafetyRulesAttribute
TradeRouting2.AccountExtraAttribute
UserAttribute2.UserAttribute
Utils.Common.Attributes.FeatureIdAttribute
Utils.Common.Attributes.HelpLinkAttribute
Utils.Common.Attributes.ParameterAttribute
Utils.Common.Attributes.TemplatePropertyAttribute
Utils.Common.Localization.EnumOrderAttribute
Utils.Common.Localization.LocalizedCategoryAttribute
Utils.Common.Localization.LocalizedDescriptionAttribute
Utils.Common.Localization.LocalizedDisplayNameAttribute
Utils.Common.Serialization.ImmutableTypeNameAttribute
Utils.Common.Serialization.NewtonsoftJsonSerializer+EnumFallbackValueAttribute
vc.cppcli.attributes.?A0x04b97c95.CppInlineNamespaceAttribute
vc.cppcli.attributes.?A0x88c8ed9d.CppInlineNamespaceAttribute
vc.cppcli.attributes.?A0x91c65320.CppInlineNamespaceAttribute
vc.cppcli.attributes.?A0x9c83a709.CppInlineNamespaceAttribute
vc.cppcli.attributes.?A0xbdbc257b.CppInlineNamespaceAttribute
vc.cppcli.attributes.?A0xdf32f105.CppInlineNamespaceAttribute
vc.cppcli.attributes.?A0xeb4f7d22.CppInlineNamespaceAttribute
```


## 7 · Keyword sweep — trade and depth entry points

**The most important section.** A named lookup can only confirm or deny the
adapter's current guess. This sweep lists every method and event across the
ATAS types whose name relates to trades or depth, so the correct entry point
is visible even when the assumed name is wrong.

```
TYPE ATAS.DataFeedsCore.AsyncConnector`2
    protected Void ProcessMyTrade(Int64 extId, String tradeId, Action<MyTrade> update)
    protected Void ProcessMyTrade(String orderId, String tradeId, Action<MyTrade> update)
TYPE ATAS.DataFeedsCore.BaseConnector`4
    event ConnectorEventHandler<IEnumerable<MarketDepth>> MarketDepthsUpdate
    event ConnectorEventHandler<IEnumerable<MyTrade>> NewMyTrades
    event ConnectorEventHandler<IEnumerable<Trade>> NewTrades
    event ConnectorEventHandler<MarketDepth> BestBidAskUpdates
    protected Void ProcessBestBidAsk(MarketDepth marketDepth)
    protected Void ProcessBestBidAsk(Security security, DateTime time, MarketDataType type, Decimal price, Decimal volume, String ecn)
    protected Void ProcessBestBidAsk(TSecurityKey securityId, DateTime time, MarketDataType type, Decimal price, Decimal volume, String ecn)
    protected Void ProcessBestBidAsk(TSecurityKey securityId, MarketDepth marketDepth)
    protected Void ProcessMarketDepths(Security security, Func<Security, ICollection<MarketDepth>> action)
    protected Void ProcessMarketDepths(TSecurityKey securityId, Func<Security, ICollection<MarketDepth>> action)
    protected Void ProcessMarketDepthsReset(TSecurityKey securityId, ICollection<MarketDepth> depth)
    protected Void ProcessMyTrade(Int64 extId, String orderId, String tradeId, T message, Action<T, MyTrade> update)
    protected Void ProcessMyTrade(Int64 extId, String tradeId, T message, Action<T, MyTrade> update)
    protected Void ProcessMyTrade(TPortfolioKey accountId, TSecurityKey securityId, Int64 extId, String orderId, String tradeId, T message, Action<T, MyTrade> update)
    protected Void ProcessTick(Security security, Int64 id, DateTime time, TradeDirection direction, Decimal price, Decimal volume, String ecn, Nullable<Decimal> openInterest)
    protected Void ProcessTick(Security security, Trade trade)
    protected Void ProcessTick(TSecurityKey securityId, Int64 id, DateTime time, TradeDirection direction, Decimal price, Decimal volume, String ecn, Nullable<Decimal> openInterest)
    protected Void ProcessTick(TSecurityKey securityId, Trade trade)
    protected Void ProcessTick(Trade trade)
    protected Void ValidateTradesHistoryRequestDates(ref DateTime& from, DateTime to, Int32 depthLimitMonths)
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
    public sealed override Task<IEnumerable<MyTrade>> GetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE ATAS.DataFeedsCore.BaseConnector`4+#=zU4BDl82OhGw7
    public sealed override MarketDepth CreateMarketDepth()
    public sealed override MyTrade GetOrCreateMyTrade(String #=z7er3$vs=, Func<String, MyTrade> #=zr2y$HMU=)
    public sealed override Trade CreateTrade()
TYPE ATAS.DataFeedsCore.BasketConnector
    event ConnectorEventHandler<IEnumerable<MarketDepth>> MarketDepthsUpdate
    event ConnectorEventHandler<IEnumerable<MyTrade>> NewMyTrades
    event ConnectorEventHandler<IEnumerable<Trade>> NewTrades
    event ConnectorEventHandler<MarketDepth> BestBidAskUpdates
    public Task<IEnumerable<MyTrade>> RecoverTradesAsync(Security security, DateTime from, DateTime to)
    public sealed override Task<IEnumerable<MyTrade>> GetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE ATAS.DataFeedsCore.BasketConnector+#=zzXkILiEkC8NsDZTQQw==
    public sealed override MarketDepth CreateMarketDepth()
    public sealed override MyTrade GetOrCreateMyTrade(String #=z7er3$vs=, Func<String, MyTrade> #=zr2y$HMU=)
    public sealed override Trade CreateTrade()
TYPE ATAS.DataFeedsCore.ConnectorLatencyManager
    public Void ProcessBestBidAsk(Security security, DateTime time)
    public Void ProcessMarketDepthTime(DateTime time)
    public Void ProcessMarketDepths(Security security, DateTime time)
    public Void ProcessTickTime(DateTime time)
    public Void ProcessTrade(Security security, DateTime time)
TYPE ATAS.DataFeedsCore.Database.Cache`1
    public sealed override ICollection<MyTrade> GetMyTrades(String accountId)
    public sealed override IEnumerable<HistoryMyTrade> GetHistoryTrades(DateTime from, DateTime to, IEnumerable<String> accounts, IEnumerable<String> securities)
    public sealed override IEnumerable<MyTrade> GetMyTrades(DateTime from, DateTime to, IEnumerable<String> accounts, IEnumerable<String> securities)
    public sealed override IEnumerable<MyTrade> GetMyTrades(String accountId, Int64 tradeId)
    public sealed override IEnumerable<MyTrade> GetOpenedMyTrades()
    public sealed override MarketDepth CreateMarketDepth()
    public sealed override MyTrade GetOrCreateMyTrade(String tradeId, Func<String, MyTrade> create)
    public sealed override MyTrade TryGetMyTrade(String accountId, String tradeId, Boolean searchInDb)
    public sealed override Trade CreateTrade()
    public sealed override Void ClearHistoryTrades()
    public sealed override Void ClearMyTrades()
TYPE ATAS.DataFeedsCore.Database.Cache`1+<>c
    internal MyTrade[] <GetOpenedMyTrades>b__95_0(TConnection db)
    internal Void <ClearHistoryTrades>b__161_0(TConnection db)
    internal Void <ClearMyTrades>b__162_0(TConnection db)
TYPE ATAS.DataFeedsCore.Database.Cache`1+<>c__DisplayClass101_0
    internal MyTrade <TryGetMyTrade>b__0(TConnection db)
TYPE ATAS.DataFeedsCore.Database.Cache`1+<>c__DisplayClass169_0
    internal Boolean <ProcessTrades>b__0(Order o)
TYPE ATAS.DataFeedsCore.Database.Cache`1+<>c__DisplayClass202_0
    internal MyTrade <GetOrCreateMyTrade>b__0(TConnection db)
TYPE ATAS.DataFeedsCore.Database.Cache`1+<>c__DisplayClass97_0
    internal MyTrade[] <GetMyTrades>b__0(TConnection db)
TYPE ATAS.DataFeedsCore.Database.Cache`1+<>c__DisplayClass98_0
    internal MyTrade[] <GetMyTrades>b__0(TConnection db)
TYPE ATAS.DataFeedsCore.Database.Cache`1+<>c__DisplayClass99_0
    internal HistoryMyTrade[] <GetHistoryTrades>b__0(TConnection db)
TYPE ATAS.DataFeedsCore.Database.Cache`1+CachedDbAccountDataProvider
    public sealed override IEnumerable<MyTrade> GetMyTrades(String account, Func<MyTrade, Boolean> filter)
TYPE ATAS.DataFeedsCore.Database.Cache`1+DbAccountDataProvider
    public sealed override IEnumerable<MyTrade> GetMyTrades(String account, Func<MyTrade, Boolean> filter)
TYPE ATAS.DataFeedsCore.Database.Cache`1+IAccountDataProvider
    public abstract IEnumerable<MyTrade> GetMyTrades(String account, Func<MyTrade, Boolean> filter)
TYPE ATAS.DataFeedsCore.Database.Cache`1+PortfolioCache
    public MyTrade TryGetMyTrade(String tradeId)
TYPE ATAS.DataFeedsCore.Database.Cache`1+PortfolioCache+<>c__DisplayClass20_0
    internal Boolean <TryClearTrades>b__0(MyTrade t)
TYPE ATAS.DataFeedsCore.Database.ICache
    public abstract ICollection<MyTrade> GetMyTrades(String accountId)
    public abstract IEnumerable<HistoryMyTrade> GetHistoryTrades(DateTime from, DateTime to, IEnumerable<String> accounts, IEnumerable<String> securities)
    public abstract IEnumerable<MyTrade> GetMyTrades(DateTime from, DateTime to, IEnumerable<String> accounts, IEnumerable<String> securities)
    public abstract IEnumerable<MyTrade> GetMyTrades(String accountId, Int64 tradeId)
    public abstract IEnumerable<MyTrade> GetOpenedMyTrades()
    public abstract MyTrade TryGetMyTrade(String accountId, String tradeId, Boolean searchInDb)
    public abstract Void ClearHistoryTrades()
    public abstract Void ClearMyTrades()
TYPE ATAS.DataFeedsCore.Exchange
    public ValueTuple<Nullable<DateTime>, Nullable<DateTime>> TrimToMinTradedRange(DateTime from, DateTime to)
TYPE ATAS.DataFeedsCore.IDataFeedConnector
    event ConnectorEventHandler<IEnumerable<MarketDepth>> MarketDepthsUpdate
    event ConnectorEventHandler<IEnumerable<MyTrade>> NewMyTrades
    event ConnectorEventHandler<IEnumerable<Trade>> NewTrades
    event ConnectorEventHandler<MarketDepth> BestBidAskUpdates
    public abstract Task<IEnumerable<MyTrade>> GetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE ATAS.DataFeedsCore.IEntityFactory
    public abstract MarketDepth CreateMarketDepth()
    public abstract MyTrade GetOrCreateMyTrade(String id, Func<String, MyTrade> create)
    public abstract Trade CreateTrade()
TYPE ATAS.DataFeedsCore.ISecurityPositionManager
    public abstract Boolean UpdateAveragePriceByTrades()
TYPE ATAS.DataFeedsCore.OrderExtendedOptions
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.DataFeedsCore.PositionTradesQueue
    public Void AddTrade(Boolean isBuy, Decimal price, Decimal volume)
    public Void AddTrade(Decimal price, Decimal volume)
TYPE ATAS.DataFeedsCore.RiskInfo
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.DataFeedsCore.SecurityPositionManager
    public sealed override Boolean UpdateAveragePriceByTrades()
TYPE ATAS.DataFeedsCore.Statistics.TradingStatistics
    public Void ClearHistoryMyTrades()
TYPE ATAS.DataFeedsCore.TradeStatistics.Matching.TradesMatchingProcessor
    event Action<HistoryMyTrade> NewTrade
TYPE #=zV4_uGdtZfm3OdkVfTJlfJE0_$sxm0Q$aH3ZTfC_kWwwW
    protected virtual Void MarketDepthChanged(MarketDataArg #=zFakBoU0=)
    protected virtual Void OnNewTrades(IEnumerable<MarketDataArg> #=zklMsX$MbyEfF)
TYPE ATAS.Indicators.Technical.AdaptiveBigTrades
    protected virtual Void OnCumulativeTrade(CumulativeTrade bigTrade)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.BigTrades
    protected virtual Void OnCumulativeTrade(CumulativeTrade bigTrade)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade bigTrade)
TYPE ATAS.Indicators.Technical.DOM_Reversal
    protected virtual Void MarketDepthChanged(MarketDataArg arg)
TYPE ATAS.Indicators.Technical.DomLevels
    protected virtual Void MarketDepthChanged(MarketDataArg arg)
    protected virtual Void OnBestBidAskChanged(MarketDataArg depth)
    protected virtual Void OnNewTrade(MarketDataArg trade)
TYPE ATAS.Indicators.Technical.DomLevels+DataCollector
    public Void ProcessBestBidAsk(MarketDataArg depth, Int32 bar)
    public Void ProcessMarketDepth(MarketDataArg arg, Int32 bar, Boolean fixMaxValues)
TYPE ATAS.Indicators.Technical.DomLevels+DataCollector+PriceSeries
    public Void ProcessMarketDepth(MarketDataArg arg, Int32 bar, Boolean fixMaxValues)
TYPE ATAS.Indicators.Technical.GreeksCalculator
    public Void ProcessTick(Decimal optionPrice, Decimal underlyingPrice)
TYPE ATAS.Indicators.Technical.MoonSoon
    protected virtual Void OnNewTrade(MarketDataArg arg)
TYPE ATAS.Indicators.Technical.Scalper_Levels
    protected virtual Void OnCumulativeTrade(CumulativeTrade arg)
TYPE ATAS.Indicators.Technical.TradesTrack
    protected virtual Void OnNewTrade(MarketDataArg trade)
TYPE ATAS.Indicators.Technical.TradingDepthOfMarket
    protected virtual Void MarketDepthChanged(MarketDataArg arg)
    protected virtual Void OnBestBidAskChanged(MarketDataArg depth)
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnNewTrades(IEnumerable<MarketDataArg> trades)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.TradingPanel
    protected virtual Void OnBestBidAskChanged(MarketDataArg depth)
TYPE DomV10.MainIndicator
    protected virtual Void MarketDepthChanged(MarketDataArg depth)
    protected virtual Void OnBestBidAskChanged(MarketDataArg depth)
    protected virtual Void OnNewTrade(MarketDataArg trade)
TYPE DomV10.RowItem
    public Void UpdateTrade(MarketDataArg trade)
TYPE DomV10.MboGridController
    public Void Tick()
    public Void UpdateTrade(MarketDataArg trade)
TYPE ATAS.Indicators.Technical.ActiveVolume
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.ClusterSearch
    protected virtual Void OnNewTrade(MarketDataArg trade)
TYPE ATAS.Indicators.Technical.DOM
    protected virtual Void MarketDepthChanged(MarketDataArg depth)
    protected virtual Void OnBestBidAskChanged(MarketDataArg depth)
TYPE ATAS.Indicators.Technical.DomPower
    protected virtual Void MarketDepthChanged(MarketDataArg depth)
TYPE ATAS.Indicators.Technical.DomStrength
    protected virtual Void MarketDepthChanged(MarketDataArg depth)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnNewTrade(MarketDataArg trade)
TYPE ATAS.Indicators.Technical.DynamicLevels
    protected virtual Void OnNewTrades(IEnumerable<MarketDataArg> trades)
TYPE ATAS.Indicators.Technical.MarketPower
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnNewTrade(MarketDataArg trade)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.MultiMarketPower
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnNewTrade(MarketDataArg trade)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.OIAnalyzer
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.OrderBookAlerts
    protected virtual Void MarketDepthChanged(MarketDataArg depth)
TYPE ATAS.Indicators.Technical.OrderFlow
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnNewTrade(MarketDataArg trade)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.SpreadVolume
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.TapePattern
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnNewTrade(MarketDataArg trade)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.Technical.ActiveVolume+<>c
    internal Decimal <OnUpdateCumulativeTrade>b__77_0(Decimal _)
    internal Decimal <OnUpdateCumulativeTrade>b__77_1(Decimal _)
TYPE ATAS.Indicators.Technical.ClusterSearch+<>c__DisplayClass49_0
    internal PriceVolumeInfo <CalculateTick>b__0()
TYPE ATAS.Indicators.Technical.DOM+<>c
    internal Decimal <MarketDepthChanged>b__111_0(MarketDataArg x)
TYPE ATAS.Indicators.Technical.DOM+<>c__DisplayClass111_0
    internal Boolean <MarketDepthChanged>b__1(KeyValuePair<Decimal, Decimal> x)
    internal Boolean <MarketDepthChanged>b__2(KeyValuePair<Decimal, MarketDataArg> x)
    internal Boolean <MarketDepthChanged>b__3(KeyValuePair<Decimal, Decimal> x)
    internal Boolean <MarketDepthChanged>b__4(KeyValuePair<Decimal, MarketDataArg> x)
TYPE ATAS.Indicators.Technical.DomStrength+<>c
    internal IEnumerable<MarketDataArg> <OnCumulativeTradesResponse>b__53_0(CumulativeTrade x)
TYPE ATAS.Indicators.Technical.DynamicLevels+DynamicCandle
    public Void AddTick(MarketDataArg tick)
TYPE ATAS.Indicators.Technical.MarketPower+<>c
    internal Boolean <OnCumulativeTradesResponse>b__64_0(CumulativeTrade t)
    internal Decimal <CalculateBarTrades>b__72_1(CumulativeTrade x)
    internal Decimal <CalculateBarTrades>b__72_4(MarketDataArg x)
    internal IEnumerable<MarketDataArg> <CalculateBarTrades>b__72_2(CumulativeTrade x)
TYPE ATAS.Indicators.Technical.MultiMarketPower+<>c
    internal Decimal <CalculateBarTrades>b__130_1(CumulativeTrade x)
    internal Decimal <CalculateBarTrades>b__130_3(CumulativeTrade x)
    internal Decimal <CalculateBarTrades>b__130_5(CumulativeTrade x)
    internal Decimal <CalculateBarTrades>b__130_7(CumulativeTrade x)
    internal Decimal <CalculateBarTrades>b__130_9(CumulativeTrade x)
TYPE ATAS.Indicators.Technical.OIAnalyzer+<>c
    internal DateTime <OnCumulativeTradesResponse>b__74_0(CumulativeTrade x)
TYPE ATAS.Indicators.Technical.OIAnalyzer+<>c__DisplayClass74_0
    internal Boolean <OnCumulativeTradesResponse>b__1(CumulativeTrade x)
TYPE ATAS.Indicators.Technical.OrderBookAlerts+<>c__DisplayClass47_0
    internal Boolean <MarketDepthChanged>b__0(PriceInfo p)
    internal Boolean <MarketDepthChanged>b__1(PriceInfo p)
TYPE ATAS.Indicators.Technical.OrderFlow+<>c
    internal Boolean <OnUpdateCumulativeTrade>b__99_0(CumulativeTrade x)
TYPE ATAS.Indicators.Technical.TapePattern+<>c
    internal DateTime <GetTradesHistory>b__154_0(CumulativeTrade x)
    internal Decimal <GetCumTradeExtended>b__158_0(MarketDataArg t)
    internal Decimal <ProcessCumulativeTickExtended>b__155_4(Decimal x)
TYPE ATAS.Indicators.Technical.TapePattern+<>c__DisplayClass152_0
    internal Boolean <ProcessTick>b__0(PriceSelectionValue x)
    internal Void <ProcessTick>b__1()
TYPE ATAS.Indicators.Technical.TapePattern+<>c__DisplayClass155_0
    internal Boolean <ProcessCumulativeTickExtended>b__0(Decimal x)
    internal Boolean <ProcessCumulativeTickExtended>b__1(Decimal x)
    internal Boolean <ProcessCumulativeTickExtended>b__3(PriceSelectionValue x)
    internal Void <ProcessCumulativeTickExtended>b__2()
TYPE ATAS.Indicators.ExtendedIndicator
    protected IMarketByOrdersWithTradesCache GetMarketByOrdersWithTradesCache(TimeSpan period)
    protected ITradesCache GetTradesCache(TimeSpan period)
    protected Void RequestForCumulativeTrades(CumulativeTradesRequest request)
    protected virtual Void MarketDepthChanged(MarketDataArg depth)
    protected virtual Void MarketDepthsChanged(IEnumerable<MarketDataArg> depths)
    protected virtual Void OnBestBidAskChanged(MarketDataArg depth)
    protected virtual Void OnCumulativeTrade(CumulativeTrade trade)
    protected virtual Void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
    protected virtual Void OnNewMyTrade(MyTrade myTrade)
    protected virtual Void OnNewTrade(MarketDataArg trade)
    protected virtual Void OnNewTrades(IEnumerable<MarketDataArg> trades)
    protected virtual Void OnUpdateCumulativeTrade(CumulativeTrade trade)
TYPE ATAS.Indicators.IMarketDepthInfoProvider
    public abstract IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
TYPE ATAS.Indicators.IOnlineDataProvider
    event Action<CumulativeTrade> NewCumulativeTrade
    event Action<CumulativeTrade> UpdateCumulativeTrade
    event Action<CumulativeTradesRequest, IEnumerable<CumulativeTrade>> HistoricalCumulativeTrades
    event Action<IEnumerable<MarketDataArg>> MarketDepthsChanged
    event Action<IEnumerable<MarketDataArg>> NewTrades
    event Action<MarketDataArg> BestBidAskChanged
    public abstract IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
    public abstract IMarketByOrdersWithTradesCache GetMarketByOrdersWithTradesCache(TimeSpan period)
    public abstract ITradesCache GetTradesCache(TimeSpan period)
    public abstract Task<IEnumerable<MarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotRequest request, CancellationToken cancellation)
    public abstract Void RequestCumulativeTrades(CumulativeTradesRequest request)
TYPE ATAS.Indicators.ITradingManager
    event Action<MyTrade> NewMyTrade
TYPE ATAS.Indicators.Indicator
    protected IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
TYPE ATAS.Indicators.MarketDepthInfoProvider
    public sealed override IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
TYPE ATAS.Indicators.TradingSessionDescription
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Strategies.ATM.BaseStopProfitSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Strategies.ATM.BreakevenSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Strategies.ATM.MultipleStopProfit
    protected virtual Void OnNewTrade(Trade trade)
TYPE ATAS.Strategies.ATM.MultipleStopProfitLevel
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Strategies.ATM.MultipleStopProfitSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Strategies.ATM.SimpleStopProfitSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Strategies.ATM.StopProfitSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Strategies.ATM.TrailingStopSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Strategies.Chart.ChartStrategy
    protected virtual Void OnBestBidAskChanged(MarketDataArg depth)
    protected virtual Void OnNewMyTrade(MyTrade myTrade)
TYPE ATAS.Strategies.Strategy
    protected ICollection<MyTrade> FilterMyTrades(IEnumerable<MyTrade> trades)
    protected virtual Void OnBestBidAsk(MarketDepth depth)
    protected virtual Void OnMarketDepth(IEnumerable<MarketDepth> depths)
    protected virtual Void OnNewMyTrade(MyTrade myTrade)
    protected virtual Void OnNewTrade(Trade trade)
TYPE ATAS.Strategies.StrategyStateDescription
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE ATAS.Types.BigTrade
    public Void AddTick(Tick t)
TYPE ATAS.Types.Candle
    public Void CalculateTick(Tick t)
TYPE ATAS.Types.DomManager
    event Action<ATASMarketDepthEventArgs> BestBidAskUpdate
    public Void SetBestBidAsk(ATASMarketDepthEventArgs value)
TYPE ATAS.Types.IDomManager
    event Action<ATASMarketDepthEventArgs> BestBidAskUpdate
TYPE ATAS.Types.ScaleDomManager
    event Action<ATASMarketDepthEventArgs> BestBidAskUpdate
TYPE ATAS.Types.SimpleBigTrade
    public Void AddTick(Tick t)
TYPE Advanced_Time_And_Sales.IInstrument
    event Action<IEnumerable<Tick>> OnNewTick
TYPE OFT.Binance.BinanceConnector
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
    public Task<IEnumerable<Trade>> GetHistoricalTrades(Security security, DateTime from, DateTime to)
TYPE OFT.Binance.BinanceFuturesMarketOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Binance.BinanceFuturesOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Binance.BinanceSpotOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Binance.Common.BinanceRiskInfo
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE #=zInKhAiM2gxIgOh4uzy973kmOXV8icVhPALyWhuY=
    protected virtual Boolean PrintMembers(StringBuilder #=ztvbTEZk=)
TYPE OFT.Bitget.BitgetConnector
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE OFT.Bitget.Common.BitgetLimitOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Bitget.Common.BitgetOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Bitmex.BitmexOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Bitmex.WebSocket.V1.Book.DepthBuilder+Depth+<>c__DisplayClass17_0
    internal MarketDepth <ToMarketDepth>b__0(Depth d)
TYPE OFT.Bybit.BybitConnector
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
    public Task<IEnumerable<Trade>> GetTickHistory(Security security, DateTime from, DateTime to)
TYPE OFT.Bybit.Common.BybitRiskInfo
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Bybit.Models.BybitConditionalLimitOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Bybit.Models.BybitConditionalMarketOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Bybit.Models.BybitLimitOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Bybit.Models.BybitWsDom
    public ValueTuple<MarketDepth[], MarketDepth[]> ToMarketDepth(Nullable<DateTime> time)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.BaseColumn
    public virtual Void BestBidAskUpdates(ATASMarketDepthEventArgs obj)
    public virtual Void MarketDepthChanged(IEnumerable<ATASMarketDepthEventArgs> obj)
    public virtual Void NewBigTrade(BigTrade trade)
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
    public virtual Void UpdateBigTrade(BigTrade trade)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.BothLimits
    public virtual Void BestBidAskUpdates(ATASMarketDepthEventArgs obj)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.BuySellColumn
    public Void ClearTrades()
    public virtual Void BestBidAskUpdates(ATASMarketDepthEventArgs obj)
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.DomChanges
    public virtual Void BestBidAskUpdates(ATASMarketDepthEventArgs obj)
    public virtual Void MarketDepthChanged(IEnumerable<ATASMarketDepthEventArgs> obj)
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.HeatMap
    public virtual Void BestBidAskUpdates(ATASMarketDepthEventArgs obj)
    public virtual Void MarketDepthChanged(IEnumerable<ATASMarketDepthEventArgs> obj)
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.HistogramColumn
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.IColumn
    public abstract Void BestBidAskUpdates(ATASMarketDepthEventArgs obj)
    public abstract Void MarketDepthChanged(IEnumerable<ATASMarketDepthEventArgs> obj)
    public abstract Void NewBigTrade(BigTrade trade)
    public abstract Void NewTrades(IEnumerable<Tick> ticks)
    public abstract Void UpdateBigTrade(BigTrade trade)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.InformationalColumn
    public virtual Void BestBidAskUpdates(ATASMarketDepthEventArgs obj)
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.LatestTradeQuantityColumn
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.Limits
    protected virtual TradeAction GetTradeAction(Boolean isLeftMouseBtn)
    public virtual Void MarketDepthChanged(IEnumerable<ATASMarketDepthEventArgs> obj)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.NotesColumn
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
TYPE Advanced_Time_And_Sales.TradingModule.Columns.OrderFlow
    public virtual Void NewTrades(IEnumerable<Tick> ticks)
TYPE OFT.Controls.SmartDom.Columns.TradableColumn
    protected virtual TradeAction GetTradeAction(Boolean isLeftMouseBtn)
TYPE OFT.Controls.SmartDom.IDomControl
    public abstract Void CallClearAllCurrentTrades()
TYPE OFT.Controls.SmartDom.SmartDOMControl
    public Void CallClearAllTrades()
    public Void CallClearAskCurrentTrades()
    public Void CallClearBidCurrentTrades()
    public Void ConnectorBestBidAskUpdates(ATASMarketDepthEventArgs obj)
    public Void MarketDepthChanged(IEnumerable<ATASMarketDepthEventArgs> obj)
    public Void NewBigTrade(BigTrade trade)
    public Void NewTrades(IEnumerable<Tick> obj)
    public Void UpdateBigTrade(BigTrade trade)
    public sealed override Void CallClearAllCurrentTrades()
TYPE OFT.Core.DataProvider.BaseCachedHistoryMarketDataProvider
    protected abstract Task<IEnumerable<BigTrade>> OnGetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract Task<IEnumerable<IMarketDepthSnapshot>> OnGetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract Task<IEnumerable<MarketDepth>> OnGetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract Task<IEnumerable<Trade>> OnGetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract Task<IEnumerable<Trade>> OnGetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract Task<Trade> OnGetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override Task<IEnumerable<BigTrade>> GetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override Task<IEnumerable<IMarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override Task<IEnumerable<MarketDepth>> GetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override Task<IEnumerable<Trade>> GetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override Task<IEnumerable<Trade>> GetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override Task<Trade> GetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.DataProvider.BaseHistoryMarketDataProvider
    protected abstract IAsyncEnumerable<BigTrade> OnGetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract IAsyncEnumerable<IMarketDepthSnapshot> OnGetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract IAsyncEnumerable<MarketDepth> OnGetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract IAsyncEnumerable<Trade> OnGetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract IAsyncEnumerable<Trade> OnGetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    protected abstract Task<Trade> OnGetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override IAsyncEnumerable<BigTrade> GetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override IAsyncEnumerable<IMarketDepthSnapshot> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override IAsyncEnumerable<MarketDepth> GetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override IAsyncEnumerable<Trade> GetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override IAsyncEnumerable<Trade> GetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    public sealed override Task<Trade> GetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.DataProvider.CachedHistoryMarketDataProvider
    protected virtual Task<IEnumerable<BigTrade>> OnGetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<IEnumerable<IMarketDepthSnapshot>> OnGetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<IEnumerable<MarketDepth>> OnGetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<IEnumerable<Trade>> OnGetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<IEnumerable<Trade>> OnGetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<Trade> OnGetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.DataProvider.CustomSessionMarketDataProvider
    protected virtual Task<IEnumerable<IMarketDepthSnapshot>> OnGetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.DataProvider.ICachedHistoryMarketDataProvider
    public abstract Task<IEnumerable<BigTrade>> GetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract Task<IEnumerable<IMarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract Task<IEnumerable<MarketDepth>> GetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract Task<IEnumerable<Trade>> GetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract Task<IEnumerable<Trade>> GetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract Task<Trade> GetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.DataProvider.IHistoryMarketDataProvider
    public abstract IAsyncEnumerable<BigTrade> GetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract IAsyncEnumerable<IMarketDepthSnapshot> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract IAsyncEnumerable<MarketDepth> GetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract IAsyncEnumerable<Trade> GetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract IAsyncEnumerable<Trade> GetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    public abstract Task<Trade> GetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.DataProvider.ProxyCachedHistoryMarketDataProvider
    protected virtual Task<IEnumerable<BigTrade>> OnGetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<IEnumerable<IMarketDepthSnapshot>> OnGetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<IEnumerable<MarketDepth>> OnGetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<IEnumerable<Trade>> OnGetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<IEnumerable<Trade>> OnGetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<Trade> OnGetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.DataProvider.ProxyMarketDataProvider
    protected virtual IAsyncEnumerable<BigTrade> OnGetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual IAsyncEnumerable<IMarketDepthSnapshot> OnGetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual IAsyncEnumerable<MarketDepth> OnGetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual IAsyncEnumerable<Trade> OnGetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual IAsyncEnumerable<Trade> OnGetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<Trade> OnGetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.DataProvider.ServerHistoryMarketDataProvider
    protected virtual IAsyncEnumerable<BigTrade> OnGetBigTradesAsync(BigTradesMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual IAsyncEnumerable<IMarketDepthSnapshot> OnGetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual IAsyncEnumerable<MarketDepth> OnGetMarketDepthsAsync(MarketDepthsRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual IAsyncEnumerable<Trade> OnGetTradesAsync(TradesRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual IAsyncEnumerable<Trade> OnGetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
    protected virtual Task<Trade> OnGetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Core.Models.BigTrade
    public Void AddTrade(Trade trade)
TYPE OFT.Core.Server.PlatformTariff
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Core.Storage.CachedStorageProvider
    public sealed override IEntityStorage<BigTrade> GetAgressiveTradeStorage(Contract contract, Int32 luft, TimeSpan min, TimeSpan max)
    public sealed override IEntityStorage<BigTrade> GetBigTradeStorage(Contract contract, Nullable<Decimal> minVolume, Nullable<Decimal> maxVolume)
    public sealed override IEntityStorage<IMarketDepthSnapshot> GetMarketDepthSnapshotStorage(Contract contract, String key)
    public sealed override IEntityStorage<IMarketDepthSnapshot> GetMarketDepthSnapshotStorage(Contract contract, TimeSpan period)
    public sealed override IEntityStorage<MarketDepth> GetMarketDepthStorage(Contract contract)
    public sealed override IEntityStorage<Trade> GetTradeStorage(Contract contract)
TYPE OFT.Core.Storage.IStorageProvider
    public abstract IEntityStorage<BigTrade> GetAgressiveTradeStorage(Contract contract, Int32 luft, TimeSpan min, TimeSpan max)
    public abstract IEntityStorage<BigTrade> GetBigTradeStorage(Contract contract, Nullable<Decimal> minVolume, Nullable<Decimal> maxVolume)
    public abstract IEntityStorage<IMarketDepthSnapshot> GetMarketDepthSnapshotStorage(Contract contract, String key)
    public abstract IEntityStorage<IMarketDepthSnapshot> GetMarketDepthSnapshotStorage(Contract contract, TimeSpan period)
    public abstract IEntityStorage<MarketDepth> GetMarketDepthStorage(Contract contract)
    public abstract IEntityStorage<Trade> GetTradeStorage(Contract contract)
TYPE OFT.Core.Storage.StorageProvider
    public sealed override IEntityStorage<BigTrade> GetAgressiveTradeStorage(Contract contract, Int32 luft, TimeSpan min, TimeSpan max)
    public sealed override IEntityStorage<BigTrade> GetBigTradeStorage(Contract contract, Nullable<Decimal> minVolume, Nullable<Decimal> maxVolume)
    public sealed override IEntityStorage<IMarketDepthSnapshot> GetMarketDepthSnapshotStorage(Contract contract, String key)
    public sealed override IEntityStorage<IMarketDepthSnapshot> GetMarketDepthSnapshotStorage(Contract contract, TimeSpan period)
    public sealed override IEntityStorage<MarketDepth> GetMarketDepthStorage(Contract contract)
    public sealed override IEntityStorage<Trade> GetTradeStorage(Contract contract)
TYPE Historical2.ConstantVolumeBar
    public Boolean ShouldSerializeTickVolume()
    public Boolean ShouldSerializeTradeDate()
    public Void ResetTickVolume()
    public Void ResetTradeDate()
TYPE Historical2.ConstantVolumeBarParameters
    public Boolean ShouldSerializeUseFlatTicks()
    public Boolean ShouldSerializeUseTickVolume()
    public Void ResetUseFlatTicks()
    public Void ResetUseTickVolume()
TYPE Historical2.PointAndFigureBar
    public Boolean ShouldSerializeTickVolume()
    public Boolean ShouldSerializeTradeDate()
    public Void ResetTickVolume()
    public Void ResetTradeDate()
TYPE Historical2.RangeBar
    public Boolean ShouldSerializeTickVolume()
    public Boolean ShouldSerializeTradeDate()
    public Void ResetTickVolume()
    public Void ResetTradeDate()
TYPE Historical2.RenkoBar
    public Boolean ShouldSerializeTickVolume()
    public Boolean ShouldSerializeTradeDate()
    public Void ResetTickVolume()
    public Void ResetTradeDate()
TYPE Historical2.TickBar
    public Boolean ShouldSerializeTradeDate()
    public Void ResetTradeDate()
TYPE Historical2.TickBarParameters
    public Boolean ShouldSerializeUseFlatTicks()
    public Void ResetUseFlatTicks()
TYPE Historical2.TimeAndSalesParameters
    public Boolean ShouldSerializeIncludeOffMarketTrades()
    public Boolean ShouldSerializeIncludeTradeAttributes()
    public Void ResetIncludeOffMarketTrades()
    public Void ResetIncludeTradeAttributes()
TYPE Historical2.TimeAndSalesReport
    public Boolean ShouldSerializeOffMarketTradesIncluded()
    public Boolean ShouldSerializeTradeAttributesIncluded()
    public Void ResetOffMarketTradesIncluded()
    public Void ResetTradeAttributesIncluded()
TYPE Historical2.TimeBar
    public Boolean ShouldSerializeCommodityTickVolume()
    public Boolean ShouldSerializeTickVolume()
    public Boolean ShouldSerializeTradeDate()
    public Void ResetCommodityTickVolume()
    public Void ResetTickVolume()
    public Void ResetTradeDate()
TYPE Historical2.VolumeProfileItem
    public Boolean ShouldSerializeTickVolume()
    public Void ResetTickVolume()
TYPE Historical2.VolumeProfileLastQuotesCumulativeStatistics
    public Boolean ShouldSerializeAskTradeVolume()
    public Boolean ShouldSerializeBidTradeVolume()
    public Boolean ShouldSerializeScaledAskTradeVolume()
    public Boolean ShouldSerializeScaledBidTradeVolume()
    public Void ResetAskTradeVolume()
    public Void ResetBidTradeVolume()
    public Void ResetScaledAskTradeVolume()
    public Void ResetScaledBidTradeVolume()
TYPE MarketData2.MarketDataSubscription
    public Boolean ShouldSerializeIncludeOffMarketTrades()
    public Boolean ShouldSerializeIncludeTradeAttributes()
    public Void ResetIncludeOffMarketTrades()
    public Void ResetIncludeTradeAttributes()
TYPE MarketData2.MarketDataSubscriptionStatus
    public Boolean ShouldSerializeOffMarketTradesIncluded()
    public Boolean ShouldSerializeTradeAttributesIncluded()
    public Void ResetOffMarketTradesIncluded()
    public Void ResetTradeAttributesIncluded()
TYPE MarketData2.MarketValues
    public Boolean ShouldSerializeScaledLastTradePrice()
    public Boolean ShouldSerializeTickVolume()
    public Void ResetScaledLastTradePrice()
    public Void ResetTickVolume()
TYPE MarketData2.RealTimeMarketData
    public Boolean ShouldSerializeQuotesTradeDate()
    public Void ResetQuotesTradeDate()
TYPE MarketData2.TradeAttributes
    public Boolean ShouldSerializeTradeType()
    public Void ResetTradeType()
TYPE Metadata2.ContractMetadata
    public Boolean ShouldSerializeExpectOffTickPrices()
    public Void ResetExpectOffTickPrices()
TYPE Metadata2.ProcessingMetadata
    public Boolean ShouldSerializeTickSize()
    public Boolean ShouldSerializeTickValue()
    public Void ResetTickSize()
    public Void ResetTickValue()
TYPE Metadata2.SecurityMetadata
    public Boolean ShouldSerializeTickSize()
    public Boolean ShouldSerializeTickValue()
    public Void ResetTickSize()
    public Void ResetTickValue()
TYPE OFT.Cqg.CqgConnector
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE Order2.CompoundOrder
    public Boolean ShouldSerializeLossTickOffset()
    public Boolean ShouldSerializeProfitTickOffset()
    public Boolean ShouldSerializeStopLimitTickOffset()
    public Void ResetLossTickOffset()
    public Void ResetProfitTickOffset()
    public Void ResetStopLimitTickOffset()
TYPE Order2.LegAllocation
    public Boolean ShouldSerializeTradeMatchId()
    public Void ResetTradeMatchId()
TYPE Order2.Trade
    public Boolean ShouldSerializeTradeCounterparty()
    public Boolean ShouldSerializeTradeMatchId()
    public Boolean ShouldSerializeTradeUtcTime()
    public Void ResetTradeCounterparty()
    public Void ResetTradeMatchId()
    public Void ResetTradeUtcTime()
TYPE Order2.TransactionStatus
    public Boolean ShouldSerializeTradeMatchId()
    public Void ResetTradeMatchId()
TYPE Strategy2.PrimaryOrdersLimit
    public Boolean ShouldSerializeTicksAwayToWork()
    public Void ResetTicksAwayToWork()
TYPE StrategyDefinition2.StrategyDefinition
    public Boolean ShouldSerializeTickSize()
    public Void ResetTickSize()
TYPE TradeRouting2.MatchedTrade
    public Boolean ShouldSerializeTradeUtcTime()
    public Void ResetTradeUtcTime()
TYPE TradeRouting2.OpenPosition
    public Boolean ShouldSerializeTradeUtcTime()
    public Void ResetTradeUtcTime()
TYPE TradingAccount2.Account
    public Boolean ShouldSerializePreTradeMidMarketMarkRequired()
    public Void ResetPreTradeMidMarketMarkRequired()
TYPE UserSession2.Logon
    public Boolean ShouldSerializeFingerprint()
    public Void ResetFingerprint()
TYPE UserSession2.LogonInit
    public Boolean ShouldSerializeFingerprint()
    public Void ResetFingerprint()
TYPE UserSession2.LogonRoutineClient
    public Boolean ShouldSerializeTraderAgreementAccepted()
    public Void ResetTraderAgreementAccepted()
TYPE UserSession2.LogonRoutineServer
    public Boolean ShouldSerializeTraderAgreementUrl()
    public Void ResetTraderAgreementUrl()
TYPE OFT.DxFeed.DxFeedConnector
    public Task<IEnumerable<Trade>> GetTradesAsync(Contract contract, OuterCandlesRequest request)
TYPE OFT.DxFeed.DxFeedSecurityParent
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE #=zoeU6ZUAqZ1Cr7HwLKdyUvZ7HAG4c
    public sealed override Void historicalTicks(Int32 #=znamsrCg=, HistoricalTick[] #=zvTbocqY=, Boolean #=zaS6iznc=)
    public sealed override Void historicalTicksBidAsk(Int32 #=znamsrCg=, HistoricalTickBidAsk[] #=zvTbocqY=, Boolean #=zaS6iznc=)
    public sealed override Void historicalTicksLast(Int32 #=znamsrCg=, HistoricalTickLast[] #=zvTbocqY=, Boolean #=zaS6iznc=)
    public sealed override Void tickByTickAllLast(Int32 #=znamsrCg=, Int32 #=zZQSE9hY=, Int64 #=zKdiKy4g=, Double #=zqW4Mn8v_lxiA, Decimal #=zg1HCdPY=, TickAttribLast #=zXq8KzNVoj7IfDgb9roQ1U_g=, String #=z4f_NIqdrLRar8VD8Mw==, String #=zFNLc55Mir4nN)
    public sealed override Void tickByTickBidAsk(Int32 #=znamsrCg=, Int64 #=zKdiKy4g=, Double #=zY0J$bMfPnJ4MIAxtng==, Double #=ztiAlhyJjmBcqW363IQ==, Decimal #=zNDeo2DxjObnr, Decimal #=zjOY_z8vWiuTL, TickAttribBidAsk #=zNCsyLmYpZ8Upow2N$g==)
    public sealed override Void tickByTickMidPoint(Int32 #=znamsrCg=, Int64 #=zKdiKy4g=, Double #=zIdQVhCAUiCcD)
    public sealed override Void tickEFP(Int32 #=z77vYnRnixCer, Int32 #=zZQSE9hY=, Double #=z6ptiH5J$Q_Y9, String #=zxKOn0TmejMNi, Double #=z$OnNNTCAc9wmwS8zPA==, Int32 #=ztYbEvrCbENeW, String #=zcxARLW53OmQ_qIY1JqKZPHg=, Double #=zrB2XTcs_90atUjGaxw==, Double #=zHZfebR3SzDiAcxVqEdUPbPw98hH_)
    public sealed override Void tickGeneric(Int32 #=z77vYnRnixCer, Int32 #=zXiaT14w=, Double #=z145aBQ8=)
    public sealed override Void tickNews(Int32 #=z77vYnRnixCer, Int64 #=zo9CQOye7Kr$o, String #=zuUOR$ug=, String #=zUCYdHtDLJ83vBPTdZA==, String #=zcnvVaES469oZsjduFw==, String #=zXPXeUzM=)
    public sealed override Void tickOptionComputation(Int32 #=z77vYnRnixCer, Int32 #=zXiaT14w=, Int32 #=z6DrmPgRFh2EM, Double #=zYFrrdoFSTJjWwcyCkt$jccV9zQ6T, Double #=zS95K1sI=, Double #=zwtbA2$U63WC6, Double #=zqL$re4Yb7lvAnkeQVA==, Double #=z9neVtZ99sodu, Double #=zK$73y8e_nXOD, Double #=zfWq4kHNs3ssR, Double #=zpjxVswc7md92h9VvsQ==)
    public sealed override Void tickPrice(Int32 #=z77vYnRnixCer, Int32 #=zXiaT14w=, Double #=zqW4Mn8v_lxiA, TickAttrib #=z29gDfBQ=)
    public sealed override Void tickReqParams(Int32 #=z77vYnRnixCer, Double #=z4SxLvmHWDHWr, String #=zB0INIL__ws3r, Int32 #=zZQqZKaAjmurh)
    public sealed override Void tickSize(Int32 #=z77vYnRnixCer, Int32 #=zXiaT14w=, Decimal #=zg1HCdPY=)
    public sealed override Void tickSnapshotEnd(Int32 #=z77vYnRnixCer)
    public sealed override Void tickString(Int32 #=z77vYnRnixCer, Int32 #=zXiaT14w=, String #=z145aBQ8=)
TYPE OrderInfo
    public List<MyTrade> GetTrades()
    public Void AddTrade(MyTrade trade)
TYPE OFT.MT5.Commander+<>c__DisplayClass23_0
    internal Object <GotTrades>g__Done|0(Object arg)
TYPE OFT.MT5.Commander+<>c__DisplayClass28_0
    internal Object <CmdSubscribeTrades>g__Done|0(Object arg)
TYPE OFT.MT5.Exports+<>c__DisplayClass4_0
    internal String <PrintLog>b__0()
TYPE OFT.Models.Lessons.Lesson
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE #=zDNFDa50yYHdfqh57d3hA1yl75S2qH5_hFA==
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zHmTTETBZgj5FdY6PwrUyrnwNzkTDZJWlxw==
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zJxkqHr6WLrv3qoyLajAkrrGDfCGG
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zY9H95znCzoWUFqyXcGK7YtGl3fn1
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zeQhjLox82yeN1UnoK6K2HLZFc$QMgiHPVQ==
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zhxm9mj$43Ts_uuL2sgPx7GGz_H7YrqkIoQ==
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zjf6GDTgZN7mXwatcAQukrybc$NSs
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zkGbp4q4XB1eJ53GP3yLTyPRg3oyx
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zn2xO24P4Nn$mu3ShpFvyWIsshd3RcrETxw==
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zyJJeg64j0TwxOz2EjKYXZMsGkbm_xM_24xiLF1Q=
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zyih8IuPZrJCKJhwk65oMKv5DQjOfXzs6Kg==
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE #=zz1na4dqPS6x3tzg33P7RlcXYbKfbnWaDSg==
    protected virtual Boolean PrintMembers(StringBuilder #=z8qTxtRA=)
TYPE OFT.NinjaTrader.Message
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE #=zpaFkg$xsv3FhznOxIb$8uToTrHt7
    protected virtual Boolean PrintMembers(StringBuilder #=zNhFW$cU=)
TYPE OFT.Okx.Common.OkxLimitOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Okx.Common.OkxOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Okx.OkxConnector
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE #=zbiDz_qpy00YDqKTwD0XtoTAzsxVExzdFITK$w4A=
    protected virtual Boolean PrintMembers(StringBuilder #=zWw4xims=)
TYPE OFT.Phemex.Common.PhemexLimitOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Phemex.Common.PhemexStopLimitOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Phemex.Common.PhemexStopOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Phemex.PhemexConnector
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE #=zDz$EscV832JqMBzge6_6NhPFSGQ87Bstevg5oMiN82Pj
    event ConnectorEventHandler<IEnumerable<MarketDepth>> MarketDepthsUpdate
    event ConnectorEventHandler<IEnumerable<MyTrade>> NewMyTrades
    event ConnectorEventHandler<IEnumerable<Trade>> NewTrades
    event ConnectorEventHandler<MarketDepth> BestBidAskUpdates
    public sealed override Task<IEnumerable<MyTrade>> GetMyTradesAsync(Portfolio #=zQQwO0xS5xJT4g0sKVA==, Security #=zqwtlgmE=, DateTime #=zmGz$drc=, DateTime #=zBwkPtMI=)
TYPE #=zVmto46fMMHh7krnWoGRkAM4FhVGLJYR9TWMQ89k=
    protected virtual Task<IEnumerable<BigTrade>> OnGetBigTradesAsync(BigTradesMarketDataRequest #=zdMmlCHc=, CancellationToken #=zThQw6Ys=, IProgress<Int32> #=zj$5gsDw=)
    protected virtual Task<IEnumerable<IMarketDepthSnapshot>> OnGetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest #=zdMmlCHc=, CancellationToken #=zThQw6Ys=, IProgress<Int32> #=zj$5gsDw=)
    protected virtual Task<Trade> OnGetLastTradeAsync(LastTradeMarketDataRequest #=zdMmlCHc=, CancellationToken #=zThQw6Ys=, IProgress<Int32> #=zj$5gsDw=)
TYPE #=zXncqmzKFo1tXDvbUSyPt0ldjsNo0KKxfCAiFsKDBQ_zv22aJLMqWiCJrTJ6Q
    public sealed override IReadOnlyCollection<MyTrade> GetMyTrades(DateTime #=zmGz$drc=, DateTime #=zBwkPtMI=, IEnumerable<String> #=zCRIeYzklwfzNWkRC8A==, IEnumerable<String> #=zwp6NfHCIxJxS$XoKMg==)
    public sealed override IReadOnlyCollection<MyTrade> GetMyTrades(String #=zvMhPsv0=)
    public sealed override IReadOnlyCollection<MyTrade> GetMyTrades(String #=zvMhPsv0=, Int64 #=zI2f9xolHCimW)
    public sealed override IReadOnlyCollection<MyTrade> GetMyTradesByOrderId(String #=zvMhPsv0=, String #=z55Xl45Y=)
    public sealed override IReadOnlyCollection<MyTrade> GetMyTradesForPeriodOrWithOpenVolume(DateTime #=zmGz$drc=, DateTime #=zBwkPtMI=, IEnumerable<String> #=zCRIeYzklwfzNWkRC8A==, IEnumerable<String> #=zwp6NfHCIxJxS$XoKMg==)
    public sealed override IReadOnlyCollection<MyTrade> GetOpenMyTrades(HashSet<String> #=zNcIfukBocWJL)
    public sealed override MyTrade TryGetMyTrade(String #=zvMhPsv0=, String #=zI2f9xolHCimW, Boolean #=z6kzXE38nSaCD)
TYPE #=zaMnMRO03E6CEdGlzAmK5Zwc3xharL9us9LWpjGwZ3m7gkwPZYgJzq6FPXFe4DSFSYg==
    public sealed override IReadOnlyCollection<HistoryMyTrade> GetHistoryTrades(DateTime #=zmGz$drc=, DateTime #=zBwkPtMI=, IEnumerable<String> #=zCRIeYzklwfzNWkRC8A==, IEnumerable<String> #=zwp6NfHCIxJxS$XoKMg==)
TYPE #=zaeZ6SdppNu436XpCgW50ZBM=
    event Action<CumulativeTrade> NewCumulativeTrade
    event Action<CumulativeTrade> UpdateCumulativeTrade
    event Action<CumulativeTradesRequest, IEnumerable<CumulativeTrade>> HistoricalCumulativeTrades
    event Action<IEnumerable<MarketDataArg>> MarketDepthsChanged
    event Action<IEnumerable<MarketDataArg>> NewTrades
    event Action<MarketDataArg> BestBidAskChanged
    public sealed override IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
    public sealed override IMarketByOrdersWithTradesCache GetMarketByOrdersWithTradesCache(TimeSpan #=zt4or9egwAmQ8)
    public sealed override ITradesCache GetTradesCache(TimeSpan #=zt4or9egwAmQ8)
    public sealed override Task<IEnumerable<MarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotRequest #=zdMmlCHc=, CancellationToken #=zq5nG0ss=)
    public sealed override Void RequestCumulativeTrades(CumulativeTradesRequest #=zdMmlCHc=)
TYPE #=zlo5pL8mxOQ_LrgiT2jHjt0PvljhXy3XWGIgHa5YABIXT
    event Action<MyTrade> NewMyTrade
TYPE OFT.Controls.Chart.IMarketProfilesElement
    public abstract Void AddTick(List<Tick> list)
TYPE OFT.Controls.Designer.DesigningCanvasElement
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Docking.Core.Extenstions.MessageBoxAnswer
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Docking.ViewModels.CheckOptionItem
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.CandleCreators.BtkCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.CandleCreator
    protected virtual Void OnAddBigTrade(BigTrade t)
    protected virtual Void OnAddTick(Tick t)
    protected virtual Void OnUpdateBigTrade(BigTrade t)
    public Void AddBigTrade(BigTrade trade)
    public Void AddTick(Tick tick)
    public Void UpdateBigTrade(BigTrade trade)
TYPE OFT.Platform.CandleCreators.CumTradesCandleCreater
    protected virtual Void OnAddBigTrade(BigTrade t)
    protected virtual Void OnUpdateBigTrade(BigTrade t)
TYPE OFT.Platform.CandleCreators.DeltaCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.MinuteCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.OrderFlowCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.RangeCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.RangeUSCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.RangeXCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.RangeXVCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.RangeZCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.RenkoCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.ReversalCandleCreator
    protected virtual Void OnAddTick(Tick tick)
TYPE OFT.Platform.CandleCreators.SecondsCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.TickCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.TimeFrameCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.CandleCreators.VolumeCandleCreator
    protected virtual Void OnAddTick(Tick t)
TYPE OFT.Platform.Core.DataFeedManager.Replay.ReplayContractLoadDataRequest
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.DataFeedManager.Replay.ReplayContractLoadDataResult
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.DataFeedManager.Replay.ReplayLoadDataRequest
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.DataFeedManager.Replay.ReplayLoadDataResult
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.DataFeedManager.Replay.ReplayOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.DataFeedManager.Replay.ReplayPeriodLoadDataResult
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.Managers.Database.Registries.IHistoryMyTradeRegistry
    public abstract IReadOnlyCollection<HistoryMyTrade> GetHistoryTrades(DateTime from, DateTime to, IEnumerable<String> accounts, IEnumerable<String> securities)
TYPE OFT.Platform.Core.Managers.Database.Registries.IMyTradesRegistry
    public abstract IReadOnlyCollection<MyTrade> GetMyTrades(DateTime from, DateTime to, IEnumerable<String> accounts, IEnumerable<String> securities)
    public abstract IReadOnlyCollection<MyTrade> GetMyTrades(String accountId)
    public abstract IReadOnlyCollection<MyTrade> GetMyTrades(String accountId, Int64 tradeId)
    public abstract IReadOnlyCollection<MyTrade> GetMyTradesByOrderId(String accountId, String orderId)
    public abstract IReadOnlyCollection<MyTrade> GetMyTradesForPeriodOrWithOpenVolume(DateTime from, DateTime to, IEnumerable<String> accounts, IEnumerable<String> securities)
    public abstract IReadOnlyCollection<MyTrade> GetOpenMyTrades(HashSet<String> accountIds)
    public abstract MyTrade TryGetMyTrade(String accountId, String tradeId, Boolean searchInDb)
TYPE OFT.Platform.Core.NativeNotifications.NativeNotificationAction
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.Providers.BigTradesManager
    event Action<Instrument, BigTrade> NewBigTrade
    event Action<Instrument, BigTrade> UpdateBigTrade
    public sealed override Task<IEnumerable<BigTrade>> GetBigTrades(Instrument instrument, ITradingSession tradingSession, DateTime from, DateTime to, Decimal minVol, Decimal maxVol)
    public sealed override Task<IEnumerable<BigTrade>> GetBigTrades(Instrument instrument, ITradingSession tradingSession, Int32 minutes, Decimal minVol, Decimal maxVol)
    public sealed override Task<IEnumerable<BigTrade>> GetSessionBigTrades(Instrument instrument, ITradingSession tradingSession, DateTime date)
    public sealed override Void SubscribeBigTrades(Instrument instrument)
    public sealed override Void UnsubscribeBigTrades(Instrument instrument)
TYPE OFT.Platform.Core.Providers.HiddenModules.HiddenModule
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.Providers.IBigTradesManager
    event Action<Instrument, BigTrade> NewBigTrade
    event Action<Instrument, BigTrade> UpdateBigTrade
    public abstract Task<IEnumerable<BigTrade>> GetBigTrades(Instrument instrument, ITradingSession tradingSession, DateTime from, DateTime to, Decimal minVol, Decimal maxVol)
    public abstract Task<IEnumerable<BigTrade>> GetBigTrades(Instrument instrument, ITradingSession tradingSession, Int32 minutes, Decimal minVol, Decimal maxVol)
    public abstract Task<IEnumerable<BigTrade>> GetSessionBigTrades(Instrument instrument, ITradingSession tradingSession, DateTime date)
    public abstract Void SubscribeBigTrades(Instrument instrument)
    public abstract Void UnsubscribeBigTrades(Instrument instrument)
TYPE OFT.Platform.Core.Providers.IMarketDepthSnapshotsProvider
    public abstract Task<IEnumerable<IMarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Platform.Core.Providers.IPlatformTradingCore
    public abstract Boolean CheckIsAtasTrade(MyTrade trade)
TYPE OFT.Platform.Core.Providers.MarketDataAdapter
    public Task RefreshLastTrades()
    public Task<IEnumerable<BigTrade>> GetBigTradesAsync(BigTradesMarketDataRequest request, IProgress<Int32> progress, CancellationToken token)
    public Task<IEnumerable<Trade>> GetTradesRangeAsync(TicksCacheRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Platform.Core.Providers.MarketDepthSnapshotsProvider
    public sealed override Task<IEnumerable<IMarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotsRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Platform.Core.Providers.OnboardingDataRequest
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Core.Providers.PlatformTradingCore
    event ConnectorEventHandler<IEnumerable<MarketDepth>> MarketDepthsUpdate
    event ConnectorEventHandler<IEnumerable<MyTrade>> NewMyTrades
    event ConnectorEventHandler<IEnumerable<Trade>> NewTrades
    event ConnectorEventHandler<MarketDepth> BestBidAskUpdates
    public sealed override Boolean CheckIsAtasTrade(MyTrade trade)
    public sealed override Task<IEnumerable<MyTrade>> GetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE OFT.Platform.Core.ViewModels.DataVisibleBehavior
    public Void OnUITick()
TYPE OFT.Platform.DrawingObjects.AnchoredVWAP
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.CustomHistogramBase
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.CustomHistogramm
    public sealed override Void AddTick(List<Tick> list)
    public virtual Void OnNewTicks(IEnumerable<Tick> ticks, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.CvdCorrelation
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.DrawingObject
    protected Int32 GetCloneOffsetTicks(Int32 cloneNumber)
    public virtual Void OnNewTicks(IEnumerable<Tick> ticks, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.DrawingObject+LineAlignInfo
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.DrawingObjects.DrawingObjectsCollection
    public Void OnNewTicks(IEnumerable<Tick> list, Instrument instrument)
TYPE OFT.Platform.DrawingObjects.DrawingPosition
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.DynamicPoc
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.GlobalHLine
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.GlobalTradingObjectsStore
    public sealed override Void ProcessNewTicks(IEnumerable<Tick> ticks, String instrumentUniqName, Nullable<Int32> chartId, Decimal tickSize)
TYPE OFT.Platform.DrawingObjects.HLine
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.IGlobalTradingObjectsStore
    public abstract Void ProcessNewTicks(IEnumerable<Tick> ticks, String instrumentUniqName, Nullable<Int32> chartId, Decimal tickSize)
TYPE OFT.Platform.DrawingObjects.RectangleObj
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.DrawingObjects.TrendLine
    public virtual Void OnNewTicks(IEnumerable<Tick> list, Decimal tickSize, String instrumentUniqName, Nullable<Int32> chartId, ISupportAlert alertsProvider)
TYPE OFT.Platform.IndicatorDataConverter
    event Action<CumulativeTrade> NewCumulativeTrade
    event Action<CumulativeTrade> UpdateCumulativeTrade
    event Action<CumulativeTradesRequest, IEnumerable<CumulativeTrade>> HistoricalCumulativeTrades
    event Action<IEnumerable<MarketDataArg>> MarketDepthsChanged
    event Action<IEnumerable<MarketDataArg>> NewTrades
    event Action<MarketDataArg> BestBidAskChanged
    public IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
    public IMarketByOrdersWithTradesCache GetMarketByOrdersWithTradesCache(TimeSpan period)
    public ITradesCache GetTradesCache(TimeSpan period)
    public Void ProcessNewBigTrade(BigTrade bigTrade)
    public Void ProcessNewTicks(IEnumerable<Tick> ticks)
    public Void ProcessUpdateBigTrade(BigTrade obj)
    public Void RequestCumulativeTrades(CumulativeTradesRequest request)
TYPE OFT.Platform.IndicatorOnlineDataProvider
    event Action<CumulativeTrade> NewCumulativeTrade
    event Action<CumulativeTrade> UpdateCumulativeTrade
    event Action<CumulativeTradesRequest, IEnumerable<CumulativeTrade>> HistoricalCumulativeTrades
    event Action<IEnumerable<MarketDataArg>> MarketDepthsChanged
    event Action<IEnumerable<MarketDataArg>> NewTrades
    event Action<MarketDataArg> BestBidAskChanged
    public sealed override IEnumerable<MarketDataArg> GetMarketDepthSnapshot()
    public sealed override IMarketByOrdersWithTradesCache GetMarketByOrdersWithTradesCache(TimeSpan period)
    public sealed override ITradesCache GetTradesCache(TimeSpan period)
    public sealed override Task<IEnumerable<MarketDepthSnapshot>> GetMarketDepthSnapshotsAsync(MarketDepthSnapshotRequest request, CancellationToken token)
    public sealed override Void RequestCumulativeTrades(CumulativeTradesRequest request)
TYPE OFT.Platform.Managers.AllLatencyData
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Managers.LatencyData
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Managers.Statistics.DbStatisticsManager
    event Action<Task> PendingTradesData
TYPE OFT.Platform.Managers.Statistics.ITradingStatisticsManager
    event Action<Task> PendingTradesData
TYPE OFT.Platform.Managers.Strategies.ATM.StrategyDescription
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Managers.Trading.MessageResult
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Managers.Trading.OpenOrderParameter
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Managers.Trading.TradingManagerBase
    protected virtual Void BestAskOnChanged()
    protected virtual Void BestBidOnChanged()
TYPE OFT.Platform.Models.ArticleSection
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Models.ArticleStory
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Models.BigTradesCreator
    event Action<BigTrade> NewBigTrade
    event Action<BigTrade> UpdateBigTrade
    public Void ProcessTick(Tick tick)
TYPE OFT.Platform.Models.DomManager
    event Action<ATASMarketDepthEventArgs> BestBidAskUpdate
TYPE OFT.Platform.Models.Instrument
    event Action<IEnumerable<Tick>> OnNewTick
    event Action<Instrument, Tick> OnTickProcessing
    event Action<MarketDepth> OnBestBidAskChanged
    public List<Tick> GetTicks(DateTime from)
    public Void OnBestBidAsk(MarketDepth depth)
    public Void OnMarketDepth(IReadOnlyCollection<MarketDepth> marketDepths)
    public Void OnNewTrade(Trade trade)
    public Void ReceiveLastTrade(Trade trade)
    public Void ReceiveMissingTrades(IReadOnlyCollection<Trade> trades)
TYPE OFT.Platform.Providers.AtasAdsDataRequest
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Providers.ConnectorHistoryMarketDataProvider
    protected virtual Task<Trade> OnGetLastTradeAsync(LastTradeMarketDataRequest request, CancellationToken token, IProgress<Int32> progress)
TYPE OFT.Platform.Providers.Interfaces.IMyTradesProvider
    public abstract IEnumerable<MyTradeCheckItemViewModel> GetEqualizedTrades()
    public abstract IEnumerable<MyTradeCheckItemViewModel> GetTrades()
    public abstract String GenerateTradeId(DateTime date, Int32 index)
TYPE OFT.Platform.Providers.MyTradesProvider
    public sealed override IEnumerable<MyTradeCheckItemViewModel> GetEqualizedTrades()
    public sealed override IEnumerable<MyTradeCheckItemViewModel> GetTrades()
    public sealed override String GenerateTradeId(DateTime date, Int32 index)
TYPE OFT.Platform.Providers.PayWallRequest
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Providers.PromotionInfo
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Charting.ChartTraderSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Charting.DOMTraderPanelSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Charting.DOMTraderSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Charting.TradingSessionSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Common.VolumeSelectorItemSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Common.VolumeSelectorSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.CopyTrading.FollowerPositionSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.CopyTrading.PortfolioCopyRatio
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Strategies.StrategiesSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Strategies.StrategySettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.ViewModels.Charting.ChartCore
    public Void TryUpdateBidAskInChartTrader()
TYPE OFT.Platform.ViewModels.Charting.ChartData
    protected BigTrade TryScaleBigTrade(BigTrade trade, Boolean isNew, Int32 chartScale, Boolean scaleByLowerValue)
    protected Void Instrument_OnNewBigTrade(Instrument instrument, BigTrade bigTrade)
    protected Void Instrument_OnUpdateBigTrade(Instrument instrument, BigTrade trade)
    protected Void instrument_OnNewTick(IEnumerable<Tick> ticks)
TYPE OFT.Platform.ViewModels.Charting.ChartTraderViewModelBase
    public sealed override Void SetLastBestBidAsk(String bestAsk, String bestBid)
TYPE OFT.Platform.ViewModels.Charting.ChartTradingManager
    event Action BestBidAskUpdated
    event Action<MyTrade> NewMyTrade
    public sealed override Void UpdateBestBidAsk()
    public sealed override Void UpdateMyTradesList()
TYPE OFT.Platform.ViewModels.Charting.IChartPlatformTradingManager
    event Action BestBidAskUpdated
    event Action<MyTrade> NewMyTrade
    public abstract Void UpdateBestBidAsk()
    public abstract Void UpdateMyTradesList()
TYPE OFT.Platform.ViewModels.Charting.IChartTraderViewModel
    public abstract Void SetLastBestBidAsk(String bestAsk, String bestBid)
TYPE OFT.Platform.ViewModels.Charting.ITradingManagerObserver
    public abstract IDisposable OnBestAskChanged(ValueChangedHandler<Nullable<Decimal>> onChanged)
    public abstract IDisposable OnBestBidChanged(ValueChangedHandler<Nullable<Decimal>> onChanged)
TYPE OFT.Platform.ViewModels.Charting.NoDataTradingManager
    event Action<MyTrade> NewMyTrade
TYPE OFT.Platform.ViewModels.Charting.OrderFlagsChangedArgs
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.ViewModels.Charting.UIThreadTradingManagerObserver
    public sealed override IDisposable OnBestAskChanged(ValueChangedHandler<Nullable<Decimal>> onChanged)
    public sealed override IDisposable OnBestBidChanged(ValueChangedHandler<Nullable<Decimal>> onChanged)
TYPE OFT.Platform.ViewModels.Portfolios.ToggleStateEventArgs
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.Platform.Settings.Common.ReplayPlayerSettings
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE #=zxdvvGprx75xI1chnulx7vDIFEAK6
    public virtual Void BestAskQuote(AskInfo #=z0cplWDw=)
    public virtual Void BestBidAskQuote(BidInfo #=zOy1$$J_pwsX2, AskInfo #=zeNGfTaw=)
    public virtual Void BestBidQuote(BidInfo #=z0cplWDw=)
    public virtual Void TradeCondition(TradeInfo #=z0cplWDw=)
    public virtual Void TradeCorrectReport(OrderTradeCorrectReport #=zhnvfA2A=)
    public virtual Void TradePrint(TradeInfo #=z0cplWDw=)
    public virtual Void TradeReplay(TradeReplayInfo #=z0cplWDw=)
    public virtual Void TradeRoute(TradeRouteInfo #=z0cplWDw=)
    public virtual Void TradeRouteList(TradeRouteListInfo #=z0cplWDw=)
    public virtual Void TradeVolume(TradeVolumeInfo #=z0cplWDw=)
TYPE OFT.Rithmic.RithmicConnector
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE OFT.Sbe.Messages.MyTradesResponse
    public TradesGroup TradesCount(Int32 count)
TYPE OFT.Sbe.SbeConnector
    protected virtual Task<IEnumerable<MyTrade>> OnGetMyTradesAsync(Portfolio portfolio, Security security, DateTime from, DateTime to)
TYPE OFT.Sbe.SbeOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.SystemStatistics.Amplitude.EducationStoryData
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE OFT.SystemStatistics.Amplitude.ReplayStartClickedData
    protected virtual Boolean PrintMembers(StringBuilder builder)
TYPE #=zptWAbFdz6dXZ4tJilUl8jOMMH8vVa7PesuJr9rwzqbWM
    protected virtual Boolean PrintMembers(StringBuilder #=zmkFFcx0=)
TYPE OFT.Whitebit.Common.WhitebitLimitOrderFlags
    protected virtual Boolean PrintMembers(StringBuilder builder)
```

Read this for two specific findings the objective depends on:

1. **Is there a per-print trade callback, or only an aggregated one?** A method
   taking a collection or a bar index rather than a single event argument
   suggests aggregation. If only aggregated trades are exposed, the research
   objective is not achievable through this API and that is the finding.
2. **Is depth incremental or a whole-book refresh?** A callback carrying one
   price level is incremental; one carrying a collection, or none at all with
   only a pollable book, means individual depth changes are not observable.


## Next step

Send this file back. Each section maps onto a numbered row in
`docs/ATAS-API-VERIFICATION.md` §3, and the adapter is corrected against it.
