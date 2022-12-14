#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniDistanceConstraintsImpl : OniConstraintsImpl
    {
        public OniDistanceConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.Distance)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniDistanceConstraintsBatchImpl(this);
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