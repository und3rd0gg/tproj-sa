using UnityEditor;
using UnityEngine;

namespace Obi
{
    [CustomEditor(typeof(ObiRopeMeshRenderer))]
    [CanEditMultipleObjects]
    public class ObiRopeMeshRendererEditor : Editor
    {
        private ObiRopeMeshRenderer renderer;

        public void OnEnable()
        {
            renderer = (ObiRopeMeshRenderer) target;
        }

        private void BakeMesh()
        {
            if (renderer != null && renderer.deformedMesh != null)
                ObiEditorUtils.SaveMesh(renderer.deformedMesh, "Save deformed mesh", "rope mesh");
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