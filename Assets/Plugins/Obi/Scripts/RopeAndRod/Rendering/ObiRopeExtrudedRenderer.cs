using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Rope Extruded Renderer", 883)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(ObiPathSmoother))]
    public class ObiRopeExtrudedRenderer : MonoBehaviour
    {
        private static ProfilerMarker m_UpdateExtrudedRopeRendererChunksPerfMarker =
            new ProfilerMarker("UpdateExtrudedRopeRenderer");

        [Range(0, 1)] public float uvAnchor;

        /**< Normalized position of texture coordinate origin along rope.*/
        public Vector2 uvScale = Vector2.one;

        /**< Scaling of uvs along rope.*/
        public bool normalizeV = true;

        public ObiRopeSection section;

        /**< Section asset to be extruded along the rope.*/
        public float thicknessScale = 0.8f;

        [HideInInspector] [NonSerialized] public Mesh extrudedMesh;
        private readonly List<Vector3> normals = new List<Vector3>();

        private ObiPathSmoother
            smoother; // Each renderer should have its own smoother. The renderer then has a method to get position and orientation at a point.

        private readonly List<Vector4> tangents = new List<Vector4>();
        private readonly List<int> tris = new List<int>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<Color> vertColors = new List<Color>();

        private readonly List<Vector3> vertices = new List<Vector3>();

        /**< Scales section thickness.*/
        private void OnEnable()
        {
            smoother = GetComponent<ObiPathSmoother>();
            smoother.OnCurveGenerated += UpdateRenderer;
            CreateMeshIfNeeded();
        }

        private void OnDisable()
        {
            smoother.OnCurveGenerated -= UpdateRenderer;
            DestroyImmediate(extrudedMesh);
        }

        private void CreateMeshIfNeeded()
        {
            if (extrudedMesh == null)
            {
                extrudedMesh = new Mesh();
                extrudedMesh.name = "extrudedMesh";
                extrudedMesh.MarkDynamic();
                GetComponent<MeshFilter>().mesh = extrudedMesh;
            }
        }

        public void UpdateRenderer(ObiActor actor)
        {
            using (m_UpdateExtrudedRopeRendererChunksPerfMarker.Auto())
            {
                if (section == null)
                    return;

                var rope = actor as ObiRopeBase;

                CreateMeshIfNeeded();
                ClearMeshData();

                var sectionIndex = 0;
                var sectionSegments = section.Segments;
                var verticesPerSection =
                    sectionSegments + 1; // the last vertex in each section must be duplicated, due to uv wraparound.
                var vCoord = -uvScale.y * rope.restLength * uvAnchor; // v texture coordinate.
                var actualToRestLengthRatio = smoother.SmoothLength / rope.restLength;

                Vector3 vertex = Vector3.zero, normal = Vector3.zero;
                var texTangent = Vector4.zero;
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

                        // calculate section thickness and scale the basis vectors by it:
                        var sectionThickness = curve.Data[i].thickness * thicknessScale;

                        // Loop around each segment:
                        var nextSectionIndex = sectionIndex + 1;
                        for (var j = 0; j <= sectionSegments; ++j)
                        {
                            // make just one copy of the section vertex:
                            var sectionVertex = section.vertices[j];

                            // calculate normal using section vertex, curve normal and binormal:
                            normal.x = (sectionVertex.x * curve.Data[i].normal.x +
                                        sectionVertex.y * curve.Data[i].binormal.x) * sectionThickness;
                            normal.y = (sectionVertex.x * curve.Data[i].normal.y +
                                        sectionVertex.y * curve.Data[i].binormal.y) * sectionThickness;
                            normal.z = (sectionVertex.x * curve.Data[i].normal.z +
                                        sectionVertex.y * curve.Data[i].binormal.z) * sectionThickness;

                            // offset curve position by normal:
                            vertex.x = curve.Data[i].position.x + normal.x;
                            vertex.y = curve.Data[i].position.y + normal.y;
                            vertex.z = curve.Data[i].position.z + normal.z;

                            // cross(normal, curve tangent)
                            texTangent.x = normal.y * curve.Data[i].tangent.z - normal.z * curve.Data[i].tangent.y;
                            texTangent.y = normal.z * curve.Data[i].tangent.x - normal.x * curve.Data[i].tangent.z;
                            texTangent.z = normal.x * curve.Data[i].tangent.y - normal.y * curve.Data[i].tangent.x;
                            texTangent.w = -1;

                            uv.x = j / (float) sectionSegments * uvScale.x;
                            uv.y = vCoord;

                            vertices.Add(vertex);
                            normals.Add(normal);
                            tangents.Add(texTangent);
                            vertColors.Add(curve.Data[i].color);
                            uvs.Add(uv);

                            if (j < sectionSegments && i < curve.Count - 1)
                            {
                                tris.Add(sectionIndex * verticesPerSection + j);
                                tris.Add(nextSectionIndex * verticesPerSection + j);
                                tris.Add(sectionIndex * verticesPerSection + j + 1);

                                tris.Add(sectionIndex * verticesPerSection + j + 1);
                                tris.Add(nextSectionIndex * verticesPerSection + j);
                                tris.Add(nextSectionIndex * verticesPerSection + j + 1);
                            }
                        }

                        sectionIndex++;
                    }
                }

                CommitMeshData();
            }
        }

        private void ClearMeshData()
        {
            extrudedMesh.Clear();
            vertices.Clear();
            normals.Clear();
            tangents.Clear();
            uvs.Clear();
            vertColors.Clear();
            tris.Clear();
        }

        private void CommitMeshData()
        {
            extrudedMesh.SetVertices(vertices);
            extrudedMesh.SetNormals(normals);
            extrudedMesh.SetTangents(tangents);
            extrudedMesh.SetColors(vertColors);
            extrudedMesh.SetUVs(0, uvs);
            extrudedMesh.SetTriangles(tris, 0, true);
        }
    }
}