using System.Reflection;
using Gopet.Data.Collections;
using Gopet.Data.Event;
using Gopet.Data.map;
using Gopet.Data.Map;
using Gopet.Language;

static partial class ArenaTests
{
    sealed class LobbyMap : GopetMap
    {
        public GopetPlace Lobby;
        public LobbyMap(MapTemplate template) : base(19, false, template) { Lobby = new MemoryArena(this); }
        public override void createZoneDefault() { }
        public override void addRandom(Player p) => Lobby.add(p);
    }
    // Replace map asset/appearance traffic, not pairing, battle startup or tournament results.
    sealed class MemoryArena : ArenaPlace
    {
        public MemoryArena(GopetMap map) : base(map, 0) { }
        public override void add(Player p)
        {
            p.getPlace()?.players.remove(p);
            p.controller.setPetBattle(null);
            players.addIfAbsent(p); p.setPlace(this);
        }
    }
    static void Set(object target, string property, object value) =>
        target.GetType().GetProperty(property).SetValue(target, value);
    static MapTemplate MapTemplate(int id)
    {
        var t = new MapTemplate { npc = Array.Empty<int>() };
        Set(t, "mapId", id); Set(t, "name", "Arena fixture");
        Set(t, "numPetDie", Array.Empty<int>()); Set(t, "boss", Array.Empty<int>()); return t;
    }
    static CopyOnWriteArrayList<ArenaPlace.ArenaData> Pairs(ArenaPlace p) =>
        (CopyOnWriteArrayList<ArenaPlace.ArenaData>)typeof(ArenaPlace)
            .GetField("_data", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(p);

    public static void RegistrationPairingAndTwoRounds()
    {
        var hiddenField = typeof(GopetManager).GetField("HiddentStatItemTemplates");
        hiddenField.SetValue(null, Array.CreateInstance(hiddenField.FieldType.GetElementType(), 0));
        const int templateId = 990101;
        var template = new PetTemplate();
        Set(template, "petId", templateId); Set(template, "str", 100); Set(template, "agi", 100);
        Set(template, "element", GopetManager.FIRE_ELEMENT);
        Set(template, "_int", 100); Set(template, "frameImg", "petFrame/0.png");
        Set(template, "frameNum", (sbyte)1); Set(template, "name", "Fixture");
        GopetManager.PETTEMPLATE_HASH_MAP.put(templateId, template);
        var language = new LanguageData(); language.PetNameLanguage[templateId] = "Fixture";
        GopetManager.Language["arena-test"] = language;
        var arena = ArenaEvent.Instance;
        var lobby = new LobbyMap(MapTemplate(19));
        var inside = new ArenaMap(20, false, MapTemplate(20)); inside.places.Clear();
        var place = new MemoryArena(inside); inside.addPlace(place);
        MapManager.maps[19] = lobby; MapManager.maps[20] = inside;
        var players = Enumerable.Range(99201, 4).Select(Player).ToArray();
        try
        {
            arena.IdPlayerJoin.Clear(); arena.IsRunning = false; arena.IsFighting = false; arena.Update();
            foreach (var p in players)
            {
                p.ApplicationVersion = new Version(1, 4, 2);
                p.playerData.petSelected = new Pet(templateId);
                PlayerManager.player_ID.put(p.user.user_id, p); lobby.addRandom(p);
                var gold = p.playerData.gold;
                arena.IsFighting = true;
                MenuController.selectMenu(MenuController.MENU_SELECT_TYPE_PAYMENT_TO_ARENA_JOURNALISM, 0, -1, p);
                Check(p.playerData.gold == gold && !arena.IdPlayerJoin.Contains(p.user.user_id), "fighting event accepted registration");
                arena.IsFighting = false;
                MenuController.selectMenu(MenuController.MENU_SELECT_TYPE_PAYMENT_TO_ARENA_JOURNALISM, 0, -1, p);
                MenuController.selectMenu(MenuController.MENU_SELECT_TYPE_PAYMENT_TO_ARENA_JOURNALISM, 0, -1, p);
                Check(p.playerData.gold == gold - GopetManager.PRICE_GOLD_ARENA_JOURNALISM, "registration charged twice");
            }
            Check(arena.IdPlayerJoin.Count == 4, "four players were not registered");
            arena.IsFighting = true; arena.NextTurn();
            Check(Pairs(place).Count == 2, "first round did not pair all players");
            Check(Pairs(place).Select(p => p.arena).Distinct().Count() == 2, "pairs share a slot");
            foreach (var pair in Pairs(place).ToArray())
            {
                Finish(pair);
                Pairs(place).remove(pair);
            }
            Check(arena.IdPlayerJoin.Count == 2, "winners not queued for next round");
            Check(players.All(p => p.getPlace().map.mapID == 19), "players not returned to lobby");
            place.petBattles.Clear();
            arena.NextTurn();
            Check(Pairs(place).Count == 1, "final was not paired");
            var final = Pairs(place).Single(); Finish(final); Pairs(place).remove(final);
            Check(players.Sum(p => p.playerData.AccumulatedPoint) == 3, "tournament awarded wrong points");
            inside.places.Clear(); arena.Update();
            Check(!arena.IsFighting && !arena.IsRunning && arena.IdPlayerJoin.IsEmpty, "tournament did not finish");
        }
        catch (Exception error) { throw new Exception(error.ToString()); }
        finally
        {
            foreach (var p in players) PlayerManager.player_ID.remove(p.user.user_id);
            arena.IdPlayerJoin.Clear(); arena.IsRunning = false; arena.IsFighting = false;
            MapManager.maps.Remove(19); MapManager.maps.Remove(20);
        }
    }

    static void Finish(ArenaPlace.ArenaData pair)
    {
        foreach (var p in new[] { pair.PlayerOne, pair.PlayerTwo }) ((RecordingSession)p.session).Messages.Clear();
        pair.PlayerOne.getPet().hp = 0;
        pair.PlayerOne.controller.getPetBattle().update();
        foreach (var p in new[] { pair.PlayerOne, pair.PlayerTwo })
            Check(((RecordingSession)p.session).Messages.Any(bytes => bytes.Length > 1 &&
                bytes[0] == GopetCMD.PET_SERVICE && bytes[1] == GopetCMD.PET_BATTLE_STATE), "missing battle result packet");
        pair.removeAllPlayer();
    }
}
