# Kế hoạch triển khai popup kết quả thắng quái

> **Trạng thái 22/09/2026:** Người dùng đã yêu cầu triển khai. Đã sửa mã và viết kiểm thử; 882 kiểm thử logic đạt. Kiểm thử Runtime/PlayMode và nghiệm thu trực quan còn chờ môi trường Unity. Xem [báo cáo thực hiện](2026-09-22-popup-ket-qua-thang-quai-ket-qua.md). Các ô “thêm/viết kiểm thử” đã đánh dấu chỉ xác nhận test đã được viết, không có nghĩa PlayMode đã chạy.

> **Dành cho người triển khai:** Sử dụng `superpowers:executing-plans` để thực hiện lần lượt các công việc và đánh dấu các ô hoàn thành.

**Mục tiêu:** Sau khi thắng quái trong Unity, hiển thị một popup tổng kết vật phẩm nhận được, thời gian trận đấu và kỹ năng pet đã sử dụng; bấm **OK** để trở lại map hiện tại.

**Kiến trúc:** Giữ phần phát thưởng ở server. Unity dùng dữ liệu kết quả hiện có, ghi nhận thời gian và kỹ năng theo từng trận, rồi dựng popup sau khi hoạt cảnh cuối kết thúc. Nút OK đóng lớp màn chiến đấu thông qua `BattleCoordinator`, không gửi yêu cầu dịch chuyển map hay nhận thưởng lần nữa.

**Công nghệ:** Unity 6, C# 9, uGUI, xUnit cho logic thuần C#, Unity Test Runner cho PlayMode.

**Đặc tả:** Yêu cầu của người dùng trong cuộc trò chuyện ngày 22/09/2026 và mục “Thiết kế đề xuất” ngay trong tài liệu này. Người dùng đã duyệt bằng yêu cầu thực hiện kế hoạch.

## Thiết kế đề xuất

### Hành vi người chơi

1. Người chơi đánh thắng quái, nhận gói kết quả từ server.
2. Chốt số liệu trận ngay lúc nhận kết quả đầu tiên; chờ hoạt cảnh đòn cuối diễn xong.
3. Hiện popup **CHIẾN THẮNG** gồm tên quái, thời gian, phần thưởng và danh sách kỹ năng.
4. Popup giữ nguyên cho đến khi bấm **OK**. Không tự đóng theo thời gian, không đóng khi chạm nền, không có nút X. Nút quay lại của màn đấu không được bỏ qua popup.
5. Bấm OK một lần: đóng màn chiến đấu, mở lại điều khiển nhân vật trên map đang có. Không tải lại map, không đổi khu, không đổi tọa độ, không hồi sinh về thành phố.

Ví dụ bố cục; tên và số liệu bên dưới chỉ minh họa:

```text
                  CHIẾN THẮNG
                Đã đánh bại: Sói

Thời gian: 1 phút 23 giây

Phần thưởng
  Ngọc: 120
  EXP kết thúc trận: 350
  Bùa cường hoá x1
  Hồng ngọc x1

Kỹ năng đã sử dụng
  Cào
  Sấm sét

                     [ OK ]
```

### Quy ước dữ liệu

