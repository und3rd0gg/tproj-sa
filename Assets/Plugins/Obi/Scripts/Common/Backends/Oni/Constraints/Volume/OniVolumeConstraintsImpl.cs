#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniVolumeConstraintsImpl : OniConstraintsImpl
    {
        public OniVolumeConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.Volume)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniVolumeConstraintsBatchImpl(this);
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