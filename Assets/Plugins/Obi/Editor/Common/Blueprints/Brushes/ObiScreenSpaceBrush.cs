using System;
using UnityEditor;
using UnityEngine;

namespace Obi
{
    public class ObiScreenSpaceBrush : ObiBrushBase
    {
        public ObiScreenSpaceBrush(Action onStrokeStart, Action onStrokeUpdate, Action onStrokeEnd) : base(
            onStrokeStart, onStrokeUpdate, onStrokeEnd)
        {
            radius = 32;
        }

        protected override float WeightFromDistance(float distance)
        {
            // anything outside the brush should have zero weight:
            if (distance > radius)
                return 0;
            return 1;
        }

        protected override void GenerateWeights(Vector3[] positions)
        {
            for (var i = 0; i < positions.Length; i++)
            {
                // get particle position in gui space:
                var pos = HandleUtility.WorldToGUIPoint(positions[i]);

                // get distance from mouse position to particle position:
                weights[i] = WeightFromDistance(Vector3.Distance(Event.current.mousePosition, pos));
            }
        }

        protected override void OnRepaint()
        {
            base.OnRepaint();

            var cam = Camera.current;
            var depth = (cam.nearClipPlane + cam.farClipPlane) * 0.5f;

            var ppp = EditorGUIUtility.pixelsPerPoint;
            var mousePos = new Vector2(Event.current.mousePosition.x * ppp,
                cam.pixelHeight - Event.current.mousePosition.y * ppp);

            Handles.color = ObiEditorSettings.GetOrCreateSettings().brushColor;
            var point = new Vector3(mousePos.x, mousePos.y, depth);
            var wsPoint = cam.ScreenToWorldPoint(point);

            var p1 = cam.ScreenToWorldPoint(new Vector3(1, 0, depth));
            var p2 = cam.ScreenToWorldPoint(new Vector3(0, 0, depth));
            var units = Vector3.Distance(p1, p2);

            Handles.DrawWireDisc(wsPoint, cam.transform.forward, radius * units);
        }
    }
}