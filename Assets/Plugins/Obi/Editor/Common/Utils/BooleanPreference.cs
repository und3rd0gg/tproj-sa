using UnityEditor;

namespace Obi
{
    public class BooleanPreference
    {
        private bool m_Loaded;
        private readonly string m_Name;
        private bool m_Value;

        public BooleanPreference(string name, bool value)
        {
            m_Name = name;
            m_Loaded = false;
            m_Value = value;
        }

        public bool value
        {
            get
            {
                Load();
                return m_Value;
            }
            set
            {
                Load();
                if (m_Value == value)
                    return;
                m_Value = value;
                EditorPrefs.SetBool(m_Name, value);
            }
        }

        private void Load()
        {
            if (m_Loaded)
                return;

            m_Loaded = true;
            m_Value = EditorPrefs.GetBool(m_Name, m_Value);
        }

        public static implicit operator bool(BooleanPreference s)
        {
            return s.value;
        }
    }
}