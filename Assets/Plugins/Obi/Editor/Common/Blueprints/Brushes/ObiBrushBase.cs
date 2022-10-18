using System;
using UnityEditor;
using UnityEngine;

namespace Obi
{
    public abstract class ObiBrushBase
    {
        private static readonly int particleBrushHash = "ObiBrushHash".GetHashCode();

        public IObiBrushMode brushMode;

        protected int controlID;
        public bool drag = true;
        public float innerRadius = 0.5f;
        protected Action onStrokeEnd;
        protected Action onStrokeStart;
        protected Action onStrokeUpdate;
        public float opacity = 1;
        public float radius = 1;
        public float speed = 0.1f;
        public float[] weights = new float[0];

        public ObiBrushBase(Action onStrokeStart, Action onStrokeUpdate, Action onStrokeEnd)
        {
            this.onStrokeStart = onStrokeStart;
            this.onStrokeUpdate = onStrokeUpdate;
            this.onStrokeEnd = onStrokeEnd;
        }

        public float SqrRadius => radius * radius;

        protected virtual float WeightFromDistance(float distance)
        {
            // anything outside the brush should have zero weight:
            if (distance > radius)
                return 0;

            var t = Mathf.InverseLerp(innerRadius * radius, radius, distance);
            return Mathf.SmoothStep(1, 0, t);
        }

        protected abstract void GenerateWeights(Vector3[] positions);

        protected virtual void OnMouseDown(Vector3[] positions)
        {
            if (Event.current.button != 0 || (Event.current.modifiers & ~EventModifiers.Shift) != EventModifiers.None)
                return;

            GUIUtility.hotControl = controlID;

            GenerateWeights(positions);

            if (onStrokeStart != null)
                onStrokeStart();

            if (brushMode != null)
                brushMode.ApplyStamps(this, (Event.current.modifiers & EventModifiers.Shift) != 0);

            if (onStrokeUpdate != null)
                onStrokeUpdate();

            Event.current.Use();
        }

        protected virtual void OnMouseMove(Vector3[] positions)
        {
        }

        protected virtual void OnMouseDrag(Vector3[] positions)
        {
            if (GUIUtility.hotControl == controlID && drag)
            {
                GenerateWeights(positions);

                if (brushMode != null)
                    brushMode.ApplyStamps(this, (Event.current.modifiers & EventModifiers.Shift) != 0);

                if (onStrokeUpdate != null)
                    onStrokeUpdate();

                Event.current.Use();
            }
        }

        protected virtual void OnMouseUp(Vector3[] positions)
        {
            if (GUIUtility.hotControl == controlID)
            {
                GUIUtility.hotControl = 0;
                Event.current.Use();

                if (onStrokeEnd != null)
                    onStrokeEnd();
            }
        }

        protected virtual void OnRepaint()
        {
        }

        public void DoBrush(Vector3[] positions)
        {
            var cachedMatrix = Handles.matrix;

            controlID = GUIUtility.GetControlID(particleBrushHash, FocusType.Passive);
            Array.Resize(ref weights, positions.Length);

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:

                    OnMouseDown(positions);

                    break;

                case EventType.MouseMove:

                    OnMouseMove(positions);

                    SceneView.RepaintAll();
                    break;

                case EventType.MouseDrag:

                    OnMouseDrag(positions);

                    break;

                case EventType.MouseUp:

                    OnMouseUp(positions);

                    break;

                case EventType.Repaint:

                    Handles.matrix = Matrix4x4.identity;

                    OnRepaint();

                    Handles.matrix = cachedMatrix;

                    break;
            }
        }
    }
}