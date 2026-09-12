using System;
using System.Linq;
using System.Reflection;

using Xunit;

namespace NFMarketReplayRecorder.Tests
{
    /// <summary>
    /// Guards the ApiStub's fidelity to the MEASURED ATAS API.
    /// </summary>
    /// <remarks>
    /// <para>These exist because the stub was once more permissive than reality and
    /// hid a real defect. ATAS declares two independent public enums named
    /// <c>MarketDataType</c> and two named <c>TradeDirection</c> — one pair in
    /// <c>ATAS.DataFeedsCore</c>, one in <c>ATAS.Indicators</c>. The stub declared
    /// only the DataFeedsCore pair, so unqualified use compiled locally and failed
    /// the first real-assembly compile with CS0104 at six call sites.</para>
    /// <para>A stub that is more permissive than the real API is worse than no stub:
    /// it converts a compile error into false confidence.</para>
    /// </remarks>
    public class AtasStubParityTests
    {
        private static Assembly Adapter => typeof(NFMarketReplayRecorder.ATAS.MarketReplayRecorderIndicator).Assembly;

        private static Type Require(string fullName)
        {
            var t = Adapter.GetType(fullName, throwOnError: false);
            Assert.True(t != null, "stub does not declare " + fullName);
            return t;
        }

        // ---------------------------------------------- the collision itself

        [Theory]
        [InlineData("ATAS.DataFeedsCore.MarketDataType")]
        [InlineData("ATAS.Indicators.MarketDataType")]
        [InlineData("ATAS.DataFeedsCore.TradeDirection")]
        [InlineData("ATAS.Indicators.TradeDirection")]
        public void Both_declarations_of_each_colliding_enum_exist(string fullName)
        {
            var t = Require(fullName);
            Assert.True(t.IsEnum, fullName + " must be an enum");
        }

        [Fact]
        public void The_colliding_enums_are_genuinely_distinct_types()
        {
            // If these ever became the same type (a forward, say), the stub would stop
            // modelling the ambiguity and unqualified use would silently compile again.
            Assert.NotEqual(Require("ATAS.DataFeedsCore.MarketDataType"),
                            Require("ATAS.Indicators.MarketDataType"));
            Assert.NotEqual(Require("ATAS.DataFeedsCore.TradeDirection"),
                            Require("ATAS.Indicators.TradeDirection"));
        }

        // ------------------------------------------- which family the payload uses

        [Fact]
        public void MarketDataArg_DataType_is_the_ATAS_Indicators_enum()
        {
            // Measured: this is the binding the adapter must use. Binding to the
            // DataFeedsCore enum instead is what failed the real compile.
            var prop = Require("ATAS.Indicators.MarketDataArg").GetProperty("DataType");
            Assert.NotNull(prop);
            Assert.Equal("ATAS.Indicators.MarketDataType", prop.PropertyType.FullName);
        }

        [Fact]
        public void MarketDataArg_Direction_is_the_ATAS_Indicators_enum()
        {
            var prop = Require("ATAS.Indicators.MarketDataArg").GetProperty("Direction");
            Assert.NotNull(prop);
            Assert.Equal("ATAS.Indicators.TradeDirection", prop.PropertyType.FullName);
        }

        // ------------------------------------------------- measured member values

        [Theory]
        [InlineData("ATAS.DataFeedsCore.MarketDataType")]
        [InlineData("ATAS.Indicators.MarketDataType")]
        public void MarketDataType_members_match_the_measured_values(string fullName)
        {
            var t = Require(fullName);
            Assert.Equal(0, (int)Enum.Parse(t, "Bid"));
            Assert.Equal(1, (int)Enum.Parse(t, "Ask"));
            Assert.Equal(2, (int)Enum.Parse(t, "Trade"));
        }

        [Theory]
        [InlineData("ATAS.DataFeedsCore.TradeDirection")]
        [InlineData("ATAS.Indicators.TradeDirection")]
        public void TradeDirection_members_match_the_measured_values(string fullName)
        {
            var t = Require(fullName);
            Assert.Equal(0, (int)Enum.Parse(t, "Between"));
            Assert.Equal(1, (int)Enum.Parse(t, "Buy"));
            Assert.Equal(2, (int)Enum.Parse(t, "Sell"));
        }

        [Fact]
        public void The_two_families_are_numerically_identical_which_is_why_the_bug_hid()
        {
            // Identical values mean the WRONG binding would have behaved correctly at
            // runtime. Only the type identity differs, so only a compiler could catch
            // it -- and only once the stub modelled both declarations.
            foreach (var name in new[] { "Bid", "Ask", "Trade" })
                Assert.Equal((int)Enum.Parse(Require("ATAS.DataFeedsCore.MarketDataType"), name),
                             (int)Enum.Parse(Require("ATAS.Indicators.MarketDataType"), name));

            foreach (var name in new[] { "Between", "Buy", "Sell" })
                Assert.Equal((int)Enum.Parse(Require("ATAS.DataFeedsCore.TradeDirection"), name),
                             (int)Enum.Parse(Require("ATAS.Indicators.TradeDirection"), name));
        }

        // ------------------------------------------------- mapping is unchanged

        private static object Invoke(string method, Type enumType, string member)
        {
            var m = typeof(NFMarketReplayRecorder.ATAS.MarketReplayRecorderIndicator)
                .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.True(m != null, "adapter has no " + method);
            return m.Invoke(null, new[] { Enum.Parse(enumType, member) });
        }

        [Theory]
        [InlineData("Buy", "buy")]
        [InlineData("Sell", "sell")]
        [InlineData("Between", "unknown")]
        public void Aggressor_mapping_is_unchanged_by_the_disambiguation(string member, string expected)
        {
            Assert.Equal(expected, Invoke("MapAggressor", Require("ATAS.Indicators.TradeDirection"), member));
        }

        [Theory]
        [InlineData("Bid", "bid")]
        [InlineData("Ask", "ask")]
        public void Book_side_mapping_is_unchanged_by_the_disambiguation(string member, string expected)
        {
            Assert.Equal(expected, Invoke("MapSide", Require("ATAS.Indicators.MarketDataType"), member));
        }

        [Fact]
        public void A_trade_data_type_is_not_a_book_side()
        {
            Assert.Null(Invoke("MapSide", Require("ATAS.Indicators.MarketDataType"), "Trade"));
        }

        // ------------------------------------------------- adapter binding

        [Fact]
        public void The_adapter_binds_the_ATAS_Indicators_enum_families()
        {
            // Reads the compiled signatures rather than the source text, so the
            // assertion cannot drift from what actually got compiled.
            var t = typeof(NFMarketReplayRecorder.ATAS.MarketReplayRecorderIndicator);

            var aggressor = t.GetMethod("MapAggressor", BindingFlags.NonPublic | BindingFlags.Static);
            var side = t.GetMethod("MapSide", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.Equal("ATAS.Indicators.TradeDirection", aggressor.GetParameters()[0].ParameterType.FullName);
            Assert.Equal("ATAS.Indicators.MarketDataType", side.GetParameters()[0].ParameterType.FullName);
        }
    }
}
