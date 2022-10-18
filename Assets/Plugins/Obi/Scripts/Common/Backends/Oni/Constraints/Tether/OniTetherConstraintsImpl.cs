#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniTetherConstraintsImpl : OniConstraintsImpl
    {
        public OniTetherConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.Tether)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniTetherConstraintsBatchImpl(this);
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