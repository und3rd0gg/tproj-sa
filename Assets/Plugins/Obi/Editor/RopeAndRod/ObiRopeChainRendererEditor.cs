using UnityEditor;
using UnityEngine;

namespace Obi
{
    [CustomEditor(typeof(ObiRopeChainRenderer))]
    [CanEditMultipleObjects]
    public class ObiRopeChainRendererEditor : Editor
    {
        private ObiRopeChainRenderer renderer;

        public void OnEnable()
        {
            renderer = (ObiRopeChainRenderer) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            // Apply changes to the serializedProperty
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();

                renderer.ClearChainLinkInstances();
                renderer.CreateChainLinkInstances(renderer.GetComponent<ObiRopeBase>());
                renderer.UpdateRenderer(renderer.GetComponent<ObiRopeBase>());
            }
        }
    }
}