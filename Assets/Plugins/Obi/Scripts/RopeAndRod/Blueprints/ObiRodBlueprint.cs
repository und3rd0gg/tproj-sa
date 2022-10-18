using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    [CreateAssetMenu(fileName = "rod blueprint", menuName = "Obi/Rod Blueprint", order = 141)]
    public class ObiRodBlueprint : ObiRopeBlueprintBase
    {
        public const float DEFAULT_PARTICLE_MASS = 0.1f;
        public const float DEFAULT_PARTICLE_ROTATIONAL_MASS = 0.01f;

        public bool keepInitialShape = true;


        protected override IEnumerator Initialize()
        {
            if (path.ControlPointCount < 2)
            {
                ClearParticleGroups();
                path.InsertControlPoint(0, Vector3.left, Vector3.left * 0.25f, Vector3.right * 0.25f, Vector3.up,
                    DEFAULT_PARTICLE_MASS, DEFAULT_PARTICLE_ROTATIONAL_MASS, 1,
                    ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 1), Color.white, "control point");
                path.InsertControlPoint(1, Vector3.right, Vector3.left * 0.25f, Vector3.right * 0.25f, Vector3.up,
                    DEFAULT_PARTICLE_MASS, DEFAULT_PARTICLE_ROTATIONAL_MASS, 1,
                    ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 1), Color.white, "control point");
            }

            path.RecalculateLenght(Matrix4x4.identity, 0.00001f, 7);

            var particlePositions = new List<Vector3>();
            var particleNormals = new List<Vector3>();
            var particleThicknesses = new List<float>();
            var particleInvMasses = new List<float>();
            var particleInvRotationalMasses = new List<float>();
            var particleFilters = new List<int>();
            var particleColors = new List<Color>();

            // In case the path is open, add a first particle. In closed paths, the last particle is also the first one.
            if (!path.Closed)
            {
                particlePositions.Add(path.points.GetPositionAtMu(path.Closed, 0));
                particleNormals.Add(path.normals.GetAtMu(path.Closed, 0));
                particleThicknesses.Add(path.thicknesses.GetAtMu(path.Closed, 0));
                particleInvMasses.Add(ObiUtils.MassToInvMass(path.masses.GetAtMu(path.Closed, 0)));
                particleInvRotationalMasses.Add(ObiUtils.MassToInvMass(path.rotationalMasses.GetAtMu(path.Closed, 0)));
                particleFilters.Add(path.filters.GetAtMu(path.Closed, 0));
                particleColors.Add(path.colors.GetAtMu(path.Closed, 0));
            }

            // Create a particle group for the first control point:
            groups[0].particleIndices.Clear();
            groups[0].particleIndices.Add(0);

            var lengthTable = path.ArcLengthTable;
            var spans = path.GetSpanCount();

            for (var i = 0; i < spans; i++)
            {
                var firstArcLengthSample = i * (path.ArcLengthSamples + 1);
                var lastArcLengthSample = (i + 1) * (path.ArcLengthSamples + 1);

                var upToSpanLength = lengthTable[firstArcLengthSample];
                var spanLength = lengthTable[lastArcLengthSample] - upToSpanLength;

                var particlesInSpan = 1 + Mathf.FloorToInt(spanLength / thickness * resolution);
                var distance = spanLength / particlesInSpan;

                for (var j = 0; j < particlesInSpan; ++j)
                {
                    var mu = path.GetMuAtLenght(upToSpanLength + distance * (j + 1));
                    particlePositions.Add(path.points.GetPositionAtMu(path.Closed, mu));
                    particleNormals.Add(path.normals.GetAtMu(path.Closed, mu));
                    particleThicknesses.Add(path.thicknesses.GetAtMu(path.Closed, mu));
                    particleInvMasses.Add(ObiUtils.MassToInvMass(path.masses.GetAtMu(path.Closed, mu)));
                    particleInvRotationalMasses.Add(
                        ObiUtils.MassToInvMass(path.rotationalMasses.GetAtMu(path.Closed, mu)));
                    particleFilters.Add(path.filters.GetAtMu(path.Closed, mu));
                    particleColors.Add(path.colors.GetAtMu(path.Closed, mu));
                }

                // Create a particle group for each control point:
                if (!(path.Closed && i == spans - 1))
                {
                    groups[i + 1].particleIndices.Clear();
                    groups[i + 1].particleIndices.Add(particlePositions.Count - 1);
                }

                if (i % 100 == 0)
                    yield return new CoroutineJob.ProgressInfo("ObiRope: generating particles...", i / (float) spans);
            }

            m_ActiveParticleCount = particlePositions.Count;
            totalParticles = m_ActiveParticleCount;

            var numSegments = m_ActiveParticleCount - (path.Closed ? 0 : 1);
            if (numSegments > 0)
                m_InterParticleDistance = path.Length / numSegments;
            else
                m_InterParticleDistance = 0;

            positions = new Vector3[totalParticles];
            orientations = new Quaternion[totalParticles];
            velocities = new Vector3[totalParticles];
            angularVelocities = new Vector3[totalParticles];
            invMasses = new float[totalParticles];
            invRotationalMasses = new float[totalParticles];
            principalRadii = new Vector3[totalParticles];
            filters = new int[totalParticles];
            restPositions = new Vector4[totalParticles];
            restOrientations = new Quaternion[totalParticles];
            colors = new Color[totalParticles];
            restLengths = new float[totalParticles];

            for (var i = 0; i < m_ActiveParticleCount; i++)
            {
                invMasses[i] = particleInvMasses[i];
                invRotationalMasses[i] = particleInvRotationalMasses[i];
                positions[i] = particlePositions[i];
                restPositions[i] = positions[i];
                restPositions[i][3] = 1; // activate rest position.
                principalRadii[i] = Vector3.one * particleThicknesses[i] * thickness;
                filters[i] = particleFilters[i];
                colors[i] = particleColors[i];

                if (i % 100 == 0)
                    yield return new CoroutineJob.ProgressInfo("ObiRod: generating particles...",
                        i / (float) m_ActiveParticleCount);
            }

            // Create edge simplices:
            CreateSimplices(numSegments);

            // Create distance constraints for the total number of particles, but only activate for the used ones.
            var dc = CreateStretchShearConstraints(particleNormals);

            while (dc.MoveNext())
                yield return dc.Current;

            // Create bending constraints:
            var bc = CreateBendTwistConstraints();

            while (bc.MoveNext())
                yield return bc.Current;

            // Create chain constraints:
            var cc = CreateChainConstraints();

            while (cc.MoveNext())
                yield return cc.Current;
        }


        protected virtual IEnumerator CreateStretchShearConstraints(List<Vector3> particleNormals)
        {
            stretchShearConstraintsData = new ObiStretchShearConstraintsData();

            stretchShearConstraintsData.AddBatch(new ObiStretchShearConstraintsBatch());
            stretchShearConstraintsData.AddBatch(new ObiStretchShearConstraintsBatch());

            // rotation minimizing frame:
            var frame = new ObiPathFrame();
            frame.Reset();

            for (var i = 0; i < totalParticles - 1; i++)
            {
                var batch = stretchShearConstraintsData.batches[i % 2];

                var indices = new Vector2Int(i, i + 1);
                var d = positions[indices.y] - positions[indices.x];
                restLengths[i] = d.magnitude;

                frame.Transport(positions[indices.x], d.normalized, 0);

                orientations[i] = Quaternion.LookRotation(frame.tangent, particleNormals[indices.x]);
                restOrientations[i] = orientations[i];

                // Also set the orientation of the next particle. If it is not the last one, we will overwrite it.
                // This makes sure that open rods provide an orientation for their last particle (or rather, a phantom segment past the last particle).

                orientations[indices.y] = orientations[i];
                restOrientations[indices.y] = orientations[i];

                batch.AddConstraint(indices, indices.x, restLengths[i], Quaternion.identity);
                batch.activeConstraintCount++;

                if (i % 500 == 0)
                    yield return new CoroutineJob.ProgressInfo("ObiRod: generating structural constraints...",
                        i / (float) (totalParticles - 1));
            }

            // if the path is closed, add the last, loop closing constraint to a new batch to avoid sharing particles.
            if (path.Closed)
            {
                var loopClosingBatch = new ObiStretchShearConstraintsBatch();
                stretchShearConstraintsData.AddBatch(loopClosingBatch);

                var indices = new Vector2Int(m_ActiveParticleCount - 1, 0);
                var d = positions[indices.y] - positions[indices.x];
                restLengths[m_ActiveParticleCount - 2] = d.magnitude;

                frame.Transport(positions[indices.x], d.normalized, 0);

                orientations[m_ActiveParticleCount - 1] =
                    Quaternion.LookRotation(frame.tangent, particleNormals[indices.x]);
                restOrientations[m_ActiveParticleCount - 1] = orientations[m_ActiveParticleCount - 1];

                loopClosingBatch.AddConstraint(indices, indices.x, restLengths[m_ActiveParticleCount - 2],
                    Quaternion.identity);
                loopClosingBatch.activeConstraintCount++;
            }

            // Recalculate rest length:
            m_RestLength = 0;
            foreach (var length in restLengths)
                m_RestLength += length;
        }

        protected virtual IEnumerator CreateBendTwistConstraints()
        {
            bendTwistConstraintsData = new ObiBendTwistConstraintsData();

            // Add two batches:
            bendTwistConstraintsData.AddBatch(new ObiBendTwistConstraintsBatch());
            bendTwistConstraintsData.AddBatch(new ObiBendTwistConstraintsBatch());

            // the last bend constraint couples the last segment and a phantom segment past the last particle.
            for (var i = 0; i < totalParticles - 1; i++)
            {
                var batch = bendTwistConstraintsData.batches[i % 2];

                var indices = new Vector2Int(i, i + 1);

                var darboux = keepInitialShape
                    ? ObiUtils.RestDarboux(orientations[indices.x], orientations[indices.y])
                    : Quaternion.identity;
                batch.AddConstraint(indices, darboux);
                batch.activeConstraintCount++;

                if (i % 500 == 0)
                    yield return new CoroutineJob.ProgressInfo("ObiRod: generating structural constraints...",
                        i / (float) (totalParticles - 1));
            }

            // if the path is closed, add the last, loop closing constraints to a new batch to avoid sharing particles.
            if (path.Closed)
            {
                var loopClosingBatch = new ObiBendTwistConstraintsBatch();
                bendTwistConstraintsData.AddBatch(loopClosingBatch);

                var indices = new Vector2Int(m_ActiveParticleCount - 1, 0);
                var darboux = keepInitialShape
                    ? ObiUtils.RestDarboux(orientations[indices.x], orientations[indices.y])
                    : Quaternion.identity;
                loopClosingBatch.AddConstraint(indices, darboux);
                loopClosingBatch.activeConstraintCount++;
            }
        }

        protected virtual IEnumerator CreateChainConstraints()
        {
            chainConstraintsData = new ObiChainConstraintsData();

            // Add a single batch:
            var batch = new ObiChainConstraintsBatch();
            chainConstraintsData.AddBatch(batch);

            var indices = new int[m_ActiveParticleCount + (path.Closed ? 1 : 0)];

            for (var i = 0; i < m_ActiveParticleCount; ++i)
                indices[i] = i;

            // Add the first particle as the last index of the chain, if closed.
            if (path.Closed)
                indices[m_ActiveParticleCount] = 0;

            // TODO: variable distance between particles:
            batch.AddConstraint(indices, m_InterParticleDistance, 1, 1);
            batch.activeConstraintCount++;

            yield return 0;
        }
    }
}