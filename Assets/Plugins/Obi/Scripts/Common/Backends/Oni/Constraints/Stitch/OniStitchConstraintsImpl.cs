#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniStitchConstraintsImpl : OniConstraintsImpl
    {
        public OniStitchConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.Stitch)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniStitchConstraintsBatchImpl(this);
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