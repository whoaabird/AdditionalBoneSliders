using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdditionalBoneSliders
{
    internal class Config
    {
        private const float defaultMinValue = 0.5f;
        private const float defaultMaxValue = 2.0f;

        public static readonly List<SliderConfigInfo> DefaultSetting = new List<SliderConfigInfo>()
        {
            new SliderConfigInfo() { Bone = null, EditedValue = PartController.EditedValues.Scale, MinValue = defaultMinValue, MaxValue = defaultMaxValue },
            new SliderConfigInfo() { Bone = null, EditedValue = PartController.EditedValues.X, MinValue = defaultMinValue, MaxValue = defaultMaxValue },
            new SliderConfigInfo() { Bone = null, EditedValue = PartController.EditedValues.Y, MinValue = defaultMinValue, MaxValue = defaultMaxValue },
            new SliderConfigInfo() { Bone = null, EditedValue = PartController.EditedValues.Z, MinValue = defaultMinValue, MaxValue = defaultMaxValue }
        };

        internal class SliderConfigInfo
        {
            public string Bone { get; set; }
            public PartController.EditedValues EditedValue { get; set; }
            public float MinValue { get; set; }
            public float MaxValue { get; set; }
        }

        private const string configFilename = "AdditionalBoneSliders.config.txt";

        private static readonly Debug.Logger _log =
            Debug.Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Dictionary<string, List<SliderConfigInfo>> Settings { get; }

        private Config()
        {
            Settings = new Dictionary<string, List<SliderConfigInfo>>();
        }

        private static void LogParseWarning(int line, string reason)
        {
            _log.Warning($"Could not parse config line number: {line}. Reason: '{reason}'");
        }

        public static Config Load()
        {
            var configPath = $"{UserData.Path}..\\Plugins\\{configFilename}";

            if (!File.Exists(configPath))
            {
                _log.Info($"Config file ({configPath}) missing. Using default values.");
                return null;
            }

            var config = new Config();

            using (var streamReader = new StreamReader(configPath))
            {
                int line = 0;
                while (!streamReader.EndOfStream)
                {
                    line++;
                    string input = streamReader.ReadLine();

                    if (string.IsNullOrEmpty(input) || input.Trim().StartsWith("//"))
                        continue;

                    var settings = input.Split(',');

                    if (settings.Length < 5)
                    {
                        LogParseWarning(line, "Missing arguments");
                        continue;
                    }

                    string bone = settings[1];

                    if (string.IsNullOrEmpty(bone))
                    {
                        LogParseWarning(line, "Missing bone name");
                        continue;
                    }

                    if (!float.TryParse(settings[2], out float minValue))
                    {
                        LogParseWarning(line, "Unabled to parse min value");
                        continue;
                    }

                    if (!float.TryParse(settings[3], out float maxValue))
                    {
                        LogParseWarning(line, "Unabled to parse max value");
                        continue;
                    }

                    for (int i = 4; i < settings.Length; i++)
                    {
                        PartController.EditedValues editedValue;
                        try
                        {
                            editedValue = (PartController.EditedValues)Enum.Parse(typeof(PartController.EditedValues), settings[i], true);
                        }
                        catch
                        {
                            LogParseWarning(line, "Value must be X, Y, Z or Scale");
                            continue;
                        }

                        var sliderConfig = new SliderConfigInfo()
                        {
                            Bone = bone.Trim(),
                            EditedValue = editedValue,
                            MaxValue = maxValue,
                            MinValue = minValue
                        };

                        if (config.Settings.TryGetValue(bone, out List<SliderConfigInfo> settingsList))
                        {
                            settingsList.Add(sliderConfig);
                        }
                        else
                        {
                            config.Settings.Add(bone, new List<SliderConfigInfo>()
                            {
                                sliderConfig
                            });
                        }
                    }
                }
            }

            _log.Info($"Config loaded: {configPath}");

            return config;
        }
    }
}
