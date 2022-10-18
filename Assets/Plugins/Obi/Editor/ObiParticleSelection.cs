using UnityEditor;
using UnityEngine;

namespace Obi
{
    public static class ObiParticleSelection
    {
        private static readonly int particleSelectorHash = "ObiParticleSelectorHash".GetHashCode();

        private static Vector2 startPos;
        private static Vector2 currentPos;
        private static bool dragging;
        private static Rect marquee;

        public static bool DoSelection(Vector3[] positions,
            bool[] selectionStatus,
            bool[] facingCamera)
        {
            var cachedMatrix = Handles.matrix;

            var controlID = GUIUtility.GetControlID(particleSelectorHash, FocusType.Passive);
            var selectedParticleIndex = -1;
            var selectionStatusChanged = false;

            // select vertex on mouse click:
            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:

                    if (Event.current.button != 0) break;

                    startPos = Event.current.mousePosition;
                    marquee.Set(0, 0, 0, 0);

                    // If the user is not pressing shift, clear selection.
                    if ((Event.current.modifiers & EventModifiers.Shift) == 0 &&
                        (Event.current.modifiers & EventModifiers.Alt) == 0)
                        for (var i = 0; i < selectionStatus.Length; i++)
                            selectionStatus[i] = false;

                    // Allow use of marquee selection
                    if (Event.current.modifiers == EventModifiers.None ||
                        (Event.current.modifiers & EventModifiers.Shift) != 0)
                        GUIUtility.hotControl = controlID;

                    var minSqrDistance = float.MaxValue;

                    for (var i = 0; i < positions.Length; i++)
                    {
                        // skip not selectable particles:
                        //if (!facingCamera[i] && (selectBackfaces & ObiActorBlueprintEditor.ParticleCulling.Back) != 0) continue;
                        //if (facingCamera[i] && (selectBackfaces & ObiActorBlueprintEditor.ParticleCulling.Front) != 0) continue;

                        // get particle position in gui space:
                        var pos = HandleUtility.WorldToGUIPoint(positions[i]);

                        // get distance from mouse position to particle position:
                        var sqrDistance = Vector2.SqrMagnitude(startPos - pos);

                        // check if this particle is closer to the cursor that any previously considered particle.
                        if (sqrDistance < 100 && sqrDistance < minSqrDistance)
                        {
                            //magic number 100 = 10*10, where 10 is min distance in pixels to select a particle.
                            minSqrDistance = sqrDistance;
                            selectedParticleIndex = i;
                        }
                    }

                    if (selectedParticleIndex >= 0)
                    {
                        // toggle particle selection status.

                        selectionStatus[selectedParticleIndex] = !selectionStatus[selectedParticleIndex];
                        selectionStatusChanged = true;
                        GUIUtility.hotControl = controlID;
                        Event.current.Use();
                    }
                    else if (Event.current.modifiers == EventModifiers.None)
                    {
                        // deselect all particles:
                        for (var i = 0; i < selectionStatus.Length; i++)
                            selectionStatus[i] = false;

                        selectionStatusChanged = true;
                    }

                    break;

                case EventType.MouseMove:
                    SceneView.RepaintAll();
                    break;

                case EventType.MouseDrag:

                    if (GUIUtility.hotControl == controlID)
                    {
                        currentPos = Event.current.mousePosition;
                        if (!dragging && Vector2.Distance(startPos, currentPos) > 5)
                        {
                            dragging = true;
                        }
                        else
                        {
                            GUIUtility.hotControl = controlID;
                            Event.current.Use();
                        }

                        //update marquee rect:
                        var left = Mathf.Min(startPos.x, currentPos.x);
                        var right = Mathf.Max(startPos.x, currentPos.x);
                        var bottom = Mathf.Min(startPos.y, currentPos.y);
                        var top = Mathf.Max(startPos.y, currentPos.y);

                        marquee = new Rect(left, bottom, right - left, top - bottom);
                    }

                    break;

                case EventType.MouseUp:

                    if (GUIUtility.hotControl == controlID)
                    {
                        dragging = false;

                        for (var i = 0; i < positions.Length; i++)
                        {
                            // skip not selectable particles:
                            //switch (selectBackfaces)
                            {
                                //case ObiActorBlueprintEditor.ParticleCulling.Back: if (!facingCamera[i]) continue; break;
                                //case ObiActorBlueprintEditor.ParticleCulling.Front: if (facingCamera[i]) continue; break;
                            }

                            // get particle position in gui space:
                            var pos = HandleUtility.WorldToGUIPoint(positions[i]);

                            if (pos.x > marquee.xMin && pos.x < marquee.xMax && pos.y > marquee.yMin &&
                                pos.y < marquee.yMax)
                            {
                                selectionStatus[i] = true;
                                selectionStatusChanged = true;
                            }
                        }

                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }

                    break;

                case EventType.Repaint:

                    Handles.matrix = Matrix4x4.identity;

                    if (dragging)
                    {
                        var oldSkin = GUI.skin;
                        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
                        Handles.BeginGUI();
                        GUI.Box(new Rect(marquee.xMin, marquee.yMin, marquee.width, marquee.height), "");
                        Handles.EndGUI();
                        GUI.skin = oldSkin;
                    }

                    Handles.matrix = cachedMatrix;

                    break;
            }

            return selectionStatusChanged;
        }
    }
}