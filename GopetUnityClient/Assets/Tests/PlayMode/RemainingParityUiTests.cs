using System.Linq;
using Gopet.Net.Pet;
using Gopet.Net.Social;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class RemainingParityUiTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Parity UI Tests", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
            PlayerPrefs.DeleteKey("Gopet.MusicEnabled");
            PlayerPrefs.DeleteKey("Gopet.EffectsEnabled");
        }

        [Test]
        public void PetProfile_ChiSelfMoiCoGymVaTattoo()
        {
            var profile = new PetProfile { Name = "Mèo" };
            var editable = PetProfileView.Create(_root.transform, profile, null, true);
            var readOnly = PetProfileView.Create(_root.transform, profile, null, false);

            Assert.AreEqual(4, editable.GetComponentsInChildren<UnityEngine.UI.Button>().Length);
            Assert.AreEqual(2, readOnly.GetComponentsInChildren<UnityEngine.UI.Button>().Length);
        }

        [Test]
        public void Mailbox_CoNutSoanVaThuCoTheBam()
        {
            var mailbox = new Mailbox
            {
                Letters = new[] { new Letter { LetterId = 7, Title = "Hi", ShortContent = "Body" } }
            };
            var view = MailboxView.Create(_root.transform, mailbox);

            Assert.NotNull(view.transform.Find("Panel/Soạn thư"));
            Assert.NotNull(view.transform.Find("Panel/Letter:7"));
        }

        [Test]
        public void Settings_CoBonTuyChonVaDong()
        {
            var sound = SoundManager.Create(_root.transform);
            var view = SettingsView.Create(_root.transform, sound, false);

            Assert.AreEqual(6, view.GetComponentsInChildren<UnityEngine.UI.Button>().Length);
        }

        [Test]
        public void ChatHistory_HienThiTranscript()
        {
            var transcript = new ChatTranscript(5);
            transcript.Add("Alice", "Xin chào");
            var view = ChatHistoryView.Create(_root.transform, "Khu vực", transcript);

            Assert.IsTrue(view.GetComponentsInChildren<UnityEngine.UI.Text>()
                .Any(text => text.text.Contains("Alice: Xin chào")));
        }
    }
}
