using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SthShader.Editor.UIToolkit
{
    public sealed class TextureObjectFieldWithPreview : BindableElement, INotifyValueChanged<Object>
    {
        public new class UxmlFactory : UxmlFactory<TextureObjectFieldWithPreview, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits { }

        private readonly Image _image;
        private readonly ObjectField _objectField;
        private Texture2D _value;

        public Object value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                var previousValue = _value;
                ((INotifyValueChanged<Object>)this).SetValueWithoutNotify(value);

                using var ev = ChangeEvent<Object>.GetPooled(previousValue, value);
                ev.target = this;
                SendEvent(ev);
            }
        }

        public TextureObjectFieldWithPreview() : this(string.Empty) { }

        public TextureObjectFieldWithPreview(string label)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("SthShader/SthFieldStyle"));
            styleSheets.Add(Resources.Load<StyleSheet>("SthShader/TextureObjectFieldWithPreviewStyle"));
            AddToClassList("sth-field-root");

            var useLabel = !string.IsNullOrEmpty(label);
            if (useLabel)
            {
                var labelElement = new Label(label);
                labelElement.AddToClassList("sth-field-label");
                Add(labelElement);
            }

            var flexContainer = new VisualElement();
            flexContainer.AddToClassList("sth-field-flex-container");
            if (useLabel)
            {
                flexContainer.AddToClassList("sth-field-labeled-flex-container");
            }
            Add(flexContainer);
            var flexItem0 = new VisualElement();
            flexItem0.AddToClassList("sth-field-flex-item-0");
            flexContainer.Add(flexItem0);
            var flexItem1 = new VisualElement();
            flexItem1.AddToClassList("sth-field-flex-item-1");
            flexContainer.Add(flexItem1);

            _image = new Image();
            flexItem1.Add(_image);

            _objectField = new ObjectField
            {
                objectType = typeof(Texture2D),
                allowSceneObjects = false,
            };
            _objectField.RegisterValueChangedCallback(ev => value = ev.newValue);
            flexItem0.Add(_objectField);
        }

        void INotifyValueChanged<Object>.SetValueWithoutNotify(Object newValue)
        {
            if (newValue != null && newValue is not Texture2D)
            {
                throw new ArgumentException($"Expected object type is {nameof(Texture2D)}");
            }

            _value = newValue as Texture2D;
            _image.image = _value;
            _objectField.SetValueWithoutNotify(newValue);
        }
    }
}