using System;
using UnityEngine;

namespace Obi
{
    public class VoxelPathFinder
    {
        private readonly bool[,,] closed;
        private readonly PriorityQueue<TargetVoxel> open;
        private readonly MeshVoxelizer voxelizer;

        public VoxelPathFinder(MeshVoxelizer voxelizer)
        {
            this.voxelizer = voxelizer;
            closed = new bool[voxelizer.resolution.x, voxelizer.resolution.y, voxelizer.resolution.z];
            open = new PriorityQueue<TargetVoxel>();
        }

        private TargetVoxel AStar(in Vector3Int start, Func<TargetVoxel, bool> termination,
            Func<Vector3Int, float> heuristic)
        {
            Array.Clear(closed, 0, closed.Length);

            // A* algorithm:
            open.Clear();
            open.Enqueue(new TargetVoxel(start, 0, 0));

            while (open.Count() != 0)
            {
                var current = open.Dequeue();

                if (termination(current))
                    return current;

                closed[current.coordinates.x, current.coordinates.y, current.coordinates.z] = true;

                for (var i = 0; i < MeshVoxelizer.fullNeighborhood.Length; ++i)
                {
                    var successorCoords = current.coordinates + MeshVoxelizer.fullNeighborhood[i];

                    if (voxelizer.VoxelExists(successorCoords) &&
                        voxelizer[successorCoords.x, successorCoords.y, successorCoords.z] !=
                        MeshVoxelizer.Voxel.Outside &&
                        !closed[successorCoords.x, successorCoords.y, successorCoords.z])
                    {
                        var successor = new TargetVoxel(successorCoords,
                            current.distance + voxelizer.GetDistanceToNeighbor(i),
                            heuristic(successorCoords));
                        //successor.parent = current;

                        var index = -1;
                        for (var j = 0; j < open.Count(); ++j)
                            if (open.data[j].coordinates == successorCoords)
                            {
                                index = j;
                                break;
                            }

                        if (index < 0)
                            open.Enqueue(successor);
                        else if (successor.distance < open.data[index].distance)
                            open.data[index] = successor;
                    }
                }
            }

            return new TargetVoxel(Vector3Int.zero, -1, -1);
        }

        public TargetVoxel FindClosestNonEmptyVoxel(in Vector3Int start)
        {
            if (voxelizer == null) return new TargetVoxel(Vector3Int.zero, -1, -1);

            if (!voxelizer.VoxelExists(start))
                return new TargetVoxel(Vector3Int.zero, -1, -1);

            if (voxelizer[start.x, start.y, start.z] != MeshVoxelizer.Voxel.Outside)
                return new TargetVoxel(start, 0, 0);

            Array.Clear(closed, 0, closed.Length);

            return AStar(start,
                v =>
                {
                    return voxelizer[v.coordinates.x, v.coordinates.y, v.coordinates.z] != MeshVoxelizer.Voxel.Outside;
                },
                c => { return 0; });
        }

        public TargetVoxel FindPath(in Vector3Int start, Vector3Int end)
        {
            if (voxelizer == null) return new TargetVoxel(Vector3Int.zero, -1, -1);

            if (!voxelizer.VoxelExists(start) || !voxelizer.VoxelExists(end))
                return new TargetVoxel(Vector3Int.zero, -1, -1);

            if (voxelizer[start.x, start.y, start.z] == MeshVoxelizer.Voxel.Outside ||
                voxelizer[end.x, end.y, end.z] == MeshVoxelizer.Voxel.Outside)
                return new TargetVoxel(Vector3Int.zero, -1, -1);

            return AStar(start,
                v => { return v.coordinates == end; },
                c => { return Vector3.Distance(c, end) * voxelizer.voxelSize; });
        }

        public struct TargetVoxel : IEquatable<TargetVoxel>, IComparable<TargetVoxel>
        {
            public Vector3Int coordinates;
            public float distance;

            public float heuristic;
            //public TargetVoxel parent;

            public float cost => distance + heuristic;

            public TargetVoxel(Vector3Int coordinates, float distance, float heuristic)
            {
                this.coordinates = coordinates;
                this.distance = distance;
                this.heuristic = heuristic;
            }

            public bool Equals(TargetVoxel other)
            {
                return coordinates.Equals(other.coordinates);
            }

            public int CompareTo(TargetVoxel other)
            {
                return cost.CompareTo(other.cost);
            }
        }
    }
}