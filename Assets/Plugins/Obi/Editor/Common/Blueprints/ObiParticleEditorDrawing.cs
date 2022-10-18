using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    public class ObiParticleEditorDrawing : MonoBehaviour
    {
        public static Mesh particlesMesh;
        public static Material particleMaterial;

        private static void CreateParticlesMesh()
        {
            if (particlesMesh == null)
            {
                particlesMesh = new Mesh();
                particlesMesh.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private static void CreateParticleMaterials()
        {
            if (!particleMaterial) particleMaterial = Resources.Load<Material>("EditorParticle");
        }

        public static void DestroyParticlesMesh()
        {
            DestroyImmediate(particlesMesh);
        }

        public static void DrawParticles(Camera cam, ObiActorBlueprint blueprint, int activeParticle, bool[] visible,
            Color[] baseColor, int[] sortedIndices, float radiusScale = 1)
        {
            CreateParticlesMesh();
            CreateParticleMaterials();

            if (!particleMaterial.SetPass(0))
                return;

            //because each vertex needs to be drawn as a quad.
            var particlesPerDrawcall = Constants.maxVertsPerMesh / 4;
            var drawcallCount = blueprint.particleCount / particlesPerDrawcall + 1;
            particlesPerDrawcall = Mathf.Min(particlesPerDrawcall, blueprint.particleCount);

            var vertices = new List<Vector3>(blueprint.activeParticleCount * 4);
            var normals = new List<Vector3>(blueprint.activeParticleCount * 4);
            var uvs = new List<Vector4>(blueprint.activeParticleCount * 4);
            var colors = new List<Color>(blueprint.activeParticleCount * 4);
            var triangles = new List<int>(blueprint.activeParticleCount * 6);

            var particleOffset0 = new Vector3(1, 1, 0);
            var particleOffset1 = new Vector3(-1, 1, 0);
            var particleOffset2 = new Vector3(-1, -1, 0);
            var particleOffset3 = new Vector3(1, -1, 0);

            var radius = new Vector4(1, 0, 0, 0.005f * radiusScale);

            for (var i = 0; i < drawcallCount; ++i)
            {
                //Draw all cloth vertices:      
                particlesMesh.Clear();
                vertices.Clear();
                uvs.Clear();
                normals.Clear();
                colors.Clear();
                triangles.Clear();

                var index = 0;

                // Run over all particles (not only active ones), since they're reordered based on distance to camera.
                // Then test if the sorted index is active or not, and skip inactive ones.
                var limit = Mathf.Min((i + 1) * particlesPerDrawcall, blueprint.particleCount);

                for (var j = i * particlesPerDrawcall; j < limit; ++j)
                {
                    var sortedIndex = sortedIndices[j];

                    // skip inactive ones: 
                    if (!blueprint.IsParticleActive(sortedIndex))
                        continue;

                    normals.Add(particleOffset0);
                    normals.Add(particleOffset1);
                    normals.Add(particleOffset2);
                    normals.Add(particleOffset3);

                    uvs.Add(radius);
                    uvs.Add(radius);
                    uvs.Add(radius);
                    uvs.Add(radius);

                    vertices.Add(blueprint.positions[sortedIndex]);
                    vertices.Add(blueprint.positions[sortedIndex]);
                    vertices.Add(blueprint.positions[sortedIndex]);
                    vertices.Add(blueprint.positions[sortedIndex]);

                    colors.Add(baseColor[sortedIndex]);
                    colors.Add(baseColor[sortedIndex]);
                    colors.Add(baseColor[sortedIndex]);
                    colors.Add(baseColor[sortedIndex]);

                    triangles.Add(index + 2);
                    triangles.Add(index + 1);
                    triangles.Add(index);
                    triangles.Add(index + 3);
                    triangles.Add(index + 2);
                    triangles.Add(index);

                    index += 4;
                }

                particlesMesh.SetVertices(vertices);
                particlesMesh.SetNormals(normals);
                particlesMesh.SetColors(colors);
                particlesMesh.SetUVs(0, uvs);
                particlesMesh.SetTriangles(triangles, 0, true);

                Graphics.DrawMeshNow(particlesMesh, Matrix4x4.identity);
            }
        }
    }
}