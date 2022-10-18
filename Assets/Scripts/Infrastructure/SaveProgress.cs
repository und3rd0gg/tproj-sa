using UnityEngine;

namespace Infrastructure
{
    public static class SaveProgress
    {
        public static int MoneyBalance
        {
            get => PlayerPrefs.GetInt(nameof(MoneyBalance));
            set => PlayerPrefs.SetInt(nameof(MoneyBalance), value);
        }

        public static int PipeUpgrade
        {
            get => PlayerPrefs.GetInt(nameof(PipeUpgrade));
            set => PlayerPrefs.SetInt(nameof(PipeUpgrade), value);
        }


        public static int BubbleUpgrade
        {
            get => PlayerPrefs.GetInt(nameof(BubbleUpgrade));
            set => PlayerPrefs.SetInt(nameof(BubbleUpgrade), value);
        }

        public static int IncomeUpgrade
        {
            get => PlayerPrefs.GetInt(nameof(IncomeUpgrade));
            set => PlayerPrefs.SetInt(nameof(IncomeUpgrade), value);
        }

        public static bool HasSave(string name)
        {
            return PlayerPrefs.HasKey(name);
        }
    }
}