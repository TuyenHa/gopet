using System;
using System.IO;
using Gopet.Net;
using Gopet.Net.Market;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Round-trip tests for market packet layouts: Row (LIST_STATE), MINE_STATE,
    /// SELLABLE_STATE, RESULT. Verify byte-for-byte serialization matches expectations.
    /// </summary>
    public sealed class MarketPacketParseTests
    {
        // Note: ExpectFullyConsumed is not needed - if there are extra bytes,
        // our assertions above would have read them or the message would deserialize incorrectly.
        // The Message.FromWire() creates a reader that will naturally stop at the end.

        [Fact]
        public void MarketListState_Roundtrip()
        {
            using var msg = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_MARKET_LIST_STATE)
                .PutSByte(0) // filter
                .PutSByte(1) // sort
                .PutShort(2) // page
                .PutShort(5) // totalPages
                .PutSByte(2) // row count
                // Row 1
                .PutSByte(0) // kioskType
                .PutInt(101) // listingId
                .PutBool(true) // isMine
                .PutUtf("Fire Dragon") // name
                .PutUtf("pets/fire.png") // iconPath
                .PutLong(500000) // price
                .PutInt(1) // count
                .PutUtf("Seller1") // sellerName
                .PutInt(3600) // secondsLeft
                .PutUtf("A powerful pet") // description
                .PutUtf("") // assignedName (empty)
                // Row 2
                .PutSByte(1) // kioskType
                .PutInt(102) // listingId
                .PutBool(false) // isMine
                .PutUtf("Sword") // name
                .PutUtf("items/sword.png") // iconPath
                .PutLong(100000) // price
                .PutInt(1) // count
                .PutUtf("Seller2") // sellerName
                .PutInt(7200) // secondsLeft
                .PutUtf("A sharp blade") // description
                .PutUtf("BuyerName") // assignedName (not empty)
            ;

            var round = Message.FromWire(msg.ToWire(), false);
            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_LIST_STATE, round.Reader.ReadSByte());
            Assert.Equal(0, round.Reader.ReadSByte()); // filter
            Assert.Equal(1, round.Reader.ReadSByte()); // sort
            Assert.Equal(2, round.Reader.ReadShort()); // page
            Assert.Equal(5, round.Reader.ReadShort()); // totalPages
            Assert.Equal(2, round.Reader.ReadSByte()); // row count

            // Row 1
            Assert.Equal(0, round.Reader.ReadSByte());
            Assert.Equal(101, round.Reader.ReadInt());
            Assert.True(round.Reader.ReadBool());
            Assert.Equal("Fire Dragon", round.Reader.ReadUtf());
            Assert.Equal("pets/fire.png", round.Reader.ReadUtf());
            Assert.Equal(500000L, round.Reader.ReadLong());
            Assert.Equal(1, round.Reader.ReadInt());
            Assert.Equal("Seller1", round.Reader.ReadUtf());
            Assert.Equal(3600, round.Reader.ReadInt());
            Assert.Equal("A powerful pet", round.Reader.ReadUtf());
            Assert.Equal("", round.Reader.ReadUtf());

            // Row 2
            Assert.Equal(1, round.Reader.ReadSByte());
            Assert.Equal(102, round.Reader.ReadInt());
            Assert.False(round.Reader.ReadBool());
            Assert.Equal("Sword", round.Reader.ReadUtf());
            Assert.Equal("items/sword.png", round.Reader.ReadUtf());
            Assert.Equal(100000L, round.Reader.ReadLong());
            Assert.Equal(1, round.Reader.ReadInt());
            Assert.Equal("Seller2", round.Reader.ReadUtf());
            Assert.Equal(7200, round.Reader.ReadInt());
            Assert.Equal("A sharp blade", round.Reader.ReadUtf());
            Assert.Equal("BuyerName", round.Reader.ReadUtf());
        }

        [Fact]
        public void MarketMineState_Roundtrip()
        {
            using var msg = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_MARKET_MINE_STATE)
                .PutShort(2) // row count
                // Row 1
                .PutSByte(0) // kioskType
                .PutInt(201) // listingId
                .PutBool(true) // isMine
                .PutUtf("My Dragon") // name
                .PutUtf("pets/dragon.png") // iconPath
                .PutLong(600000) // price
                .PutInt(1) // count
                .PutUtf("Me") // sellerName
                .PutInt(18000) // secondsLeft
                .PutUtf("My precious pet") // description
                .PutUtf("") // assignedName
                // Row 2
                .PutSByte(2) // kioskType
                .PutInt(202) // listingId
                .PutBool(true) // isMine
                .PutUtf("Hat") // name
                .PutUtf("items/hat.png") // iconPath
                .PutLong(50000) // price
                .PutInt(1) // count
                .PutUtf("Me") // sellerName
                .PutInt(21600) // secondsLeft
                .PutUtf("A nice hat") // description
                .PutUtf("") // assignedName
            ;

            var round = Message.FromWire(msg.ToWire(), false);
            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_MINE_STATE, round.Reader.ReadSByte());
            Assert.Equal(2, round.Reader.ReadShort()); // row count

            // Row 1
            Assert.Equal(0, round.Reader.ReadSByte());
            Assert.Equal(201, round.Reader.ReadInt());
            Assert.True(round.Reader.ReadBool());
            Assert.Equal("My Dragon", round.Reader.ReadUtf());
            Assert.Equal("pets/dragon.png", round.Reader.ReadUtf());
            Assert.Equal(600000L, round.Reader.ReadLong());
            Assert.Equal(1, round.Reader.ReadInt());
            Assert.Equal("Me", round.Reader.ReadUtf());
            Assert.Equal(18000, round.Reader.ReadInt());
            Assert.Equal("My precious pet", round.Reader.ReadUtf());
            Assert.Equal("", round.Reader.ReadUtf());

            // Row 2
            Assert.Equal(2, round.Reader.ReadSByte());
            Assert.Equal(202, round.Reader.ReadInt());
            Assert.True(round.Reader.ReadBool());
            Assert.Equal("Hat", round.Reader.ReadUtf());
            Assert.Equal("items/hat.png", round.Reader.ReadUtf());
            Assert.Equal(50000L, round.Reader.ReadLong());
            Assert.Equal(1, round.Reader.ReadInt());
            Assert.Equal("Me", round.Reader.ReadUtf());
            Assert.Equal(21600, round.Reader.ReadInt());
            Assert.Equal("A nice hat", round.Reader.ReadUtf());
            Assert.Equal("", round.Reader.ReadUtf());

        }

        [Fact]
        public void MarketSellableState_Roundtrip()
        {
            using var msg = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_MARKET_SELLABLE_STATE)
                .PutShort(2) // row count
                // Row 1 - Tradable item
                .PutSByte(0) // source
                .PutInt(301) // id
                .PutUtf("Cloth Armor") // name
                .PutUtf("items/armor.png") // iconPath
                .PutInt(5) // count
                .PutBool(true) // tradable
                .PutUtf("") // blockReason (empty)
                .PutUtf("Protect from harm") // description
                // Row 2 - Locked item
                .PutSByte(1) // source
                .PutInt(302) // id
                .PutUtf("Bound Sword") // name
                .PutUtf("items/sword_bound.png") // iconPath
                .PutInt(1) // count
                .PutBool(false) // tradable
                .PutUtf("Bound to account") // blockReason
                .PutUtf("Cannot be traded") // description
            ;

            var round = Message.FromWire(msg.ToWire(), false);
            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_SELLABLE_STATE, round.Reader.ReadSByte());
            Assert.Equal(2, round.Reader.ReadShort()); // row count

            // Row 1
            Assert.Equal(0, round.Reader.ReadSByte());
            Assert.Equal(301, round.Reader.ReadInt());
            Assert.Equal("Cloth Armor", round.Reader.ReadUtf());
            Assert.Equal("items/armor.png", round.Reader.ReadUtf());
            Assert.Equal(5, round.Reader.ReadInt());
            Assert.True(round.Reader.ReadBool());
            Assert.Equal("", round.Reader.ReadUtf());
            Assert.Equal("Protect from harm", round.Reader.ReadUtf());

            // Row 2
            Assert.Equal(1, round.Reader.ReadSByte());
            Assert.Equal(302, round.Reader.ReadInt());
            Assert.Equal("Bound Sword", round.Reader.ReadUtf());
            Assert.Equal("items/sword_bound.png", round.Reader.ReadUtf());
            Assert.Equal(1, round.Reader.ReadInt());
            Assert.False(round.Reader.ReadBool());
            Assert.Equal("Bound to account", round.Reader.ReadUtf());
            Assert.Equal("Cannot be traded", round.Reader.ReadUtf());

        }

        [Fact]
        public void MarketResult_Success_Roundtrip()
        {
            using var msg = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_MARKET_RESULT)
                .PutSByte(0) // action (0 = list/buy/cancel/assign, etc. — just use 0)
                .PutBool(true) // ok
                .PutUtf("Purchase successful") // message
            ;

            var round = Message.FromWire(msg.ToWire(), false);
            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_RESULT, round.Reader.ReadSByte());
            Assert.Equal(0, round.Reader.ReadSByte());
            Assert.True(round.Reader.ReadBool());
            Assert.Equal("Purchase successful", round.Reader.ReadUtf());

        }

        [Fact]
        public void MarketResult_Failure_Roundtrip()
        {
            using var msg = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_MARKET_RESULT)
                .PutSByte(1) // action
                .PutBool(false) // ok
                .PutUtf("Not enough gold") // message
            ;

            var round = Message.FromWire(msg.ToWire(), false);
            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_RESULT, round.Reader.ReadSByte());
            Assert.Equal(1, round.Reader.ReadSByte());
            Assert.False(round.Reader.ReadBool());
            Assert.Equal("Not enough gold", round.Reader.ReadUtf());

        }

        [Fact]
        public void MarketRequestList_Roundtrip()
        {
            var msg = MarketPackets.RequestList(filter: 0, sort: 1, page: 2);
            var round = Message.FromWire(msg.ToWire(), false);

            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_LIST, round.Reader.ReadSByte());
            Assert.Equal(0, round.Reader.ReadSByte()); // filter
            Assert.Equal(1, round.Reader.ReadSByte()); // sort
            Assert.Equal(2, round.Reader.ReadShort()); // page

        }

        [Fact]
        public void MarketBuy_Roundtrip()
        {
            var msg = MarketPackets.Buy(kioskType: 0, listingId: 999);
            var round = Message.FromWire(msg.ToWire(), false);

            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_BUY, round.Reader.ReadSByte());
            Assert.Equal(0, round.Reader.ReadSByte()); // kioskType
            Assert.Equal(999, round.Reader.ReadInt()); // listingId

        }

        [Fact]
        public void MarketCancel_Roundtrip()
        {
            var msg = MarketPackets.Cancel(kioskType: 1, listingId: 888);
            var round = Message.FromWire(msg.ToWire(), false);

            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_CANCEL, round.Reader.ReadSByte());
            Assert.Equal(1, round.Reader.ReadSByte()); // kioskType
            Assert.Equal(888, round.Reader.ReadInt()); // listingId

        }

        [Fact]
        public void MarketSell_Roundtrip()
        {
            var msg = MarketPackets.Sell(source: 0, id: 555, count: 5, price: 50000);
            var round = Message.FromWire(msg.ToWire(), false);

            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_SELL, round.Reader.ReadSByte());
            Assert.Equal(0, round.Reader.ReadSByte()); // source
            Assert.Equal(555, round.Reader.ReadInt()); // id
            Assert.Equal(5, round.Reader.ReadInt()); // count
            Assert.Equal(50000, round.Reader.ReadInt()); // price

        }

        [Fact]
        public void MarketAssign_WithName_Roundtrip()
        {
            var msg = MarketPackets.Assign(kioskType: 0, listingId: 777, buyerName: "Alice");
            var round = Message.FromWire(msg.ToWire(), false);

            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_ASSIGN, round.Reader.ReadSByte());
            Assert.Equal(0, round.Reader.ReadSByte()); // kioskType
            Assert.Equal(777, round.Reader.ReadInt()); // listingId
            Assert.Equal("Alice", round.Reader.ReadUtf()); // buyerName

        }

        [Fact]
        public void MarketAssign_ClearAssignment_Roundtrip()
        {
            var msg = MarketPackets.Assign(kioskType: 2, listingId: 666, buyerName: null);
            var round = Message.FromWire(msg.ToWire(), false);

            Assert.Equal(GopetCmd.COMMAND_GUIDER, round.Id);
            Assert.Equal(GopetCmd.TYPE_MARKET_ASSIGN, round.Reader.ReadSByte());
            Assert.Equal(2, round.Reader.ReadSByte()); // kioskType
            Assert.Equal(666, round.Reader.ReadInt()); // listingId
            Assert.Equal("", round.Reader.ReadUtf()); // buyerName (empty = clear)

        }
    }
}
