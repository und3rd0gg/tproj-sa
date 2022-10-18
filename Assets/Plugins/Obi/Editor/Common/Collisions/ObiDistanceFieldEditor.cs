using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Obi
{
    [CustomEditor(typeof(ObiDistanceField))]
    public class ObiDistanceFieldEditor : Editor
    {
        private ObiDistanceField distanceField;
        private Vector2 previewDir;

        private PreviewHelpers previewHelper;
        private Material previewMaterial;

        private Mesh previewMesh;

        protected IEnumerator routine;
        private Texture3D volumeTexture;

        public void OnEnable()
        {
            distanceField = (ObiDistanceField) target;
            previewHelper = new PreviewHelpers();
            UpdatePreview();
        }

        public void OnDisable()
        {
            EditorUtility.ClearProgressBar();
            previewHelper.Cleanup();
            CleanupPreview();
        }

        private void UpdatePreview()
        {
            CleanupPreview();

            if (distanceField.InputMesh != null)
            {
                previewMesh = CreateMeshForBounds(distanceField.FieldBounds);
                previewMesh.hideFlags = HideFlags.HideAndDontSave;

                volumeTexture = distanceField.GetVolumeTexture(64);
                volumeTexture.hideFlags = HideFlags.HideAndDontSave;

                previewMaterial = Resources.Load<Material>("DistanceFieldPreview");
                previewMaterial.SetTexture("_Volume", volumeTexture);
                previewMaterial.SetVector("_AABBMin", -distanceField.FieldBounds.extents);
                previewMaterial.SetVector("_AABBMax", distanceField.FieldBounds.extents);
            }
        }

        private void CleanupPreview()
        {
            DestroyImmediate(previewMesh);
            DestroyImmediate(volumeTexture);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            GUI.enabled = distanceField.InputMesh != null;
            if (GUILayout.Button("Generate"))
            {
                // Start a coroutine job in the editor.
                EditorUtility.SetDirty(target);
                var job = new CoroutineJob();
                routine = job.Start(distanceField.Generate());
                EditorCoroutine.ShowCoroutineProgressBar("Generating distance field", ref routine);
                UpdatePreview();
                GUIUtility.ExitGUI();
            }

            GUI.enabled = true;

            var nodeCount = distanceField.nodes != null ? distanceField.nodes.Count : 0;
            var resolution = distanceField.FieldBounds.size.x / distanceField.EffectiveSampleSize;
            EditorGUILayout.HelpBox("Nodes: " + nodeCount + "\n" +
                                    "Size in memory: " + (nodeCount * 0.062f).ToString("0.#") + " kB\n" +
                                    "Compressed to: " + (nodeCount / Mathf.Pow(resolution, 3) * 100).ToString("0.##") +
                                    "%", MessageType.Info);

            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnInteractivePreviewGUI(Rect region, GUIStyle background)
        {
            previewDir = PreviewHelpers.Drag2D(previewDir, region);

            if (Event.current.type != EventType.Repaint || previewMesh == null) return;

            var quaternion = Quaternion.Euler(previewDir.y, 0f, 0f) * Quaternion.Euler(0f, previewDir.x, 0f) *
                             Quaternion.Euler(0, 120, -20f);

            previewHelper.BeginPreview(region, background);

            var bounds = previewMesh.bounds;
            var magnitude = Mathf.Sqrt(bounds.extents.sqrMagnitude);
            var num = 4f * magnitude;
            previewHelper.m_Camera.transform.position = -Vector3.forward * num;
            previewHelper.m_Camera.transform.rotation = Quaternion.identity;
            previewHelper.m_Camera.nearClipPlane = num - magnitude * 1.1f;
            previewHelper.m_Camera.farClipPlane = num + magnitude * 1.1f;

            // Compute matrix to rotate the mesh around the center of its bounds:
            var matrix = Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one) *
                         Matrix4x4.TRS(-bounds.center, Quaternion.identity, Vector3.one);

            Graphics.DrawMesh(previewMesh, matrix, previewMaterial, 1, previewHelper.m_Camera, 0);

            var texture = previewHelper.EndPreview();
            GUI.DrawTexture(region, texture, ScaleMode.StretchToFill, true);
        }

        /**
		 * Creates a solid mesh from some Bounds. This is used to display the distance field volumetric preview.
		 */
        private Mesh CreateMeshForBounds(Bounds b)
        {
            var m = new Mesh();

            /** Indices of bounds corners:

                         Y
                         2	   6
                       +------+
                   3  .'|  7 .'|
                   +---+--+'  |
                   |   |  |   |
                   |   +--+---+   X
                   | .' 0 | .' 4
                   +------+'
                Z 1        5

            */
            var vertices = new Vector3[8]
            {
                b.center + new Vector3(-b.extents.x, -b.extents.y, -b.extents.z), //0
                b.center + new Vector3(-b.extents.x, -b.extents.y, b.extents.z), //1
                b.center + new Vector3(-b.extents.x, b.extents.y, -b.extents.z), //2
                b.center + new Vector3(-b.extents.x, b.extents.y, b.extents.z), //3
                b.center + new Vector3(b.extents.x, -b.extents.y, -b.extents.z), //4
                b.center + new Vector3(b.extents.x, -b.extents.y, b.extents.z), //5
                b.center + new Vector3(b.extents.x, b.extents.y, -b.extents.z), //6
                b.center + new Vector3(b.extents.x, b.extents.y, b.extents.z) //7
            };
            var triangles = new int[36]
            {
                2, 3, 7,
                6, 2, 7,

                7, 5, 4,
                6, 7, 4,

                3, 1, 5,
                7, 3, 5,

                2, 0, 3,
                3, 0, 1,

                6, 4, 2,
                2, 4, 0,

                4, 5, 0,
                5, 1, 0
            };

            m.vertices = vertices;
            m.triangles = triangles;
            return m;
        }
    }
}