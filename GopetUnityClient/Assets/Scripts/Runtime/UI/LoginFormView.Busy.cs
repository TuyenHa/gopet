using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Trạng thái "đang chờ server" của form: khoá thao tác và quay một vòng loading
    /// GIỮA MÀN HÌNH. Không có gì báo đang chờ thì người chơi tưởng nút hỏng và bấm lại.
    ///
    /// <para>Bật khi bấm nút hành động chính của form — màn đăng nhập là "Đăng nhập",
    /// màn đăng ký là "Đăng ký". Nút còn lại chỉ điều hướng trong máy, xong tức thì,
    /// không có gì để chờ.</para>
    /// </summary>
    public sealed partial class LoginFormView
    {
        /// <summary>Đường kính vòng xoay so với chiều cao màn hình.</summary>
        private const float SpinnerHeightFrac = 0.12f;

        /// <summary>Cỡ dự phòng khi khung chưa qua layout nên <c>rect.height</c> còn 0 —
        /// vòng cỡ 0 là vòng vô hình.</summary>
        private const float SpinnerFallbackSize = 64f;

        /// <summary>Xanh navy đậm, cùng tông chữ của form. Vòng TRẮNG thì mất hút: vòng
        /// nằm giữa màn, tức là nằm trên mặt panel màu trắng xanh rất nhạt.</summary>
        private static readonly Color SpinnerColor = new Color(0.08f, 0.25f, 0.62f, 1f);

        /// <summary>
        /// Chốt chặn tự mở khoá. Đường đăng nhập đã có đồng hồ riêng của
        /// <see cref="LoginFlow"/>, nhưng đường ĐĂNG KÝ thì KHÔNG: <c>SubmitRegistration</c>
        /// không bật đồng hồ nào, server im lặng là form khoá vĩnh viễn. Cộng thêm vài
        /// giây để khi cả hai cùng có hiệu lực thì đồng hồ của flow xử lý trước.
        /// </summary>
        private static readonly float BusyGuardSeconds = LoginFlow.ReplyTimeoutMs / 1000f + 5f;

        private LoadingSpinner _spinner;
        private float _busySince;

        /// <summary>Đang chờ server trả lời hay không.</summary>
        public bool IsBusy => _spinner != null;

        /// <summary>Khoá form và bật vòng xoay trong lúc chờ hồi âm; tắt khi có hồi âm.</summary>
        public void SetBusy(bool busy)
        {
            // Gọi vô điều kiện chứ không chỉ khi đổi trạng thái: SetBusy(false) còn là
            // đường mở khoá form sau khi server từ chối, kể cả khi chưa từng bận.
            SetInteractionEnabled(!busy);

            if (busy && _spinner == null)
            {
                _busySince = Time.unscaledTime;
                // Gắn vào GỐC view (đã stretch kín màn) chứ không vào panel hay nút:
                // chỉ có gốc mới cho "giữa màn hình" đúng nghĩa. Là con thêm SAU CÙNG
                // nên vẽ đè lên panel.
                var root = (RectTransform)transform;
                var height = root.rect.height;
                _spinner = LoadingSpinner.Attach(root,
                    height > 0f ? height * SpinnerHeightFrac : SpinnerFallbackSize, SpinnerColor);
            }
            else if (!busy && _spinner != null)
            {
                Destroy(_spinner.gameObject);
                _spinner = null;
            }
        }

        private void Update()
        {
            if (!IsBusy || Time.unscaledTime - _busySince < BusyGuardSeconds) return;

            // Hết hạn chờ mà vẫn chưa ai gọi SetBusy(false): trả form lại cho người
            // chơi. Khoá cứng một form không còn đường thoát là hỏng nặng hơn hẳn
            // việc mất hiệu ứng chờ.
            SetBusy(false);
        }
    }
}
