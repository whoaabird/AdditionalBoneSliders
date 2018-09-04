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
        public enum EditedValues
        {
            X,
            Y,
            Z,
            Scale
        };

        private float _minValue;
        private float _maxValue;

        public Bone Bone { get; }
        public GameObject Part { get; }
        public InputField InputField { get; }
        public Slider Slider { get; }
        public Button Button { get; }

        public event EventHandler<BoneValueChangedEvent> BoneValueChanged;

        private const string numberFormat = "F1";

        public string Name { get; private set; }

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

        public EditedValues EditedValue { get; }

        public float Value
        {
            get
            {
                switch (EditedValue)
                {
                    case EditedValues.X:
                        return Bone.X;
                    case EditedValues.Y:
                        return Bone.Y;
                    case EditedValues.Z:
                        return Bone.Z;
                    case EditedValues.Scale:
                        return Bone.Scale;
                    default:
                        throw new NotImplementedException($"{EditedValue}");
                }                
            }
            set
            {
                switch (EditedValue)
                {
                    case EditedValues.X:
                        if (Bone.X == value)
                            return;
                        Bone.X = value;
                        break;
                    case EditedValues.Y:
                        if (Bone.Y == value)
                            return;
                        Bone.Y = value;
                        break;
                    case EditedValues.Z:
                        if (Bone.Z == value)
                            return;
                        Bone.Z = value;
                        break;
                    case EditedValues.Scale:
                        if (Bone.Scale == value)
                            return;
                        Bone.Scale = value;
                        break;
                }

                UpdateGUI();
                OnValueChanged();
            }
        }

        public PartController(string name, GameObject part, InputField inputField, Slider slider, Button button, Bone bone, EditedValues editedValue, float minValue, float maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;

            if (_maxValue <= _minValue)
                throw new ArgumentException("minValue has to larger than maxValue.");

            Name = name;            Bone = bone;
            Part = part;
            InputField = inputField;
            Slider = slider;
            Button = button;
            EditedValue = editedValue;

            AddListeners();

            UpdateGUI();
        }

        private void AddListeners()
        {
            InputField.onEndEdit.AddListener((x) => InputText = x);
            Slider.onValueChanged.AddListener((x) => Value = _minValue + Slider.normalizedValue * (_maxValue - _minValue));
            Button.onClick.AddListener(() => ResetEditedValue());
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

            InputField.text = Value.ToString(numberFormat);
            Slider.normalizedValue = (Value - _minValue) / (_maxValue - _minValue);

            AddListeners();
        }

        public void ResetEditedValue()
        {
            switch (EditedValue)
            {
                case EditedValues.Scale:
                    Bone.Scale = Bone.DefaultValues.Scale;
                    break;
                case EditedValues.X:
                    Bone.X = Bone.DefaultValues.X;
                    break;
                case EditedValues.Y:
                    Bone.Y = Bone.DefaultValues.Y;
                    break;
                case EditedValues.Z:
                    Bone.Z = Bone.DefaultValues.Z;
                    break;
            }
            OnValueChanged();
            UpdateGUI();
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
