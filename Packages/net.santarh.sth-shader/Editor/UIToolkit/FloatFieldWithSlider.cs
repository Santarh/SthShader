using UnityEngine;
using UnityEngine.UIElements;

namespace SthShader.Editor.UIToolkit
{
    public sealed class FloatFieldWithSlider : BindableElement, INotifyValueChanged<float>
    {
        private readonly FloatField _floatField;
        private readonly Slider _slider;
        private float _value;

        public float value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                var previousValue = _value;
                ((INotifyValueChanged<float>)this).SetValueWithoutNotify(value);

                using var ev = ChangeEvent<float>.GetPooled(previousValue, value);
                ev.target = this;
                SendEvent(ev);
            }
        }

        public FloatFieldWithSlider(float start, float end, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0)
            : this(string.Empty, start, end, direction, pageSize)
        {
        }

        public FloatFieldWithSlider(string label, float start, float end, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("SthShader/SthFieldStyle"));
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

            _floatField = new FloatField();
            _floatField.RegisterValueChangedCallback(ev => value = ev.newValue);
            flexItem1.Add(_floatField);

            _slider = new Slider(start, end, direction, pageSize);
            _slider.RegisterValueChangedCallback(ev => value = ev.newValue);
            flexItem0.Add(_slider);
        }

        void INotifyValueChanged<float>.SetValueWithoutNotify(float newValue)
        {
            _value = newValue;
            _floatField.SetValueWithoutNotify(newValue);
            _slider.SetValueWithoutNotify(newValue);
        }
    }
}