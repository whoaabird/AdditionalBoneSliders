using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AdditionalBoneSliders.Util
{
    internal static class ObjectExtension
    {
        private readonly static HashSet<object> _doOnceTokens = new HashSet<object>();

        public static void DoOnce<TObject>(this TObject token, Action<TObject> action) where TObject : class
        {
            if (_doOnceTokens.Contains(token))
                return;

            _doOnceTokens.Add(token);

            action(token);
        }

        public static bool TryGetNonPublic<TObject, TField>(this TObject o, string fieldName, out TField value) where TField : class
        {
            if (o == null)
            {
                value = null;
                return false;
            }

            var fieldInfo = o.GetType().BaseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                value = fieldInfo.GetValue(o) as TField;
                return true;
            }

            value = null;

            return false;
        }
    }

    internal static class GameObjectExtension
    {
        public static GameObject GetChild(this GameObject gameObject, string name)
        {
            foreach (Transform transform in gameObject.transform)
            {
                if (transform.name == name)
                    return transform.gameObject;
            }

            return null;
        }

        public static TComponent GetChildComponent<TComponent>(this GameObject gameObject, string childName) where TComponent : Component
        {
            var child = gameObject.GetChild(childName);

            if (child == null)
                return null;

            return child.GetComponent<TComponent>();
        }
    }

    internal static class RectTransformExtension
    {
        // Courtesy of: http://orbcreation.com/orbcreation/page.orb?1099

        public static void SetSize(this RectTransform trans, Vector2 newSize)
        {
            Vector2 oldSize = trans.rect.size;
            Vector2 deltaSize = newSize - oldSize;
            trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
            trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
        }

        public static void SetHeight(this RectTransform trans, float newSize)
        {
            SetSize(trans, new Vector2(trans.rect.size.x, newSize));
        }
    }
}
