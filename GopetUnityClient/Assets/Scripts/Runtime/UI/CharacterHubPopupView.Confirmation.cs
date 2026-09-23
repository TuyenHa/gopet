using System;
using Gopet.Net.Guider;
using Gopet.Net.Pet;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        private void ShowEmbeddedConfirm(MenuSelection.ConfirmPrompt prompt, Action onYes)
        {
            if (_equipRequestPending || prompt == null || onYes == null) return;
            var message = string.IsNullOrWhiteSpace(prompt.Text)
                ? "Bạn có muốn sử dụng vật phẩm này cho nhân vật không?"
                : prompt.Text;
            ShowConfirmation(message, prompt.ConfirmLabel, prompt.CancelLabel, () =>
            {
                _equipRequestPending = true;
                try { onYes(); }
                finally { _equipRequestPending = false; }
            });
        }

        /// <summary>
        /// Kho cánh của JAR là một protocol chuyên biệt: xác nhận xong phải gửi
        /// PET_SERVICE / WING / WING_TYPE_USE / inventoryIndex, không phải guider menu.
        /// </summary>
        private bool TryUseWing(MenuScreen screen, int index)
        {
            if (_equipRequestPending || _wings == null || screen == null ||
                index < 0 || index >= screen.Items.Length) return true;

            var item = screen.Items[index];
            if (!item.CanSelect) return true;

            var equipped = item.ItemId == -1;
            var action = equipped ? (Action)_wings.Unequip : () => _wings.Use(item.ItemId);
            var message = equipped
                ? $"B\u1ea1n c\u00f3 mu\u1ed1n th\u00e1o {item.Title} kh\u00f4ng?"
                : string.IsNullOrWhiteSpace(item.DialogText)
                    ? $"B\u1ea1n c\u00f3 mu\u1ed1n s\u1eed d\u1ee5ng {item.Title} kh\u00f4ng?"
                    : item.DialogText;

            ShowConfirmation(message, item.LeftCommandText, item.RightCommandText, () =>
            {
                _equipRequestPending = true;
                try
                {
                    action();
                    // JAR Ä‘Ã³ng danh sÃ¡ch sau khi gá»­i; server sáº½ tráº£ kho CÃ¡nh má»›i.
                    ClearMenu(true);
                }
                finally { _equipRequestPending = false; }
            });
            return true;
        }

        private void ShowConfirmation(string message, string yesLabel, string noLabel, Action onYes)
        {
            if (_confirmation != null || _equipRequestPending) return;

            _confirmation = YesNoDialog.Create(transform,
                string.IsNullOrWhiteSpace(message) ? "B\u1ea1n c\u00f3 mu\u1ed1n s\u1eed d\u1ee5ng v\u1eadt ph\u1ea9m n\u00e0y kh\u00f4ng?" : message,
                string.IsNullOrWhiteSpace(yesLabel) ? "\u0110\u1ed3ng \u00fd" : yesLabel,
                string.IsNullOrWhiteSpace(noLabel) ? "Kh\u00f4ng" : noLabel);
            _confirmation.Confirmed += () =>
            {
                DismissConfirmation();
                onYes?.Invoke();
            };
            _confirmation.Cancelled += DismissConfirmation;
        }

        private void DismissConfirmation()
        {
            if (_confirmation == null) return;
            var dialog = _confirmation;
            _confirmation = null;
            if (dialog != null) Destroy(dialog.gameObject);
        }
    }
}
