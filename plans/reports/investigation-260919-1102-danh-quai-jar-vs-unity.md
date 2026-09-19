# Điều tra: cách đánh quái — JAR gốc vs Unity

Ngày 2026-09-19. Nguồn: `client.jar_Decompiler.com` (obfuscated, có xác minh bằng `javap`),
`GopetUnityClient/Assets/Scripts`, `SRCGOPETGOC/GServer` (ground truth).

## 1. Khác biệt lớn nhất: CÁCH CHỌN QUÁI

| | JAR | Unity |
|---|---|---|
| Cơ chế | **Đi đâm vào quái** (collision) | **Click chuột lên sprite quái** |
| Hitbox | `gy(-10,-10,20,20)` quanh quái (`de.java:26`) | `BoxCollider2D` bằng bounds sprite (`WorldActorView.Factory.cs:50`) |
| Điều kiện kích hoạt | Chỉ khi đang **bấm phím di chuyển** trong tick đó (`ew.java:406` `if (var17)`) | Bất kỳ lúc nào, kể cả đứng yên |
| Chống lặp | Latch `var23.f`, reset khi ra khỏi hitbox (`ew.java:410,417-419`) | **Không có** |
| Hiệu ứng khi bấm | Nổ tia trắng ở chỗ quái 2s (`d.java`, `dj.java:248-251`) | Không có |
| Gói gửi | `[81][36][int mobId]` (`dj.java:255-257`) | `[81][36][int mobId]` (`BattleHandler.cs:52`) — **giống hệt** |

Wire format khớp 100%. Khác nhau hoàn toàn ở **input layer**.

## 2. Gate trước khi gửi

Cả hai đều **không** check khoảng cách / pet sống / đang trong trận / dialog xác nhận.
Khác biệt thực chất:

- JAR có 2 rào tự nhiên: phải **đang di chuyển** + latch một-phát-một-lần. Muốn đánh lại
  phải bước ra rồi bước vào → tốc độ spam bị giới hạn bởi tốc độ đi bộ.
- Unity **không có rào nào**. Click liên tiếp = spam opcode 36.

**Rủi ro thật:** `GopetPlace.cs:406-414` — trên map 12, nếu 2 lần giết quái cách nhau
< 4500ms → `user.ban(BAN_TIME, BanUserByAutoAttackMob)` + đóng session. Unity đang để
người chơi tự bắn vào chân mình. (Nút building Unity thì lại có throttle 500ms —
`GameSession.Interactions.cs:34`.)

## 3. Auto-attack (tự động đánh)

| | JAR (`ej.java`) | Unity (`AutoAttackLoop.cs`) |
|---|---|---|
| Khi **không** trong trận | gửi `[81][22]` xin lại list quái, throttle **1s** | gửi `[81][22]` |
| Khi **đang** trong trận | tự bấm nút Đánh (cd 318) → `[81][37][1]`, throttle **4s** | không làm gì |
| Guard | bỏ qua nếu `fr.b == 1` (đã gửi đánh lượt này) | — |
| Tắt tự động | không pet, hoặc map type 12 (`ej.java:38-45`) | — |
| Nhịp | 1s (tìm quái) / 4s (đánh) | **4s cho cả hai** |

Unity hiểu sai `ej`: chỉ port nhánh "xin list quái", **bỏ mất nhánh tự đánh trong trận**.
Comment trong `AutoAttackLoop.cs` ghi "Nhịp auto đánh của JAR (el/ej)" nhưng thực tế jar
dùng 2 throttle khác nhau cho 2 trạng thái khác nhau.
Ngoài ra JAR có hot-zone vô hình `gy(15,15,30,30)` góc trên trái — chạm vào để tắt auto
(`fr.java:118-125`, `:667-669`). Unity tắt qua menu/settings.

## 4. Màn hình chiến đấu

