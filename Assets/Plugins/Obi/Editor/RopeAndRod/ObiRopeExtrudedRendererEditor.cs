using UnityEditor;
using UnityEngine;

namespace Obi
{
    [CustomEditor(typeof(ObiRopeExtrudedRenderer))]
    [CanEditMultipleObjects]
    public class ObiRopeExtrudedRendererEditor : Editor
    {
        private ObiRopeExtrudedRenderer renderer;

        public void OnEnable()
        {
            renderer = (ObiRopeExtrudedRenderer) target;
        }

        private void BakeMesh()
        {
            if (renderer != null && renderer.extrudedMesh != null)
                ObiEditorUtils.SaveMesh(renderer.extrudedMesh, "Save extruded mesh", "rope mesh");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            if (GUILayout.Button("BakeMesh")) BakeMesh();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            // Apply changes to the serializedProperty
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();

                renderer.UpdateRenderer(renderer.GetComponent<ObiRopeBase>());
            }
        }
    }
}