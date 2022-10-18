#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniBendTwistConstraintsImpl : OniConstraintsImpl
    {
        public OniBendTwistConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.BendTwist)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniBendTwistConstraintsBatchImpl(this);
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