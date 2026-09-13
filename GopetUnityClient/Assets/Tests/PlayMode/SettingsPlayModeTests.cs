using System.Collections;
using System.Linq;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class SettingsPlayModeTests
    {
        [UnityTest]
        public IEnumerator DoiNgonNgu_CapNhatNgayNhanVaPlayerPrefs()
        {
            var original = JarStrings.Current;
            var root = new GameObject("Settings language test");
            try
            {
                var sound = SoundManager.Create(root.transform);
                var view = SettingsView.Create(root.transform, sound, false);
                var language = view.GetComponentsInChildren<Button>(true)
                    .Single(button => button.GetComponentInChildren<Text>()?.text.StartsWith("Ngôn ngữ:") == true);

                language.onClick.Invoke();
                yield return null;

                var expected = original == Language.Vi ? Language.En : Language.Vi;
                Assert.AreEqual(expected, JarStrings.Current);
                Assert.AreEqual((int)expected, PlayerPrefs.GetInt("gopet.language"));
                StringAssert.Contains(expected == Language.En ? "English" : "Tiếng Việt",
                    language.GetComponentInChildren<Text>().text);
            }
            finally
            {
                JarStrings.Current = original;
                Object.DestroyImmediate(root);
            }
        }
    }
}