- **Vật phẩm:** Hiển thị đầy đủ từng dòng `BattleResult.Messages` từ server. Hiện tại đây là chuỗi tên và số lượng, không có ID/icon vật phẩm riêng. Hiển thị văn bản, không tự đoán icon hoặc tách chuỗi theo ký tự `x`. Bỏ dòng trắng; nếu không có dòng nào thì ghi **Không nhận được vật phẩm**.
- **Ngọc và EXP:** Lấy `Coin` và `Experience` trong kết quả. Nhãn **EXP kết thúc trận** tránh nhầm với EXP nhỏ giọt đã nhận theo từng đòn. Không cộng thưởng trong client và không coi OK là thao tác lĩnh thưởng.
- **Thời gian:** Từ thời điểm client nhận bắt đầu trận tới khi nhận kết quả hợp lệ đầu tiên. Dùng đồng hồ đơn điệu không phụ thuộc `Time.timeScale`, ưu tiên `Time.realtimeSinceStartupAsDouble`; hiển thị tổng phút và giây, ví dụ `0 phút 08 giây`, `61 phút 02 giây`. Bỏ phần lẻ giây, chặn giá trị âm về 0. Thời gian gồm phần mở trận và chờ lượt; không gồm thời gian đọc popup hoặc chờ hoạt cảnh cuối sau gói kết quả. Đây là thời gian phía client, không phải thống kê được server lưu.
- **Kỹ năng:** Chỉ ghi kỹ năng của pet người chơi, lấy tên từ `BattleStart.LocalPet.Skills`. Xét `BattleTurn.ActorId` để biết người ra đòn; `BattleEffect.ActorId` có thể là mục tiêu nên không dùng nó để xác định chủ kỹ năng. Mỗi ID chỉ hiện một lần, theo thứ tự xuất hiện đầu tiên; chưa thêm số lần sử dụng.
- **Xác nhận kỹ năng:** Ghi nhận các ID kỹ năng thực trong `turn.Effects` của lượt người chơi, từ `BattleEffectNames.FirstSkillId` trở lên. ID lạ hiển thị `Kỹ năng #<id>`. Không tính marker 0/1/2, đòn thường, uống bình hoặc kỹ năng của quái. Kỹ năng buff và kỹ năng đã thi triển nhưng gây 0 sát thương vẫn được ghi nếu server gửi ID.
- **Kỹ năng không thi triển:** Thiếu MP, hồi chiêu, bị định thân hoặc trượt trước khi thi triển không được tính. Server hiện gửi marker trượt mà không gửi ID trong nhánh trượt trước khi thi triển (`PetBattle.cs:1100`); không suy đoán kỹ năng từ nút vừa bấm. Nếu không ghi nhận kỹ năng nào, hiện **Không sử dụng kỹ năng**.
- **Phạm vi:** Popup dành cho trận `BattleKind.Mob`, người chơi tham gia và `WinnerId == LocalPet.ActorId`. Thua quái, xin thua, PvP và xem trận giữ luồng hiện tại. Boss cũng có thể thuộc `BattleKind.Mob`; dùng thông báo thưởng server gửi, không tự suy diễn quyền nhận thưởng. Hộp thoại thưởng boss riêng hiện có cần được kiểm tra khi nghiệm thu, không mở rộng kế hoạch sang thay đổi phát thưởng boss.

## Ràng buộc chung

- Toàn bộ nhãn mới bằng tiếng Việt có dấu; nút xác nhận ghi đúng **OK**.
- Không thay đổi giao thức, server, JAR, tỷ lệ rơi đồ hoặc database.
- Dùng C# 9 và thành phần UI có sẵn, không thêm thư viện.
- Popup chỉ tạo một lần cho một trận, kể cả nhận lặp gói kết quả trong lúc hoạt cảnh còn chạy.
- Danh sách dài dùng vùng cuộn; tiêu đề và nút OK luôn nhìn thấy. Chặn click xuyên xuống nút chiến đấu/map.
- Khi đã có kết quả, chặn mọi hành động chiến đấu, kể cả lúc đang chờ diễn xong hoạt cảnh.
- Theo yêu cầu bổ sung của người dùng: cập nhật map và gói bắt đầu trận không được đóng hoặc thay popup thắng đang chờ OK, kể cả lúc còn đợi hoạt cảnh cuối. Map vẫn cập nhật phía dưới; chỉ OK mới đóng lớp kết quả để hiện map hiện tại. Mất kết nối/kết thúc phiên vẫn dọn phiên như hiện có.
- Tạo file `.meta` cho các script Unity mới bằng cơ chế của dự án, giữ GUID ổn định.

## Căn cứ mã nguồn

| File | Trạng thái hiện tại và vai trò khi sửa |
|---|---|
| `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs` | Cộng vật phẩm vào túi, gửi tên/số lượng qua gói kết quả; chỉ tham khảo |
| `GopetUnityClient/Assets/Scripts/Net/Battle/BattleHandler.cs` | Đã đọc `Messages`; không cần mở rộng packet |
| `GopetUnityClient/Assets/Scripts/Runtime/World/BattleView.cs` | Nhận lượt, dựng màn đấu, tự đóng sau 2,5 giây |
| `GopetUnityClient/Assets/Scripts/Runtime/World/BattleView.State.cs` | Hoãn kết quả tới cuối hoạt cảnh; hiện thưởng chỉ có ngọc/EXP |
| `GopetUnityClient/Assets/Scripts/Runtime/World/BattleView.Actions.cs` | Nút quay lại hiện có thể đóng kết quả trực tiếp |
| `GopetUnityClient/Assets/Scripts/Runtime/World/BattleCoordinator.cs` | Điều phối đóng màn đấu, FAST_REMOVE và timeout |

