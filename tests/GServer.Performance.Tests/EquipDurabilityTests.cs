using Gopet.Adapter;
using Gopet.Data.GopetItem;
using Gopet.Data.item;
using Gopet.Manager;
using Newtonsoft.Json;

static class EquipDurabilityTests
{
    const int WeaponTemplate = 990001, FoodTemplate = 990002;

    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }

    /// <summary>Đăng ký template giả (không cần DB): một kiếm và một vật phẩm thường.</summary>
    static void EnsureTemplates()
    {
        void Add(int id, int type)
        {
            if (GopetManager.itemTemplate.ContainsKey(id)) return;
            var t = new ItemTemplate();
            typeof(ItemTemplate).GetProperty("itemId").SetValue(t, id);
            typeof(ItemTemplate).GetProperty("type").SetValue(t, type);
            GopetManager.itemTemplate.put(id, t);
        }
        Add(WeaponTemplate, GopetManager.PET_EQUIP_WEAPON);
        Add(FoodTemplate, GopetManager.ITEM_ENERGY);
    }

    static Item NewItem(int template) => new Item { itemTemplateId = template };

    public static void WearAndThresholds()
    {
        EnsureTemplates();
        var sword = NewItem(WeaponTemplate);
        Check(sword.durability == EquipDurability.Max, "new item not full");
        Check(EquipDurability.Wear(sword, lost: false) == WearResult.None && sword.durability == 79, "win must cost 1");
        Check(EquipDurability.Wear(sword, lost: true) == WearResult.None && sword.durability == 77, "loss must cost 2");

        sword.durability = EquipDurability.WarnAt + 1;
        Check(EquipDurability.Wear(sword, false) == WearResult.Warned, "warn not raised when crossing threshold");
        Check(EquipDurability.Wear(sword, false) == WearResult.None, "warn raised twice");

        sword.durability = 1;
        Check(EquipDurability.Wear(sword, lost: true) == WearResult.Broke && sword.durability == 0, "break must clamp at 0");
        Check(EquipDurability.IsBroken(sword), "zero durability not broken");
        Check(EquipDurability.Wear(sword, true) == WearResult.None && sword.durability == 0, "broken item reported again / went negative");
        // Client Unity (popup Thợ Rèn) đọc số "Độ bền: X/Max" từ mô tả — món hỏng cũng phải có.
        Check(EquipDurability.Describe(sword).Contains($"Độ bền: 0/{EquipDurability.Max}"), "broken item text lost its numbers");

        EquipDurability.Repair(sword);
        Check(sword.durability == EquipDurability.Max && !EquipDurability.IsBroken(sword), "repair not full");

        var food = NewItem(FoodTemplate);
        Check(EquipDurability.Wear(food, true) == WearResult.None && food.durability == EquipDurability.Max, "non-equip item worn");
        Check(EquipDurability.Describe(food) == "", "non-equip item shows durability");
    }

    public static void JsonRoundTrip()
    {
        EnsureTemplates();
        // Item cũ trong DB không có trường durability → nạp lên phải đầy.
        var legacy = JsonConvert.DeserializeObject<Item>($"{{\"itemId\":5,\"itemTemplateId\":{WeaponTemplate}}}",
            JsonAdapter<Item>.SerializerSettings);
        Check(legacy.durability == EquipDurability.Max, "legacy item did not load full durability");

        var worn = NewItem(WeaponTemplate);
        worn.durability = 13;
        var back = JsonConvert.DeserializeObject<Item>(JsonConvert.SerializeObject(worn, JsonAdapter<Item>.SerializerSettings),
            JsonAdapter<Item>.SerializerSettings);
        Check(back.durability == 13, "durability not persisted");
    }

    public static void RepairRules()
    {
        EnsureTemplates();
        var gate = new object();
        int stones = 1;
        bool Consume() { if (stones <= 0) return false; stones--; return true; }

        var sword = NewItem(WeaponTemplate);
        Check(EquipRepairService.TryRepair(sword, gate, Consume) == RepairResult.NotNeeded && stones == 1, "repaired a full item");

        sword.durability = 0;
        Check(EquipRepairService.TryRepair(sword, gate, Consume) == RepairResult.Ok && stones == 0, "repair did not use 1 stone");
        Check(sword.durability == EquipDurability.Max, "repair not full");

        sword.durability = 3;
        Check(EquipRepairService.TryRepair(sword, gate, Consume) == RepairResult.NoStone && sword.durability == 3, "repaired without stone");
        Check(EquipRepairService.TryRepair(NewItem(FoodTemplate), gate, Consume) == RepairResult.NotFound, "repaired non-equip item");
        Check(EquipRepairService.TryRepair(null, gate, Consume) == RepairResult.NotFound, "null item accepted");
    }

    public static void ConcurrentRepairUsesOneStone()
    {
        EnsureTemplates();
        var gate = new object();
        var sword = NewItem(WeaponTemplate);
        sword.durability = 0;
        int used = 0;
        Parallel.For(0, 32, _ => EquipRepairService.TryRepair(sword, gate, () => { Interlocked.Increment(ref used); return true; }));
        Check(used == 1, $"concurrent repairs used {used} stones");
    }
}
