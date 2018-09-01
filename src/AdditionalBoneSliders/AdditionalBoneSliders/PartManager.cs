using AdditionalBoneSliders.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AdditionalBoneSliders
{
    public class PartManager
    {
        private static readonly Debug.Logger _log =
            Debug.Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<PartController> _parts { get; set; }
        private Bone[] _bones { get; set; }

        public bool HasGUI { get; private set; } = false;

        private bool mirrorEdit { get; } = true;

        public PartManager()
        {
            _parts = new List<PartController>();
            _bones = null;
        }

        public void Load()
        {
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

                CreateGUI(bones);

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

        private void CreateGUI(Bone[] bones)
        {
            const int menuOffsetY = 70;
            const int menuItemHeight = 35;

            var source = GameObject.Find("Parts32");

            _bones = bones;

            var ordered = bones.OrderBy(b => b.Values.Name);

            foreach (var bone in bones)
            {
                if (bone.Values.Index == -1)
                    continue;

                var part = UnityEngine.Object.Instantiate(source);
                part.name = bone.Values.Name;
                part.transform.SetParent(source.transform.parent);
                part.transform.position = source.transform.position;
                part.transform.localPosition = source.transform.localPosition;
                part.transform.localScale = source.transform.localScale;

                part.transform.position -= new Vector3(0, menuOffsetY + menuItemHeight * _parts.Count, 0);

                var text = part.GetChildComponent<Text>("SubItem");

                text.text = GetDisplayName(bone.Values.Name);

                var inputField = part.GetChildComponent<InputField>("InputField");

                var slider = part.GetChildComponent<Slider>("Slider");
                slider.minValue = 0;
                slider.maxValue = 100f;
                slider.wholeNumbers = true;
                slider.value = 1f;

                var button = part.GetChildComponent<Button>("btn_def");

                var partController = new PartController(part, inputField, slider, button, bone, 0.5f, 4f);

                _parts.Add(partController);

                partController.BoneValueChanged += PartController_BoneValueChanged;
            }

            var bodyShapeDControlPanel = source.transform.parent.gameObject;

            var scrollView = bodyShapeDControlPanel.transform.parent.gameObject;
            var scrollRect = scrollView.GetComponent<ScrollRect>();
            var rectTransform = scrollRect.content;

            rectTransform.SetHeight(5000f);
        }

        private string GetDisplayName(string boneName)
        {
            string displayName = boneName
                .Replace("cf_hit_", "Hit ")
                .Replace("cf_J_", "J ")
                .Replace("cf_N_", "N ")
                .Replace("_", " ");

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

                if (name.EndsWith("_L"))
                    mirroredName = name.Substring(0, name.Length - 2) + "_R";
                else if (name.EndsWith("_R"))
                    mirroredName = name.Substring(0, name.Length - 2) + "_L";

                if (mirroredName != null)
                {
                    var controller = _parts.Where(b => b.Bone.Values.Name == mirroredName).SingleOrDefault();

                    if (controller != null)
                        controller.Scale = part.Scale;
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

            foreach (var partController in _parts)
            {
                UnityEngine.Object.Destroy(partController.Part);

                partController.BoneValueChanged -= PartController_BoneValueChanged;
            }

            // TODO: unload GUI menu height

            _parts = new List<PartController>();
            _bones = null;

            HasGUI = false;

            _log.Info("Unload complete.");
        }

        public void ResetAll()
        {
            foreach (var part in _parts)
            {
                part.Reset();
            }

            OnBoneValueChanged(null);

            _log.Info("All values have been reset.");
        }
    }
}
