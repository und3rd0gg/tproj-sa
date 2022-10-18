using UnityEditor;
using UnityEngine;

public class BubbleShaderTexturedEditor : ShaderGUI
{
    private static readonly Color SLIDER_RED = new Color(.8f, .3f, .3f, 1f);
    private static readonly Color SLIDER_GREEN = new Color(.3f, .8f, .3f, 1f);
    private static readonly Color SLIDER_BLUE = new Color(.3f, .3f, .8f, 1f);


    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (materialEditor.targets.Length > 1)
        {
            EditorGUILayout.HelpBox("Multiple selection is not supported.", MessageType.Error);
            return;
        }

        GUILayout.Label("Base", EditorStyles.boldLabel);

        var mainTex = FindProperty("_MainTexture", properties);

        var mainColor = FindProperty("_MainColor", properties);
        var glossiness = FindProperty("_Glossiness", properties);
        var metallic = FindProperty("_Metal", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture"), mainTex, mainColor);
        //materialEditor.ColorProperty(mainColor, "Base color");
        materialEditor.RangeProperty(glossiness, "Smoothness");
        materialEditor.RangeProperty(metallic, "Metallic");

        GUILayout.Space(10f);

        materialEditor.EnableInstancingField();
        materialEditor.RenderQueueField();

        GUILayout.Space(20f);

        GUILayout.Label("Animation", EditorStyles.boldLabel);
        var amplitude = FindProperty("_BounceAmplitude", properties);
        var frequency = FindProperty("_BounceFrequency", properties);

        materialEditor.RangeProperty(amplitude, "Bounce Amplitude");
        materialEditor.RangeProperty(frequency, "Bounce Frequency");


        GUILayout.Space(20f);

        GUILayout.Label("Color shifting", EditorStyles.boldLabel);

        var shiftMode = FindProperty("_ColorShiftMode", properties);

        var blendMode = (BlendMode) (int) shiftMode.floatValue;
        blendMode = (BlendMode) EditorGUILayout.EnumPopup("Shift Blend Mode", blendMode);

        shiftMode.floatValue = (float) blendMode;

        var shiftMul = FindProperty("_ColorShifting", properties);
        materialEditor.RangeProperty(shiftMul, "Shifting Intensity");

        EditorGUILayout.HelpBox("You can play with the shifting's color channels to achieve different effects.",
            MessageType.Info);
        var peaks = FindProperty("_ColorShiftPeak", properties);
        var range = FindProperty("_ColorShiftBand", properties);

        Vector3 p = peaks.vectorValue;
        Vector3 r = range.vectorValue;

        var min = p - r / 2f;
        var max = p + r / 2f;

        GUI.color = SLIDER_RED;
        EditorGUILayout.MinMaxSlider("Red Shifting", ref min.x, ref max.x, 0, 1);

        GUI.color = SLIDER_GREEN;
        EditorGUILayout.MinMaxSlider("Green Shifting", ref min.y, ref max.y, 0, 1);

        GUI.color = SLIDER_BLUE;
        EditorGUILayout.MinMaxSlider("Blue Shifting", ref min.z, ref max.z, 0, 1);

        GUI.color = Color.white;
        min = Vector3.Max(Vector3.zero, min);
        max = Vector3.Min(Vector3.one, max);

        p = (min + max) / 2f;
        r = max - min;

        peaks.vectorValue = p;
        range.vectorValue = r;


        GUILayout.Space(10f);

        var spatialNoise = FindProperty("_SpatialNoise", properties);
        materialEditor.ShaderProperty(spatialNoise, new GUIContent("Enable spatial noise"));

        var shiftNoise = FindProperty("_ColorShiftNoise", properties);

        Vector3 noiseValues = shiftNoise.vectorValue;

        noiseValues.x = EditorGUILayout.Slider("Shift Noise Intensity", noiseValues.x, 0f, 1f);
        noiseValues.y = EditorGUILayout.Slider("Shift Noise Scale", noiseValues.y, 0f, 2f);
        noiseValues.z = EditorGUILayout.Slider("Shift Noise Speed", noiseValues.z, 0f, 10f);

        shiftNoise.vectorValue = noiseValues;
    }
}