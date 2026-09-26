using System.Net.Sockets;
using Gopet.Data.Event;
using Gopet.Data.Map;
using Gopet.IO;
using Gopet.Language;

static partial class ArenaTests
{
    sealed class RecordingSession : ISession
    {
        public bool clientOK { get; set; } = true;
        public Socket CSocket { get; set; }
        public readonly List<sbyte[]> Messages = new();
        public void sendMessage(Message m) => Messages.Add(m.getBuffer());
        public void Close() { }
        public bool isConnected() => true;
        public void readKey() { }
        public void setClientOK(bool value) => clientOK = value;
        public void setHandler(IHandleMessage value) { }
    }
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    static Player Player(int id) => new(new RecordingSession())
    {
        user = new UserData { user_id = id }, _languageCode = "arena-test",
        playerData = new PlayerData { user_id = id, name = "Arena" + id, gold = 1_000_000 }
    };
    public static void SlotAndTimer()
    {
        var first = Player(99101); var second = Player(99102);
        var slot = ArenaPlace.POINTS[2];
        var pair = new ArenaPlace.ArenaData(first, second, slot);
        Check(pair.arena == slot, "arena slot was not retained; later pairs overlap");
        Check(first.playerData.x == slot.X2 && second.playerData.x == slot.X1, "spawn slot changed");
        var bytes = ((RecordingSession)first.session).Messages.Single();
        var message = new Message(bytes);
        Check(message.id == GopetCMD.PET_SERVICE && message.readsbyte() == GopetCMD.TIME_PLACE, "missing countdown");
        var seconds = message.readInt();
        Check(seconds > 115 && seconds <= 120, "countdown must be seconds, not milliseconds");
    }
    public static void RegistrationRejectsInvalidPetAndClosedWindow()
    {
        var arena = ArenaEvent.Instance;
        GopetManager.Language["arena-test"] = new LanguageData();
        var player = Player(99101);
        arena.IdPlayerJoin.Clear();
        arena.IsRunning = false; arena.IsFighting = false;
        arena.Update();
        var before = player.playerData.gold;
        try
        {
            MenuController.selectMenu(MenuController.MENU_SELECT_TYPE_PAYMENT_TO_ARENA_JOURNALISM, 0, -1, player);
            Check(!arena.IdPlayerJoin.Contains(player.user.user_id), "registered a player without a pet or lobby");
            Check(before == player.playerData.gold, "invalid registration consumed currency");
        }
        finally { arena.IdPlayerJoin.Clear(); arena.IsRunning = false; arena.IsFighting = false; }
    }
}
