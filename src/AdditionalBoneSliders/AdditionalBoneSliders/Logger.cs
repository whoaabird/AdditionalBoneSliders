using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdditionalBoneSliders.Debug
{
    internal class Logger
    {
        private const string timeStampFormat = "yyyy-mm-dd HH:mm:ss";
        private const string defaultLogPath = "log.txt";

        private static readonly Dictionary<string, object> _fileLockObjects = new Dictionary<string, object>();

        private static string _defaultPath = defaultLogPath;

        public static string Path
        {
            get { return _defaultPath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new InvalidOperationException();

                _defaultPath = value;
            }
        }

        private string _path;
        private string _context;

        protected Logger(Type typeContext, string path)
        {
            _context = typeContext.Name;
            _path = path;

            Initialize(path);
        }

        private void Initialize(string path)
        {
            if (string.IsNullOrEmpty(path))
                path = Path;

            lock (GetFileLock(path))
            {
                if (!File.Exists(path))
                {
                    using (var stream = File.Create(path))
                    {
                        stream.Close();
                    }
                }
            }
        }

        private StreamWriter GetStreamWriter()
        {
            var streamWriter = new StreamWriter(_path, true);
            streamWriter.AutoFlush = true;

            return streamWriter;
        }

        private static object GetFileLock(string path)
        {
            if (!_fileLockObjects.TryGetValue(path, out object value))
            {
                value = new object();
                _fileLockObjects.Add(path, value);
            }

            return value;
        }

        public static Logger GetLogger(Type typeContext)
        {
            return GetLogger(typeContext, Path);
        }

        public static Logger GetLogger(Type typeContext, string path)
        {
            return new Logger(typeContext, path);
        }

        public void Info(string entry)
        {
            Log("Info", entry);
        }

        public void Warning(string entry)
        {
            Log("Warning", entry);
        }

        public void Error(string entry)
        {
            Log("Error", entry);
        }

        protected void Log(string level, string entry)
        {
            lock (GetFileLock(_path))
            {
                using (var streamWriter = GetStreamWriter())
                {
                    streamWriter.WriteLine($"{DateTime.Now.ToString(timeStampFormat)} [{level}][{_context}] {entry ?? "null"}");
                }
            }
        }
    }

    internal class GameObjectLogger : Logger
    {
        protected GameObjectLogger(Type typeContext, string path) : base(typeContext, path)
        {
        }

        public new static GameObjectLogger GetLogger(Type typeContext, string path)
        {
            return new GameObjectLogger(typeContext, path);
        }

        public void GameObjectHierarchy(UnityEngine.GameObject gameObject)
        {
            if (gameObject.transform != null)
            {
                if (gameObject.transform.parent != null)
                {
                    var parent = gameObject.transform.parent.gameObject;

                    if (parent != null)
                        GameObjectHierarchy(parent);
                }
            }

            Info(" + " + gameObject.name);

            foreach (var component in gameObject.GetComponents<UnityEngine.Component>())
            {
                Info("      " + component.GetType());
            }
        }

        public void GameObjectChildren(UnityEngine.GameObject gameObject)
        {
            GameObjectChildren(gameObject, 0);
        }

        private void GameObjectChildren(UnityEngine.GameObject gameObject, int indent)
        {
            var indentation = new string(' ', indent * 4);

            Info(indentation + " + " + gameObject.name);

            foreach (var component in gameObject.GetComponents<UnityEngine.Component>())
                Info(indentation + " | - - " + component.GetType());

            foreach (UnityEngine.Transform transform in gameObject.transform)
                GameObjectChildren(transform.gameObject, indent + 1);
        }
    }
}
