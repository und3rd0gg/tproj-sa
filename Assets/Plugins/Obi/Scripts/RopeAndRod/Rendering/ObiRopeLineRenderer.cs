using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Rope Line Renderer", 884)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(ObiPathSmoother))]
    public class ObiRopeLineRenderer : MonoBehaviour
    {
        private static ProfilerMarker m_UpdateLineRopeRendererChunksPerfMarker =
            new ProfilerMarker("UpdateLineRopeRenderer");

        [Range(0, 1)] public float uvAnchor;

        /**< Normalized position of texture coordinate origin along rope.*/
        public Vector2 uvScale = Vector2.one;

        /**< Scaling of uvs along rope.*/
        public bool normalizeV = true;

        public float thicknessScale = 0.8f;

        [HideInInspector] [NonSerialized] public Mesh lineMesh;
        private readonly List<Vector3> normals = new List<Vector3>();

#if (UNITY_2019_1_OR_NEWER)
        private Action<ScriptableRenderContext, Camera> renderCallback;
#endif

        private ObiRopeBase rope;
        private ObiPathSmoother smoother;
        private readonly List<Vector4> tangents = new List<Vector4>();
        private readonly List<int> tris = new List<int>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<Color> vertColors = new List<Color>();

        private readonly List<Vector3> vertices = new List<Vector3>();

        /**< Scales section thickness.*/
        private void OnEnable()
        {
            CreateMeshIfNeeded();

#if (UNITY_2019_1_OR_NEWER)
            renderCallback = (cntxt, cam) => { UpdateRenderer(cam); };
            RenderPipelineManager.beginCameraRendering += renderCallback;
#endif
            Camera.onPreCull += UpdateRenderer;

            rope = GetComponent<ObiRopeBase>();
            smoother = GetComponent<ObiPathSmoother>();
        }

        private void OnDisable()
        {
#if (UNITY_2019_1_OR_NEWER)
            RenderPipelineManager.beginCameraRendering -= renderCallback;
#endif
            Camera.onPreCull -= UpdateRenderer;

            DestroyImmediate(lineMesh);
        }

        private void CreateMeshIfNeeded()
        {
            if (lineMesh == null)
            {
                lineMesh = new Mesh();
                lineMesh.name = "extrudedMesh";
                lineMesh.MarkDynamic();
                GetComponent<MeshFilter>().mesh = lineMesh;
            }
        }

        public void UpdateRenderer(Camera camera)
        {
            using (m_UpdateLineRopeRendererChunksPerfMarker.Auto())
            {
                if (camera == null || !rope.gameObject.activeInHierarchy)
                    return;

                CreateMeshIfNeeded();
                ClearMeshData();

                var actualToRestLengthRatio = smoother.SmoothLength / rope.restLength;

                var vCoord = -uvScale.y * rope.restLength * uvAnchor; // v texture coordinate.
                var sectionIndex = 0;

                var localSpaceCamera = rope.transform.InverseTransformPoint(camera.transform.position);
                Vector3 vertex = Vector3.zero, normal = Vector3.zero;
                var bitangent = Vector4.zero;
                var uv = Vector2.zero;

                for (var c = 0; c < smoother.smoothChunks.Count; ++c)
                {
                    var curve = smoother.smoothChunks[c];

                    for (var i = 0; i < curve.Count; ++i)
                    {
                        // Calculate previous and next curve indices:
                        var prevIndex = Mathf.Max(i - 1, 0);

                        // advance v texcoord:
                        vCoord += uvScale.y *
                                  (Vector3.Distance(curve.Data[i].position, curve.Data[prevIndex].position) /
                                   (normalizeV ? smoother.SmoothLength : actualToRestLengthRatio));

                        // calculate section thickness (either constant, or particle radius based):
                        var sectionThickness = curve.Data[i].thickness * thicknessScale;


                        normal.x = curve.Data[i].position.x - localSpaceCamera.x;
                        normal.y = curve.Data[i].position.y - localSpaceCamera.y;
                        normal.z = curve.Data[i].position.z - localSpaceCamera.z;
                        normal.Normalize();

                        bitangent.x = -(normal.y * curve.Data[i].tangent.z - normal.z * curve.Data[i].tangent.y);
                        bitangent.y = -(normal.z * curve.Data[i].tangent.x - normal.x * curve.Data[i].tangent.z);
                        bitangent.z = -(normal.x * curve.Data[i].tangent.y - normal.y * curve.Data[i].tangent.x);
                        bitangent.w = 0;
                        bitangent.Normalize();

                        vertex.x = curve.Data[i].position.x - bitangent.x * sectionThickness;
                        vertex.y = curve.Data[i].position.y - bitangent.y * sectionThickness;
                        vertex.z = curve.Data[i].position.z - bitangent.z * sectionThickness;
                        vertices.Add(vertex);

                        vertex.x = curve.Data[i].position.x + bitangent.x * sectionThickness;
                        vertex.y = curve.Data[i].position.y + bitangent.y * sectionThickness;
                        vertex.z = curve.Data[i].position.z + bitangent.z * sectionThickness;
                        vertices.Add(vertex);

                        normals.Add(-normal);
                        normals.Add(-normal);

                        bitangent.w = 1;
                        tangents.Add(bitangent);
                        tangents.Add(bitangent);

                        vertColors.Add(curve.Data[i].color);
                        vertColors.Add(curve.Data[i].color);

                        uv.x = 0;
                        uv.y = vCoord;
                        uvs.Add(uv);
                        uv.x = 1;
                        uvs.Add(uv);

                        if (i < curve.Count - 1)
                        {
                            tris.Add(sectionIndex * 2);
                            tris.Add((sectionIndex + 1) * 2);
                            tris.Add(sectionIndex * 2 + 1);

                            tris.Add(sectionIndex * 2 + 1);
                            tris.Add((sectionIndex + 1) * 2);
                            tris.Add((sectionIndex + 1) * 2 + 1);
                        }

                        sectionIndex++;
                    }
                }

                CommitMeshData();
            }
        }

        private void ClearMeshData()
        {
            lineMesh.Clear();
            vertices.Clear();
            normals.Clear();
            tangents.Clear();
            uvs.Clear();
            vertColors.Clear();
            tris.Clear();
        }

        private void CommitMeshData()
        {
            lineMesh.SetVertices(vertices);
            lineMesh.SetNormals(normals);
            lineMesh.SetTangents(tangents);
            lineMesh.SetColors(vertColors);
            lineMesh.SetUVs(0, uvs);
            lineMesh.SetTriangles(tris, 0, true);
        }
    }
}