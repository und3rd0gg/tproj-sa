using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    public interface IObiConstraints
    {
        Oni.ConstraintType? GetConstraintType();

        IObiConstraintsBatch GetBatch(int i);
        int GetBatchCount();
        void Clear();

        bool AddToSolver(ObiSolver solver);
        bool RemoveFromSolver();

        int GetConstraintCount();
        int GetActiveConstraintCount();
        void DeactivateAllConstraints();

        void Merge(ObiActor actor, IObiConstraints other);
    }

    [Serializable]
    public abstract class ObiConstraints<T> : IObiConstraints where T : class, IObiConstraintsBatch
    {
        [HideInInspector] public List<T> batches = new List<T>();
        [NonSerialized] protected ObiSolver m_Solver;

        // merges constraints from a given actor with this one.
        public void Merge(ObiActor actor, IObiConstraints other)
        {
            var others = other as ObiConstraints<T>;

            if (others == null || !other.GetConstraintType().HasValue)
                return;

            var constraintType = (int) other.GetConstraintType().Value;

            // clear batch offsets for this constraint type:
            actor.solverBatchOffsets[constraintType].Clear();

            // create new empty batches if needed:
            var newBatches = Mathf.Max(0, others.GetBatchCount() - GetBatchCount());
            for (var i = 0; i < newBatches; ++i)
                AddBatch(CreateBatch());

            for (var i = 0; i < other.GetBatchCount(); ++i)
            {
                // store this batch's offset:
                actor.solverBatchOffsets[constraintType].Add(batches[i].activeConstraintCount);

                // merge both batches:
                batches[i].Merge(actor, others.batches[i]);
            }
        }

        public IObiConstraintsBatch GetBatch(int i)
        {
            if (batches != null && i >= 0 && i < batches.Count)
                return batches[i];
            return null;
        }

        public int GetBatchCount()
        {
            return batches == null ? 0 : batches.Count;
        }

        public int GetConstraintCount()
        {
            var count = 0;
            if (batches == null) return count;

            foreach (var batch in batches)
                if (batch != null)
                    count += batch.constraintCount;

            return count;
        }

        public int GetActiveConstraintCount()
        {
            var count = 0;
            if (batches == null) return count;

            foreach (var batch in batches)
                if (batch != null)
                    count += batch.activeConstraintCount;

            return count;
        }

        public void DeactivateAllConstraints()
        {
            if (batches != null)
                foreach (var batch in batches)
                    if (batch != null)
                        batch.DeactivateAllConstraints();
        }

        public Oni.ConstraintType? GetConstraintType()
        {
            if (batches != null && batches.Count > 0)
                return batches[0].constraintType;
            return null;
        }

        public void Clear()
        {
            RemoveFromSolver();

            if (batches != null)
                batches.Clear();
        }

        public bool AddToSolver(ObiSolver solver)
        {
            if (m_Solver != null || batches == null)
                return false;

            m_Solver = solver;

            foreach (var batch in batches)
                batch.AddToSolver(m_Solver);

            return true;
        }

        public bool RemoveFromSolver()
        {
            if (m_Solver == null || batches == null)
                return false;

            foreach (var batch in batches)
                batch.RemoveFromSolver(m_Solver);

            m_Solver = null;

            return true;
        }

        public T GetFirstBatch()
        {
            return batches != null && batches.Count > 0 ? batches[0] : null;
        }

        public virtual T CreateBatch(T source = null)
        {
            return null;
        }

        public void AddBatch(T batch)
        {
            if (batch != null)
                batches.Add(batch);
        }

        public bool RemoveBatch(T batch)
        {
            if (batches == null || batch == null)
                return false;
            return batches.Remove(batch);
        }
    }
}