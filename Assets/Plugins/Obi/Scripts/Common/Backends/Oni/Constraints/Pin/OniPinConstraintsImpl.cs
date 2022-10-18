#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniPinConstraintsImpl : OniConstraintsImpl
    {
        public OniPinConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.Pin)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniPinConstraintsBatchImpl(this);
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