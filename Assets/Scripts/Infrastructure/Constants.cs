using UnityEngine;

namespace Infrastructure
{
    public static class Constants
    {
        public static class AnimatorPipeController
        {
            public static class States
            {
                public static readonly int Upgrade = Animator.StringToHash("Upgrade");
            }
        }

        public static class AnimatorTapToStartController
        {
            public static class States
            {
                public static readonly int Shake = Animator.StringToHash("Shake");
            }
        }

        public static class Scenes
        {
            public static readonly string Level1 = nameof(Level1);
            public static readonly string Level2 = nameof(Level2);
            public static readonly string Level3 = nameof(Level3);
            public static readonly string Level4 = nameof(Level4);
            public static readonly string Level5 = nameof(Level5);
            public static readonly string Level6 = nameof(Level6);
            public static readonly string Level7 = nameof(Level7);
            public static readonly string Level8 = nameof(Level8);
        }
    }
}