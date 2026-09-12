# Stub parity — CS0104 negative test

Captured 2026-09-12T08:11:06Z.

Proves the repaired ApiStub reproduces the real-assembly failure. On a scratch
copy the defect was reintroduced (import both namespaces, use bare enum names)
and the LOCAL stub build produced the identical errors Cowork saw against the
real ATAS assemblies:

```
error CS0104: 'MarketDataType' is an ambiguous reference between 'ATAS.DataFeedsCore.MarketDataType' and 'ATAS.Indicators.MarketDataType'
error CS0104: 'TradeDirection' is an ambiguous reference between 'ATAS.DataFeedsCore.TradeDirection' and 'ATAS.Indicators.TradeDirection'
4 x error CS0104
```

The authoritative tree builds clean; only the scratch copy was made to fail.
Guarded going forward by AtasStubParityTests (19 cases).
