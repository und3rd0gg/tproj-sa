using UnityEngine;

namespace Obi
{
    [RequireComponent(typeof(ObiSolver))]
    public class ObiParticleGridDebugger : MonoBehaviour
    {
        private ObiNativeAabbList cells;

        private ObiSolver solver;

        private void LateUpdate()
        {
            cells.count = solver.implementation.GetParticleGridSize();
            solver.implementation.GetParticleGrid(cells);
        }

        private void OnEnable()
        {
            solver = GetComponent<ObiSolver>();
            cells = new ObiNativeAabbList();
        }

        private void OnDisable()
        {
            cells.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (cells != null)
            {
                Gizmos.color = Color.yellow;
                for (var i = 0; i < cells.count; ++i)
                    Gizmos.DrawWireCube(cells[i].center, cells[i].size);
            }
        }
    }
}