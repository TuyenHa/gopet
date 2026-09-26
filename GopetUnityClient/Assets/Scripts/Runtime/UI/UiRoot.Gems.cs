using System;
using Gopet.Net.Pet;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    public sealed partial class UiRoot
    {
        private GemHandler _gemHandler;
        private GemInventoryView _gemView;
        /// <summary>_gemView nằm nhúng trong popup khác (không Push) → huỷ thẳng, không Close.</summary>
        private bool _gemViewEmbedded;

        /// <summary>
        /// Chỗ nhúng Kho ngọc nếu có (tab Pet của Hành lý đang mở trang Kho ngọc — xem
        /// <c>GameSession.GemInventoryHost</c>); null = mở popup riêng như cũ.
        /// </summary>
        public Func<Transform> GemInventoryHost { get; set; }
        private int _activeGemId;
        private int _gemMaterial1;

        public void InitializeGems(GemHandler handler)
        {
            _gemHandler = handler;
            handler.InventoryReceived += ShowGemInventory;
            handler.GemUpdated += OnGemUpdated;
            handler.GemRemoved += OnGemRemoved;
            handler.EnchantMaterialSelected += OnEnchantMaterial;
            handler.TierMaterialSelected += OnTierMaterial;
        }

        private void ShowGemInventory(GemInventory inventory)
        {
            var host = GemInventoryHost?.Invoke();
            if (host != null && _gemViewEmbedded && _gemView != null && _gemView.transform.parent == host)
            {
                _gemView.ApplyInventory(inventory);
                return;
            }
            DisposeGemView();
            if (host != null)
            {
                _gemView = GemInventoryView.CreateEmbedded(host, _font, _assets);
                _gemViewEmbedded = true;
                _gemView.ApplyInventory(inventory);
                _gemView.GemSelected += ShowGemActions;
                return;
            }

            var view = GemInventoryView.Create(transform, _font, _assets);
            _gemView = view;
            view.ApplyInventory(inventory);
            view.CloseRequested += () => CloseGemInventory(view);
            view.GemSelected += ShowGemActions;
            Push(view, view.gameObject);
        }

        private void DisposeGemView()
        {
            if (_gemView != null)
            {
                if (_gemViewEmbedded) Destroy(_gemView.gameObject);
                else Close(_gemView);
            }
            _gemView = null;
            _gemViewEmbedded = false;
        }

        private void CloseGemInventory(GemInventoryView view)
        {
            Close(view);
            if (ReferenceEquals(_gemView, view)) _gemView = null;
        }

        private void ShowGemActions(GemItemInfo item)
        {
            var dialog = ChoiceDialogView.Create(transform, _font);
            dialog.Bind($"{item.Name} — Lv {item.Level}",
                new[] { "Cường hoá", "Tiến hoá", "Huỷ ngọc" });
            // Không còn nút "Đóng" — nút X của dialog đảm nhiệm việc đóng.
            dialog.Closed += () => Close(dialog);
            dialog.Chosen += index =>
            {
                Close(dialog);
                _activeGemId = item.ItemId;
                _gemMaterial1 = 0;
                if (index == 0)
                {
                    _gemHandler.BeginEnchant(item.ItemId);
                    ShowToast("Chọn nguyên liệu cường hoá thứ nhất.");
                }
                else if (index == 1)
                {
                    _gemHandler.BeginTier();
                    ShowToast("Chọn một ngọc nguyên liệu cùng loại.");
                }
                else if (index == 2)
                {
                    _gemHandler.Remove(item.ItemId); // server sẽ hỏi Yes/No
                }
            };
            Push(dialog, dialog.gameObject);
        }

        private void OnEnchantMaterial(GemMaterialSelection material)
        {
            if (_activeGemId <= 0) return;
            // Server gửi slot 7 cho nguyên liệu 1 và slot 12 cho nguyên liệu 2.
            if (_gemMaterial1 == 0 || material.Slot == 7)
            {
                _gemMaterial1 = material.ItemOrTemplateId;
                _gemHandler.BeginEnchant(_activeGemId);
                ShowToast("Chọn nguyên liệu cường hoá thứ hai.");
                return;
            }
            _gemHandler.ConfirmEnchant(_activeGemId, _gemMaterial1, material.ItemOrTemplateId);
            _gemMaterial1 = 0;
        }

        private void OnTierMaterial(GemMaterialSelection material)
        {
            if (_activeGemId <= 0) return;
            if (_activeGemId == material.ItemOrTemplateId)
            {
                ShowToast("Ngọc chính và ngọc nguyên liệu phải khác nhau.");
                return;
            }
            _gemHandler.ConfirmTier(_activeGemId, material.ItemOrTemplateId);
        }

        // So null kiểu Unity: bản nhúng bị huỷ cùng tab Pet mà field vẫn giữ tham chiếu.
        private void OnGemUpdated(GemItemInfo item)
        {
            if (_gemView != null) _gemView.UpdateGem(item);
        }

        private void OnGemRemoved(int itemId)
        {
            if (_gemView != null) _gemView.RemoveGem(itemId);
        }

        private void UnbindGems()
        {
            if (_gemHandler == null) return;
            _gemHandler.InventoryReceived -= ShowGemInventory;
            _gemHandler.GemUpdated -= OnGemUpdated;
            _gemHandler.GemRemoved -= OnGemRemoved;
            _gemHandler.EnchantMaterialSelected -= OnEnchantMaterial;
            _gemHandler.TierMaterialSelected -= OnTierMaterial;
        }
    }
}
