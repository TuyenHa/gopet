# Ma trận protocol parity còn lại

| Envelope | Sub/opcode | Hướng | Trạng thái | Parser/fixture |
|---|---:|---|---|---|
| `PET_SERVICE` | `CHAT_GLOBAL` | hai chiều | handled | `ChatHandlerTests` |
| `PET_SERVICE` | `MAGIC` | hai chiều | handled | unit + LiveSmoke JJ |
| `PET_SERVICE` | `GYM` | hai chiều | handled | unit + LiveSmoke KK |
| `PET_SERVICE` | `UP_TIEM_NANG` | hai chiều | handled | `PetProfileHandlerTests` |
| `PET_SERVICE/TATTOO` | `1` | server → client | handled | tattoo screen fixture |
| `PET_SERVICE/TATTOO` | `7` | server → client | handled | material fixture |
| `PET_SERVICE` | `USE_EQUIP_ITEM` | server → client | handled | `PetEquipHandlerTests` |
| `PET_SERVICE` | `UNEQUIP_ITEM` | server → client | handled | `PetEquipHandlerTests` |
| `PET_SERVICE` | `REMOVE_ITEM_EQUIP` | server → client | handled | `PetEquipHandlerTests` |
| `PET_SERVICE` | `ON_UNQUIP_GEM` | server → client | handled | full item fixture |
| `LETTER_COMMAND` | `13` | hai chiều, sub bất đối xứng | handled | unit + LiveSmoke LL |
| `LETTER_COMMAND` | `15/16/17` | client → server | handled | `LetterPacketsTests` |

Lưu ý wire của mail: client gửi sub bằng `sbyte`, server trả sub bằng `int`. Handler mail
vì vậy đăng ký top-level và tự đọc `int`, không dùng sub-router của `PET_SERVICE`.
