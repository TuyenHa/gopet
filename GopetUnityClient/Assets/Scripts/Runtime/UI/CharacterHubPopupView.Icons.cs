using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        private static float TabLabelWidth(int index)
        {
            switch (index)
            {
                case 0: return 74f;
                case 1: return 24f;
                case 2: return 54f;
                case 3: return 46f;
                default: return 64f;
            }
        }

        private static Sprite HubIcon(int index)
        {
            if (index < 0 || index > 4) return null;
            if (_hubIcons[index] != null) return _hubIcons[index];

            var texture = Resources.Load<Texture2D>("Ui/Generated/character_hub_icons");
            if (texture == null) return null;

            // Sprite sheet tạo gồm 5 ô cùng bề rộng. Cắt phần trung tâm để loại
            // khoảng trong suốt dư thừa, nhờ vậy icon tab hiển thị đậm và rõ.
            var cellWidth = texture.width / 5f;
            var rect = new Rect(index * cellWidth, texture.height * 0.2f,
                cellWidth, texture.height * 0.6f);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Character hub icon " + index;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _hubIcons[index] = sprite;
            return sprite;
        }

        private static readonly Sprite[] _hubIcons = new Sprite[5];

        private static Sprite HubActionIcon(int index)
        {
            if (index < 0 || index > 2) return null;
            if (_hubActionIcons[index] != null) return _hubActionIcons[index];
            var texture = Resources.Load<Texture2D>("Ui/Generated/character_hub_actions");
            if (texture == null) return null;
            var cellWidth = texture.width / 3f;
            var rect = new Rect(index * cellWidth, texture.height * 0.2f,
                cellWidth, texture.height * 0.6f);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Character hub action icon " + index;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _hubActionIcons[index] = sprite;
            return sprite;
        }

        private static readonly Sprite[] _hubActionIcons = new Sprite[3];

        private static Sprite SettingsIcon(int index)
        {
            if (index < 0 || index > 7) return null;
            if (_settingsIcons[index] != null) return _settingsIcons[index];
            var texture = Resources.Load<Texture2D>("Ui/Generated/settings_icons");
            if (texture == null) return null;

            // Sprite sheet 4x2: speaker, music, effects, language / swords, lock, logout, exit.
            var cellWidth = texture.width / 4f;
            var cellHeight = texture.height / 2f;
            var column = index % 4;
            var rowFromBottom = index < 4 ? 1 : 0;
            var rect = new Rect(column * cellWidth, rowFromBottom * cellHeight,
                cellWidth, cellHeight);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Settings icon " + index;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _settingsIcons[index] = sprite;
            return sprite;
        }

        private static readonly Sprite[] _settingsIcons = new Sprite[8];

        private static Sprite FriendsIcon(int index)
        {
            if (index < 0 || index > 2) return null;
            if (_friendsIcons[index] != null) return _friendsIcons[index];
            var texture = Resources.Load<Texture2D>("Ui/Generated/friends_icons");
            if (texture == null) return null;
            var cellWidth = texture.width / 3f;
            var rect = new Rect(index * cellWidth, 0f, cellWidth, texture.height);
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Friends icon " + index;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _friendsIcons[index] = sprite;
            return sprite;
        }

        private static readonly Sprite[] _friendsIcons = new Sprite[3];

    }
}
