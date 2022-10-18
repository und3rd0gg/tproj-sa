using UnityEditor;
using UnityEngine;

namespace Obi
{
    internal class PreviewHelpers
    {
        // Preview interaction related stuff:
        private static readonly int sliderHash = "Slider".GetHashCode();

        public Camera m_Camera;
        public float m_CameraFieldOfView = 30f;
        public Light[] m_Light = new Light[2];
        internal RenderTexture m_RenderTexture;

        public PreviewHelpers() : this(false)
        {
        }

        public PreviewHelpers(bool renderFullScene)
        {
            var gameObject =
                EditorUtility.CreateGameObjectWithHideFlags("PreRenderCamera", HideFlags.HideAndDontSave,
                    typeof(Camera));
            m_Camera = gameObject.GetComponent<Camera>();
            m_Camera.fieldOfView = m_CameraFieldOfView;
            m_Camera.cullingMask = 1 << 1;
            m_Camera.enabled = false;
            m_Camera.clearFlags = CameraClearFlags.SolidColor;
            m_Camera.farClipPlane = 10f;
            m_Camera.nearClipPlane = 1f;
            m_Camera.backgroundColor = new Color(0.192156866f, 0.192156866f, 0.192156866f, 0);
            m_Camera.renderingPath = RenderingPath.Forward;
            m_Camera.useOcclusionCulling = false;

            for (var i = 0; i < 2; i++)
            {
                var gameObject2 =
                    EditorUtility.CreateGameObjectWithHideFlags("PreRenderLight", HideFlags.HideAndDontSave,
                        typeof(Light));
                m_Light[i] = gameObject2.GetComponent<Light>();
                m_Light[i].type = LightType.Directional;
                m_Light[i].intensity = 1f;
                m_Light[i].enabled = false;
            }

            m_Light[0].color = new Color(0.4f, 0.4f, 0.45f, 0f);
            m_Light[1].transform.rotation = Quaternion.Euler(340f, 218f, 177f);
            m_Light[1].color = new Color(0.4f, 0.4f, 0.45f, 0f) * 0.7f;
        }

        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            var controlID = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
            var current = Event.current;
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f)
                    {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID) GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        scrollPosition -= current.delta * (!current.shift ? 1 : 3) /
                            Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                        current.Use();
                        GUI.changed = true;
                    }

                    break;
            }

            return scrollPosition;
        }

        public void Cleanup()
        {
            if (m_Camera) Object.DestroyImmediate(m_Camera.gameObject, true);
            if (m_RenderTexture)
            {
                Object.DestroyImmediate(m_RenderTexture);
                m_RenderTexture = null;
            }

            var light = m_Light;
            for (var i = 0; i < light.Length; i++)
            {
                var light2 = light[i];
                if (light2) Object.DestroyImmediate(light2.gameObject, true);
            }
        }

        private void InitPreview(Rect r)
        {
            var num = (int) r.width;
            var num2 = (int) r.height;
            if (!m_RenderTexture || m_RenderTexture.width != num || m_RenderTexture.height != num2)
            {
                if (m_RenderTexture)
                {
                    Object.DestroyImmediate(m_RenderTexture);
                    m_RenderTexture = null;
                }

                var scaleFactor = GetScaleFactor(num, num2);
                m_RenderTexture = new RenderTexture((int) (num * scaleFactor), (int) (num2 * scaleFactor), 16);
                m_RenderTexture.hideFlags = HideFlags.HideAndDontSave;
                m_Camera.targetTexture = m_RenderTexture;
            }

            var num3 = m_RenderTexture.width > 0
                ? Mathf.Max(1f, m_RenderTexture.height / (float) m_RenderTexture.width)
                : 1f;
            m_Camera.fieldOfView = Mathf.Atan(num3 * Mathf.Tan(m_CameraFieldOfView * 0.5f * 0.0174532924f)) *
                                   57.29578f * 2f;
        }

        public float GetScaleFactor(float width, float height)
        {
            var a = Mathf.Max(Mathf.Min(width * 2f, 1024f), width) / width;
            var b = Mathf.Max(Mathf.Min(height * 2f, 1024f), height) / height;
            return Mathf.Min(a, b);
        }

        public void BeginPreview(Rect r, GUIStyle previewBackground)
        {
            InitPreview(r);
            if (previewBackground == null || previewBackground == GUIStyle.none) return;
        }

        public Texture EndPreview()
        {
            m_Camera.Render();
            return m_RenderTexture;
        }
    }
}