using System;
using UnityEngine;

namespace Obi
{
    public static class GraphColoring
    {
        /**
         * General greedy graph coloring algorithm for constraints. Input:
         * - List of particle indices used by all constraints.
         * - List of per-constraint offsets of the first constrained particle in the previous array, with the total amount of particle indices in the last position.
         * 
         * The output is a color for each constraint. Constraints of the same color are guaranteed to not share any partices.
         * If particle order is important within each constraint, make sure to pass a copy for particleIndices, as the order is altered by this function.
         */
        public static int[] Colorize(int[] particleIndices, int[] constraintIndices)
        {
            var constraintCount = Mathf.Max(0, constraintIndices.Length - 1);
            if (constraintCount == 0)
                return new int[0];

            var colors = new int[constraintCount];
            var availability = new bool[constraintCount];

            for (var i = 0; i < constraintCount; ++i)
            {
                // Sort particle indices for all constraints. This allows for efficient neighbour checks.
                Array.Sort(particleIndices, constraintIndices[i], constraintIndices[i + 1] - constraintIndices[i]);
                colors[i] = -1;
                availability[i] = true;
            }

            // For each constraint:
            for (var i = 0; i < constraintCount; ++i)
            {
                // Iterate over all other constraints:
                for (var j = 0; j < constraintCount; ++j)
                {
                    if (i == j) continue;

                    // Check if the constraints share any particle:
                    var sizeI = constraintIndices[i + 1] - constraintIndices[i];
                    var sizeJ = constraintIndices[j + 1] - constraintIndices[j];
                    var counterI = 0;
                    var counterJ = 0;
                    while (counterI < sizeI && counterJ < sizeJ)
                    {
                        var p1 = particleIndices[constraintIndices[i] + counterI];
                        var p2 = particleIndices[constraintIndices[j] + counterJ];

                        if (p1 > p2)
                        {
                            counterJ++;
                        }
                        else if (p1 < p2)
                        {
                            counterI++;
                        }
                        else
                        {
                            // Mark the neighbour color as unavailable:
                            if (colors[j] >= 0)
                                availability[colors[j]] = false;
                            break;
                        }
                    }
                }

                // Assign the first available color:
                for (colors[i] = 0; colors[i] < constraintCount; ++colors[i])
                    if (availability[colors[i]])
                        break;

                // Reset availability flags:
                for (var j = 0; j < constraintCount; ++j)
                    availability[j] = true;
            }

            return colors;
        }
    }
}