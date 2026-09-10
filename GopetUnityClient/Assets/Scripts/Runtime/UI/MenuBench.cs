using System.Text;
using Gopet.Net.Guider;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Dụng cụ đo tiêu chí "menu 200 dòng phải trên 50 fps trên máy tầm trung".
    ///
    /// <para>Con số đó <b>không suy ra được từ test</b>: PlayMode test chứng minh
    /// danh sách 500 dòng chỉ dựng ~12 GameObject, nhưng fps thì phụ thuộc GPU, độ
    /// phân giải và trình điều khiển của từng máy. Nên ở đây chỉ dựng cảnh đo; con
    /// số phải lấy trên thiết bị thật.</para>
    ///
    /// <para>Cách dùng: gắn vào một GameObject trong scene rỗng, build ra máy cần đo,
    /// chạy <see cref="DurationSeconds"/> giây rồi đọc dòng chữ trên màn hình (hoặc
    /// logcat). Cuộn tự động để bắt cả chi phí dựng dòng mới — đứng yên nhìn một
    /// danh sách tĩnh thì đo được đúng chi phí vẽ, tức là phần dễ.</para>
    /// </summary>
    public sealed class MenuBench : MonoBehaviour
    {
        [SerializeField] private int itemCount = 200;
        [SerializeField] private float viewportHeight = 720f;
        [SerializeField] private float scrollSpeed = 400f;

        [Tooltip("Đo bao lâu rồi kết luận, tính bằng giây.")]
        [SerializeField] private float durationSeconds = 10f;

        private readonly FpsSampler _fps = new FpsSampler();

        private GenericMenuView _menu;
        private UnityEngine.UI.Text _readout;
        private float _scroll;
        private float _elapsed;
        private bool _done;

        public float DurationSeconds => durationSeconds;

        public FpsSampler Fps => _fps;

        /// <summary>Kết quả sau khi đo xong; rỗng khi còn đang chạy.</summary>
        public string Result { get; private set; } = string.Empty;

        private void Start()
        {
            var canvas = new GameObject("BenchCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var font = UiBuilder.BuiltinFont();

            _menu = GenericMenuView.Create(canvas.transform, font);
            _menu.Bind(BuildScreen(itemCount), null, new GuiderHandler(m => m.Dispose()));
            _menu.SetViewport(viewportHeight);

            _readout = UiBuilder.MakeText(canvas.transform, font, "Readout", 24, false);
            UiBuilder.PlaceRow((RectTransform)_readout.transform, 0f, 64f, 8f);
        }

        private void Update()
        {
            if (_done) return;

            _fps.Add(Time.unscaledDeltaTime);
            _elapsed += Time.unscaledDeltaTime;

            // Cuộn tới đáy rồi quay lại, để mỗi vòng đều phải dựng dòng mới.
            var span = Mathf.Max(1f, itemCount * MenuItemRow.Height - viewportHeight);
            _scroll = Mathf.PingPong(_elapsed * scrollSpeed, span);
            _menu.SetScroll(_scroll);

            if (_elapsed < durationSeconds)
            {
                _readout.text = $"Đang đo… {_elapsed:F0}/{durationSeconds:F0}s";
                return;
            }

            _done = true;
            Result = $"{itemCount} dòng | {_fps}";
            _readout.text = Result;
            Debug.Log($"[Gopet] MenuBench: {Result}");
        }

        /// <summary>Danh sách giả nhưng ĐÚNG hình dạng gói thật — không icon, để đo chi phí của danh sách chứ không của mạng.</summary>
        private static MenuScreen BuildScreen(int count)
        {
            var items = new MenuItemInfo[count];
            for (var i = 0; i < count; i++)
            {
                items[i] = new MenuItemInfo
                {
                    ItemId = i,
                    Title = new StringBuilder("Dòng ").Append(i).ToString(),
                    Description = "Mô tả dài vừa phải để chữ phải xuống dòng thật",
                    ImagePath = string.Empty,
                    CanSelect = true
                };
            }

            return new MenuScreen { ListId = 0, Title = "Bench", Items = items };
        }
    }
}
