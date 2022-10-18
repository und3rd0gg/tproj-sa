using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Obi
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VisibleIf : MultiPropertyAttribute
    {
        private MethodInfo eventMethodInfo;
        private FieldInfo fieldInfo;

        public VisibleIf(string methodName, bool negate = false)
        {
            MethodName = methodName;
            Negate = negate;
        }

        public string MethodName { get; }
        public bool Negate { get; }

#if UNITY_EDITOR
        internal override bool IsVisible(SerializedProperty property)
        {
            return Visibility(property) == !Negate;
        }

        private bool Visibility(SerializedProperty property)
        {
            var eventOwnerType = property.serializedObject.targetObject.GetType();
            var eventName = MethodName;

            // Try finding a method with the name provided:
            if (eventMethodInfo == null)
                eventMethodInfo = eventOwnerType.GetMethod(eventName,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // If we could not find a method with that name, look for a field:
            if (eventMethodInfo == null && fieldInfo == null)
                fieldInfo = eventOwnerType.GetField(eventName,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (eventMethodInfo != null)
                return (bool) eventMethodInfo.Invoke(property.serializedObject.targetObject, null);
            if (fieldInfo != null)
                return (bool) fieldInfo.GetValue(property.serializedObject.targetObject);
            Debug.LogWarning(string.Format("VisibleIf: Unable to find method or field {0} in {1}", eventName,
                eventOwnerType));

            return true;
        }
#endif
    }
}