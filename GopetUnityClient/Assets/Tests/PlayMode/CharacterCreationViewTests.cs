using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.PlayModeTests
{
    public sealed class CharacterCreationViewTests
    {
        private GameObject _canvas;
        private GameObject _eventSystem;
        private CharacterCreationView _view;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            _view = CharacterCreationView.Create(_canvas.transform, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) Object.DestroyImmediate(_canvas);
            if (_eventSystem != null) Object.DestroyImmediate(_eventSystem);
        }

        [Test]
        public void NameIsSanitizedAndSubmitStartsDisabledUntilValid()
        {
            Assert.IsFalse(_view.SubmitButton.interactable);
            _view.NameInput.Field.onValueChanged.Invoke("AB C1@");

            Assert.AreEqual("abc1", _view.NameInput.Value);
            Assert.IsFalse(_view.NameInput.IsValid);

            _view.NameInput.Field.onValueChanged.Invoke("abc12");
            Assert.IsTrue(_view.NameInput.IsValid);
            Assert.IsTrue(_view.SubmitButton.interactable);
        }

        [Test]
        public void ClickingFemaleSlotSubmitsFemaleAndName()
        {
            _view.NameInput.Field.onValueChanged.Invoke("abc12");
            var submitted = false;
            sbyte gender = -1;
            string name = null;
            _view.Submitted += (g, n) => { submitted = true; gender = g; name = n; };

            var data = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(_view.Preview.FemaleSlot.gameObject, data, ExecuteEvents.pointerClickHandler);
            _view.SubmitButton.onClick.Invoke();

            Assert.IsTrue(submitted);
            Assert.AreEqual((sbyte)1, gender);
            Assert.AreEqual("abc12", name);
        }
    }
}
