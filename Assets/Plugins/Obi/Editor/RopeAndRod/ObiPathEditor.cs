using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace Obi
{
    [EditorTool("Obi Path Editor Tool", typeof(ObiRopeBase))]
    public class ObiPathEditor : EditorTool
    {
        protected static Color handleColor = new Color(1, 0.55f, 0.1f);

        public bool needsRepaint;

        private PathEditorTool currentTool = PathEditorTool.TranslatePoints;
        protected int lastSelected;
        protected GUIContent m_IconContent;

        private ObiPath path;

        private Quaternion prevRot = Quaternion.identity;
        private Vector3 prevScale = Vector3.one;
        protected int selectedCount;

        protected bool[] selectedStatus;
        protected Vector3 selectionAverage;
        private bool showTangentHandles = true;
        private bool showThicknessHandles = true;
        protected bool useOrientation;

        private int windowId;

        public override GUIContent toolbarIcon
        {
            get
            {
                if (m_IconContent == null)
                    m_IconContent = new GUIContent
                    {
                        image = Resources.Load<Texture2D>("EditCurves"),
                        text = "Obi Path Editor Tool",
                        tooltip = "Obi Path Editor Tool"
                    };
                return m_IconContent;
            }
        }

        private ObiRopeBlueprintBase blueprint => (target as ObiRopeBase).sharedBlueprint as ObiRopeBlueprintBase;

        public void OnEnable()
        {
            useOrientation = target is ObiRod;
            selectedStatus = new bool[0];
        }

        public void ResizeCPArrays()
        {
            Array.Resize(ref selectedStatus, path.ControlPointCount);
        }

        public override void OnToolGUI(EditorWindow window)
        {
            needsRepaint = false;

            var thicknessScale = blueprint.thickness;
            path = (target as ObiRopeBase).path;
            var matrix = (target as ObiRopeBase).transform.localToWorldMatrix;

            ResizeCPArrays();

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            // get a window ID:
            if (Event.current.type != EventType.Used)
                windowId = GUIUtility.GetControlID(FocusType.Passive);

            var prevMatrix = Handles.matrix;
            Handles.matrix = matrix;

            // Draw control points:
            Handles.color = handleColor;
            for (var i = 0; i < path.ControlPointCount; ++i) needsRepaint |= DrawControlPoint(i);

            // Control point selection handle:
            needsRepaint |= ObiPathHandles.SplineCPSelector(path, selectedStatus);

            // Count selected and calculate average position:
            selectionAverage = GetControlPointAverage(out lastSelected, out selectedCount);

            // Draw cp tool handles:
            needsRepaint |= SplineCPTools(matrix);

            if (showThicknessHandles)
                needsRepaint |= DoThicknessHandles(thicknessScale);

            // Sceneview GUI:
            Handles.BeginGUI();
            GUILayout.Window(windowId, new Rect(10, 28, 0, 0), DrawUIWindow, "Path editor");
            Handles.EndGUI();

            Handles.matrix = prevMatrix;

            // During edit mode, allow to add/remove control points.
            if (currentTool == PathEditorTool.InsertPoints)
                AddControlPointsMode(matrix);

            if (currentTool == PathEditorTool.RemovePoints)
                RemoveControlPointsMode(matrix);

            if (needsRepaint)
                window.Repaint();
        }

        private void AddControlPointsMode(Matrix4x4 matrix)
        {
            var mu = ScreenPointToCurveMu(path, Event.current.mousePosition, matrix);

            var pointOnSpline = matrix.MultiplyPoint3x4(path.points.GetPositionAtMu(path.Closed, mu));

            var size = HandleUtility.GetHandleSize(pointOnSpline) * 0.12f;

            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Handles.color = Color.green;
            Handles.DrawDottedLine(pointOnSpline, ray.origin, 4);
            Handles.SphereHandleCap(0, pointOnSpline, Quaternion.identity, size, Event.current.type);


            if (Event.current.type == EventType.MouseDown && Event.current.modifiers == EventModifiers.None)
            {
                Undo.RecordObject(blueprint, "Add");

                var newIndex = path.InsertControlPoint(mu);
                if (newIndex >= 0)
                {
                    ResizeCPArrays();
                    for (var i = 0; i < selectedStatus.Length; ++i)
                        selectedStatus[i] = false;
                    selectedStatus[newIndex] = true;
                }

                path.FlushEvents();
                Event.current.Use();
            }

            // Repaint the scene, so that the add control point helpers are updated every frame.
            SceneView.RepaintAll();
        }

        private void RemoveControlPointsMode(Matrix4x4 matrix)
        {
            var mu = ScreenPointToCurveMu(path, Event.current.mousePosition, matrix);

            var pointOnSpline = matrix.MultiplyPoint3x4(path.points.GetPositionAtMu(path.Closed, mu));

            var size = HandleUtility.GetHandleSize(pointOnSpline) * 0.12f;

            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            Handles.color = Color.red;
            Handles.DrawDottedLine(pointOnSpline, ray.origin, 4);

            var index = path.GetClosestControlPointIndex(mu);
            Handles.SphereHandleCap(0, matrix.MultiplyPoint3x4(path.points[index].position), Quaternion.identity, size,
                Event.current.type);

            if (Event.current.type == EventType.MouseDown && Event.current.modifiers == EventModifiers.None &&
                index >= 0 && path.ControlPointCount > 2)
            {
                Undo.RecordObject(blueprint, "Remove");

                path.RemoveControlPoint(index);
                ResizeCPArrays();
                for (var i = 0; i < selectedStatus.Length; ++i)
                    selectedStatus[i] = false;

                path.FlushEvents();
                Event.current.Use();
            }

            // Repaint the scene, so that the add control point helpers are updated every frame.
            SceneView.RepaintAll();
        }

        protected bool DrawControlPoint(int i)
        {
            var repaint = false;
            var wp = path.points[i];
            var size = HandleUtility.GetHandleSize(wp.position) * 0.04f;

            if (selectedStatus[i] && showTangentHandles)
            {
                Handles.color = handleColor;

                if (!(i == 0 && !path.Closed))
                {
                    var tangentPosition = wp.inTangentEndpoint;

                    if (Event.current.type == EventType.Repaint)
                        Handles.DrawDottedLine(tangentPosition, wp.position, 2);

                    EditorGUI.BeginChangeCheck();
                    Handles.DotHandleCap(0, tangentPosition, Quaternion.identity, size, Event.current.type);
                    var newTangent = Handles.PositionHandle(tangentPosition, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(blueprint, "Modify tangent");
                        wp.SetInTangentEndpoint(newTangent);
                        path.points[i] = wp;
                        path.FlushEvents();
                        repaint = true;
                    }
                }

                if (!(i == path.ControlPointCount - 1 && !path.Closed))
                {
                    var tangentPosition = wp.outTangentEndpoint;

                    if (Event.current.type == EventType.Repaint)
                        Handles.DrawDottedLine(tangentPosition, wp.position, 2);

                    EditorGUI.BeginChangeCheck();
                    Handles.DotHandleCap(0, tangentPosition, Quaternion.identity, size, Event.current.type);
                    var newTangent = Handles.PositionHandle(tangentPosition, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(blueprint, "Modify tangent");
                        wp.SetOutTangentEndpoint(newTangent);
                        path.points[i] = wp;
                        path.FlushEvents();
                        repaint = true;
                    }
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                Handles.color = selectedStatus[i] ? handleColor : Color.white;
                var pos = wp.position;

                if (currentTool == PathEditorTool.OrientPoints)
                    Handles.ArrowHandleCap(0, pos, Quaternion.LookRotation(path.normals[i]),
                        HandleUtility.GetHandleSize(pos), EventType.Repaint);

                Handles.SphereHandleCap(0, pos, Quaternion.identity, size * 3, EventType.Repaint);
            }

            return repaint;
        }

        protected Vector3 GetControlPointAverage(out int lastSelected, out int selectedCount)
        {
            lastSelected = -1;
            selectedCount = 0;
            var averagePos = Vector3.zero;

            // Find center of all selected control points:
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    averagePos += path.points[i].position;
                    selectedCount++;
                    lastSelected = i;
                }

            if (selectedCount > 0)
                averagePos /= selectedCount;
            return averagePos;
        }

        protected bool SplineCPTools(Matrix4x4 matrix)
        {
            var repaint = false;

            // Calculate handle rotation, for local or world pivot modes.
            var handleRotation = Tools.pivotRotation == PivotRotation.Local
                ? Quaternion.identity
                : Quaternion.Inverse(matrix.rotation);

            // Reset initial handle rotation/orientation after using a tool:
            if (GUIUtility.hotControl == 0)
            {
                prevRot = handleRotation;
                prevScale = Vector3.one;

                if (selectedCount == 1 && Tools.pivotRotation == PivotRotation.Local &&
                    currentTool == PathEditorTool.OrientPoints)
                {
                    //prevRot = Quaternion.LookRotation(GetNormal(lastSelected));
                }
            }

            // Transform handles:
            if (selectedCount > 0)
            {
                if (useOrientation && currentTool == PathEditorTool.OrientPoints)
                    repaint |= OrientTool(selectionAverage, handleRotation);
                else
                    switch (currentTool)
                    {
                        case PathEditorTool.TranslatePoints:
                        {
                            repaint |= MoveTool(selectionAverage, handleRotation);
                        }
                            break;

                        case PathEditorTool.ScalePoints:
                        {
                            repaint |= ScaleTool(selectionAverage, handleRotation);
                        }
                            break;

                        case PathEditorTool.RotatePoints:
                        {
                            repaint |= RotateTool(selectionAverage, handleRotation);
                        }
                            break;
                    }
            }

            return repaint;
        }

        protected bool MoveTool(Vector3 handlePosition, Quaternion handleRotation)
        {
            EditorGUI.BeginChangeCheck();
            var newPos = Handles.PositionHandle(handlePosition, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Move control point");

                var delta = newPos - handlePosition;

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                    {
                        var wp = path.points[i];
                        wp.Transform(delta, Quaternion.identity, Vector3.one);
                        path.points[i] = wp;
                    }

                path.FlushEvents();
                return true;
            }

            return false;
        }

        protected bool ScaleTool(Vector3 handlePosition, Quaternion handleRotation)
        {
            EditorGUI.BeginChangeCheck();
            var scale = Handles.ScaleHandle(prevScale, handlePosition, handleRotation,
                HandleUtility.GetHandleSize(handlePosition));

            if (EditorGUI.EndChangeCheck())
            {
                var deltaScale = new Vector3(scale.x / prevScale.x, scale.y / prevScale.y, scale.z / prevScale.z);
                prevScale = scale;

                Undo.RecordObject(blueprint, "Scale control point");

                if (Tools.pivotMode == PivotMode.Center && selectedCount > 1)
                {
                    for (var i = 0; i < path.ControlPointCount; ++i)
                        if (selectedStatus[i])
                        {
                            var wp = path.points[i];
                            var newPos = handlePosition + Vector3.Scale(wp.position - handlePosition, deltaScale);
                            wp.Transform(newPos - wp.position, Quaternion.identity, Vector3.one);
                            path.points[i] = wp;
                        }
                }
                else
                {
                    // Scale all handles of selected control points relative to their control point:
                    for (var i = 0; i < path.ControlPointCount; ++i)
                        if (selectedStatus[i])
                        {
                            var wp = path.points[i];
                            wp.Transform(Vector3.zero, Quaternion.identity, deltaScale);
                            path.points[i] = wp;
                        }
                }

                path.FlushEvents();
                return true;
            }

            return false;
        }

        protected bool RotateTool(Vector3 handlePosition, Quaternion handleRotation)
        {
            EditorGUI.BeginChangeCheck();

            // TODO: investigate weird rotation gizmo:
            var newRotation = Handles.RotationHandle(prevRot, handlePosition);

            if (EditorGUI.EndChangeCheck())
            {
                var delta = newRotation * Quaternion.Inverse(prevRot);
                prevRot = newRotation;

                Undo.RecordObject(blueprint, "Rotate control point");

                if (Tools.pivotMode == PivotMode.Center && selectedCount > 1)
                {
                    // Rotate all selected control points around their average:
                    for (var i = 0; i < path.ControlPointCount; ++i)
                        if (selectedStatus[i])
                        {
                            var wp = path.points[i];
                            var newPos = handlePosition + delta * (wp.position - handlePosition);
                            wp.Transform(newPos - wp.position, Quaternion.identity, Vector3.one);
                            path.points[i] = wp;
                        }
                }
                else
                {
                    // Rotate all handles of selected control points around their control point:
                    for (var i = 0; i < path.ControlPointCount; ++i)
                        if (selectedStatus[i])
                        {
                            var wp = path.points[i];
                            wp.Transform(Vector3.zero, delta, Vector3.one);
                            path.points[i] = wp;
                        }
                }

                path.FlushEvents();
                return true;
            }

            return false;
        }

        protected bool OrientTool(Vector3 averagePos, Quaternion pivotRotation)
        {
            EditorGUI.BeginChangeCheck();
            var newRotation = Handles.RotationHandle(prevRot, averagePos);

            if (EditorGUI.EndChangeCheck())
            {
                var delta = newRotation * Quaternion.Inverse(prevRot);
                prevRot = newRotation;

                Undo.RecordObject(blueprint, "Orient control point");

                // Rotate all selected control points around their average:
                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                        path.normals[i] = delta * path.normals[i];

                path.FlushEvents();
                return true;
            }

            return false;
        }


        protected bool DoThicknessHandles(float scale)
        {
            var oldColor = Handles.color;
            Handles.color = handleColor;

            EditorGUI.BeginChangeCheck();
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    var position = path.points[i].position;

                    var tangent = path.points.GetTangent(i);
                    if (!tangent.Equals(Vector3.zero))
                    {
                        var orientation = Quaternion.LookRotation(tangent);

                        var offset = 0.05f;
                        var thickness = path.thicknesses[i] * scale + offset;

                        EditorGUI.BeginChangeCheck();
                        thickness = DoRadiusHandle(orientation, position, thickness);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(blueprint, "Change control point thickness");
                            path.thicknesses[i] = Mathf.Max(0, (thickness - offset) / scale);
                            path.FlushEvents();
                            return true;
                        }
                    }
                }

            Handles.color = oldColor;

            return false;
        }

        public void DrawUIWindow(int windowID)
        {
            DrawToolButtons();

            DrawControlPointInspector();
        }

        private void DrawToolButtons()
        {
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(currentTool == PathEditorTool.TranslatePoints,
                new GUIContent(Resources.Load<Texture2D>("TranslateControlPoint"), "Translate CPs"), "Button",
                GUILayout.MaxHeight(24), GUILayout.Width(38));
            if (EditorGUI.EndChangeCheck()) currentTool = PathEditorTool.TranslatePoints;

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(currentTool == PathEditorTool.RotatePoints,
                new GUIContent(Resources.Load<Texture2D>("RotateControlPoint"), "Rotate CPs"), "Button",
                GUILayout.MaxHeight(24), GUILayout.Width(38));
            if (EditorGUI.EndChangeCheck()) currentTool = PathEditorTool.RotatePoints;

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(currentTool == PathEditorTool.ScalePoints,
                new GUIContent(Resources.Load<Texture2D>("ScaleControlPoint"), "Scale CPs"), "Button",
                GUILayout.MaxHeight(24), GUILayout.Width(38));
            if (EditorGUI.EndChangeCheck()) currentTool = PathEditorTool.ScalePoints;

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(currentTool == PathEditorTool.InsertPoints,
                new GUIContent(Resources.Load<Texture2D>("AddControlPoint"), "Add CPs"), "Button",
                GUILayout.MaxHeight(24), GUILayout.Width(38));
            if (EditorGUI.EndChangeCheck()) currentTool = PathEditorTool.InsertPoints;

            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(currentTool == PathEditorTool.RemovePoints,
                new GUIContent(Resources.Load<Texture2D>("RemoveControlPoint"), "Remove CPs"), "Button",
                GUILayout.MaxHeight(24), GUILayout.Width(38));
            if (EditorGUI.EndChangeCheck()) currentTool = PathEditorTool.RemovePoints;

            EditorGUI.BeginChangeCheck();
            var closed = GUILayout.Toggle(path.Closed,
                new GUIContent(Resources.Load<Texture2D>("OpenCloseCurve"), "Open/Close the path"), "Button",
                GUILayout.MaxHeight(24), GUILayout.Width(38));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Open/close path");
                path.Closed = closed;
                path.FlushEvents();
                needsRepaint = true;
            }

            if (useOrientation)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Toggle(currentTool == PathEditorTool.OrientPoints,
                    new GUIContent(Resources.Load<Texture2D>("OrientControlPoint"), "Orientation tool"), "Button",
                    GUILayout.MaxHeight(24), GUILayout.Width(38));
                if (EditorGUI.EndChangeCheck()) currentTool = PathEditorTool.OrientPoints;
            }

            showTangentHandles = GUILayout.Toggle(showTangentHandles,
                new GUIContent(Resources.Load<Texture2D>("ShowTangentHandles"), "Show tangent handles"), "Button",
                GUILayout.MaxHeight(24), GUILayout.Width(38));
            showThicknessHandles = GUILayout.Toggle(showThicknessHandles,
                new GUIContent(Resources.Load<Texture2D>("ShowThicknessHandles"), "Show thickness handles"), "Button",
                GUILayout.MaxHeight(24), GUILayout.Width(38));

            GUILayout.EndHorizontal();
        }

        private void DrawPositionField(Rect rect, string label, int index)
        {
            EditorGUI.showMixedValue = false;
            float pos = 0;
            var firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        pos = path.points[i].position[index];
                        firstSelected = false;
                    }
                    else if (!Mathf.Approximately(pos, path.points[i].position[index]))
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 10;
            pos = EditorGUI.FloatField(rect, label, pos);
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control points position");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                    {
                        var wp = path.points[i];
                        wp.position[index] = pos;
                        path.points[i] = wp;
                    }

                path.FlushEvents();
                needsRepaint = true;
            }
        }

        private void DrawInTangentField(Rect rect, string label, int index)
        {
            EditorGUI.showMixedValue = false;
            float pos = 0;
            var firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        pos = path.points[i].inTangent[index];
                        firstSelected = false;
                    }
                    else if (!Mathf.Approximately(pos, path.points[i].inTangent[index]))
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 10;
            pos = EditorGUI.FloatField(rect, label, pos);
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control points tangent");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                    {
                        var wp = path.points[i];
                        var newInTangent = wp.inTangent;
                        newInTangent[index] = pos;
                        wp.SetInTangent(newInTangent);
                        path.points[i] = wp;
                    }

                path.FlushEvents();
                needsRepaint = true;
            }
        }

        private void DrawOutTangentField(Rect rect, string label, int index)
        {
            EditorGUI.showMixedValue = false;
            float pos = 0;
            var firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        pos = path.points[i].outTangent[index];
                        firstSelected = false;
                    }
                    else if (!Mathf.Approximately(pos, path.points[i].outTangent[index]))
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 10;
            pos = EditorGUI.FloatField(rect, label, pos);
            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control points tangent");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                    {
                        var wp = path.points[i];
                        var newOutTangent = wp.outTangent;
                        newOutTangent[index] = pos;
                        wp.SetOutTangent(newOutTangent);
                        path.points[i] = wp;
                    }

                path.FlushEvents();
                needsRepaint = true;
            }
        }

        private void DrawControlPointInspector()
        {
            GUI.enabled = selectedCount > 0;

            var wideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = 100;

            EditorGUILayout.BeginVertical();

            GUILayout.Box("", ObiEditorUtils.GetSeparatorLineStyle());

            // position:
            var rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Position"));
            rect.width /= 3.0f;
            DrawPositionField(rect, "X", 0);
            rect.x += rect.width;
            DrawPositionField(rect, "Y", 1);
            rect.x += rect.width;
            DrawPositionField(rect, "Z", 2);
            rect.x += rect.width;

            // in tangent:
            rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive),
                new GUIContent("In Tangent"));
            rect.width /= 3.0f;
            DrawInTangentField(rect, "X", 0);
            rect.x += rect.width;
            DrawInTangentField(rect, "Y", 1);
            rect.x += rect.width;
            DrawInTangentField(rect, "Z", 2);
            rect.x += rect.width;

            // out tangent:
            rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive),
                new GUIContent("Out Tangent"));
            rect.width /= 3.0f;
            DrawOutTangentField(rect, "X", 0);
            rect.x += rect.width;
            DrawOutTangentField(rect, "Y", 1);
            rect.x += rect.width;
            DrawOutTangentField(rect, "Z", 2);
            rect.x += rect.width;

            // tangent mode:
            EditorGUI.showMixedValue = false;
            var mode = ObiWingedPoint.TangentMode.Free;
            var firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        mode = path.points[i].tangentMode;
                        firstSelected = false;
                    }
                    else if (mode != path.points[i].tangentMode)
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            var newMode =
                (ObiWingedPoint.TangentMode) EditorGUILayout.EnumPopup("Tangent mode", mode, GUILayout.MinWidth(94));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control points mode");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                    {
                        var wp = path.points[i];
                        wp.tangentMode = newMode;
                        path.points[i] = wp;
                    }

                path.FlushEvents();
                needsRepaint = true;
            }

            // thickness:
            EditorGUI.showMixedValue = false;
            float thickness = 0;
            firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        thickness = path.thicknesses[i];
                        firstSelected = false;
                    }
                    else if (!Mathf.Approximately(thickness, path.thicknesses[i]))
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            thickness = EditorGUILayout.FloatField("Thickness", thickness, GUILayout.MinWidth(94));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control point thickness");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                        path.thicknesses[i] = Mathf.Max(0, thickness);
                path.FlushEvents();
                needsRepaint = true;
            }

            // mass:
            EditorGUI.showMixedValue = false;
            float mass = 0;
            firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        mass = path.masses[i];
                        firstSelected = false;
                    }
                    else if (!Mathf.Approximately(mass, path.masses[i]))
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            mass = EditorGUILayout.FloatField("Mass", mass, GUILayout.MinWidth(94));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control point mass");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                        path.masses[i] = mass;
                path.FlushEvents();
                needsRepaint = true;
            }

            if (useOrientation)
            {
                // rotational mass:
                EditorGUI.showMixedValue = false;
                float rotationalMass = 0;
                firstSelected = true;
                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                    {
                        if (firstSelected)
                        {
                            rotationalMass = path.rotationalMasses[i];
                            firstSelected = false;
                        }
                        else if (!Mathf.Approximately(rotationalMass, path.rotationalMasses[i]))
                        {
                            EditorGUI.showMixedValue = true;
                            break;
                        }
                    }

                EditorGUI.BeginChangeCheck();
                rotationalMass = EditorGUILayout.FloatField("Rotational mass", rotationalMass, GUILayout.MinWidth(94));
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(blueprint, "Change control point rotational mass");

                    for (var i = 0; i < path.ControlPointCount; ++i)
                        if (selectedStatus[i])
                            path.rotationalMasses[i] = rotationalMass;
                    path.FlushEvents();
                    needsRepaint = true;
                }
            }

            // category:
            EditorGUI.showMixedValue = false;
            var category = 0;
            firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        category = ObiUtils.GetCategoryFromFilter(path.filters[i]);
                        firstSelected = false;
                    }
                    else if (!Mathf.Approximately(category, ObiUtils.GetCategoryFromFilter(path.filters[i])))
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            category = EditorGUILayout.Popup("Category", category, ObiUtils.categoryNames, GUILayout.MinWidth(94));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control point category");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                        path.filters[i] = ObiUtils.MakeFilter(ObiUtils.GetMaskFromFilter(path.filters[i]), category);
                path.FlushEvents();
                needsRepaint = true;
            }

            // mask:
            EditorGUI.showMixedValue = false;
            var mask = 0;
            firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        mask = ObiUtils.GetMaskFromFilter(path.filters[i]);
                        firstSelected = false;
                    }
                    else if (!Mathf.Approximately(mask, ObiUtils.GetMaskFromFilter(path.filters[i])))
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            mask = EditorGUILayout.MaskField("Collides with", mask, ObiUtils.categoryNames, GUILayout.MinWidth(94));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control point mask");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                        path.filters[i] = ObiUtils.MakeFilter(mask, ObiUtils.GetCategoryFromFilter(path.filters[i]));
                path.FlushEvents();
                needsRepaint = true;
            }

            // color:
            EditorGUI.showMixedValue = false;
            var color = Color.white;
            firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        color = path.colors[i];
                        firstSelected = false;
                    }
                    else if (color != path.colors[i])
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            color = EditorGUILayout.ColorField("Color", color, GUILayout.MinWidth(94));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control point color");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                        path.colors[i] = color;
                path.FlushEvents();
                needsRepaint = true;
            }

            // name:
            EditorGUI.showMixedValue = false;
            var cpname = "";
            firstSelected = true;
            for (var i = 0; i < path.ControlPointCount; ++i)
                if (selectedStatus[i])
                {
                    if (firstSelected)
                    {
                        cpname = path.GetName(i);
                        firstSelected = false;
                    }
                    else if (cpname != path.GetName(i))
                    {
                        EditorGUI.showMixedValue = true;
                        break;
                    }
                }

            EditorGUI.BeginChangeCheck();
            cpname = EditorGUILayout.DelayedTextField("Name", cpname, GUILayout.MinWidth(94));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(blueprint, "Change control point name");

                for (var i = 0; i < path.ControlPointCount; ++i)
                    if (selectedStatus[i])
                        path.SetName(i, cpname);
                path.FlushEvents();
                needsRepaint = true;
            }


            EditorGUILayout.EndVertical();

            EditorGUIUtility.wideMode = wideMode;

            GUI.enabled = true;
        }

        internal static float DoRadiusHandle(Quaternion rotation, Vector3 position, float radius)
        {
            Vector3[] vector3Array;

            Vector3 camToPosition;
            if (Camera.current.orthographic)
            {
                camToPosition = Camera.current.transform.forward;
                Handles.DrawWireDisc(position, camToPosition, radius);

                vector3Array = new Vector3[4]
                {
                    Camera.current.transform.right,
                    Camera.current.transform.up,
                    -Camera.current.transform.right,
                    -Camera.current.transform.up
                };
            }
            else
            {
                camToPosition = position - Camera.current.transform.position;
                Handles.DrawWireDisc(position, rotation * Vector3.forward, radius);

                vector3Array = new Vector3[4]
                {
                    rotation * Vector3.right,
                    rotation * Vector3.up,
                    rotation * -Vector3.right,
                    rotation * -Vector3.up
                };
            }

            for (var index = 0; index < 4; ++index)
            {
                var controlId = GUIUtility.GetControlID("ObiPathThicknessHandle".GetHashCode(), FocusType.Keyboard);
                var position1 = position + radius * vector3Array[index];
                var changed = GUI.changed;
                GUI.changed = false;
                var a = Handles.Slider(controlId, position1, vector3Array[index],
                    HandleUtility.GetHandleSize(position1) * 0.03f, Handles.DotHandleCap, 0.0f);
                if (GUI.changed)
                    radius = Vector3.Distance(a, position);
                GUI.changed |= changed;
            }

            return radius;
        }

        public static float ScreenPointToCurveMu(ObiPath path, Vector2 screenPoint, Matrix4x4 referenceFrame,
            int samples = 30)
        {
            if (path.ControlPointCount >= 2)
            {
                samples = Mathf.Max(1, samples);
                var step = 1 / (float) samples;

                float closestMu = 0;
                var minDistance = float.MaxValue;

                for (var k = 0; k < path.GetSpanCount(); ++k)
                {
                    var nextCP = (k + 1) % path.ControlPointCount;

                    var wp1 = path.points[k];
                    var wp2 = path.points[nextCP];

                    var _p = referenceFrame.MultiplyPoint3x4(wp1.position);
                    var p = referenceFrame.MultiplyPoint3x4(wp1.outTangentEndpoint);
                    var p_ = referenceFrame.MultiplyPoint3x4(wp2.inTangentEndpoint);
                    var p__ = referenceFrame.MultiplyPoint3x4(wp2.position);

                    var lastPoint = HandleUtility.WorldToGUIPoint(path.m_Points.Evaluate(_p, p, p_, p__, 0));
                    for (var i = 1; i <= samples; ++i)
                    {
                        var currentPoint =
                            HandleUtility.WorldToGUIPoint(path.m_Points.Evaluate(_p, p, p_, p__, i * step));

                        float mu;
                        var distance = Vector2.SqrMagnitude(
                            (Vector2) ObiUtils.ProjectPointLine(screenPoint, lastPoint, currentPoint, out mu) -
                            screenPoint);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestMu = (k + (i - 1) * step + mu / samples) / path.GetSpanCount();
                        }

                        lastPoint = currentPoint;
                    }
                }

                return closestMu;
            }

            Debug.LogWarning("Curve needs at least 2 control points to be defined.");
            return 0;
        }

        private enum PathEditorTool
        {
            TranslatePoints,
            RotatePoints,
            ScalePoints,
            OrientPoints,
            InsertPoints,
            RemovePoints
        }
    }
}