using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Phần DÀN CẢNH của <see cref="BattleFlameFallFx"/>: chia làn, chọn cỡ, chọn lớp
    /// trước/sau pet, tính điểm thả và góc nghiêng. Tách khỏi phần nhịp để giữ file dưới
    /// ngưỡng 200 dòng.</summary>
    public sealed partial class BattleFlameFallFx
    {
        private static Transform NewLayer(Transform parent, string label, bool behindPets)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            // Chỉ số 1: sau nền nhưng trước các card pet — cùng cách BattleGroundSigil dùng.
            if (behindPets) go.transform.SetSiblingIndex(1);
            return go.transform;
        }

        /// <param name="lean">Độ lệch ngang khi rơi, theo quãng rơi. Dấu cho biết rơi từ bên
        /// nào; 0 là rơi thẳng.</param>
        private static Jet Build(Sprite sprite, RectTransform target, int index,
            float canvasHeight, float petH, float aspect, float drop, float lean,
            Transform back, Transform front)
        {
            // Rải đều theo làn rồi xê dịch nhẹ: rải ngẫu nhiên hoàn toàn hay bị dồn cục.
            var lane = JetCount == 1 ? 0f : index / (JetCount - 1f) - 0.5f;
            var spread = lane * 2f;
            var inFront = Mathf.Abs(spread) > FrontMinOffset && index % 3 == 1;

            var go = new GameObject($"Ngọn {index}", typeof(RectTransform), typeof(Image),
                typeof(CanvasGroup));
            go.transform.SetParent(inFront ? front : back, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;

            var height = canvasHeight * MaxHeightRatio
                * (1f - EdgeFalloff * Mathf.Pow(Mathf.Abs(spread), 1.4f))
                * Random.Range(0.80f, 1.06f);

            var rect = (RectTransform)go.transform;
            // Pivot ĐÁY: ngọn bùng LÊN từ chân, và scaleY 0→1 là trào lên đúng chiều.
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(height * aspect, height);
            // Ngọn trước pet hạ thấp xuống (gần người xem hơn), ngọn sau thì nhích lên.
            var depth = inFront ? Random.Range(-0.40f, -0.18f) : Random.Range(0.04f, 0.22f);
            var to = new Vector3(
                target.position.x + (spread * SpreadRatio + Random.Range(-0.05f, 0.05f)) * canvasHeight,
                target.position.y + depth * petH, target.position.z);
            rect.position = to;
            rect.localScale = Vector3.zero;

            var fall = drop * Random.Range(0.85f, 1.15f);
            return new Jet
            {
                Rect = rect,
                Group = go.GetComponent<CanvasGroup>(),
                Delay = index * StaggerSeconds + Random.Range(0f, 0.015f),
                FallRatio = Random.Range(FallMinRatio, FallMaxRatio),
                // Nghiêng đúng bằng góc của đường bay, KHÔNG phải một số chọn tay: lệch nhau
                // thì thân lửa không nằm trên quỹ đạo và trông như trượt ngang.
                Tilt = -Mathf.Atan(lean) * Mathf.Rad2Deg,
                From = to + new Vector3(lean * fall, fall, 0f),
                To = to,
            };
        }
    }
}
