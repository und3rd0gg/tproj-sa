#if (OBI_ONI_SUPPORTED)
namespace Obi
{
    public class OniAerodynamicConstraintsBatchImpl : OniConstraintsBatchImpl, IAerodynamicConstraintsBatchImpl
    {
        public OniAerodynamicConstraintsBatchImpl(OniAerodynamicConstraintsImpl constraints) : base(constraints,
            Oni.ConstraintType.Aerodynamics)
        {
        }

        public void SetAerodynamicConstraints(ObiNativeIntList particleIndices, ObiNativeFloatList aerodynamicCoeffs,
            int count)
        {
            Oni.SetAerodynamicConstraints(oniBatch, particleIndices.GetIntPtr(), aerodynamicCoeffs.GetIntPtr(), count);
        }
    }
}
#endif