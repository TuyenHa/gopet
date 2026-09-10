using Gopet.Net;
using Gopet.Net.Auth;
using Gopet.Net.Chat;
using Gopet.Net.Map;
using Gopet.Net.Guider;
using Gopet.Net.Guild;
using Gopet.Net.Battle;
using Gopet.Net.Pet;
using Gopet.Net.Player;
using Gopet.Net.Social;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Ráp phần gameplay sau LOGIN_SUCCES: <see cref="MapScene"/>, <see cref="MapHandler"/>,
    /// <see cref="ChatHandler"/>, <see cref="CameraFollower"/>, <see cref="MovementController"/>.
    ///
    /// <para><b>Server TỰ ĐỘNG</b> đẩy player vào map 11 (làng khởi đầu) qua
    /// <c>controller.LoadMap()</c> chạy ngay sau <c>loginOK()</c> (xem <c>Player.cs:522</c>).
    /// Client KHÔNG cần gửi <c>ON_PLAYER_CHANGE_CHANNEL</c> — gửi thừa sẽ gây double
    /// <c>place.remove + place.add</c>, bơm INIT/EXIT/ENTER lặp và có thể xoá nhầm avatar
    /// vừa spawn.</para>
    ///
    /// <para>Server chỉ gửi <c>ON_PLAYER_ENTER_MAP</c> (opcode 24) BROADCAST — self không
    /// nhận vì server thêm self vào <c>players</c> SAU sendNewPlayer. Self nhận
    /// <c>INIT_PLAYER</c> (31) + <c>ON_UPDATE_PLAYER_IN_MAP</c> (29). Đây là gói mang
    /// toạ độ spawn.</para>
    /// </summary>
    public sealed partial class GameSession
    {
        /// <summary>Map khởi đầu — làng, cũng là nền màn đăng nhập.</summary>
        public const int DefaultMapId = 11;

        /// <summary>Khu vực khởi đầu — hầu hết map có ít nhất place 0.</summary>
        public const int DefaultPlaceId = 0;

        private readonly GopetClient _client;
        private readonly LoginSuccess _login;
        private MapScene _scene;
        private MapHandler _mapHandler;
        private ChatHandler _chatHandler;
        private CameraFollower _camera;
        private WorldObjectHandler _worldHandler;
        private WorldStatusHandler _worldStatusHandler;
        private ChannelHandler _channelHandler;
        private MapTeleportHandler _mapTeleportHandler;
        private MovementController _movement;
        private GameHud _hud;
        private BattleHandler _battleHandler;
        private BattleCoordinator _battle;
        private PlayerStatsHandler _statsHandler;
        private CurrencyBar _currency;
        private ExpBuffIndicator _expBuffIndicator;
        private CharacterMenuButton _menuButton;
        private CharacterMenuView _menuView;
        private PetActionButton _petButton;
        private PetActionRadial _petRadial;
        private float _petActionCooldownUntil;
        private Transform _hudParent;
        private LetterHandler _letterHandler;
        private MailboxView _mailboxView;
        private RemoteAssetCache _assets;
        private PetEquipHandler _petEquipHandler;
        private PetEquipView _petEquipView;
        private ChoiceDialogView _channelDialog;
        private ChoiceDialogView _teleportDialog;
        private PetZoneHandler _petZoneHandler;
        private PetLayer _petLayer;
        private bool _hasPetFollowing;
        private GuildInfoHandler _guildInfoHandler;
        private CharacterAnimationHandler _characterAnimationHandler;
        private CharacterAnimationLayer _characterAnimationLayer;
        private CharacterSkinHandler _characterSkinHandler;
        private CharacterSkinLayer _characterSkinLayer;
        private CharacterWingLayer _characterWingLayer;
        private WarpFadeOverlay _warpFade;
        /// <summary>User bấm menu "Trang bị pet" mở lần này → tự spawn view khi EQUIP_INFO tới.
        /// Nếu không có cờ, EQUIP_INFO đến do server tự bơm (sau equip/unequip) → chỉ update view
        /// đang mở, không tự spawn mới.</summary>
        private bool _petEquipRequestPending;
        private readonly ActionThrottle _actionThrottle = new ActionThrottle();

        private GameSession(GopetClient client, LoginSuccess login)
        {
            _client = client;
            _login = login;
        }

        public MapScene Scene => _scene;

        /// <summary>Gọi sau LOGIN_SUCCES. Tự dựng scene, wire handler, xin server cho vào map.</summary>
        public static GameSession Start(GopetClient client, LoginSuccess login,
            RemoteAssetCache assets, GuiderHandler guider, Transform parent = null, WingHandler wings = null)
        {
            var s = new GameSession(client, login);
            s._scene = MapScene.Create(parent);
            s._scene.LoadMap(DefaultMapId);

            s._mapHandler = new MapHandler(client.Send);
            s._mapHandler.RegisterOn(client.Router);
            s._scene.Subscribe(s._mapHandler);

            s._chatHandler = new ChatHandler(client.Send);
            s._chatHandler.RegisterOn(client.Router);
            s._scene.SubscribeChat(s._chatHandler);

            s._worldHandler = new WorldObjectHandler();
            s._worldHandler.RegisterOn(client.Router);

            s._worldStatusHandler = new WorldStatusHandler();
            s._worldStatusHandler.RegisterOn(client.Router);

            s._channelHandler = new ChannelHandler(client.Send);
            s._channelHandler.RegisterOn(client.Router);
            s._channelHandler.ChannelsReceived += s.OnChannelsReceived;

            s._mapTeleportHandler = new MapTeleportHandler(client.Send);
            s._mapTeleportHandler.RegisterOn(client.Router);
            s._mapTeleportHandler.OptionsReceived += s.OnTeleportOptionsReceived;

            s._battleHandler = new BattleHandler(client.Send, login.UserId);
            s._battleHandler.RegisterOn(client.Router);
            s._scene.SubscribeWorld(s._worldHandler, assets, guider.TalkToNpc,
                s._battleHandler.SendAttackMob);

            var mainCamera = EnsureMainCamera();
            if (mainCamera.GetComponent<Physics2DRaycaster>() == null)
                mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
            s._camera = CameraFollower.Attach(mainCamera, s._scene);
            s._hud = GameHud.Create(parent ?? s._scene.transform, s._chatHandler, login.Name);
            s._hud.Send = client.Send;
            s._hud.Character.BindAssets(assets);
            guider.BannerShown += s._hud.Ticker.Show;
            s._worldStatusHandler.BossHpUpdated += s._scene.ApplyBossHp;
            s._worldStatusHandler.PlaceTimeUpdated += s._hud.ShowPlaceTime;
            s._worldStatusHandler.BigTextShown += s._hud.ShowBigText;
            s._battle = new BattleCoordinator(parent ?? s._scene.transform, assets,
                s._battleHandler, s.SetBattleMode);

            // Stats & tiền tệ nhân vật — MONEY_INFO/STAR_INFO/ENERGY_INFO của PET_SERVICE.
            // Char KHÔNG có HP/MP/Level; đó là stat pet (xem CharacterHud comment).
            s._statsHandler = new PlayerStatsHandler();
            s._statsHandler.RegisterOn(client.Router);
            // Canvas overlay riêng cho HUD phụ + popup. ScreenSpaceOverlay + sortOrder 35
            // để ngồi trên GameHud (30) nhưng dưới BattleView (thường 40+). Nếu attach
            // trực tiếp vào world transform sẽ KHÔNG hiện — UI cần Canvas parent.
            s._hudParent = CreateHudOverlayCanvas(parent ?? s._scene.transform).transform;
            s._currency = CurrencyBar.Create(s._hudParent, assets);
            s._statsHandler.StatsUpdated += stats => s._currency.ApplyStats(stats);
            s._expBuffIndicator = ExpBuffIndicator.Create(s._hudParent);
            s._worldStatusHandler.ExpBuffUpdated += status => s._expBuffIndicator.Apply(status, assets);

            s._menuButton = CharacterMenuButton.Create(s._hudParent);
            s._menuButton.Clicked += s.OpenCharacterMenu;

            s._petButton = PetActionButton.Create(s._hudParent);
            s._petButton.Clicked += s.OpenPetRadial;

            s._letterHandler = new LetterHandler();
            s._letterHandler.RegisterOn(client.Router);
            s._letterHandler.MailboxReceived += s.OnMailboxReceived;
            s._letterHandler.HasLetterReceived += n =>
                Debug.Log($"[Gopet] HAS_LETTER: {(n.HasUnread ? "có thư mới" : "hết thư mới")}");

            s._assets = assets;
            s._petEquipHandler = new PetEquipHandler();
            s._petEquipHandler.RegisterOn(client.Router);
            s._petEquipHandler.EquipInfoReceived += s.OnPetEquipInfo;

            // Pet-follow render — SEND_LIST_PET_ZONE khi vào map + PET_UNFOLLOW + MY_PET_INFO.
            // Bang: nghe CLAN_INFO để có clanId cho chat SEND.
            s._guildInfoHandler = new GuildInfoHandler();
            s._guildInfoHandler.RegisterOn(client.Router);
            s._hud.ClanIdProvider = () => s._guildInfoHandler.ClanId;
            // Prime: xin CLAN_INFO ngay để user có thể chat bang mà không cần gõ 2 lần.
            client.Send(GuildPackets.RequestClanInfo());

            s._petZoneHandler = new PetZoneHandler();
            s._petZoneHandler.RegisterOn(client.Router);
            s._petLayer = PetLayer.Create(s._scene.transform, assets,
                userId => s._scene.TryGetAvatarTransform(userId));
            s._petZoneHandler.PetZoneReceived += update =>
            {
                s._petLayer.ApplyZone(update);
                Debug.Log($"[Gopet] SEND_LIST_PET_ZONE — {update.Entries.Length} pet trong zone.");
                // Tìm pet của self để bind portrait + level vào CharacterHud.
                var selfFound = false;
                foreach (var e in update.Entries)
                {
                    if (e.OwnerUserId == login.UserId)
                    {
                        s._hud.Character.SetPetPortrait(e.FrameImagePath);
                        s._hud.Character.SetLevel(e.Level);
                        s._hud.Character.SetName(e.DisplayName);
                        selfFound = true;
                        break;
                    }
                }
                s._hasPetFollowing = selfFound;
            };
            s._petZoneHandler.PetUnfollowed += u =>
            {
                s._petLayer.Remove(u.OwnerUserId);
                if (u.OwnerUserId == login.UserId) s._hasPetFollowing = false;
            };
            s._petZoneHandler.MyPetInfoReceived += p =>
            {
                // HP/MP self pet — nối vào CharacterHud (3 thanh còn "--" ở Phase 1).
                s._hud.Character.SetStats(p.Hp, p.MaxHp, p.Mp, p.MaxMp, 0, 100);
            };

            // Danh hiệu/decoration đang dùng của mọi người trong map. Server có thể gửi danh
            // sách trước khi avatar được dựng; CharacterAnimationLayer giữ pending theo userId.
            s._characterAnimationHandler = new CharacterAnimationHandler();
            s._characterAnimationHandler.RegisterOn(client.Router);
            s._characterAnimationLayer = new CharacterAnimationLayer(
                s._scene, assets, s._characterAnimationHandler);

            s._characterSkinHandler = new CharacterSkinHandler();
            s._characterSkinHandler.RegisterOn(client.Router);
            s._characterSkinLayer = new CharacterSkinLayer(s._scene, assets, s._characterSkinHandler);

            if (wings == null)
            {
                wings = new WingHandler(client.Send);
                wings.RegisterOn(client.Router);
            }
            s._characterWingLayer = new CharacterWingLayer(s._scene, assets, wings);

            // Đổi map → camera phải recenter theo map MỚI. Không thì nó đứng chỗ cũ và
            // với map nhỏ hơn sẽ nhìn hoàn toàn ra ngoài.
            s._scene.MapLoaded += () => s._camera?.Recenter();

            // Gắn MovementController khi SELF vừa spawn — sự kiện đến từ opcode 29
            // (ON_UPDATE_PLAYER_IN_MAP), KHÔNG phải opcode 24 (ON_PLAYER_ENTER_MAP)
            // vì server broadcast ENTER_MAP TRƯỚC khi thêm self vào players, nên self
            // không nhận được gói của chính mình.
            s._scene.SelfSpawned += s.OnSelfSpawned;
            // Sau khi self avatar dựng, thử flush pet đang chờ owner (thường là chính self).
            s._scene.SelfSpawned += evt => s._petLayer.OnOwnerSpawned(evt.UserId);
            s._warpFade = WarpFadeOverlay.Create(s._hudParent);
            s._scene.PortalSelected += portal =>
            {
                s._warpFade.FadeOut();
                s._mapHandler.SendWarp(portal.ExtraA, portal.ExtraB, 1);
            };
            // Sau khi map mới nạp xong → mở fade (đã có MapLoaded ở Recenter phía trên).
            s._scene.MapLoaded += () => s._warpFade.FadeIn();
            s._scene.BuildingSelected += s.OnBuildingSelected;
            s._scene.AvatarTapped += s.OnAvatarTapped;

            // Debug: log các sự kiện chính để dễ chẩn đoán khi avatar không hiện.
            s._mapHandler.PlayerInitReceived += p =>
            {
                s._hud.Character.SetName(p.Name);
                Debug.Log($"[Gopet] INIT_PLAYER #{p.UserId} {p.Name} gender={p.Gender}");
            };
            s._mapHandler.MapUpdated += m => Debug.Log($"[Gopet] MAP_UPDATE map={m.MapId} zone={m.ZoneId} self=({m.SelfX},{m.SelfY}) others={m.Others.Length}");
            s._mapHandler.PlayerEntered += p => Debug.Log($"[Gopet] PLAYER_ENTER #{p.UserId} {p.Name} at ({p.X},{p.Y})");
            client.Router.OnUnhandled = m => Debug.Log($"[Gopet] Bỏ opcode {m.Id} (chưa có handler) — {m.Reader.Remaining} byte thân");

            // KHÔNG gửi changeChannel — server đã tự đẩy player vào map 11 qua LoadMap()
            // ngay sau loginOK (Player.cs:522). Gửi thừa gây double init/exit/enter.
            return s;
        }

        private void OnSelfSpawned(PlayerEnterMap evt)
        {
            if (_movement == null)
            {
                _movement = MovementController.Attach(_scene, _mapHandler, _camera, _hud,
                    mapId: _scene.MapId, userId: evt.UserId,
                    initialJarX: evt.X, initialJarY: evt.Y);
                _hud?.Ticker?.Show(
                    "Chào mừng bạn đến với Gopet! Đi đánh quái để nhận EXP và vật phẩm.   " +
                    "Chơi game quá 180 phút liên tục sẽ ảnh hưởng đến sức khỏe — nhớ nghỉ ngơi nhé!");
            }
            else
            {
                _movement.ResetForMap(_scene.MapId, evt.X, evt.Y);
            }
        }

        private void SetBattleMode(bool active)
        {
            if (_movement != null) _movement.InputEnabled = !active;
            if (_hud != null) _hud.SetBattleMode(active);
        }

        private void OpenCharacterMenu()
        {
            if (_menuView != null) return;    // đã mở
            _menuView = CharacterMenuView.Create(_hudParent);
            _menuView.CloseRequested += CloseCharacterMenu;
            _menuView.ItemSelected += OnCharacterMenuAction;
        }

        private void CloseCharacterMenu()
        {
            if (_menuView == null) return;
            Object.Destroy(_menuView.gameObject);
            _menuView = null;
        }

        private void OnCharacterMenuAction(CharacterMenuAction action)
        {
            if (CharacterMenu.TryBuildServerMessage(action, out var msg))
            {
                _client.Send(msg);
                Debug.Log($"[Gopet] Menu -> gửi {action} (opcode {msg.Id}). Chờ handler (Phase 4/5).");
                CloseCharacterMenu();
                return;
            }

            switch (action)
            {
                case CharacterMenuAction.PlaceChat:
                    // Chat khu vực — chỉ đơn giản là mở focus vào chat input.
                    // GameHud giữ input; MessageRouter đã register ON_PLACE_CHAT.
                    Debug.Log("[Gopet] Menu -> tập trung vào chat khu vực (đã có sẵn ở dưới HUD).");
                    break;
                case CharacterMenuAction.ChangePassword:
                    OpenChangePassword();
                    break;
                case CharacterMenuAction.Settings:
                    Debug.Log("[Gopet] Menu -> Cài đặt (đợi Phase 5).");
                    break;
                case CharacterMenuAction.Logout:
                    Debug.Log("[Gopet] Menu -> Đăng xuất (đợi Phase 5 wire LoginFlow.Reset).");
                    break;
                case CharacterMenuAction.PetEquipment:
                    _petEquipRequestPending = true;
                    _client.Send(PetEquipPackets.RequestEquipInfo(_login.UserId));
                    Debug.Log("[Gopet] Menu -> yêu cầu EQUIP_INFO cho self.");
                    break;
                case CharacterMenuAction.Channels:
                    _channelHandler.RequestChannels();
                    break;
                case CharacterMenuAction.Teleport:
                    _mapTeleportHandler.RequestOptions();
                    break;
                case CharacterMenuAction.Exit:
                    Application.Quit();
                    break;
            }
            CloseCharacterMenu();
        }

        private void OnPetEquipInfo(PetEquipInfo info)
        {
            // Chỉ mở view mới nếu user vừa bấm menu; nếu không, chỉ update view đang mở
            // (sau equip/unequip server auto-bumps EQUIP_INFO — không mở popup ngoài ý).
            if (_petEquipView == null)
            {
                if (!_petEquipRequestPending) return;
                _petEquipView = PetEquipView.Create(_hudParent, _assets);
                _petEquipView.CloseRequested += ClosePetEquipView;
                _petEquipView.ActionChosen += OnPetEquipAction;
                _petEquipView.EmptySlotTapped += _ =>
                {
                    _client.Send(PetEquipPackets.RequestNormalInventory());
                    Debug.Log("[Gopet] Slot rỗng -> yêu cầu NORMAL_INVENTORY menu.");
                };
            }
            _petEquipRequestPending = false;
            _petEquipView.ApplyInfo(info);
        }

        private void ClosePetEquipView()
        {
            if (_petEquipView == null) return;
            Object.Destroy(_petEquipView.gameObject);
            _petEquipView = null;
        }

        private void OnPetEquipAction(PetEquipItem item, PetSlotActionsView.Action action)
        {
            switch (action)
            {
                case PetSlotActionsView.Action.Unequip:
                    _client.Send(PetEquipPackets.Unequip(item.ItemId));
                    break;

                case PetSlotActionsView.Action.MountGem:
                    _client.Send(PetEquipPackets.SelectGem(item.ItemId));
                    break;

                case PetSlotActionsView.Action.Enchant:
                    OpenEnchantEvolve(item, EnchantEvolveView.Mode.Enchant);
                    break;

                case PetSlotActionsView.Action.UpTier:
                    OpenEnchantEvolve(item, EnchantEvolveView.Mode.UpTier);
                    break;

                case PetSlotActionsView.Action.Destroy:
                    OpenDestroyConfirm(item);
                    break;
            }
        }

        private EnchantEvolveView _enchantView;
        private YesNoDialog _destroyDialog;
        private TargetPlayerMenu _targetMenu;

        /// <summary>
        /// Mở menu Xem info / Thách đấu / PK / Xem đồ / Kết bạn cho 1 player khác.
        /// Gọi từ tầng world actor khi user tap 1 avatar khác.
        /// </summary>
        public void OpenTargetPlayerMenu(int userId, string name)
        {
            if (_targetMenu != null) Object.Destroy(_targetMenu.gameObject);
            _targetMenu = TargetPlayerMenu.Create(_hudParent, userId, name);
            _targetMenu.CloseRequested += () =>
            {
                if (_targetMenu != null) Object.Destroy(_targetMenu.gameObject);
                _targetMenu = null;
            };
            _targetMenu.Chosen += action =>
            {
                if ((action == TargetPlayerMenu.Action.Challenge || action == TargetPlayerMenu.Action.Pk) &&
                    !_actionThrottle.TryAcquire($"target:{action}", 1500, out var remainingMs))
                {
                    ShowToast($"Thao tác quá nhanh, thử lại sau {(remainingMs + 999) / 1000} giây.");
                    return;
                }
                Message msg = action switch
                {
                    TargetPlayerMenu.Action.ViewInfo  => Gopet.Net.Social.TargetPlayerPackets.RequestInfo(userId),
                    TargetPlayerMenu.Action.Challenge => Gopet.Net.Social.TargetPlayerPackets.Challenge(userId),
                    TargetPlayerMenu.Action.Pk        => Gopet.Net.Social.TargetPlayerPackets.SendPk(userId),
                    TargetPlayerMenu.Action.ViewEquip => Gopet.Net.Social.TargetPlayerPackets.ViewEquipment(userId),
                    TargetPlayerMenu.Action.AddFriend => Gopet.Net.Social.FriendPackets.AddFriendById(userId),
                    _ => null,
                };
                if (msg != null) _client.Send(msg);
                if (_targetMenu != null) Object.Destroy(_targetMenu.gameObject);
                _targetMenu = null;
            };
        }

        private void OpenEnchantEvolve(PetEquipItem item, EnchantEvolveView.Mode mode)
        {
            if (_enchantView != null) Object.Destroy(_enchantView.gameObject);
            _enchantView = EnchantEvolveView.Create(_hudParent, mode, item.ItemId, item.DisplayName);
            _enchantView.CloseRequested += () =>
            {
                if (_enchantView != null) Object.Destroy(_enchantView.gameObject);
                _enchantView = null;
            };
            _enchantView.Confirmed += (matA, matB) =>
            {
                var msg = mode == EnchantEvolveView.Mode.Enchant
                    ? PetEquipPackets.ConfirmEnchant(item.ItemId, matA, matB)
                    : PetEquipPackets.UpTierItem(item.ItemId, matA);
                _client.Send(msg);
                Debug.Log($"[Gopet] {mode} #{item.ItemId} mat=({matA},{matB}) → opcode {msg.Id}");
            };
        }

        private void OpenDestroyConfirm(PetEquipItem item)
        {
            if (_destroyDialog != null) Object.Destroy(_destroyDialog.gameObject);
            _destroyDialog = YesNoDialog.Create(_hudParent,
                $"Xác nhận HUỶ {item.DisplayName}?\nHành động không thể hoàn tác.",
                "Huỷ đồ", "Không");
            _destroyDialog.Confirmed += () =>
            {
                _client.Send(PetEquipPackets.RequestDestroyEquip(item.ItemId));
                Debug.Log($"[Gopet] Huỷ item #{item.ItemId} — chờ server YN dialog xác nhận lần 2.");
                if (_destroyDialog != null) Object.Destroy(_destroyDialog.gameObject);
                _destroyDialog = null;
            };
            _destroyDialog.Cancelled += () =>
            {
                if (_destroyDialog != null) Object.Destroy(_destroyDialog.gameObject);
                _destroyDialog = null;
            };
        }

        private ChangePasswordView _passwordView;

        private void OpenChangePassword()
        {
            if (_passwordView != null) return;
            _passwordView = ChangePasswordView.Create(_hudParent);
            _passwordView.Send = _client.Send;
            _passwordView.CloseRequested += () =>
            {
                if (_passwordView != null) Object.Destroy(_passwordView.gameObject);
                _passwordView = null;
            };
        }

        private void OpenPetRadial()
        {
            if (_petRadial != null) return;
            _petRadial = PetActionRadial.Create(_hudParent);
            _petRadial.CloseRequested += ClosePetRadial;
            _petRadial.ActionSelected += OnPetAction;
        }

        private void OnMailboxReceived(Mailbox mailbox)
        {
            if (_mailboxView != null) Object.Destroy(_mailboxView.gameObject);
            _mailboxView = MailboxView.Create(_hudParent, mailbox);
            _mailboxView.CloseRequested += () =>
            {
                if (_mailboxView != null) Object.Destroy(_mailboxView.gameObject);
                _mailboxView = null;
            };
        }

        private void ClosePetRadial()
        {
            if (_petRadial == null) return;
            Object.Destroy(_petRadial.gameObject);
            _petRadial = null;
        }

        private void OnPetAction(PetActionRadial.Action action)
        {
            if (Time.unscaledTime < _petActionCooldownUntil) return;
            _petActionCooldownUntil = Time.unscaledTime + 0.5f;

            Message msg;
            switch (action)
            {
                case PetActionRadial.Action.Play:
                    msg = PetActionPackets.Interact(PetActionPackets.Play);
                    break;
                case PetActionRadial.Action.Kiss:
                    msg = PetActionPackets.Interact(PetActionPackets.Kiss);
                    break;
                case PetActionRadial.Action.Poke:
                    msg = PetActionPackets.Interact(PetActionPackets.Poke);
                    break;
                case PetActionRadial.Action.Heal:
                    msg = PetActionPackets.Heal();
                    break;
                default:
                    return;
            }

            _client.Send(msg);
            Debug.Log($"[Gopet] Pet action -> {action} sent.");
            ClosePetRadial();
        }

        /// <summary>
        /// Dựng Canvas ScreenSpaceOverlay riêng cho các HUD phụ (CurrencyBar, MenuButton,
        /// PetButton) và các popup (CharacterMenuView, PetActionRadial, MailboxView,
        /// EnchantEvolveView, YesNoDialog, TargetPlayerMenu, ChangePasswordView, PetEquipView).
        ///
        /// <para><b>Bắt buộc:</b> UI của uGUI cần Canvas parent. Attach trực tiếp vào
        /// world transform (như <c>_scene.transform</c>) thì nút không hiện, không bấm được.
        /// Đây là nguyên nhân giao diện phase 5-8 không xuất hiện lần đầu chạy.</para>
        /// </summary>
        private static GameObject CreateHudOverlayCanvas(Transform parent)
        {
            var go = new GameObject("HUD Overlay", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 35;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 1f;
            return go;
        }

        private static Camera EnsureMainCamera()
        {
            if (Camera.main != null) return Camera.main;

            var go = new GameObject("MainCamera", typeof(Camera));
            go.tag = "MainCamera";
            return go.GetComponent<Camera>();
        }
    }
}
