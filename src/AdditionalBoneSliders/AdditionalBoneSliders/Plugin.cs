using AdditionalBoneSliders.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AdditionalBoneSliders.Debug;

namespace AdditionalBoneSliders
{
    public class Plugin : IllusionPlugin.IPlugin
    {
        private static readonly string _setupLogPath = 
            (Debug.Logger.Path = $"{UserData.Path}..\\Plugins\\AdditionalBoneSliders.log");

        private static readonly GameObjectLogger _initLog = 
            GameObjectLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, _setupLogPath);

        private static readonly GameObjectLogger _log = _initLog;

        private static PartManager _partManager = null;
        private static string _editedBoneModeFilePath = null;

        public string Name { get { return "AdditionalBoneSliders"; } }

        public string Version { get { return "0.1"; } }

        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
            _log.Info("Starting...");
        }

        public void OnFixedUpdate()
        {
        }

        public void OnLateUpdate()
        {

        }

        public void OnLevelWasInitialized(int level)
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (sceneName == "CustomScene")
            {
                _partManager = new PartManager();
                _log.Info("Created new PartManager");
            }
        }

        public void OnLevelWasLoaded(int level)
        {
            if (_partManager != null)
            {
                _partManager.Unload();
                _partManager = null;
            }
        }

        public void OnUpdate()
        {
            try
            {
                if (_partManager != null)
                {
                    var currentBoneModFile = PartManager.GetCurrentlyEditedBoneModFilePath();

                    if (currentBoneModFile == null && _partManager.HasGUI)
                    {
                        // Character changed to one w/o bone mod
                        _partManager.Unload();
                        _editedBoneModeFilePath = null;
                    }

                    if (currentBoneModFile != null && currentBoneModFile != _editedBoneModeFilePath)
                    {
                        // Loaded BoneModFile changed changed
                        _partManager.Unload();
                    }

                    if (currentBoneModFile != null)
                    {
                        if (!_partManager.HasGUI && PartManager.CanCreateGUI())
                        {
                            if (PartManager.EditableFileExists())
                            {
                                _partManager.Load();
                                _editedBoneModeFilePath = currentBoneModFile;
                            }
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    // log selected GameObject once
                    var gameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

                    gameObject.DoOnce((x) =>
                    {
                        _log.Info("Children:");
                        _log.GameObjectChildren(x);
                        _log.Info("Hierarchy:");
                        _log.GameObjectHierarchy(x);
                    });
                }

            }
            catch (Exception e)
            {
                _log.Error($"Exception of type {e.GetType()} thrown: {e}");
                throw;
            }
        }
    }
}
