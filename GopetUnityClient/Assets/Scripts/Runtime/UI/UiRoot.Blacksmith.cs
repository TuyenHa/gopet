namespace Gopet.Runtime.UI
{
    /// <summary>Popup NPC Thợ Rèn (sửa độ bền trang bị pet), cùng khung với popup cửa hàng.</summary>
    public sealed partial class UiRoot
    {
        // NPC "Thợ Rèn" (DB npcId=-42, Thành phố Linh Thú) — opt 98 sửa trang bị, 99 giải thích độ bền.
        private const int BlacksmithNpcId = -42;

        private BlacksmithNpcTabsView _blacksmithNpcTabs;

        private void ShowBlacksmithNpcTabs(Gopet.Net.Guider.NpcOptions options)
        {
            if (_blacksmithNpcTabs != null) Close(_blacksmithNpcTabs);

            // Create tự gửi option 98 (tab đầu); gói lưới về ở frame sau, lúc đó view đã được
            // gán vào _blacksmithNpcTabs để ShowMenu giao cho nó.
            var view = BlacksmithNpcTabsView.Create(transform, _font, options, _assets, _guider);
            _blacksmithNpcTabs = view;
            view.Closed += () => Close(view);
            Push(view, view.gameObject);
        }
    }
}
