using UnityEditor;
using UnityEngine;

namespace Obi
{
    [CustomEditor(typeof(ObiPathSmoother), true)]
    [CanEditMultipleObjects]
    public class ObiPathSmootherEditor : Editor
    {
        private ObiPathSmoother shape;

        public void OnEnable()
        {
            shape = (ObiPathSmoother) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            // Apply changes to the serializedProperty
            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();
        }
    }
}