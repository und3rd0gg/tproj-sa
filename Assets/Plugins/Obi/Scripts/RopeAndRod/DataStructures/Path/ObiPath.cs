using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Obi
{
    [Serializable]
    public class PathControlPointEvent : UnityEvent<int>
    {
    }

    [Serializable]
    public class ObiPath
    {
        protected const int arcLenghtSamples = 20;
        [HideInInspector] [SerializeField] private List<string> m_Names = new List<string>();
        [HideInInspector] [SerializeField] public ObiPointsDataChannel m_Points = new ObiPointsDataChannel();
        [HideInInspector] [SerializeField] private ObiNormalDataChannel m_Normals = new ObiNormalDataChannel();
        [HideInInspector] [SerializeField] private ObiColorDataChannel m_Colors = new ObiColorDataChannel();
        [HideInInspector] [SerializeField] private ObiThicknessDataChannel m_Thickness = new ObiThicknessDataChannel();
        [HideInInspector] [SerializeField] private ObiMassDataChannel m_Masses = new ObiMassDataChannel();

        [HideInInspector] [SerializeField]
        private ObiRotationalMassDataChannel m_RotationalMasses = new ObiRotationalMassDataChannel();

        [FormerlySerializedAs("m_Phases")] [HideInInspector] [SerializeField]
        private ObiPhaseDataChannel m_Filters = new ObiPhaseDataChannel();

        [HideInInspector] [SerializeField] private bool m_Closed;
        [HideInInspector] [SerializeField] protected List<float> m_ArcLengthTable = new List<float>();
        [HideInInspector] [SerializeField] protected float m_TotalSplineLenght;

        public UnityEvent OnPathChanged = new UnityEvent();
        public PathControlPointEvent OnControlPointAdded = new PathControlPointEvent();
        public PathControlPointEvent OnControlPointRemoved = new PathControlPointEvent();
        public PathControlPointEvent OnControlPointRenamed = new PathControlPointEvent();

        protected bool dirty;

        public ObiPointsDataChannel points => m_Points;
        public ObiNormalDataChannel normals => m_Normals;
        public ObiColorDataChannel colors => m_Colors;
        public ObiThicknessDataChannel thicknesses => m_Thickness;
        public ObiMassDataChannel masses => m_Masses;
        public ObiRotationalMassDataChannel rotationalMasses => m_RotationalMasses;
        public ObiPhaseDataChannel filters => m_Filters;

        public ReadOnlyCollection<float> ArcLengthTable => m_ArcLengthTable.AsReadOnly();

        public float Length => m_TotalSplineLenght;

        public int ArcLengthSamples => arcLenghtSamples;

        public int ControlPointCount => m_Points.Count;

        public bool Closed
        {
            get => m_Closed;
            set
            {
                if (value != m_Closed)
                {
                    m_Closed = value;
                    dirty = true;
                }
            }
        }

        private IEnumerable<IObiPathDataChannel> GetDataChannels()
        {
            yield return m_Points;
            yield return m_Normals;
            yield return m_Colors;
            yield return m_Thickness;
            yield return m_Masses;
            yield return m_RotationalMasses;
            yield return m_Filters;
        }

        public int GetSpanCount()
        {
            return m_Points.GetSpanCount(m_Closed);
        }

        public int GetSpanControlPointForMu(float mu, out float spanMu)
        {
            return m_Points.GetSpanControlPointAtMu(m_Closed, mu, out spanMu);
        }

        public int GetClosestControlPointIndex(float mu)
        {
            float spanMu;
            var cp = GetSpanControlPointForMu(mu, out spanMu);

            if (spanMu > 0.5f)
                return (cp + 1) % ControlPointCount;
            return cp % ControlPointCount;
        }

        /**
         * Returns the curve parameter (mu) at a certain length of the curve, using linear interpolation
         * of the values cached in arcLengthTable.
         */
        public float GetMuAtLenght(float length)
        {
            if (length <= 0) return 0;
            if (length >= m_TotalSplineLenght) return 1;

            int i;
            for (i = 1; i < m_ArcLengthTable.Count; ++i)
                if (length < m_ArcLengthTable[i])
                    break;

            var prevMu = (i - 1) / (float) (m_ArcLengthTable.Count - 1);
            var nextMu = i / (float) (m_ArcLengthTable.Count - 1);

            var s = (length - m_ArcLengthTable[i - 1]) / (m_ArcLengthTable[i] - m_ArcLengthTable[i - 1]);

            return prevMu + (nextMu - prevMu) * s;
        }

        /**
         * Recalculates spline arc lenght in world space using Gauss-Lobatto adaptive integration. 
         * @param acc minimum accuray desired (eg 0.00001f)
         * @param maxevals maximum number of spline evaluations we want to allow per segment.
         */
        public float RecalculateLenght(Matrix4x4 referenceFrame, float acc, int maxevals)
        {
            if (referenceFrame == null)
            {
                m_TotalSplineLenght = 0;
                return 0;
            }

            m_TotalSplineLenght = 0.0f;
            m_ArcLengthTable.Clear();
            m_ArcLengthTable.Add(0);

            var step = 1 / (float) (arcLenghtSamples + 1);
            var controlPoints = ControlPointCount;

            if (controlPoints >= 2)
            {
                var spans = GetSpanCount();

                for (var cp = 0; cp < spans; ++cp)
                {
                    var nextCP = (cp + 1) % controlPoints;
                    var wp1 = m_Points[cp];
                    var wp2 = m_Points[nextCP];

                    var _p = referenceFrame.MultiplyPoint3x4(wp1.position);
                    var p = referenceFrame.MultiplyPoint3x4(wp1.outTangentEndpoint);
                    var p_ = referenceFrame.MultiplyPoint3x4(wp2.inTangentEndpoint);
                    var p__ = referenceFrame.MultiplyPoint3x4(wp2.position);

                    for (var i = 0; i <= Mathf.Max(1, arcLenghtSamples); ++i)
                    {
                        var a = i * step;
                        var b = (i + 1) * step;

                        var segmentLength = GaussLobattoIntegrationStep(_p, p, p_, p__, a, b,
                            m_Points.EvaluateFirstDerivative(_p, p, p_, p__, a).magnitude,
                            m_Points.EvaluateFirstDerivative(_p, p, p_, p__, b).magnitude, 0, maxevals, acc);

                        m_TotalSplineLenght += segmentLength;

                        m_ArcLengthTable.Add(m_TotalSplineLenght);
                    }
                }
            }
            else
            {
                Debug.LogWarning("A path needs at least 2 control points to be defined.");
            }

            return m_TotalSplineLenght;
        }

        /**
         * One step of the adaptive integration method using Gauss-Lobatto quadrature.
         * Takes advantage of the fact that the arc lenght of a vector function is equal to the
         * integral of the magnitude of first derivative.
         */
        private float GaussLobattoIntegrationStep(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
            float a, float b,
            float fa, float fb, int nevals, int maxevals, float acc)
        {
            if (nevals >= maxevals) return 0;

            // Constants used in the algorithm
            var alpha = Mathf.Sqrt(2.0f / 3.0f);
            var beta = 1.0f / Mathf.Sqrt(5.0f);

            // Here the abcissa points and function values for both the 4-point
            // and the 7-point rule are calculated (the points at the end of
            // interval come from the function call, i.e., fa and fb. Also note
            // the 7-point rule re-uses all the points of the 4-point rule.)
            var h = (b - a) / 2;
            var m = (a + b) / 2;

            var mll = m - alpha * h;
            var ml = m - beta * h;
            var mr = m + beta * h;
            var mrr = m + alpha * h;
            nevals += 5;

            var fmll = m_Points.EvaluateFirstDerivative(p1, p2, p3, p4, mll).magnitude;
            var fml = m_Points.EvaluateFirstDerivative(p1, p2, p3, p4, ml).magnitude;
            var fm = m_Points.EvaluateFirstDerivative(p1, p2, p3, p4, m).magnitude;
            var fmr = m_Points.EvaluateFirstDerivative(p1, p2, p3, p4, mr).magnitude;
            var fmrr = m_Points.EvaluateFirstDerivative(p1, p2, p3, p4, mrr).magnitude;

            // Both the 4-point and 7-point rule integrals are evaluted
            var integral4 = h / 6 * (fa + fb + 5 * (fml + fmr));
            var integral7 = h / 1470 * (77 * (fa + fb) + 432 * (fmll + fmrr) + 625 * (fml + fmr) + 672 * fm);

            // The difference betwen the 4-point and 7-point integrals is the
            // estimate of the accuracy

            if (integral4 - integral7 < acc || mll <= a || b <= mrr)
            {
                if (!(m > a && b > m))
                    Debug.LogError("Spline integration reached an interval with no more machine numbers");
                return integral7;
            }

            return GaussLobattoIntegrationStep(p1, p2, p3, p4, a, mll, fa, fmll, nevals, maxevals, acc)
                   + GaussLobattoIntegrationStep(p1, p2, p3, p4, mll, ml, fmll, fml, nevals, maxevals, acc)
                   + GaussLobattoIntegrationStep(p1, p2, p3, p4, ml, m, fml, fm, nevals, maxevals, acc)
                   + GaussLobattoIntegrationStep(p1, p2, p3, p4, m, mr, fm, fmr, nevals, maxevals, acc)
                   + GaussLobattoIntegrationStep(p1, p2, p3, p4, mr, mrr, fmr, fmrr, nevals, maxevals, acc)
                   + GaussLobattoIntegrationStep(p1, p2, p3, p4, mrr, b, fmrr, fb, nevals, maxevals, acc);
        }

        public void SetName(int index, string name)
        {
            m_Names[index] = name;
            if (OnControlPointRenamed != null)
                OnControlPointRenamed.Invoke(index);
            dirty = true;
        }

        public string GetName(int index)
        {
            return m_Names[index];
        }

        public void AddControlPoint(Vector3 position, Vector3 inTangentVector, Vector3 outTangentVector, Vector3 normal,
            float mass, float rotationalMass, float thickness, int filter, Color color, string name)
        {
            InsertControlPoint(ControlPointCount, position, inTangentVector, outTangentVector, normal, mass,
                rotationalMass, thickness, filter, color, name);
        }

        public void InsertControlPoint(int index, Vector3 position, Vector3 inTangentVector, Vector3 outTangentVector,
            Vector3 normal, float mass, float rotationalMass, float thickness, int filter, Color color, string name)
        {
            m_Points.data.Insert(index, new ObiWingedPoint(inTangentVector, position, outTangentVector));
            m_Colors.data.Insert(index, color);
            m_Normals.data.Insert(index, normal);
            m_Thickness.data.Insert(index, thickness);
            m_Masses.data.Insert(index, mass);
            m_RotationalMasses.data.Insert(index, rotationalMass);
            m_Filters.data.Insert(index, filter);
            m_Names.Insert(index, name);

            if (OnControlPointAdded != null)
                OnControlPointAdded.Invoke(index);

            dirty = true;
        }

        public int InsertControlPoint(float mu)
        {
            var controlPoints = ControlPointCount;
            if (controlPoints >= 2)
                if (!float.IsNaN(mu))
                {
                    float p;
                    var i = GetSpanControlPointForMu(mu, out p);

                    var next = (i + 1) % controlPoints;

                    var wp1 = m_Points[i];
                    var wp2 = m_Points[next];

                    var P0_1 = (1 - p) * wp1.position + p * wp1.outTangentEndpoint;
                    var P1_2 = (1 - p) * wp1.outTangentEndpoint + p * wp2.inTangentEndpoint;
                    var P2_3 = (1 - p) * wp2.inTangentEndpoint + p * wp2.position;

                    var P01_12 = (1 - p) * P0_1 + p * P1_2;
                    var P12_23 = (1 - p) * P1_2 + p * P2_3;

                    var P0112_1223 = (1 - p) * P01_12 + p * P12_23;

                    wp1.SetOutTangentEndpoint(P0_1);
                    wp2.SetInTangentEndpoint(P2_3);

                    m_Points[i] = wp1;
                    m_Points[next] = wp2;

                    var color = m_Colors.Evaluate(m_Colors[i],
                        m_Colors[i],
                        m_Colors[next],
                        m_Colors[next], p);

                    var normal = m_Normals.Evaluate(m_Normals[i],
                        m_Normals[i],
                        m_Normals[next],
                        m_Normals[next], p);

                    var thickness = m_Thickness.Evaluate(m_Thickness[i],
                        m_Thickness[i],
                        m_Thickness[next],
                        m_Thickness[next], p);

                    var mass = m_Masses.Evaluate(m_Masses[i],
                        m_Masses[i],
                        m_Masses[next],
                        m_Masses[next], p);

                    var rotationalMass = m_RotationalMasses.Evaluate(m_RotationalMasses[i],
                        m_RotationalMasses[i],
                        m_RotationalMasses[next],
                        m_RotationalMasses[next], p);

                    var filter = m_Filters.Evaluate(m_Filters[i],
                        m_Filters[i],
                        m_Filters[next],
                        m_Filters[next], p);

                    InsertControlPoint(i + 1, P0112_1223, P01_12 - P0112_1223, P12_23 - P0112_1223, normal, mass,
                        rotationalMass, thickness, filter, color, GetName(i));

                    return i + 1;
                }

            return -1;
        }

        public void Clear()
        {
            for (var i = ControlPointCount - 1; i >= 0; --i)
                RemoveControlPoint(i);

            m_TotalSplineLenght = 0.0f;
            m_ArcLengthTable.Clear();
            m_ArcLengthTable.Add(0);
        }

        public void RemoveControlPoint(int index)
        {
            foreach (var channel in GetDataChannels())
                channel.RemoveAt(index);

            m_Names.RemoveAt(index);

            if (OnControlPointRemoved != null)
                OnControlPointRemoved.Invoke(index);

            dirty = true;
        }

        public void FlushEvents()
        {
            var isDirty = dirty;
            foreach (var channel in GetDataChannels())
            {
                isDirty |= channel.Dirty;
                channel.Clean();
            }

            if (OnPathChanged != null && isDirty)
            {
                dirty = false;
                OnPathChanged.Invoke();
            }
        }
    }
}