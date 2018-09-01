using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AdditionalBoneSliders
{
    public class PartController
    {
        private float _minValue;
        private float _maxValue;

        public Bone Bone { get; }
        public GameObject Part { get; }
        public InputField InputField { get; }
        public Slider Slider { get; }
        public Button Button { get; }

        public event EventHandler<BoneValueChangedEvent> BoneValueChanged;

        private const string numberFormat = "F1";

        public string InputText
        {
            set
            {
                if (float.TryParse(value, out float input))
                {
                    Slider.value = input;
                }
            }
        }

        public float Scale
        {
            get { return Bone.Scale; }
            set
            {
                if (Bone != null && Bone.Scale != value)
                {
                    Bone.Scale = value;

                    Bone.Values.Enabled =
                        Bone.Values.Scale != Bone.DefaultValues.Scale;

                    UpdateGUI();

                    OnValueChanged();
                }
            }
        }

        public PartController(GameObject part, InputField inputField, Slider slider, Button button, Bone bone, float minValue, float maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;

            if (_maxValue <= _minValue)
                throw new ArgumentException("minValue has to larger than maxValue.");

            Bone = bone;
            Part = part;
            InputField = inputField;
            Slider = slider;
            Button = button;

            UpdateGUI();

            AddListeners();
        }

        private void AddListeners()
        {
            InputField.onEndEdit.AddListener((x) => InputText = x);
            Slider.onValueChanged.AddListener((x) => Scale = _minValue + Slider.normalizedValue * (_maxValue - _minValue));
            Button.onClick.AddListener(() => Reset());
        }

        private void RemoveListeners()
        {
            InputField.onEndEdit.RemoveAllListeners();
            Slider.onValueChanged.RemoveAllListeners();
            Button.onClick.RemoveAllListeners();
        }

        public void UpdateGUI()
        {
            RemoveListeners();

            if (InputField != null)
                InputField.text = Bone.Scale.ToString(numberFormat);

            if (Slider != null)
                Slider.normalizedValue = (Bone.Scale - _minValue) / (_maxValue - _minValue);

            AddListeners();
        }

        public void Reset()
        {
            Bone.Reset();
            OnValueChanged();
            UpdateGUI();
        }

        private void OnValueChanged()
        {
            BoneValueChanged?.Invoke(this, new BoneValueChangedEvent(Bone));
        }
    }
}
