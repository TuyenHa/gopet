using System.Diagnostics;
using System.Reflection;
using Gopet.IO;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var tests = new (string, Action)[] {
    ("arena retains slot and sends countdown", ArenaTests.SlotAndTimer),
    ("arena registration pairing and two rounds", ArenaTests.RegistrationPairingAndTwoRounds),
    ("arena rejects ineligible registration without charge", ArenaTests.RegistrationRejectsInvalidPetAndClosedWindow),
    ("block writes preserve signed payload and header", BlockWrites),
    ("truncated payload stops at EOF", TruncatedPayload),
    ("fragmented payload is read completely", FragmentedPayload),
    ("fragmented handshake reads all nine bytes", FragmentedKey),
    ("concurrent close invokes disconnect once", CloseOnce),
    ("sender failure closes session", SenderFailure),
    ("drain rejects later packets", DrainRejects),
    ("broadcast payload freezes retained writer", FreezeMessage),
    ("slow client queue is bounded", BoundedQueue),
    ("close returns before a blocked send drains", CloseNonBlocking),
    ("durable history retries the same snapshot after restart", HistoryTests.Run),
    ("lookup uses a stable snapshot and only one enumeration", GameplayTests.Search),
    ("dead mob deadline remains fixed across ticks", GameplayTests.Deadline),
    ("gift sampling bounds work and preserves candidate selection", GameplayTests.Selection),
    ("logger survives sink failure without recursion", LoggerTests.Run),
    ("backup runs independently without overlapping jobs", RuntimeTests.Backup),
    ("map tick sleeps only the remaining budget", RuntimeTests.Tick),
    ("legacy variants and Unity decoder preserve wire bytes", NetworkTests.WireVariants),
    ("real socket drains final packet and releases lifetime", NetworkTests.FinalPacket),
    ("disconnect waits for active message handler", NetworkTests.DisconnectWaitsForHandler),
    ("history batch bounds serialized bytes", HistoryTests.ByteBoundedBatch),
    ("backup shutdown waits and prevents new work", RuntimeTests.BackupShutdown),
    ("packet-size rejection splits history without losing records", HistoryTests.SplitRejectedBatch),
    ("backup publishes only completed exports", RuntimeTests.BackupPublication),
    ("battle background buy charges price, selects, blocks re-buy", BattleBackgroundTests.BuyAndSelect),
    ("battle background rejects bad ids, unowned, no gold", BattleBackgroundTests.Rejections),
    ("battle background concurrent buys charge once", BattleBackgroundTests.ConcurrentBuyChargesOnce),
    ("equip durability wear, thresholds, repair", EquipDurabilityTests.WearAndThresholds),
    ("equip durability persists and legacy items load full", EquipDurabilityTests.JsonRoundTrip),
    ("equip repair uses one stone and rejects invalid", EquipDurabilityTests.RepairRules),
    ("equip concurrent repairs use one stone", EquipDurabilityTests.ConcurrentRepairUsesOneStone),
    ("market kiosk sell item locking prevents race conditions", MarketKioskTests.SellItemLockingPreventsRaceConditions),
    ("market kiosk payout calculates seller share", MarketKioskTests.KioskPayoutCalculatesSellerShare),
    ("market kiosk payout assign fees", MarketKioskTests.KioskPayoutAssignFees),
    ("market kiosk sell item expiration time", MarketKioskTests.SellItemExpirationTime),
    ("market kiosk sell item transition", MarketKioskTests.SellItemTransition),
    ("market kiosk price validation", MarketKioskTests.KioskPriceValidation),
    ("market kiosk list request structure", MarketKioskTests.ListRequestStructure),
    ("market query filter by single kiosk type", MarketQueryTests.FilterBySingleKioskType),
    ("market query filter all types", MarketQueryTests.FilterAllTypes),
    ("market query sort newest", MarketQueryTests.SortNewest),
    ("market query sort price ascending", MarketQueryTests.SortPriceAscending),
    ("market query sort price descending", MarketQueryTests.SortPriceDescending),
    ("market query sort stable", MarketQueryTests.SortStable),
    ("market query pagination clamps", MarketQueryTests.PaginationClamps),
    ("market query is mine visibility", MarketQueryTests.IsMineVisibility),
    ("market query assigned item hidden from third party", MarketQueryTests.AssignedItemHiddenFromThirdParty),
    ("market query unassigned item visible to all", MarketQueryTests.UnassignedItemVisibleToAll),
    ("market query total pages calculation", MarketQueryTests.TotalPagesCalculation),
    ("market sellable row normalizes non-stackable count to 1", MarketFixesTests.WriteSellableRowNormalizesNonStackableCount),
    ("market sellable row keeps stackable count", MarketFixesTests.WriteSellableRowKeepsStackableCount),
    ("market remaining price clamps at zero and matches outstanding charge", MarketFixesTests.RemainingPriceClampsAtZeroAndMatchesOutstanding),
    ("market FlushMarketSaveIfDirty keeps dirty flag on failed save", MarketFixesTests.FlushMarketSaveIfDirtyKeepsFlagOnFailedSave),
};
int failed = 0;
foreach (var (name, test) in tests) {
    try { test(); Console.WriteLine("PASS " + name); }
    catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e.GetBaseException().Message); }
}
return failed == 0 ? 0 : 1;

