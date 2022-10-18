#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public interface IOniConstraintsImpl : IConstraints
    {
        IConstraintsBatchImpl CreateConstraintsBatch();
        void RemoveBatch(IConstraintsBatchImpl batch);
    }

    public abstract class OniConstraintsImpl : IOniConstraintsImpl
    {
        protected Oni.ConstraintType m_ConstraintType;
        protected OniSolverImpl m_Solver;

        public OniConstraintsImpl(OniSolverImpl solver, Oni.ConstraintType constraintType)
        {
            m_ConstraintType = constraintType;
            m_Solver = solver;
        }

        public ISolverImpl solver => m_Solver;

        public Oni.ConstraintType constraintType => m_ConstraintType;

        public abstract IConstraintsBatchImpl CreateConstraintsBatch();

        public abstract void RemoveBatch(IConstraintsBatchImpl batch);

        public int GetConstraintCount()
        {
            return Oni.GetConstraintCount(m_Solver.oniSolver, (int) m_ConstraintType);
        }
    }
}
#endif