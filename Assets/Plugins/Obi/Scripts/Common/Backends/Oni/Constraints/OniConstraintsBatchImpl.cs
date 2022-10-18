#if (OBI_ONI_SUPPORTED)
using System;

namespace Obi
{
    public class OniConstraintsBatchImpl : IConstraintsBatchImpl
    {
        protected IConstraints m_Constraints;
        protected Oni.ConstraintType m_ConstraintType;
        protected bool m_Enabled;
        protected IntPtr m_OniBatch;

        public OniConstraintsBatchImpl(IConstraints constraints, Oni.ConstraintType type)
        {
            m_Constraints = constraints;
            m_ConstraintType = type;

            m_OniBatch = Oni.CreateBatch((int) type);
        }

        public IntPtr oniBatch => m_OniBatch;

        public Oni.ConstraintType constraintType => m_ConstraintType;

        public IConstraints constraints => m_Constraints;

        public bool enabled
        {
            set
            {
                if (m_Enabled != value)
                {
                    m_Enabled = value;
                    Oni.EnableBatch(m_OniBatch, m_Enabled);
                }
            }
            get => m_Enabled;
        }

        public void Destroy()
        {
            //Oni.DestroyBatch(m_OniBatch);

            // remove the constraint batch from the solver 
            // (no need to destroy it as its destruction is managed by the solver)
            // just reset the reference.
            m_OniBatch = IntPtr.Zero;
        }

        public void SetConstraintCount(int constraintCount)
        {
            Oni.SetBatchConstraintCount(m_OniBatch, constraintCount);
        }

        public int GetConstraintCount()
        {
            return Oni.GetBatchConstraintCount(m_OniBatch);
        }
    }
}
#endif