| | JAR | Unity |
|---|---|---|
| Nơi diễn ra | **Ngay trên bản đồ** — 2 sprite `ei` đứng cạnh nhau, chỉ thêm thanh nút dưới đáy (`fr.java:740-787`) | **Overlay full-screen** canvas sortingOrder 40 (`BattleView.cs:42`) |
| Vị trí nhân vật | Snap sang cạnh quái, lưu vị trí cũ vào `di.b[0]/c[0]` để trả về sau | Không đụng vị trí |
| Quái trên map | **Xoá sprite ngay khi vào trận** (trừ boss); chỉ add lại **nếu thua** (`e.java` case 9) | Giữ nguyên, chỉ xoá khi nhận opcode 96 |
| Đồng hồ lượt | **Pie đỏ** vẽ trên đầu sprite đang tới lượt: `fillArc(..., remaining*360/max)` (`ei.java:162-171`) | **Không có** — `RemainingMs` parse xong rồi vứt |
| Xem trận người khác | Có — battle của player khác vẫn vẽ trên map (`fr.a` là Hashtable theo playerId) | **Không** — `if (!start.IsParticipant) return` (`BattleCoordinator.cs:58`) |
| Khoá di chuyển | `ew.b = false` + `dj.b = true` | `SetBattleMode(true)` (`GameSession.cs:340`) |

## 5. Vòng lượt — nút bấm

| Hành động | JAR | Unity | Gói |
|---|---|---|---|
| Đánh thường | cd 318, guard `if (this.b != 1)` → **1 lần/lượt** (`fr.java:240-251`) | nút Tấn công, `_actionBar.Lock()` 3.5s | `[81][37][1]` |
| Kỹ năng | cd 320 mở list → cd 322, guard `this.b != 4 && this.c != skillId` (`fr.java:280-294`) | panel skill, **không nằm trong Lock()** | `[81][37][4][int]` |
| Vật phẩm | cd 321 → server trả menu chọn item | nút Thuốc, itemId hardcode 0 | `[81][37][3][int 0]` |
| Xin thua | **JAR không gửi sub 5** — "Bỏ cuộc" (`gw.a(129)`) đi đường khác (`cd(41,...)`) | có gửi sub 5 | `[81][37][5]` |

**Điểm quan trọng:** JAR khoá theo **trạng thái lượt** (`fr.b`/`fr.c` = sub-cmd đang chờ,
reset về -1 khi nhận gói 37 — `dj.a(en)` @3894), không khoá theo thời gian.
Unity khoá theo **timer 3.5s** và **skill không được khoá** → có thể double-send skill
trong 1 lượt. Cách của JAR đúng hơn về mặt logic.

## 6. Render kết quả lượt (opcode 37)

| | JAR | Unity |
|---|---|---|
| Kiến trúc | **Hàng đợi animation** `Vector a` of `e`, drain **1 step/frame** (`di.java:38-89`) → tuần tự, không chồng lấn | Apply tất cả effect **ngay lập tức** trong 1 frame (`BattleView.cs:61-78`) |
| Phân loại effect `type` | `<0` số damage trần; `0..2` hit/né/chí mạng; `101..124` frame-anim; `>=125` nạp `.anu` từ `/pet/battle/skills/<id>.anu` (`ei.java:325-348`) | `skillId >= 125` → `BattleActorEffectView` `.anu`; còn lại map thô |
| Float text | đúng **1000ms**, HP rồi MP **cách nhau ~1s** (`bd.java:167-195`, `ei.java:213-291`) | hiện cùng lúc |
| Thanh HP | **lerp** (chia đôi delta mỗi frame, `bd.java:196-240`) | snap |
| Đòn cận chiến | lao ±20px tới mục tiêu → FX → lùi về (`ei.java:202-211`) | không có lunge |
| Âm thanh | `s_hit` theo từng frame FX (`bd.java:122,127`), `s_attack_crit` cho `.anu` (`:273`) | `PlayHitSound` 1 lần/effect theo skillId 1=miss 2=crit |

Đây là khác biệt **cảm giác game** lớn nhất sau input. JAR có nhịp diễn hoạt;
Unity đang "tính xong hiện luôn".

## 7. Kết thúc trận

