using System;
using Gopet.Net.Social;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hộp thư, dựng trên <see cref="GamePopupFrame"/> nên cùng một khung, cùng badge
    /// tiêu đề và cùng bảng màu với popup cửa hàng.
    ///
    /// <para>Hàng tab lọc theo loại thư, cộng thêm tab "Soạn thư" đổi luôn nội dung của
    /// popup thành form soạn — không mở hộp thoại thứ hai đè lên.</para>
    /// </summary>
    public sealed partial class MailboxView : MonoBehaviour
    {
        /// <summary>Loại thư của server (<c>Data/User/Letter.cs:13-15</c>). 0 là "tất cả",
        /// không phải một loại thật — jar cũng chia đúng ba nhóm này.</summary>
        private const sbyte TypeAll = 0, TypeFriend = 1, TypeAdmin = 2, TypeEvent = 3;

        /// <summary>Tab cuối không lọc thư mà đổi nội dung sang form soạn.</summary>
        private const int ComposeTab = 4;

        private static readonly sbyte[] TabFilters = { TypeAll, TypeAdmin, TypeEvent, TypeFriend };
        private static readonly string[] TabLabels = { "Tất cả", "Admin", "Sự kiện", "Bạn bè", "Soạn thư" };

        // To hơn mặc định 400×300 của khung: hộp thư là màn đọc, cần bề ngang cho tiêu đề
        // thư và chiều cao cho nhiều dòng. Trần chiều cao là ~405 ref-unit (chiều cao khả
        // kiến ở 16:9, xem GamePopupFrame) — 350 cộng nửa badge nhô lên vẫn còn dư.
        private const float Width = 520f;
        private const float Height = 350f;

        /// <summary>Phần bề ngang vùng nội dung dành cho danh sách; phần còn lại là ô đọc thư.</summary>
        private const float ListWidthFraction = 0.44f;

        /// <summary>Khe giữa danh sách và ô đọc thư.</summary>
        private const float SplitGap = 8f;

        private const string ListFooter = "Chạm vào thư để đọc nội dung.";
        private const string ComposeFooter = "Điền người nhận và nội dung rồi bấm Gửi.";

        private Mailbox _mailbox;
        private Font _font;
        private GamePopupFrame _frame;
        private PopupTabRail _rail;
        private PopupItemList _list;
        private LetterDetailPane _detail;
        private ComposeLetterView _compose;

        public event Action CloseRequested;

        /// <summary>Người chơi bấm "Đánh dấu đã đọc" trong ô đọc thư.</summary>
        public event Action<int> MarkRequested;

        /// <summary>Người chơi bấm "Xoá". Mang cả lá thư để bên nhận hỏi xác nhận cho đúng loại.</summary>
        public event Action<Letter> RemoveRequested;

        /// <summary>Người chơi gửi thư từ tab soạn: (người nhận, nội dung).</summary>
        public event Action<string, string> SendRequested;

        public static MailboxView Create(Transform parent, Mailbox mailbox)
        {
            var font = UiBuilder.BuiltinFont();

            // Nền mờ + bấm ra ngoài để đóng. Component nằm ở ĐÂY chứ không ở khung popup:
            // người gọi huỷ một object là mất cả nền lẫn khung.
            var root = new GameObject("Mailbox", typeof(RectTransform), typeof(Image),
                typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, .5f);

            var view = root.AddComponent<MailboxView>();
            view._mailbox = mailbox;
            view._font = font;
            root.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            view._frame = GamePopupFrame.Create(root.transform, font, "Hộp thư",
                Width, Height, ListFooter);
            view._frame.Closed += () => view.CloseRequested?.Invoke();

            view._rail = PopupTabRail.Create(view._frame.Content, font,
                view._frame.ContentWidth, TabLabels);
            view._rail.Selected += view.SelectTab;

            view.BuildList();
            view.BuildDetail();
            view.BuildCompose();
            view._rail.Select(0);
            return view;
        }

        /// <summary>Thay dữ liệu và dựng lại danh sách, giữ nguyên tab đang xem.</summary>
        public void Bind(Mailbox mailbox)
        {
            _mailbox = mailbox;
            if (_rail.ActiveIndex != ComposeTab) RebuildList();
        }

        private void SelectTab(int index)
        {
            var composing = index == ComposeTab;
            _list.gameObject.SetActive(!composing);
            _detail.gameObject.SetActive(!composing);
            _compose.gameObject.SetActive(composing);
            _frame.SetFooter(composing ? ComposeFooter : ListFooter);

            if (composing)
            {
                _compose.Reset();
                return;
            }
            RebuildList();
            // Đổi tab là đổi tập thư — thư đang mở có thể không còn trong danh sách nữa.
            _detail.Show(null);
        }

        /// <summary>
        /// Ô đọc thư chiếm NỬA PHẢI vùng nội dung. Soạn thư thì chiếm trọn bề ngang, nên
        /// nó không dùng chung cách chia này.
        /// </summary>
        private void BuildDetail()
        {
            _detail = LetterDetailPane.Create(_frame.Content, _font);
            var rect = (RectTransform)_detail.transform;
            rect.offsetMin = new Vector2(_frame.ContentWidth * ListWidthFraction + SplitGap, 0f);
            rect.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));

            _detail.MarkRequested += id => MarkRequested?.Invoke(id);
            _detail.RemoveRequested += letter => RemoveRequested?.Invoke(letter);
        }

        private void BuildCompose()
        {
            _compose = ComposeLetterView.Create(_frame.Content, _font);
            // Chừa chỗ khay tab y như danh sách, nếu không form chui lên dưới hàng tab.
            ((RectTransform)_compose.transform).offsetMax =
                new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));

            _compose.SendRequested += (recipient, content) =>
            {
                SendRequested?.Invoke(recipient, content);
                _rail.Select(0); // gửi xong quay về danh sách, đồng thời xoá ô đã gõ
            };
            // Huỷ = quay lại danh sách, KHÔNG đóng cả popup.
            _compose.Cancelled += () => _rail.Select(0);
            _compose.gameObject.SetActive(false);
        }
    }
}