## Điểm cần kiểm tra kỹ

1. Kết quả tới trước khi hoạt cảnh cuối xong, sau đó FAST_REMOVE tới ngay: vẫn chỉ hiện một popup và phải chờ OK — kiểm thử ở công việc 2.
2. Đọc popup lâu hơn cả 2,5 giây và timeout trận: popup không bị tự đóng — kiểm thử ở công việc 2.
3. Kỹ năng buff trỏ vào pet mình, kỹ năng quái trỏ vào pet mình, kỹ năng trượt không mang ID: danh sách không ghi nhầm — kiểm thử ở công việc 1.
4. Không có đồ, tên dài, nhiều dòng thưởng, ký tự giống rich text: nội dung rõ ràng và nút OK không bị đẩy khỏi màn hình — kiểm thử ở công việc 2 và 3.
5. Bấm OK liên tiếp và bắt đầu trận mới: đóng đúng một lần, không cộng thưởng lại, không mang thời gian/kỹ năng trận cũ sang — kiểm thử ở công việc 1 và 2.

## Công việc 1: Thu thập và chốt dữ liệu tổng kết

**Tạo:**

- `GopetUnityClient/Assets/Scripts/UiLogic/BattleSummaryTracker.cs`
- `GopetUnityClient/Assets/Scripts/UiLogic/BattleSummarySnapshot.cs`
- `GopetUnityClient/tests/Gopet.Net.Tests/BattleSummaryTrackerTests.cs`

**Hợp đồng dự kiến:** Đặt các kiểu mới trong namespace `Gopet.UiLogic`. `BattleSummarySnapshot` cung cấp `OpponentName`, `DurationText`, `Coin`, `Experience`, `RewardLines`, `SkillNames`; danh sách được sao chép và chỉ đọc sau khi chốt.

```csharp
public BattleSummaryTracker(BattleStart start, double startedAtSeconds);
public void RecordTurn(BattleTurn turn);
public BattleSummarySnapshot Complete(BattleResult result, double endedAtSeconds);
```

- [x] Viết kiểm thử trước cho thời gian 0, 8, 83, 3662 giây, thời điểm kết thúc nhỏ hơn bắt đầu và kết quả lặp. `Complete` lặp trả cùng snapshot, không kéo dài thời gian; sai `BattleId` bị từ chối bằng `ArgumentException`.
- [x] Viết kiểm thử lượt người chơi có ID 101, lượt quái có ID 105, buff 103 lên bản thân, effect trùng ID, marker 0/1/2 và ID lạ. Danh sách chỉ có kỹ năng hợp lệ, giữ thứ tự xuất hiện, không nhân bản.
- [x] Viết kiểm thử phần thưởng rỗng/nhiều dòng, giữ nguyên chuỗi tên/số lượng; một tracker mới không nhận dữ liệu tracker cũ. Sau khi `Complete`, `RecordTurn` không thay đổi snapshot.

Ví dụ kiểm thử quan trọng, dùng fixture trực tiếp trong file kiểm thử mới:

```csharp
[Fact]
public void KetQuaLap_KhongTinhThemThoiGianDocPopup()
{
    var start = new BattleStart {
        BattleId = 7, Kind = BattleKind.Mob, IsParticipant = true,
        LocalPet = new BattlePet { ActorId = 7 },
        Opponent = new BattlePet { ActorId = -99, Name = "Sói" }
    };
    var tracker = new BattleSummaryTracker(start, 100d);
    var result = new BattleResult { BattleId = 7, WinnerId = 7 };
    var first = tracker.Complete(result, 183d);
    Assert.Equal("1 phút 23 giây", first.DurationText);
    Assert.Same(first, tracker.Complete(result, 300d));
}
```