| Opcode | JAR | Unity |
|---|---|---|
| 16 `PET_BATTLE_STATE` | anim chết → thưởng **so le 1s**: "N (ngoc)" rồi "N EXP" rồi "Nhận được <item>"; thua → dialog "Thua rồi, bạn có muốn về thành phố để điều trị?" (bỏ qua nếu map 12) (`e.java` case 8) | panel kết quả tĩnh 1 dòng "Ngọc: N    EXP: N" (`BattleResultPanel.cs`) |
| 99 `FAST_REMOVE_MOB` | tháo trận tức thì, không anim, hiện lại avatar pet (`e.java` case 12) | `Close()` |
| 18 `UPDATE_PET_LVL` | hiệu ứng hạt `ea` + tăng cấp hiển thị + sound + popup "Chúc mừng! Pet đã đạt cấp N" (`dj.java` @1859-1942) | **chỉ phát sound**, vứt giá trị level (`GameSession.cs:164`) |
| 17 (emote trong trận) | có — `fr.java:727-738`, anim `bi` | **không parse** |

## 8. Danh sách gap Unity cần xử lý (xếp theo mức độ)

**P0 — rủi ro ban acc / sai logic**
1. Không throttle `ATTACK_MOB`. Server ban acc nếu < 4500ms trên map 12. → thêm throttle
   + guard "đang trong trận" trước `SendAttackMob`.
2. Nút skill không bị khoá sau khi bấm → double-send. Nên đổi sang model `fr.b`/`fr.c` của
   JAR (khoá theo lượt, mở khi gói 37 về) thay vì timer 3.5s.
3. Overlay trận cho click xuyên xuống quái phía dưới (`raycastTarget=false` ở
   `BattleView.cs:97`, `BattlePetCard.cs:52`) → đang trong trận vẫn bấm được quái khác.

**P1 — thiếu tính năng có trong JAR**
4. Auto-attack thiếu nhánh tự đánh trong trận (4s) và throttle 1s khi tìm quái.
5. Không có đồng hồ lượt (`RemainingMs` parse rồi vứt) — JAR có pie đỏ trên đầu sprite.
6. `UPDATE_PET_LVL` chỉ kêu tiếng, không popup/không cập nhật cấp.
7. Opcode 17 (emote trong trận) chưa parse.
8. Không xem được trận của người chơi khác.
9. Quái không bị ẩn khi vào trận và không được add lại khi thua.

**P2 — cảm giác game**
10. Không có hàng đợi animation → mọi effect nổ cùng lúc. Cần port `di`/`e` (1 step/frame).
11. Thanh HP snap thay vì lerp; float text HP/MP không so le; thiếu lunge cận chiến.
12. Thiếu hiệu ứng nổ tia trắng khi bấm vào quái.
13. `SendUseItem` hardcode itemId 0 — JAR để server trả menu chọn item, Unity chưa xử lý
    menu đó.

## 9. Quyết định cần user chốt

- **Input đánh quái**: giữ click chuột (Unity) hay port cơ chế đâm-vào-quái của JAR,
  hay hỗ trợ cả hai? Ảnh hưởng trực tiếp tới mục 1-3 ở trên.
- **Màn hình trận**: JAR đánh ngay trên map, Unity overlay full-screen theo mockup
  `260917-2238-pet-vs-mob-battle-screen`. Overlay là quyết định đã chốt trước đó →
  mục 5 (xem trận người khác), 9 (ẩn quái) có còn ý nghĩa không?
- Có port hàng đợi animation (mục 10-11) không, hay chấp nhận hiện tại?

## Câu hỏi chưa giải quyết

- Sub-cmd 2 của `PET_BATTLE` (cd 319 trong `fr.java:252-263`) là dead code hai phía —
  có phải "Phòng thủ/Trốn" bị gỡ? Không ảnh hưởng parity.
- JAR không gửi `PET_BATTLE_SURRENDER(5)`; Unity có. Server chấp nhận cả hai → không lỗi,
  nhưng là tính năng mới, không phải parity.
- `ResyncDenied` trong `SkillCooldownTracker` là dead code — ai đáng lẽ gọi nó?
- PvE server gửi trường turn-time là **thời gian đã trôi** (`PetBattle.cs:332`
  `(DateTime.Now - MobAttackTime).TotalSeconds` — đơn vị **giây**), PvP gửi **thời gian
  còn lại** (ms). JAR đọc cả hai như nhau vào `di.a(int,int)` → pie lượt PvE có thể sai.
  Cần kiểm chứng trên server chạy thật.
