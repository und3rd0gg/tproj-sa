#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniStretchShearConstraintsImpl : OniConstraintsImpl
    {
        public OniStretchShearConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.StretchShear)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniStretchShearConstraintsBatchImpl(this);
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