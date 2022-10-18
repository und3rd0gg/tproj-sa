#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniBendConstraintsImpl : OniConstraintsImpl
    {
        public OniBendConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.Bending)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniBendConstraintsBatchImpl(this);
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