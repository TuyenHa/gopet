using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Cho một con quái đi lảng vảng quanh chỗ nó sinh ra. Luật đi nằm ở
    /// <see cref="MobWander"/> (thuần C#, test được); ở đây chỉ nối nó với map, đồng hồ
    /// và <see cref="WorldActorView"/>.
    /// </summary>
    public sealed class MobWanderer : MonoBehaviour
    {
        private WorldActorView _view;
        private JarMapLayout _map;
        private MobWander _wander;

        public static void Attach(WorldActorView view, JarMapLayout map, int mobId, int jarX, int jarY)
        {
            if (view == null || map == null) return;

            var wanderer = view.gameObject.AddComponent<MobWanderer>();
            wanderer._view = view;
            wanderer._map = map;
            // Seed theo mobId: mỗi con một đường đi riêng, và chạy lại vẫn ra đúng đường
            // đó — lỗi "quái đi vào vách" tái hiện được thay vì hên xui mỗi lần.
            wanderer._wander = new MobWander(jarX, jarY, mobId);
        }

        private void Update()
        {
            if (_view == null || _wander == null) return;

            _wander.Tick(Time.deltaTime, CanStand);
            _view.MoveTo(Mathf.RoundToInt(_wander.X), Mathf.RoundToInt(_wander.Y), _map.HeightPixels);
        }

        private bool CanStand(int x, int y) => MapCollision.CanStand(_map, x, y);
    }
}
