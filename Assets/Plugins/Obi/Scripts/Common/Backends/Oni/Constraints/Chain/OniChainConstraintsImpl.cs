#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniChainConstraintsImpl : OniConstraintsImpl
    {
        public OniChainConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.Chain)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniChainConstraintsBatchImpl(this);
            Oni.AddBatch(((OniSolverImpl) solver).oniSolver, batch.oniBatch);
            return batch;
        }

        public override void RemoveBatch(IConstraintsBatchImpl batch)
        {
            Oni.RemoveBatch(((OniSolverImpl) solver).oniSolver, ((OniConstraintsBatchImpl) batch).oniBatch);
        }
    }
}
#endif