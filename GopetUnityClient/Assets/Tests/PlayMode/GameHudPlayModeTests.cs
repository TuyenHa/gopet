using System.Collections;
using Gopet.Net.Chat;
using Gopet.Net.Guider;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class GameHudPlayModeTests
    {
        [UnityTest]
        public IEnumerator Hud_GuiNoiDungChatTuInput()
        {
            Gopet.Net.Message sent = null;
            var chat = new ChatHandler(message => sent = message);
            var root = new GameObject("HUD test root");
            var hud = GameHud.Create(root.transform, chat);
            hud.SetChatExpanded(true);
            var input = hud.GetComponentInChildren<InputField>(true);
            input.text = "xin chào";
            hud.transform.Find("Place Chat/Chat Body/Send").GetComponent<Button>().onClick.Invoke();
            Assert.IsNotNull(sent);
            var round = Gopet.Net.Message.FromWire(sent.ToWire(), false);
            Assert.AreEqual("xin chào", round.Reader.ReadUtf());
            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Hud_ChatGonVaNamGiuaPhiaDuoiManHinh()
        {
            var root = new GameObject("HUD layout test root");
            var hud = GameHud.Create(root.transform, new ChatHandler(_ => { }));
            var chat = hud.transform.Find("Place Chat") as RectTransform;
            Assert.IsNotNull(chat);
            Assert.AreEqual(new Vector2(0.5f, 0f), chat.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 0f), chat.anchorMax);
            Assert.AreEqual(new Vector2(0.5f, 0f), chat.pivot);
            Assert.AreEqual(0f, chat.anchoredPosition.x, 0.01f);
            Assert.That(chat.sizeDelta.x, Is.InRange(420f, 520f));
            Assert.AreEqual(8f, chat.anchoredPosition.y, 0.01f);
            Assert.AreEqual(42f, chat.sizeDelta.y, 0.01f);
            Assert.IsFalse(hud.IsChatExpanded);
            chat.Find("Toggle Chat").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(hud.IsChatExpanded);
            Assert.AreEqual(194f, chat.sizeDelta.y, 0.01f);
            Assert.IsTrue(chat.Find("Chat Body").gameObject.activeSelf);
            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChatBangHoi_NamCanhKhuVucVaTheGioi()
        {
            var root = new GameObject("Guild chat HUD test");
            var hud = GameHud.Create(root.transform, new ChatHandler(_ => { }));
            var historyRequested = false;
            string sent = null;
            hud.ClanIdProvider = () => 7;
            hud.SetGuildAvailable(true);
            hud.GuildHistoryRequested = () => historyRequested = true;
            hud.GuildChatRequested = text => sent = text;

            var guild = hud.transform.Find("Place Chat/Chat Channels/Guild");
            Assert.IsNotNull(guild);
            Assert.IsFalse(guild.gameObject.activeSelf);
            hud.SetGuildAvailable(true);
            Assert.IsTrue(guild.gameObject.activeSelf);
            guild.GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(historyRequested);
            Assert.IsTrue(hud.IsGuildChannel);

            var input = hud.GetComponentInChildren<InputField>(true);
            input.text = "xin chào bang";
            hud.transform.Find("Place Chat/Chat Body/Send").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual("xin chào bang", sent);

            Object.DestroyImmediate(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TaskTracker_NamDuoiHpMpVaHienNhiemVuDau()
        {
            var root = new GameObject("Task tracker HUD test");
            var hud = GameHud.Create(root.transform, new ChatHandler(_ => { }));
            var clicked = false;
            hud.TaskTracker.Clicked += () => clicked = true;
            hud.TaskTracker.SetFirstTask(new MenuScreen
            {
                Items = new[]
                {
                    new MenuItemInfo { Title = "Thu thập gỗ", CanSelect = true }
                }
            });

            Assert.AreSame(hud.Character.transform, hud.TaskTracker.transform.parent);
            StringAssert.Contains("Thu thập gỗ", hud.TaskTracker.GetComponentInChildren<Text>().text);
            hud.TaskTracker.GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(clicked);
            Object.DestroyImmediate(root);
            yield return null;
        }

        /// <summary>Mô tả menu 1034 là tiến độ mỗi yêu cầu một hàng: HUD phải hiện thẳng
        /// ra và giãn cao theo, không bắt người chơi bấm mở popup.</summary>
        [UnityTest]
        public IEnumerator TaskTracker_HienTienDoVaGianChieuCao()
        {
            var root = new GameObject("Task tracker progress test");
            var hud = GameHud.Create(root.transform, new ChatHandler(_ => { }));
            var reported = 0f;
            hud.TaskTracker.HeightChanged += h => reported = h;
            hud.TaskTracker.SetFirstTask(new MenuScreen
            {
                Items = new[]
                {
                    new MenuItemInfo
                    {
                        Title = "Nhiệm vụ 1", CanSelect = true,
                        Description = "Tiêu diệt Khủng long 3 / 10\nTiêu diệt Gà rừng 0 / 10"
                    }
                }
            });

            var texts = hud.TaskTracker.GetComponentsInChildren<Text>();
            Assert.IsTrue(System.Array.Exists(texts, t => t.text.Contains("Khủng long 3 / 10")));
            Assert.Greater(hud.TaskTracker.CurrentHeight, Gopet.Runtime.UI.TaskTrackerWidget.Height);
            Assert.AreEqual(hud.TaskTracker.CurrentHeight, reported);
            Object.DestroyImmediate(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChatKhuVuc_HienBongBongTrenDauNhanVat()
        {
            var avatar = PlayerAvatar.Spawn(null, 7, "Người chơi", 0, 100, 100, 480);
            ChatBubble.AttachOrUpdate(avatar, "Xin chào");
            var bubble = avatar.GetComponentInChildren<ChatBubble>();
            Assert.IsNotNull(bubble);
            Assert.AreEqual("Xin chào", bubble.GetComponentInChildren<TextMesh>().text);
            Assert.IsNotNull(bubble.GetComponentInChildren<SpriteRenderer>().sprite);
            Assert.Greater(bubble.transform.localPosition.y, 0f);
            Object.Destroy(avatar.gameObject);
            yield return null;
        }
    }
}
