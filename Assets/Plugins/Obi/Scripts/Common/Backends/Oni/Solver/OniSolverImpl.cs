#if (OBI_ONI_SUPPORTED)
using System;
using UnityEngine;

namespace Obi
{
    public class OniSolverImpl : ISolverImpl
    {
        // Per-type constraints array:
        private readonly IOniConstraintsImpl[] constraints;

        // Pool job handles to avoid runtime alloc:
        private readonly JobHandlePool<OniJobHandle> jobHandlePool;

        public OniSolverImpl(IntPtr solver)
        {
            oniSolver = solver;

            jobHandlePool = new JobHandlePool<OniJobHandle>(4);

            constraints = new IOniConstraintsImpl[Oni.ConstraintTypeCount];
            constraints[(int) Oni.ConstraintType.Tether] = new OniTetherConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.Volume] = new OniVolumeConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.Chain] = new OniChainConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.Bending] = new OniBendConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.Distance] = new OniDistanceConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.ShapeMatching] = new OniShapeMatchingConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.BendTwist] = new OniBendTwistConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.StretchShear] = new OniStretchShearConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.Pin] = new OniPinConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.Skin] = new OniSkinConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.Aerodynamics] = new OniAerodynamicConstraintsImpl(this);
            constraints[(int) Oni.ConstraintType.Stitch] = new OniStitchConstraintsImpl(this);
        }

        public IntPtr oniSolver { get; private set; }

        public void Destroy()
        {
            Oni.DestroySolver(oniSolver);
            oniSolver = IntPtr.Zero;
        }

        public void InitializeFrame(Vector4 translation, Vector4 scale, Quaternion rotation)
        {
            Oni.InitializeFrame(oniSolver, ref translation, ref scale, ref rotation);
        }

        public void UpdateFrame(Vector4 translation, Vector4 scale, Quaternion rotation, float deltaTime)
        {
            Oni.UpdateFrame(oniSolver, ref translation, ref scale, ref rotation, deltaTime);
        }

        public void ApplyFrame(float worldLinearInertiaScale, float worldAngularInertiaScale, float deltaTime)
        {
            Oni.ApplyFrame(oniSolver, 0, 0, worldLinearInertiaScale, worldAngularInertiaScale, deltaTime);
        }

        public int GetDeformableTriangleCount()
        {
            return Oni.GetDeformableTriangleCount(oniSolver);
        }

        public void SetDeformableTriangles(int[] indices, int num, int destOffset)
        {
            Oni.SetDeformableTriangles(oniSolver, indices, num, destOffset);
        }

        public int RemoveDeformableTriangles(int num, int sourceOffset)
        {
            return Oni.RemoveDeformableTriangles(oniSolver, num, sourceOffset);
        }

        public void SetSimplices(int[] simplices, SimplexCounts counts)
        {
            Oni.SetSimplices(oniSolver, simplices, counts.pointCount, counts.edgeCount, counts.triangleCount);
        }

        public void ParticleCountChanged(ObiSolver solver)
        {
            Oni.SetParticlePositions(oniSolver, solver.positions.GetIntPtr());
            Oni.SetParticlePreviousPositions(oniSolver, solver.prevPositions.GetIntPtr());
            Oni.SetRestPositions(oniSolver, solver.restPositions.GetIntPtr());
            Oni.SetParticleOrientations(oniSolver, solver.orientations.GetIntPtr());
            Oni.SetParticlePreviousOrientations(oniSolver, solver.prevOrientations.GetIntPtr());
            Oni.SetRestOrientations(oniSolver, solver.restOrientations.GetIntPtr());
            Oni.SetParticleVelocities(oniSolver, solver.velocities.GetIntPtr());
            Oni.SetParticleAngularVelocities(oniSolver, solver.angularVelocities.GetIntPtr());
            Oni.SetParticleInverseMasses(oniSolver, solver.invMasses.GetIntPtr());
            Oni.SetParticleInverseRotationalMasses(oniSolver, solver.invRotationalMasses.GetIntPtr());
            Oni.SetParticlePrincipalRadii(oniSolver, solver.principalRadii.GetIntPtr());
            Oni.SetParticleCollisionMaterials(oniSolver, solver.collisionMaterials.GetIntPtr());
            Oni.SetParticlePhases(oniSolver, solver.phases.GetIntPtr());
            Oni.SetParticleFilters(oniSolver, solver.filters.GetIntPtr());
            Oni.SetRenderableParticlePositions(oniSolver, solver.renderablePositions.GetIntPtr());
            Oni.SetRenderableParticleOrientations(oniSolver, solver.renderableOrientations.GetIntPtr());
            Oni.SetParticleAnisotropies(oniSolver, solver.anisotropies.GetIntPtr());
            Oni.SetParticleSmoothingRadii(oniSolver, solver.smoothingRadii.GetIntPtr());
            Oni.SetParticleBuoyancy(oniSolver, solver.buoyancies.GetIntPtr());
            Oni.SetParticleRestDensities(oniSolver, solver.restDensities.GetIntPtr());
            Oni.SetParticleViscosities(oniSolver, solver.viscosities.GetIntPtr());
            Oni.SetParticleSurfaceTension(oniSolver, solver.surfaceTension.GetIntPtr());
            Oni.SetParticleVorticityConfinement(oniSolver, solver.vortConfinement.GetIntPtr());
            Oni.SetParticleAtmosphericDragPressure(oniSolver, solver.atmosphericDrag.GetIntPtr(),
                solver.atmosphericPressure.GetIntPtr());
            Oni.SetParticleDiffusion(oniSolver, solver.diffusion.GetIntPtr());
            Oni.SetParticleVorticities(oniSolver, solver.vorticities.GetIntPtr());
            Oni.SetParticleFluidData(oniSolver, solver.fluidData.GetIntPtr());
            Oni.SetParticleUserData(oniSolver, solver.userData.GetIntPtr());
            Oni.SetParticleExternalForces(oniSolver, solver.externalForces.GetIntPtr());
            Oni.SetParticleExternalTorques(oniSolver, solver.externalTorques.GetIntPtr());
            Oni.SetParticleWinds(oniSolver, solver.wind.GetIntPtr());
            Oni.SetParticlePositionDeltas(oniSolver, solver.positionDeltas.GetIntPtr());
            Oni.SetParticleOrientationDeltas(oniSolver, solver.orientationDeltas.GetIntPtr());
            Oni.SetParticlePositionConstraintCounts(oniSolver, solver.positionConstraintCounts.GetIntPtr());
            Oni.SetParticleOrientationConstraintCounts(oniSolver, solver.orientationConstraintCounts.GetIntPtr());
            Oni.SetParticleNormals(oniSolver, solver.normals.GetIntPtr());
            Oni.SetParticleInverseInertiaTensors(oniSolver, solver.invInertiaTensors.GetIntPtr());

            Oni.SetCapacity(oniSolver, solver.positions.capacity);
        }

        public void SetRigidbodyArrays(ObiSolver solver)
        {
            Oni.SetRigidbodyLinearDeltas(oniSolver, solver.rigidbodyLinearDeltas.GetIntPtr());
            Oni.SetRigidbodyAngularDeltas(oniSolver, solver.rigidbodyAngularDeltas.GetIntPtr());
        }

        public void SetActiveParticles(int[] indices, int num)
        {
            Oni.SetActiveParticles(oniSolver, indices, num);
        }

        public void ResetForces()
        {
            Oni.ResetForces(oniSolver);
        }

        public void GetBounds(ref Vector3 min, ref Vector3 max)
        {
            Oni.GetBounds(oniSolver, ref min, ref max);
        }

        public void SetParameters(Oni.SolverParameters parameters)
        {
            Oni.SetSolverParameters(oniSolver, ref parameters);
        }

        public int GetConstraintCount(Oni.ConstraintType type)
        {
            return Oni.GetConstraintCount(oniSolver, (int) type);
        }

        public void GetCollisionContacts(Oni.Contact[] contacts, int count)
        {
            Oni.GetCollisionContacts(oniSolver, contacts, count);
        }

        public void GetParticleCollisionContacts(Oni.Contact[] contacts, int count)
        {
            Oni.GetParticleCollisionContacts(oniSolver, contacts, count);
        }

        public void SetConstraintGroupParameters(Oni.ConstraintType type, ref Oni.ConstraintParameters parameters)
        {
            Oni.SetConstraintGroupParameters(oniSolver, (int) type, ref parameters);
        }

        public IConstraintsBatchImpl CreateConstraintsBatch(Oni.ConstraintType constraintType)
        {
            return constraints[(int) constraintType].CreateConstraintsBatch();
        }

        public void DestroyConstraintsBatch(IConstraintsBatchImpl batch)
        {
            if (batch != null)
                constraints[(int) batch.constraintType].RemoveBatch(batch);
        }

        public IObiJobHandle CollisionDetection(float stepTime)
        {
            Oni.RecalculateInertiaTensors(oniSolver);
            return jobHandlePool.Borrow().SetPointer(Oni.CollisionDetection(oniSolver, stepTime));
        }

        public IObiJobHandle Substep(float stepTime, float substepTime, int substeps)
        {
            return jobHandlePool.Borrow().SetPointer(Oni.Step(oniSolver, stepTime, substepTime, substeps));
        }

        public void ApplyInterpolation(ObiNativeVector4List startPositions, ObiNativeQuaternionList startOrientations,
            float stepTime, float unsimulatedTime)
        {
            Oni.ApplyPositionInterpolation(oniSolver, startPositions.GetIntPtr(), startOrientations.GetIntPtr(),
                stepTime, unsimulatedTime);
        }

        public void InterpolateDiffuseProperties(ObiNativeVector4List properties, ObiNativeVector4List diffusePositions,
            ObiNativeVector4List diffuseProperties, ObiNativeIntList neighbourCount, int diffuseCount)
        {
            Oni.InterpolateDiffuseParticles(oniSolver, properties.GetIntPtr(), diffusePositions.GetIntPtr(),
                diffuseProperties.GetIntPtr(), neighbourCount.GetIntPtr(), diffuseCount);
        }

        public int GetParticleGridSize()
        {
            return Oni.GetParticleGridSize(oniSolver);
        }

        public void GetParticleGrid(ObiNativeAabbList cells)
        {
            //Oni.GetParticleGrid(oniSolver, cells.GetIntPtr());
        }

        public void SpatialQuery(ObiNativeQueryShapeList shapes, ObiNativeAffineTransformList transforms,
            ObiNativeQueryResultList results)
        {
            var count = Oni.SpatialQuery(oniSolver, shapes.GetIntPtr(), transforms.GetIntPtr(), shapes.count);

            results.ResizeUninitialized(count);
            Oni.GetQueryResults(oniSolver, results.GetIntPtr(), count);
        }

        public void ReleaseJobHandles()
        {
            jobHandlePool.ReleaseAll();
        }
    }
}
#endif