using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    public class ASDF
    {
        private const float sqrt3 = 1.73205f;

        private static readonly Vector4[] corners =
        {
            new Vector4(-1, -1, -1, -1),
            new Vector4(-1, -1, 1, -1),
            new Vector4(-1, 1, -1, -1),
            new Vector4(-1, 1, 1, -1),

            new Vector4(1, -1, -1, -1),
            new Vector4(1, -1, 1, -1),
            new Vector4(1, 1, -1, -1),
            new Vector4(1, 1, 1, -1)
        };

        private static readonly Vector4[] samples =
        {
            new Vector4(0, 0, 0, 0),
            new Vector4(1, 0, 0, 0),
            new Vector4(-1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, -1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, -1, 0),

            new Vector4(0, -1, -1, 0),
            new Vector4(0, -1, 1, 0),
            new Vector4(0, 1, -1, 0),
            new Vector4(0, 1, 1, 0),

            new Vector4(-1, 0, -1, 0),
            new Vector4(-1, 0, 1, 0),
            new Vector4(1, 0, -1, 0),
            new Vector4(1, 0, 1, 0),

            new Vector4(-1, -1, 0, 0),
            new Vector4(-1, 1, 0, 0),
            new Vector4(1, -1, 0, 0),
            new Vector4(1, 1, 0, 0)
        };

        public static IEnumerator Build(float maxError, int maxDepth, Vector3[] vertexPositions, int[] triangleIndices,
            List<DFNode> nodes, int yieldAfterNodeCount = 32)
        {
            // Empty vertex or triangle lists, return.
            if (maxDepth <= 0 ||
                nodes == null ||
                vertexPositions == null || vertexPositions.Length == 0 ||
                triangleIndices == null || triangleIndices.Length == 0)
                yield break;

            // Build a bounding interval hierarchy from the triangles, to speed up distance queries:
            var t = new IBounded[triangleIndices.Length / 3];
            for (var i = 0; i < t.Length; ++i)
            {
                var t1 = triangleIndices[i * 3];
                var t2 = triangleIndices[i * 3 + 1];
                var t3 = triangleIndices[i * 3 + 2];
                t[i] = new Triangle(t1, t2, t3, vertexPositions[t1], vertexPositions[t2], vertexPositions[t3]);
            }

            var bih = BIH.Build(ref t);

            // Copy reordered triangles over to a new array:
            var tris = Array.ConvertAll(t, x => (Triangle) x);

            // Build angle weighted normals, used to determine the sign of the distance field.
            var angleNormals = ObiUtils.CalculateAngleWeightedNormals(vertexPositions, triangleIndices);

            // Calculate bounding box of the mesh:
            var bounds = new Bounds(vertexPositions[0], Vector3.zero);
            for (var i = 1; i < vertexPositions.Length; ++i)
                bounds.Encapsulate(vertexPositions[i]);

            bounds.Expand(0.2f);

            // Auxiliar variables to keep track of current tree depth:
            var depth = 0;
            var nodesToNextLevel = 1;

            // Initialize node list:
            Vector4 center = bounds.center;
            var boundsExtents = bounds.extents;
            center[3] = Mathf.Max(boundsExtents[0], Math.Max(boundsExtents[1], boundsExtents[2]));
            nodes.Clear();
            nodes.Add(new DFNode(center));


            var queue = new Queue<int>();
            queue.Enqueue(0);

            var processedNodeCount = 0;
            while (queue.Count > 0)
            {
                // get current node:
                var index = queue.Dequeue();
                var node = nodes[index];

                // measure distance at the 8 node corners:
                for (var i = 0; i < 8; ++i)
                {
                    var point = node.center + corners[i] * node.center[3];
                    point[3] = 0;
                    var distance = BIH.DistanceToSurface(bih, tris, vertexPositions, angleNormals, point);

                    if (i < 4)
                        node.distancesA[i] = distance;
                    else
                        node.distancesB[i - 4] = distance;
                }

                // only subdivide those nodes intersecting the surface:
                if (depth < maxDepth &&
                    Mathf.Abs(BIH.DistanceToSurface(bih, tris, vertexPositions, angleNormals, node.center)) <
                    node.center[3] * sqrt3)
                {
                    // calculate mean squared error between measured distances and interpolated ones:
                    float mse = 0;
                    for (var i = 0; i < samples.Length; ++i)
                    {
                        var point = node.center + samples[i] * node.center[3];
                        var groundTruth = BIH.DistanceToSurface(bih, tris, vertexPositions, angleNormals, point);
                        var estimation = node.Sample(point);
                        var d = groundTruth - estimation;
                        mse += d * d;
                    }

                    mse /= samples.Length;

                    // if error > threshold, subdivide the node:
                    if (mse > maxError)
                    {
                        node.firstChild = nodes.Count;
                        for (var i = 0; i < 8; ++i)
                        {
                            queue.Enqueue(nodes.Count);
                            nodes.Add(new DFNode(node.center + corners[i] * node.center[3] * 0.5f));
                        }
                    }

                    // keep track of current depth:
                    if (--nodesToNextLevel == 0)
                    {
                        depth++;
                        nodesToNextLevel = queue.Count;
                    }
                }

                // feed the modified node back:
                nodes[index] = node;

                // if we've processed enough nodes, yield.
                if (nodes.Count - processedNodeCount >= yieldAfterNodeCount)
                {
                    processedNodeCount = nodes.Count;
                    yield return null;
                }
            }
        }

        public static float Sample(List<DFNode> nodes, Vector3 position)
        {
            if (nodes != null && nodes.Count > 0)
            {
                var queue = new Queue<int>();
                queue.Enqueue(0);

                while (queue.Count > 0)
                {
                    // get current node:
                    var node = nodes[queue.Dequeue()];

                    if (node.firstChild > -1)
                        queue.Enqueue(node.firstChild + node.GetOctant(position));
                    else
                        return node.Sample(position);
                }
            }

            return 0;
        }
    }
}