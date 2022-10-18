#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniShapeMatchingConstraintsImpl : OniConstraintsImpl
    {
        public OniShapeMatchingConstraintsImpl(OniSolverImpl solver) : base(solver, Oni.ConstraintType.ShapeMatching)
        {
        }

        public override IConstraintsBatchImpl CreateConstraintsBatch()
        {
            var batch = new OniShapeMatchingConstraintsBatchImpl(this);
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