static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
static void BlockWrites() {
    using var stream = new CountingStream();
    using var writer = new BinaryWriter(stream);
    writer.WriteInt(5); writer.Write((byte)0); writer.Write(new sbyte[] { -128, -1, 0, 127 });
    Check(stream.ToArray().SequenceEqual(new byte[] { 0,0,0,5,0,128,255,0,127 }), "wire bytes changed");
    Check(stream.ByteWrites <= 1, $"expected block payload/header writes, got {stream.ByteWrites} byte writes");
}
static Message Read(Session session) {
    try { return (Message)typeof(MsgReader).GetMethod("readMessage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(new MsgReader(session), null); }
    catch (TargetInvocationException e) { throw e.InnerException; }
}
static void TruncatedPayload() {
    var session = new Session(null) { dis = new BinaryReader(new FragmentStream(new byte[] {0,0,0,5,0,42,1})) };
    try { Read(session); throw new Exception("accepted truncated payload"); }
    catch (EndOfStreamException) { }
}
static void FragmentedPayload() {
    var session = new Session(null) { dis = new BinaryReader(new FragmentStream(new byte[] {0,0,0,4,0,42,0,7})) };
    var message = Read(session);
    Check(message.id == 42 && message.readShort() == 7, "fragmented packet changed");
}
static void FragmentedKey() {
    var session = new Session(null) { dis = new BinaryReader(new FragmentStream(new byte[] {0,1,2,3,4,5,6,7,8})) };
    session.readKey();
    var expected = new TEA(0x0102030405060708L).encrypt(new sbyte[] {1,2,3});
    Check(session.tea.encrypt(new sbyte[] {1,2,3}).SequenceEqual(expected), "partial key accepted");
}
static void CloseOnce() {
    var handler = new Handler(); var session = new Session(null) { messageHandler = handler };
    Parallel.For(0, 32, _ => session.Close());
    Check(SpinWait.SpinUntil(() => handler.Count > 0, 3000), "disconnect never called");
    Thread.Sleep(100);
    Check(handler.Count == 1, $"disconnect called {handler.Count} times");
    Check(session.messageHandler == null, "handler retained after cleanup");
}
static void SenderFailure() {
    var handler = new Handler();
    var session = new Session(null) { isSocketConnected = true, messageHandler = handler, dos = new BinaryWriter(new BrokenStream()) };
    var sender = new MsgSender(session); session.setSender(sender);
    sender.addMessage(new Message(42)); sender.run();
    Check(SpinWait.SpinUntil(() => handler.Count == 1, 3000), "failed sender left session alive");
    Check(!MsgSender.msgSenders.Contains(sender), "sender retained");
}
static void DrainRejects() {
    using var stream = new MemoryStream();
    var session = new Session(null) { isSocketConnected = true, dos = new BinaryWriter(stream) };
    var sender = new MsgSender(session); session.setSender(sender);
    sender.addMessage(new Message(42)); sender.requestDrain(); sender.addMessage(new Message(43)); sender.run();
    Check(stream.ToArray().SequenceEqual(new byte[] {0,0,0,2,0,42}), "packet accepted after drain");
    sender.stop();
}
static void FreezeMessage() {
    var message = new Message(-7); var writer = message.writer(); writer.writeShort(0x1234);
    var first = message.getBuffer();
    try { writer.writeByte(5); throw new Exception("retained writer mutated queued payload"); }
    catch (ObjectDisposedException) { }
    first[0] = 0;
    Check(message.getBuffer().SequenceEqual(new sbyte[] {-7,0x12,0x34}), "shared payload mutated");
}
static void BoundedQueue() {
    var session = new Session(null) { isSocketConnected = true }; var sender = new MsgSender(session); session.setSender(sender);
    for (int i=0; i<2000; i++) sender.addMessage(new Message(42));
    Check(session.IsClosing, "overflow did not close client");
    Check(sender.QueuedMessages <= 1024, "unbounded queue");
    Check(session.Completion.Wait(3000), "overflow cleanup timed out");
    Check(sender.QueuedMessages == 0 && sender.QueuedBytes == 0, "queued payload retained");
}
static void CloseNonBlocking() {
    var session = new Session(null) { isSocketConnected = true }; session.setSender(new MsgSender(session));
    var timer = Stopwatch.StartNew(); session.Close();
    Check(timer.ElapsedMilliseconds < 200, "Close blocked caller on drain");
    Check(session.Completion.Wait(3000), "close deadline did not complete");
}
sealed class Handler : IHandleMessage {
    public int Count; public void onMessage(Message ms) { }
    public void onDisconnected() { Interlocked.Increment(ref Count); }
}
sealed class CountingStream : MemoryStream {
    public int ByteWrites;
    public override void WriteByte(byte value) { ByteWrites++; base.WriteByte(value); }
}
sealed class FragmentStream : MemoryStream {
    int eofReads;
    public FragmentStream(byte[] bytes) : base(bytes) { }
    public override int Read(byte[] buffer, int offset, int count) {
        if (Position == Length && ++eofReads > 10) throw new InvalidOperationException("reader spun after EOF");
        return base.Read(buffer, offset, Math.Min(count, 1));
    }
}
sealed class BrokenStream : MemoryStream {
    public override void WriteByte(byte value) => throw new IOException("injected send failure");
    public override void Write(byte[] buffer, int offset, int count) => throw new IOException("injected send failure");
    public override void Write(ReadOnlySpan<byte> buffer) => throw new IOException("injected send failure");
}