- [x] Chạy kiểm thử mới, xác nhận thất bại vì thiếu chức năng; sau đó triển khai tracker thuần C#, không phụ thuộc Unity. Dùng tập ID để bỏ trùng và danh sách riêng để giữ thứ tự.
- [x] Chạy lại nhóm kiểm thử đến khi đạt:

```powershell
dotnet test GopetUnityClient/tests/Gopet.Net.Tests/Gopet.Net.Tests.csproj --filter FullyQualifiedName~BattleSummaryTrackerTests
```

## Công việc 2: Popup và vòng đời chờ OK

**Tạo:**

- `GopetUnityClient/Assets/Scripts/Runtime/World/Battle/BattleVictoryPopup.cs`
- `GopetUnityClient/Assets/Tests/PlayMode/BattleVictoryPopupTests.cs`

**Sửa:** `BattleView.cs`, `BattleView.State.cs`, `BattleView.Actions.cs`, `BattleCoordinator.cs` trong thư mục `GopetUnityClient/Assets/Scripts/Runtime/World/`.

**Hợp đồng popup:**

```csharp
public static BattleVictoryPopup Create(
    Transform parent, BattleSummarySnapshot summary, Action onOk);
```

`Create` trả component giữ root popup. Callback chỉ chạy một lần; vô hiệu nút ngay khi bấm. Dùng root `gameObject` làm `_result` để tiếp tục tận dụng vòng đời hiện tại.

- [x] Thêm kiểm thử PlayMode cho đầy đủ nội dung, trạng thái không có đồ/kỹ năng, chỉ một nút OK, chặn click nền, danh sách cuộn và callback một lần. Truy cập nút theo tên/component, không phụ thuộc thứ tự mảng tất cả nút trong màn đấu.
- [x] Dựng popup theo mẫu bố cục trong thiết kế. Dùng font/style UI dự án; nền mờ phủ màn đấu, panel giữa màn hình, `ScrollRect` cho nội dung, tiêu đề và OK cố định. Đặt `supportRichText = false` cho nội dung nhận từ server. Không dùng nguyên `ChoiceDialogView` hiện tại vì có nút X và cắt nội dung dài.
- [x] Tạo tracker trước khi `BattleView.Build` chạy. `Apply` ghi lượt đúng trận trước khi xếp hoạt cảnh; bỏ cập nhật thống kê sau khi đã có kết quả.
- [x] Tại lần nhận kết quả hợp lệ đầu tiên, chốt snapshot ngay cả khi animator chưa rảnh. Không để gói lặp ghi đè `_pendingResult`. Tách bước tiếp nhận/chốt dữ liệu khỏi bước dựng UI để `ShowPendingResult` không bị guard của chính nó ngăn hiển thị.
- [x] Với thắng PvE, dựng `BattleVictoryPopup` thay cho banner; hiển thị thưởng trong popup và không gọi `ShowRewards` lần nữa ở nhánh này. Nhánh thua/PvP vẫn dùng banner đang có.
- [x] Thêm trạng thái công khai `AwaitingVictoryConfirmation` cho `BattleView`, bật từ lúc chấp nhận kết quả thắng PvE đầu tiên, giữ tới lúc đóng. Điều kiện nhánh thắng:

```csharp
var victory = _start.Kind == BattleKind.Mob
    && _start.IsParticipant
    && result.WinnerId == _start.LocalPet.ActorId;
```

- [x] Trong `Update`, chỉ chạy tự đóng kết quả nếu không chờ xác nhận. Trong `BattleCoordinator.CheckStalled`, bỏ timeout khi có kết quả hợp lệ, kể cả còn chờ hoạt cảnh:

```csharp
if (_view == null || _view.HasResult || _timeoutSeconds <= 0f) return;
```

