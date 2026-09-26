import { readFileSync } from "node:fs";
import path from "node:path";
import { describe, expect, it } from "vitest";
import {
  applyPetEdit,
  collectAllPets,
  findDuplicatePetIds,
  findPetById,
  parsePetList,
  parseSinglePet,
  removeFromEquip,
  sortPetList,
  stringifyPetList,
  stringifySinglePet,
} from "@/lib/game-json/pet-json";

const fixtures = (name: string) => readFileSync(path.resolve(import.meta.dirname, "../fixtures", name), "utf-8");

// Trích thật từ `gopettae_tae2`.`player`: pets (ID=4, rút gọn 4 pet đầu), petSelected (ID=102 —
// pet này đang đeo item 516587094 dùng chung với fixture item để test xoá-item-gỡ-equip),
// PetDefLeague (ID=4).
const petsRaw = fixtures("player-pets-4.json");
const petSelectedRaw = fixtures("player-pet-selected-102.json");
const petDefLeagueRaw = fixtures("player-pet-def-league-4.json");

describe("parsePetList/parseSinglePet — round-trip fixture thật", () => {
  it("pets[]: không sửa gì -> stringify lại đúng y hệt văn bản gốc", () => {
    const list = parsePetList(petsRaw);
    expect(stringifyPetList(list)).toBe(petsRaw);
  });

  it("petSelected: không sửa gì -> stringify lại đúng y hệt văn bản gốc", () => {
    const pet = parseSinglePet(petSelectedRaw);
    expect(stringifySinglePet(pet)).toBe(petSelectedRaw);
  });

  it("cột null/rỗng -> parseSinglePet trả về null", () => {
    expect(parseSinglePet(null)).toBeNull();
    expect(parseSinglePet("")).toBeNull();
    expect(parseSinglePet("null")).toBeNull();
  });

  it("pets[] đã sắp tăng dần theo petId (bất biến server)", () => {
    const list = parsePetList(petsRaw);
    const ids = list.map((p) => Number(p.petId));
    expect(ids).toEqual([...ids].sort((a, b) => a - b));
  });
});

describe("collectAllPets / findDuplicatePetIds — gộp 3 nguồn, kiểm trùng petId", () => {
  it("gộp đúng số lượng + gắn đúng source", () => {
    const pets = parsePetList(petsRaw);
    const petSelected = parseSinglePet(petSelectedRaw);
    const petDefLeague = parseSinglePet(petDefLeagueRaw);
    const all = collectAllPets(pets, petSelected, petDefLeague);
    expect(all).toHaveLength(pets.length + 2);
    expect(all.filter((e) => e.source === "petSelected")).toHaveLength(1);
    expect(all.filter((e) => e.source === "PetDefLeague")).toHaveLength(1);
  });

  it("dữ liệu thật không trùng petId trên 3 nguồn", () => {
    const pets = parsePetList(petsRaw);
    const petSelected = parseSinglePet(petSelectedRaw);
    const petDefLeague = parseSinglePet(petDefLeagueRaw);
    const all = collectAllPets(pets, petSelected, petDefLeague);
    expect(findDuplicatePetIds(all)).toEqual([]);
  });

  it("phát hiện đúng khi có petId trùng (dữ liệu giả lập lỗi)", () => {
    const pets = parsePetList(petsRaw);
    const petSelected = parseSinglePet(petSelectedRaw)!;
    // Ép trùng id với pet đầu tiên trong kho để mô phỏng dữ liệu lệch.
    const dupId = Number(pets[0].petId);
    petSelected.petId = dupId;
    const all = collectAllPets(pets, petSelected, null);
    expect(findDuplicatePetIds(all)).toEqual([dupId]);
  });

  it("findPetById tìm đúng pet bất kể nguồn", () => {
    const pets = parsePetList(petsRaw);
    const petSelected = parseSinglePet(petSelectedRaw);
    const all = collectAllPets(pets, petSelected, null);
    const found = findPetById(all, Number(petSelected!.petId));
    expect(found?.source).toBe("petSelected");
  });
});

describe("applyPetEdit — sửa chỉ số cơ bản, giữ nguyên field khác (equip/skill...)", () => {
  it("sửa lvl/star/exp/str/agi/_int/hp/mp/maxHp/maxMp — equip/skill không đổi", () => {
    const pet = parseSinglePet(petSelectedRaw)!;
    const equipBefore = JSON.stringify(pet.equip);
    const skillBefore = JSON.stringify(pet.skill);
    applyPetEdit(pet, { lvl: 10, star: 3, exp: 999, str: 600, agi: 600, _int: 600, hp: 100, mp: 100, maxHp: 500, maxMp: 500, tiemnang_point: 5, skillPoint: 1, name: "Đã đổi tên" });
    expect(pet.lvl).toBe(10);
    expect(pet.star).toBe(3);
    expect(pet.exp).toBe(999);
    expect(pet.name).toBe("Đã đổi tên");
    expect(JSON.stringify(pet.equip)).toBe(equipBefore);
    expect(JSON.stringify(pet.skill)).toBe(skillBefore);
  });
});

describe("removeFromEquip — dọn tham chiếu item bị xoá khỏi equip của pet (xoá-item-gỡ-equip)", () => {
  it("gỡ đúng itemId khỏi equip, giữ nguyên phần tử khác", () => {
    const pet = parseSinglePet(petSelectedRaw)!;
    expect((pet.equip as unknown[]).map(Number)).toEqual([516587094, 1815042862]);
    const changed = removeFromEquip(pet, 516587094);
    expect(changed).toBe(true);
    expect((pet.equip as unknown[]).map(Number)).toEqual([1815042862]);
  });

  it("itemId không có trong equip -> trả về false, equip không đổi", () => {
    const pet = parseSinglePet(petSelectedRaw)!;
    const changed = removeFromEquip(pet, 42);
    expect(changed).toBe(false);
    expect((pet.equip as unknown[]).map(Number)).toEqual([516587094, 1815042862]);
  });

  it("sau khi sửa petId không đổi -> sortPetList vẫn giữ đúng thứ tự tăng dần", () => {
    const list = parsePetList(petsRaw);
    applyPetEdit(list[2], { lvl: 99, star: 0, exp: 0, str: 0, agi: 0, _int: 0, hp: 0, mp: 0, maxHp: 1, maxMp: 1, tiemnang_point: 0, skillPoint: 0 });
    sortPetList(list);
    const ids = list.map((p) => Number(p.petId));
    expect(ids).toEqual([...ids].sort((a, b) => a - b));
  });
});
