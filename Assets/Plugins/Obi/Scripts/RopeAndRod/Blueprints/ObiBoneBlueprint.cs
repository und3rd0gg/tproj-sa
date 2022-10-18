using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    [CreateAssetMenu(fileName = "bone blueprint", menuName = "Obi/Bone Blueprint", order = 142)]
    public class ObiBoneBlueprint : ObiActorBlueprint
    {
        public const float DEFAULT_PARTICLE_MASS = 0.1f;
        public const float DEFAULT_PARTICLE_ROTATIONAL_MASS = 0.1f;
        public const float DEFAULT_PARTICLE_RADIUS = 0.05f;
        public Transform root;

        [HideInInspector] public List<Transform> transforms = new List<Transform>();
        [HideInInspector] public List<Quaternion> restTransformOrientations = new List<Quaternion>();
        [HideInInspector] public List<int> parentIndices = new List<int>();
        [HideInInspector] public List<float> normalizedLengths = new List<float>();

        public Quaternion root2WorldR;

        [HideInInspector] [NonSerialized] public List<ObiBone.IgnoredBone> ignored;
        [HideInInspector] [NonSerialized] public ObiBone.BonePropertyCurve mass;
        [HideInInspector] [NonSerialized] public ObiBone.BonePropertyCurve radius;
        [HideInInspector] [NonSerialized] public ObiBone.BonePropertyCurve rotationalMass;

        private ObiBone.IgnoredBone GetIgnoredBone(Transform bone)
        {
            for (var i = 0; i < ignored.Count; ++i)
                if (ignored[i].bone == bone)
                    return ignored[i];

            return null;
        }

        protected override IEnumerator Initialize()
        {
            ClearParticleGroups();

            transforms.Clear();
            restTransformOrientations.Clear();
            parentIndices.Clear();
            normalizedLengths.Clear();

            var particlePositions = new List<Vector3>();
            var particleOrientations = new List<Quaternion>();

            var world2Root = root.transform.worldToLocalMatrix;
            var world2RootR = world2Root.rotation;
            root2WorldR = Quaternion.Inverse(world2RootR);

            // create a queue to traverse the hierarchy in a width-first fashion.
            var bones = new Queue<Transform>();

            // insert the root bone:
            bones.Enqueue(root);
            parentIndices.Add(-1);
            normalizedLengths.Add(0);

            // initialize hierarchy length:
            float maxLength = 0;

            while (bones.Count > 0)
            {
                var bone = bones.Dequeue();

                if (bone != null)
                {
                    var ig = GetIgnoredBone(bone);

                    if (ig == null)
                    {
                        transforms.Add(bone);
                        restTransformOrientations.Add(bone.localRotation);
                        particlePositions.Add(world2Root.MultiplyPoint3x4(bone.position));
                        particleOrientations.Add(world2RootR * bone.rotation);
                    }

                    if (ig == null || !ig.ignoreChildren)
                        foreach (Transform child in bone)
                        {
                            ig = GetIgnoredBone(child);

                            if (ig == null)
                            {
                                var parentIndex = transforms.Count - 1;
                                parentIndices.Add(parentIndex);

                                var distanceToParent = Vector3.Distance(child.position, bone.position);
                                var distanceToRoot = normalizedLengths[parentIndex] + distanceToParent;
                                maxLength = Mathf.Max(maxLength, distanceToRoot);
                                normalizedLengths.Add(distanceToRoot);
                            }

                            bones.Enqueue(child);
                        }
                }
            }

            // normalize lengths:
            if (maxLength > 0)
                for (var i = 0; i < normalizedLengths.Count; ++i)
                    normalizedLengths[i] /= maxLength;


            // calculate orientations that minimize the Darboux vector:
            var avgChildrenDirection = new Vector3[parentIndices.Count];
            var childCount = new int[parentIndices.Count];

            for (var i = 0; i < parentIndices.Count; ++i)
            {
                var parent = parentIndices[i];
                if (parent >= 0)
                {
                    var vector = particlePositions[i] - particlePositions[parent];
                    avgChildrenDirection[parent] += vector;
                    childCount[parent]++;
                }
            }

            for (var i = 0; i < parentIndices.Count; ++i)
                if (childCount[i] > 0)
                    particleOrientations[i] = Quaternion.LookRotation(avgChildrenDirection[i] / childCount[i]);
                else if (parentIndices[i] >= 0)
                    particleOrientations[i] = particleOrientations[parentIndices[i]];


            m_ActiveParticleCount = particlePositions.Count;

            positions = new Vector3[m_ActiveParticleCount];
            orientations = new Quaternion[m_ActiveParticleCount];
            velocities = new Vector3[m_ActiveParticleCount];
            angularVelocities = new Vector3[m_ActiveParticleCount];
            invMasses = new float[m_ActiveParticleCount];
            invRotationalMasses = new float[m_ActiveParticleCount];
            principalRadii = new Vector3[m_ActiveParticleCount];
            filters = new int[m_ActiveParticleCount];
            restPositions = new Vector4[m_ActiveParticleCount];
            restOrientations = new Quaternion[m_ActiveParticleCount];
            colors = new Color[m_ActiveParticleCount];

            for (var i = 0; i < m_ActiveParticleCount; i++)
            {
                invMasses[i] =
                    ObiUtils.MassToInvMass(mass != null ? mass.Evaluate(normalizedLengths[i]) : DEFAULT_PARTICLE_MASS);
                invRotationalMasses[i] = ObiUtils.MassToInvMass(rotationalMass != null
                    ? rotationalMass.Evaluate(normalizedLengths[i])
                    : DEFAULT_PARTICLE_ROTATIONAL_MASS);
                positions[i] = particlePositions[i];
                restPositions[i] = positions[i];
                restPositions[i][3] = 1; // activate rest position.
                orientations[i] = particleOrientations[i];
                restOrientations[i] = /*world2RootR */ transforms[i].rotation;
                principalRadii[i] = Vector3.one *
                                    (radius != null ? radius.Evaluate(normalizedLengths[i]) : DEFAULT_PARTICLE_RADIUS);
                filters[i] = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
                colors[i] = Color.white;

                if (i % 100 == 0)
                    yield return new CoroutineJob.ProgressInfo("ObiRod: generating particles...",
                        i / (float) m_ActiveParticleCount);
            }

            // Create edge simplices:
            CreateSimplices();

            // Create stretch constraints:
            var dc = CreateStretchShearConstraints(particlePositions);
            while (dc.MoveNext()) yield return dc.Current;

            // Create bending constraints:
            var bc = CreateBendTwistConstraints(particlePositions);
            while (bc.MoveNext()) yield return bc.Current;

            // Create skin constraints:
            var sc = CreateSkinConstraints(particlePositions);
            while (sc.MoveNext()) yield return sc.Current;

            yield return new CoroutineJob.ProgressInfo("ObiBone: complete", 1);
        }

        protected void CreateSimplices()
        {
            edges = new int[(parentIndices.Count - 1) * 2];
            for (var i = 0; i < parentIndices.Count - 1; ++i)
            {
                edges[i * 2] = i + 1;
                edges[i * 2 + 1] = parentIndices[i + 1];
            }
        }

        protected virtual IEnumerator CreateStretchShearConstraints(List<Vector3> particlePositions)
        {
            var particleIndices = new List<int>();
            var constraintIndices = new List<int>();

            for (var i = 1; i < particlePositions.Count; ++i)
            {
                var parent = parentIndices[i];
                if (parent >= 0)
                {
                    particleIndices.Add(parent);
                    particleIndices.Add(i);
                    constraintIndices.Add(constraintIndices.Count * 2);
                }
            }

            constraintIndices.Add(constraintIndices.Count * 2);

            stretchShearConstraintsData = new ObiStretchShearConstraintsData();

            var constraintColors = GraphColoring.Colorize(particleIndices.ToArray(), constraintIndices.ToArray());

            for (var i = 0; i < constraintColors.Length; ++i)
            {
                var color = constraintColors[i];
                var cIndex = constraintIndices[i];

                // Add a new batch if needed:
                if (color >= stretchShearConstraintsData.GetBatchCount())
                    stretchShearConstraintsData.AddBatch(new ObiStretchShearConstraintsBatch());

                var index1 = particleIndices[cIndex];
                var index2 = particleIndices[cIndex + 1];

                var vector = particlePositions[index2] - particlePositions[index1];
                var rest = Quaternion.LookRotation(Quaternion.Inverse(orientations[index1]) * vector);

                stretchShearConstraintsData.batches[color]
                    .AddConstraint(new Vector2Int(index1, index2), index1, vector.magnitude, rest);
                stretchShearConstraintsData.batches[color].activeConstraintCount++;

                if (i % 500 == 0)
                    yield return new CoroutineJob.ProgressInfo("ObiBone: generating stretch constraints...",
                        i / constraintColors.Length);
            }
        }

        protected virtual IEnumerator CreateBendTwistConstraints(List<Vector3> particlePositions)
        {
            var particleIndices = new List<int>();
            var constraintIndices = new List<int>();

            for (var i = 1; i < particlePositions.Count; ++i)
            {
                var parent = parentIndices[i];
                if (parent >= 0)
                {
                    particleIndices.Add(parent);
                    particleIndices.Add(i);
                    constraintIndices.Add(constraintIndices.Count * 2);
                }
            }

            constraintIndices.Add(constraintIndices.Count * 2);

            bendTwistConstraintsData = new ObiBendTwistConstraintsData();

            var constraintColors = GraphColoring.Colorize(particleIndices.ToArray(), constraintIndices.ToArray());

            for (var i = 0; i < constraintColors.Length; ++i)
            {
                var color = constraintColors[i];
                var cIndex = constraintIndices[i];

                // Add a new batch if needed:
                if (color >= bendTwistConstraintsData.GetBatchCount())
                    bendTwistConstraintsData.AddBatch(new ObiBendTwistConstraintsBatch());

                var index1 = particleIndices[cIndex];
                var index2 = particleIndices[cIndex + 1];

                var darboux = ObiUtils.RestDarboux(orientations[index1], orientations[index2]);
                bendTwistConstraintsData.batches[color].AddConstraint(new Vector2Int(index1, index2), darboux);
                bendTwistConstraintsData.batches[color].activeConstraintCount++;

                if (i % 500 == 0)
                    yield return new CoroutineJob.ProgressInfo("ObiBone: generating bend constraints...",
                        i / constraintColors.Length);
            }
        }

        protected virtual IEnumerator CreateSkinConstraints(List<Vector3> particlePositions)
        {
            skinConstraintsData = new ObiSkinConstraintsData();
            var skinBatch = new ObiSkinConstraintsBatch();
            skinConstraintsData.AddBatch(skinBatch);

            for (var i = 0; i < particlePositions.Count; ++i)
            {
                skinBatch.AddConstraint(i, particlePositions[i], Vector3.up, 0, 0, 0, 0);
                skinBatch.activeConstraintCount++;

                if (i % 500 == 0)
                    yield return new CoroutineJob.ProgressInfo("ObiCloth: generating skin constraints...",
                        i / (float) particlePositions.Count);
            }
        }
    }
}