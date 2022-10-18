using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Obi
{
    public class ObiRaycastBrush : ObiBrushBase
    {
        public bool drawVolume = false;
        private readonly List<ObiRaycastHit> hits = new List<ObiRaycastHit>();

        public ObiBrushMirrorSettings mirror;
        public Mesh raycastTarget;
        public Matrix4x4 raycastTransform = Matrix4x4.identity;
        private readonly List<Ray> rays = new List<Ray>();

        public ObiRaycastBrush(Mesh raycastTarget, Action onStrokeStart, Action onStrokeUpdate, Action onStrokeEnd) :
            base(onStrokeStart, onStrokeUpdate, onStrokeEnd)
        {
            radius = 0.1f;
            this.raycastTarget = raycastTarget;
            rays = new List<Ray>();
        }

        protected override void GenerateWeights(Vector3[] positions)
        {
            if (raycastTarget != null)
            {
                rays.Clear();
                hits.Clear();

                for (var i = 0; i < positions.Length; i++)
                    weights[i] = 0;

                var vertices = raycastTarget.vertices;
                var triangles = raycastTarget.triangles;

                var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                rays.Add(mouseRay);

                var currentAxis = mirror;

                if (mirror.axis != ObiBrushMirrorSettings.MirrorAxis.None)
                    for (var i = 0; i < 3; i++)
                    {
                        currentAxis.axis = (ObiBrushMirrorSettings.MirrorAxis) (1u << i);
                        if (((uint) mirror.axis & (1u << i)) < 1)
                            continue;

                        var mirrorVector = currentAxis.ToAxis();

                        if (currentAxis.space == ObiBrushMirrorSettings.MirrorSpace.World)
                        {
                            var center = raycastTarget.bounds.center;
                            rays.Add(new Ray(Vector3.Scale(mouseRay.origin - center, mirrorVector) + center,
                                Vector3.Scale(mouseRay.direction, mirrorVector)));
                        }
                        else
                        {
                            var t = SceneView.lastActiveSceneView.camera.transform;
                            var o = t.InverseTransformPoint(mouseRay.origin);
                            var d = t.InverseTransformDirection(mouseRay.direction);
                            rays.Add(new Ray(t.TransformPoint(Vector3.Scale(o, mirrorVector)),
                                t.TransformDirection(Vector3.Scale(d, mirrorVector))));
                        }
                    }

                foreach (var ray in rays)
                    if (ObiMeshUtils.WorldRaycast(ray, raycastTransform, vertices, triangles, out var hit))
                    {
                        hit.position = raycastTransform.MultiplyPoint3x4(hit.position);
                        hit.normal = raycastTransform.MultiplyVector(hit.normal);
                        hits.Add(hit);

                        for (var i = 0; i < positions.Length; i++)
                        {
                            // get distance from hit position to particle position:
                            var weight = WeightFromDistance(Vector3.Distance(hit.position, positions[i]));
                            weights[i] = Mathf.Max(weights[i], weight);
                        }
                    }
            }
        }

        protected override void OnMouseMove(Vector3[] positions)
        {
            base.OnMouseMove(positions);
            GenerateWeights(positions);
        }

        protected override void OnRepaint()
        {
            base.OnRepaint();

            if (raycastTarget != null)
            {
                var brushColor = ObiEditorSettings.GetOrCreateSettings().brushColor;

                foreach (var hit in hits)
                    if (hit != null && hit.triangle >= 0)
                    {
                        Handles.color = brushColor;
                        Handles.DrawLine(hit.position, hit.position + hit.normal.normalized * radius);
                        Handles.DrawWireDisc(hit.position, hit.normal, radius);
                        Handles.DrawWireDisc(hit.position, hit.normal, innerRadius * radius);

                        if (drawVolume)
                        {
                            Handles.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.2f);
                            Handles.SphereHandleCap(0, hit.position, Quaternion.identity, radius * 2,
                                EventType.Repaint);
                        }
                    }
            }
        }
    }
}