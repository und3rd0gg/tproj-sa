using System;
using UnityEngine;

[Serializable]
public struct ObiBrushMirrorSettings
{
    [Flags]
    public enum MirrorAxis
    {
        None = 0x0,
        X = 0x1,
        Y = 0x2,
        Z = 0x4
    }

    public enum MirrorSpace
    {
        World = 0,
        Camera = 1
    }

    public MirrorAxis axis;
    public MirrorSpace space;

    public Vector3 ToAxis()
    {
        var m = (uint) axis;

        var xMirror = (m & (uint) MirrorAxis.X) > 0;
        var yMirror = (m & (uint) MirrorAxis.Y) > 0;
        var zMirror = (m & (uint) MirrorAxis.Z) > 0;

        if (axis < 0 || (int) axis > (int) MirrorAxis.X + (int) MirrorAxis.Y + (int) MirrorAxis.Z) return Vector3.one;

        var reflection = Vector3.one;

        if (xMirror)
            reflection.x = -1f;

        if (yMirror)
            reflection.y = -1f;

        if (zMirror)
            reflection.z = -1f;

        return reflection;
    }
}