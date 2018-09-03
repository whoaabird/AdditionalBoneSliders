using AdditionalBoneSliders.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AdditionalBoneSliders
{
    public class PartManager : MonoBehaviour
    {
        private const float menuOffsetY = 70f;
        private const float partHeight = 35f;
        private const float defaultMenuHeight = 800f;
        private const float menuItemHeight = 26f;

        private static readonly Debug.Logger _log =
            Debug.Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, PartController> _parts { get; set; } = new Dictionary<string, PartController>();
        private Bone[] _bones { get; set; } = null;

        public bool HasGUI { get; private set; } = false;

        private bool mirrorEdit { get; } = true;

        private PartManager()
        {
        }

        public void Load()
        {
            var config = Config.Load();
            
            var path = GetCurrentlyEditedBoneModFilePath();

            if (path == null)
                return;

            if (File.Exists(path))
            {
                _log.Info("Loading: " + path);

                var bones = ReadBones(path);

                if (bones.Length == 0)
                    return;

                if (HasGUI)
                    Unload();

                StartCoroutine(CreateGUI(config, bones));

                _log.Info("Loading complete.");

                HasGUI = true;
            }
        }

        public static bool EditableFileExists()
        {
            var path = GetCurrentlyEditedBoneModFilePath();

            if (path == null)
                return false;

            return File.Exists(path);
        }

        public static string GetCurrentlyEditedBoneModFilePath()
        {
            var customControls = UnityEngine.Object.FindObjectsOfType<CustomControl>();
            if (customControls.Length == 0)
                return null;

            var charInfo = customControls[0].chainfo;
            if (charInfo == null)
                return null;

            var charFile = charInfo.chaFile;
            if (charFile == null)
                return null;

            var path = $"{UserData.Path}{charFile.CharaFileDirectory}{charFile.charaFileName}.bonemod.txt";

            return path;
        }

        private static Bone[] ReadBones(string path)
        {
            var bones = new List<Bone>();

            using (var file = new StreamReader(path))
            {
                while (!file.EndOfStream)
                {
                    var input = file.ReadLine();
                    var boneInfo = BoneInfo.FromString(input);

                    if (boneInfo != null)
                        bones.Add(new Bone(boneInfo));
                }
            }

            return bones.ToArray();
        }

        public static bool CanCreateGUI()
        {
            return GameObject.Find("Parts32") != null;
        }

        private IEnumerator CreateGUI(Config config, Bone[] bones)
        {
            var source = GameObject.Find("Parts32");

            _bones = bones;

            foreach (var bone in bones)
            {
                List<Config.SliderConfigInfo> settings;
                if (config == null)
                {
                    settings = Config.DefaultSetting;
                }
                else if (!config.Settings.TryGetValue(bone.Values.Name, out settings))
                {
                    continue;
                }

                foreach (var setting in settings)
                {
                    var partController = CreatePartGUI(source, setting, bone);

                    _parts.Add(partController.Name, partController);

                    partController.BoneValueChanged += PartController_BoneValueChanged;

                }

                yield return new WaitForEndOfFrame();
            }

            _log.Info($"Created {_parts.Count} sliders");

            yield return StartCoroutine(SetMenuHeight(source));

            yield return null;
        }

        private IEnumerator SetMenuHeight(GameObject source)
        {
            var bodyShapeDControlPanel = source.transform.parent.gameObject;

            var scrollView = bodyShapeDControlPanel.transform.parent.gameObject;
            var scrollRect = scrollView.GetComponent<ScrollRect>();
            var rectTransform = scrollRect.content;

            rectTransform.SetHeight(defaultMenuHeight + _parts.Count * menuItemHeight);

            yield return null;
        }

        private PartController CreatePartGUI(GameObject source, Config.SliderConfigInfo settings, Bone bone)
        {
            var part = UnityEngine.Object.Instantiate(source);
            part.name = bone.Values.Name;
            part.transform.SetParent(source.transform.parent);
            part.transform.position = source.transform.position;
            part.transform.localPosition = source.transform.localPosition;
            part.transform.localScale = source.transform.localScale;

            part.transform.position -= new Vector3(0, menuOffsetY + partHeight * _parts.Count, 0);

            var displayName = GetDisplayName(bone.Values.Name, settings.EditedValue);

            var text = part.GetChildComponent<Text>("SubItem");
            text.text = displayName;

            var inputField = part.GetChildComponent<InputField>("InputField");

            var slider = part.GetChildComponent<Slider>("Slider");
            slider.value = 1f;
            slider.wholeNumbers = false;
            slider.minValue = 0;
            slider.maxValue = 100f;

            var button = part.GetChildComponent<Button>("btn_def");

            var partController = new PartController(displayName, part, inputField, slider, button, bone, settings.EditedValue, settings.MinValue, settings.MaxValue);

            return partController;
        }

        private string GetDisplayName(string boneName, PartController.EditedValues value)
        {
            string displayName = $"{boneName.Replace("cf_hit_", "Hit ").Replace("cf_J_", "J ").Replace("cf_N_", "N ").Replace("_", " ")} ({value.ToString().ToUpper()})";

            return displayName;
        }

        private void PartController_BoneValueChanged(object sender, BoneValueChangedEvent e)
        {
            OnBoneValueChanged(sender as PartController);
        }

        private void OnBoneValueChanged(PartController part)
        {
            if (mirrorEdit && part != null)
            {
                string name = part.Bone.Values.Name;
                string mirroredName = null;

                if (name.Contains(" L "))
                    mirroredName = name.Replace(" L ", " R ");
                else if (name.Contains(" R "))
                    mirroredName = name.Replace(" R ", " L ");

                if (mirroredName != null)
                {
                    if (_parts.TryGetValue(mirroredName, out var controller))
                        controller.Value = part.Value;
                }
            }

            var path = GetCurrentlyEditedBoneModFilePath();

            WriteBones(path);
        }

        private void WriteBones(string path)
        {
            if (_bones == null)
                return;

            if (!File.Exists(path))
            {
                using (var stream = File.Create(path))
                {
                    stream.Close();
                }
            }

            using (var stream = new StreamWriter(path))
            {
                foreach (var bone in _bones)
                {
                    stream.WriteLine(bone.ToString());
                }
                stream.Close();
            }
        }

        public void Unload()
        {
            if (!HasGUI)
                return;

            foreach (var partController in _parts.Values)
            {
                UnityEngine.Object.Destroy(partController.Part);

                partController.BoneValueChanged -= PartController_BoneValueChanged;
            }

            // TODO: unload GUI menu height

            _parts = new Dictionary<string, PartController>();
            _bones = null;

            HasGUI = false;

            _log.Info("Unload complete.");
        }

        public void ResetAll()
        {
            foreach (var part in _parts.Values)
            {
                part.Reset();
            }

            OnBoneValueChanged(null);

            _log.Info("All values have been reset.");
        }
    }
}