- [x] Giữ guard `HasResult` của `OnRemoved` để FAST_REMOVE không đóng mất popup. `RefreshLocks` dùng `!HasResult`; các handler hành động và quay lại cũng phải kiểm tra trạng thái kết quả. Khi chờ OK, nút quay lại không đóng màn hoặc mở xác nhận xin thua.
- [x] Nối OK vào `RequestClose`, dùng cờ `_closeRequested` chống bấm lặp. Coordinator đóng view và gọi `_setBattleMode(false)` như hiện tại; không gửi `SendAttackMob`, dịch chuyển hay yêu cầu lĩnh thưởng.
- [x] Thêm kiểm thử tích hợp cho kết quả lặp lúc animator đang chạy, FAST_REMOVE ngay sau kết quả, chờ quá thời gian tự đóng và timeout. Dùng gói qua router/coordinator để kiểm thử đường đi thực; xác nhận popup còn tồn tại, OK đóng một lần và callback đổi battle mode về false. Khi kiểm thử timeout dài, dùng seam đồng hồ kiểm thử hoặc điều chỉnh mốc riêng trong fixture, không bắt suite ngủ hàng phút.
- [ ] Kiểm thử trận thua/PvP vẫn kết thúc như trước; `OnPlaceChanged` chỉ dọn trận chưa chờ xác nhận thắng. Map/start cập nhật trong lúc chờ kết quả hoặc đang hiện popup phải giữ nguyên popup và khóa điều khiển đến OK. Kiểm thử hai trận liên tiếp để phát hiện thống kê chưa reset.

## Công việc 3: Xác minh và nghiệm thu

**Kiểm tra/sửa kiểm thử liên quan:** `GopetUnityClient/Assets/Tests/PlayMode/BattlePlayModeTests.cs`. Test hiện tại còn giả định số nút, tên object và thời điểm kết quả theo UI cũ; chỉ cập nhật các giả định chịu tác động để kiểm tra hành vi mới, không bỏ assertion nhằm làm suite xanh.

- [ ] Chạy toàn bộ kiểm thử logic và biên dịch Runtime/PlayMode từ thư mục gốc repository:

```powershell
dotnet test GopetUnityClient/tests/Gopet.Net.Tests/Gopet.Net.Tests.csproj
dotnet build GopetUnityClient/tests/Gopet.Runtime.UnityCompat/Gopet.Runtime.UnityCompat.csproj
dotnet build GopetUnityClient/tests/Gopet.PlayMode.Compile/Gopet.PlayMode.Compile.csproj
```

- [ ] Chạy PlayMode trong Unity Test Runner nếu Editor đang mở; nếu Editor đã đóng, dùng script hiện có. Không tự tắt Editor đang có công việc của người dùng:

```powershell
powershell -ExecutionPolicy Bypass -File GopetUnityClient/run-playmode-tests.ps1
```

- [ ] Kiểm tra trực quan trên màn hình ngang nhỏ và lớn: tiếng Việt không mất dấu, tên dài xuống dòng, nhiều phần thưởng cuộn được, OK luôn nằm trong màn hình. Chụp ảnh popup có thưởng và không có thưởng để đối chiếu.
- [ ] Đánh một trận thật bằng kỹ năng, kiểm tra popup và túi đồ; chờ trên popup rồi bấm OK, xác nhận vẫn ở đúng map/khu/vị trí và không nhận thưởng lần hai. Dùng dữ liệu gói giả lập trong test để bảo đảm trường hợp rơi đồ được kiểm tra, không sửa tỷ lệ rơi chỉ để nghiệm thu.
- [ ] Kiểm tra một trận boss nếu môi trường có boss: phân biệt popup tổng kết với hộp thoại thưởng boss riêng của server và ghi lại nếu có chồng lớp; không tuyên bố đã hợp nhất hai hộp thoại trong phạm vi này.
- [x] Chạy `git diff --check`, xem lại diff đúng các file dự kiến. Báo cáo riêng kết quả unit test, compile và PlayMode; nếu chỉ compile thì không ghi là đã kiểm thử UI.

## Điều kiện hoàn thành

- Thắng quái: hiện đúng một popup sau hoạt cảnh, có đầy đủ đồ nhận được, thời gian phút/giây và tên kỹ năng đã thi triển.
- Không rơi đồ hoặc không dùng kỹ năng: có thông báo rõ ràng.
- Để lâu, FAST_REMOVE, kết quả lặp và nút quay lại không làm popup biến mất ngoài ý muốn.
- OK trở về map hiện tại đúng một lần; đồ đã được server cộng không bị cộng lại.
- Dữ liệu được reset ở trận mới; thua/PvP không bị hỏng. Cập nhật map không làm ẩn popup thắng; bấm OK mới hiện map hiện tại.
- Các kiểm thử liên quan đạt, có bằng chứng kiểm tra giao diện hoặc nêu rõ phần chưa chạy.
