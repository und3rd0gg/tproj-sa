using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Obi
{
    public class ParticleImpostorRendering
    {
        private static ProfilerMarker m_ParticlesToMeshPerfMarker = new ProfilerMarker("ParticlesToMesh");

        private readonly List<Vector4> anisotropy1 = new List<Vector4>(4000);
        private readonly List<Vector4> anisotropy2 = new List<Vector4>(4000);
        private readonly List<Vector4> anisotropy3 = new List<Vector4>(4000);
        private readonly List<Color> colors = new List<Color>(4000);
        private int drawcallCount;

        private readonly List<Mesh> meshes = new List<Mesh>();
        private readonly List<Vector3> normals = new List<Vector3>(4000);

        private readonly Vector3 particleOffset0 = new Vector3(1, 1, 0);
        private readonly Vector3 particleOffset1 = new Vector3(-1, 1, 0);
        private readonly Vector3 particleOffset2 = new Vector3(-1, -1, 0);
        private readonly Vector3 particleOffset3 = new Vector3(1, -1, 0);

        private int particlesPerDrawcall;
        private readonly List<int> triangles = new List<int>(6000);

        private readonly List<Vector3> vertices = new List<Vector3>(4000);

        public IEnumerable<Mesh> Meshes => meshes.AsReadOnly();

        private void Apply(Mesh mesh)
        {
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetUVs(0, anisotropy1);
            mesh.SetUVs(1, anisotropy2);
            mesh.SetUVs(2, anisotropy3);
            mesh.SetTriangles(triangles, 0, true);
        }

        public void ClearMeshes()
        {
            foreach (var mesh in meshes)
                Object.DestroyImmediate(mesh);
            meshes.Clear();
        }

        public void UpdateMeshes(IObiParticleCollection collection, bool[] visible = null, Color[] tint = null)
        {
            using (m_ParticlesToMeshPerfMarker.Auto())
            {
                // figure out the size of our drawcall arrays:
                particlesPerDrawcall = Constants.maxVertsPerMesh / 4;
                drawcallCount = collection.activeParticleCount / particlesPerDrawcall + 1;
                particlesPerDrawcall = Mathf.Min(particlesPerDrawcall, collection.activeParticleCount);

                // If the amount of meshes we need to draw the particles has changed:
                if (drawcallCount != meshes.Count)
                {
                    // Re-generate meshes:
                    ClearMeshes();
                    for (var i = 0; i < drawcallCount; i++)
                    {
                        var mesh = new Mesh();
                        mesh.name = "Particle impostors";
                        mesh.hideFlags = HideFlags.HideAndDontSave;
                        meshes.Add(mesh);
                    }
                }

                Vector3 position;
                var basis1 = new Vector4(1, 0, 0, 0);
                var basis2 = new Vector4(0, 1, 0, 0);
                var basis3 = new Vector4(0, 0, 1, 0);
                Color color;

                var visibleLength = visible != null ? visible.Length : 0;
                var tintLength = tint != null ? tint.Length : 0;

                //Convert particle data to mesh geometry:
                for (var i = 0; i < drawcallCount; i++)
                {
                    // Clear all arrays
                    vertices.Clear();
                    normals.Clear();
                    colors.Clear();
                    triangles.Clear();
                    anisotropy1.Clear();
                    anisotropy2.Clear();
                    anisotropy3.Clear();

                    var index = 0;
                    var limit = Mathf.Min((i + 1) * particlesPerDrawcall, collection.activeParticleCount);

                    for (var j = i * particlesPerDrawcall; j < limit; ++j)
                    {
                        if (j < visibleLength && !visible[j])
                            continue;

                        var runtimeIndex = collection.GetParticleRuntimeIndex(j);
                        position = collection.GetParticlePosition(runtimeIndex);
                        collection.GetParticleAnisotropy(runtimeIndex, ref basis1, ref basis2, ref basis3);
                        color = collection.GetParticleColor(runtimeIndex);

                        if (j < tintLength)
                            color *= tint[j];

                        vertices.Add(position);
                        vertices.Add(position);
                        vertices.Add(position);
                        vertices.Add(position);

                        normals.Add(particleOffset0);
                        normals.Add(particleOffset1);
                        normals.Add(particleOffset2);
                        normals.Add(particleOffset3);

                        colors.Add(color);
                        colors.Add(color);
                        colors.Add(color);
                        colors.Add(color);

                        anisotropy1.Add(basis1);
                        anisotropy1.Add(basis1);
                        anisotropy1.Add(basis1);
                        anisotropy1.Add(basis1);

                        anisotropy2.Add(basis2);
                        anisotropy2.Add(basis2);
                        anisotropy2.Add(basis2);
                        anisotropy2.Add(basis2);

                        anisotropy3.Add(basis3);
                        anisotropy3.Add(basis3);
                        anisotropy3.Add(basis3);
                        anisotropy3.Add(basis3);

                        triangles.Add(index + 2);
                        triangles.Add(index + 1);
                        triangles.Add(index);
                        triangles.Add(index + 3);
                        triangles.Add(index + 2);
                        triangles.Add(index);

                        index += 4;
                    }

                    Apply(meshes[i]);
                }
            }
        }
    }
}