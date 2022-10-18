#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniSkinConstraintsImpl : OniConstraintsImpl
    {
        public OniSkinConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.Skin)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniSkinConstraintsBatchImpl(this);
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