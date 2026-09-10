using Gopet.Net.Guider;

namespace Gopet.Runtime.UI
{
    public sealed partial class UiRoot
    {
        private AnimationMenuHandler _animationMenuHandler;

        public void InitializeAnimationMenus(AnimationMenuHandler handler)
        {
            _animationMenuHandler = handler;
            handler.ScreenShown += ShowAnimationMenu;
        }

        private void ShowAnimationMenu(AnimationMenuScreen screen)
        {
            var view = AnimationMenuView.Create(transform, _font, screen, _assets);
            view.CommandSelected += command =>
            {
                // Server hiện chỉ dùng AnimationMenuCommand làm nút thoát ở màn thành tựu.
                // Không tự đoán wire format cho RepliesToServer: server cũng chưa có nhánh nhận
                // PET_SERVICE/ANIMATION_MENU từ client.
                if (command.RepliesToServer)
                    ShowToast("Thao tác này chưa được server hỗ trợ.");
                if (command.ClosesScreen) Close(view);
            };
            Push(view, view.gameObject);
        }

        private void UnbindAnimationMenus()
        {
            if (_animationMenuHandler == null) return;
            _animationMenuHandler.ScreenShown -= ShowAnimationMenu;
        }
    }
}
