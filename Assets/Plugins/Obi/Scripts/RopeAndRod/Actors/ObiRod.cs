using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Rod", 881)]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class ObiRod : ObiRopeBase, IStretchShearConstraintsUser, IBendTwistConstraintsUser, IChainConstraintsUser
    {
        [SerializeField] protected ObiRodBlueprint m_RodBlueprint;

        // distance constraints:
        [SerializeField] protected bool _stretchShearConstraintsEnabled = true;
        [SerializeField] protected float _stretchCompliance;
        [SerializeField] protected float _shear1Compliance;
        [SerializeField] protected float _shear2Compliance;

        // bend constraints:
        [SerializeField] protected bool _bendTwistConstraintsEnabled = true;
        [SerializeField] protected float _torsionCompliance;
        [SerializeField] protected float _bend1Compliance;
        [SerializeField] protected float _bend2Compliance;
        [SerializeField] [Range(0, 0.1f)] protected float _plasticYield;
        [SerializeField] protected float _plasticCreep;

        // chain constraints:
        [SerializeField] protected bool _chainConstraintsEnabled = true;
        [SerializeField] [Range(0, 1)] protected float _tightness = 1;

        /// <summary>
        ///     Whether particles in this actor colide with particles using the same phase value.
        /// </summary>
        public bool selfCollisions
        {
            get => m_SelfCollisions;
            set
            {
                if (value != m_SelfCollisions)
                {
                    m_SelfCollisions = value;
                    SetSelfCollisions(m_SelfCollisions);
                }
            }
        }

        /// <summary>
        ///     Compliance of this actor's stretch/shear constraints, along their length.
        /// </summary>
        public float stretchCompliance
        {
            get => _stretchCompliance;
            set
            {
                _stretchCompliance = value;
                SetConstraintsDirty(Oni.ConstraintType.StretchShear);
            }
        }

        /// <summary>
        ///     Shearing compliance of this actor's stretch/shear constraints, along the first axis orthogonal to their length.
        /// </summary>
        public float shear1Compliance
        {
            get => _shear1Compliance;
            set
            {
                _shear1Compliance = value;
                SetConstraintsDirty(Oni.ConstraintType.StretchShear);
            }
        }

        /// <summary>
        ///     Shearing compliance of this actor's stretch/shear constraints, along the second axis orthogonal to their length.
        /// </summary>
        public float shear2Compliance
        {
            get => _shear2Compliance;
            set
            {
                _shear2Compliance = value;
                SetConstraintsDirty(Oni.ConstraintType.StretchShear);
            }
        }

        /// <summary>
        ///     Torsional compliance of this actor's bend/twist constraints along their length.
        /// </summary>
        public float torsionCompliance
        {
            get => _torsionCompliance;
            set
            {
                _torsionCompliance = value;
                SetConstraintsDirty(Oni.ConstraintType.BendTwist);
            }
        }

        /// <summary>
        ///     Bending compliance of this actor's bend/twist constraints along the first axis orthogonal to their length.
        /// </summary>
        public float bend1Compliance
        {
            get => _bend1Compliance;
            set
            {
                _bend1Compliance = value;
                SetConstraintsDirty(Oni.ConstraintType.BendTwist);
            }
        }

        /// <summary>
        ///     Bending compliance of this actor's bend/twist constraints along the second axis orthogonal to their length.
        /// </summary>
        public float bend2Compliance
        {
            get => _bend2Compliance;
            set
            {
                _bend2Compliance = value;
                SetConstraintsDirty(Oni.ConstraintType.BendTwist);
            }
        }

        /// <summary>
        ///     Threshold for plastic behavior.
        /// </summary>
        /// Once bending goes above this value, a percentage of the deformation (determined by
        /// <see cref="plasticCreep" />
        /// ) will be permanently absorbed into the rod's rest shape.
        public float plasticYield
        {
            get => _plasticYield;
            set
            {
                _plasticYield = value;
                SetConstraintsDirty(Oni.ConstraintType.BendTwist);
            }
        }

        /// <summary>
        ///     Percentage of deformation that gets absorbed into the rest shape per second, once deformation goes above the
        ///     <see cref="plasticYield" /> threshold.
        /// </summary>
        public float plasticCreep
        {
            get => _plasticCreep;
            set
            {
                _plasticCreep = value;
                SetConstraintsDirty(Oni.ConstraintType.BendTwist);
            }
        }

        /// <summary>
        ///     Average distance between consecutive particle centers in this rod.
        /// </summary>
        public float interParticleDistance => m_RodBlueprint.interParticleDistance;

        public override ObiActorBlueprint sourceBlueprint => m_RodBlueprint;

        public ObiRodBlueprint rodBlueprint
        {
            get => m_RodBlueprint;
            set
            {
                if (m_RodBlueprint != value)
                {
                    RemoveFromSolver();
                    ClearState();
                    m_RodBlueprint = value;
                    AddToSolver();
                }
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            SetConstraintsDirty(Oni.ConstraintType.BendTwist);
            SetupRuntimeConstraints();
        }

        /// <summary>
        ///     Whether this actor's bend/twist constraints are enabled.
        /// </summary>
        public bool bendTwistConstraintsEnabled
        {
            get => _bendTwistConstraintsEnabled;
            set
            {
                if (value != _bendTwistConstraintsEnabled)
                {
                    _bendTwistConstraintsEnabled = value;
                    SetConstraintsDirty(Oni.ConstraintType.BendTwist);
                }
            }
        }

        public Vector3 GetBendTwistCompliance(ObiBendTwistConstraintsBatch batch, int constraintIndex)
        {
            return new Vector3(bend1Compliance, bend2Compliance, torsionCompliance);
        }

        public Vector2 GetBendTwistPlasticity(ObiBendTwistConstraintsBatch batch, int constraintIndex)
        {
            return new Vector2(plasticYield, plasticCreep);
        }

        /// <summary>
        ///     Whether this actor's chain constraints are enabled.
        /// </summary>
        public bool chainConstraintsEnabled
        {
            get => _chainConstraintsEnabled;
            set
            {
                if (value != _chainConstraintsEnabled)
                {
                    _chainConstraintsEnabled = value;
                    SetConstraintsDirty(Oni.ConstraintType.BendTwist);
                }
            }
        }

        /// <summary>
        ///     Tightness of this actor's chain constraints.
        /// </summary>
        /// Controls how much chain constraints are allowed to compress.
        public float tightness
        {
            get => _tightness;
            set
            {
                _tightness = value;
                SetConstraintsDirty(Oni.ConstraintType.Chain);
            }
        }

        /// <summary>
        ///     Whether this actor's stretch/shear constraints are enabled.
        /// </summary>
        public bool stretchShearConstraintsEnabled
        {
            get => _stretchShearConstraintsEnabled;
            set
            {
                if (value != _stretchShearConstraintsEnabled)
                {
                    _stretchShearConstraintsEnabled = value;
                    SetConstraintsDirty(Oni.ConstraintType.StretchShear);
                }
            }
        }

        public Vector3 GetStretchShearCompliance(ObiStretchShearConstraintsBatch batch, int constraintIndex)
        {
            return new Vector3(shear1Compliance, shear2Compliance, stretchCompliance);
        }

        public override void LoadBlueprint(ObiSolver solver)
        {
            base.LoadBlueprint(solver);
            RebuildElementsFromConstraints();
            SetupRuntimeConstraints();
        }

        private void SetupRuntimeConstraints()
        {
            SetConstraintsDirty(Oni.ConstraintType.StretchShear);
            //SetConstraintsDirty(Oni.ConstraintType.BendTwist);
            SetConstraintsDirty(Oni.ConstraintType.Chain);
            SetSelfCollisions(selfCollisions);
            RecalculateRestLength();
            SetSimplicesDirty();
        }

        protected override void RebuildElementsFromConstraintsInternal()
        {
            var dc =
                GetConstraintsByType(Oni.ConstraintType.StretchShear) as
                    ObiConstraints<ObiStretchShearConstraintsBatch>;
            if (dc == null || dc.GetBatchCount() < 2)
                return;

            var constraintCount = dc.batches[0].activeConstraintCount + dc.batches[1].activeConstraintCount;

            elements = new List<ObiStructuralElement>(constraintCount);

            for (var i = 0; i < constraintCount; ++i)
            {
                var batch = dc.batches[i % 2];
                var constraintIndex = i / 2;

                var e = new ObiStructuralElement();
                e.particle1 = solverIndices[batch.particleIndices[constraintIndex * 2]];
                e.particle2 = solverIndices[batch.particleIndices[constraintIndex * 2 + 1]];
                e.restLength = batch.restLengths[constraintIndex];
                elements.Add(e);
            }

            if (dc.batches.Count > 2)
            {
                var batch = dc.batches[2];
                var e = new ObiStructuralElement();
                e.particle1 = solverIndices[batch.particleIndices[0]];
                e.particle2 = solverIndices[batch.particleIndices[1]];
                e.restLength = batch.restLengths[0];
                elements.Add(e);
            }
        }
    }
}