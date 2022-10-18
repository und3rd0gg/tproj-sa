using UnityEditor;
using UnityEngine;

namespace Obi
{
    [CustomEditor(typeof(ObiRopeLineRenderer))]
    [CanEditMultipleObjects]
    public class ObiRopeLineRendererEditor : Editor
    {
        private ObiRopeLineRenderer renderer;

        public void OnEnable()
        {
            renderer = (ObiRopeLineRenderer) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            // Apply changes to the serializedProperty
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();

                renderer.UpdateRenderer(null);
            }
        }
    }
}